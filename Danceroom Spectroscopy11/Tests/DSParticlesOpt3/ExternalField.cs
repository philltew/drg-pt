using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticlesOpt3
{
	public class ExternalField : ForceField
	{
		#region Private Members
		
		private DPVector[] forces;

		// stuff for gaussian smoothing
		private double[] gaussian;
		private int sigma, nGaussPoints, RangeEitherSide;

		// pixels for the background and at time t
		private List<double> BackgroundPixels;						// the background Pixels
		private List<double> calibrationCountVector;			// count of how many pixels went into the calibration average
		private List<double> timeTpixels;								// the time dependent Pixels
		private List<double> consecutiveZerosCount;      // the count of how many consecutive zeros there are

		private int BoxHeight, BoxWidth;
		private int GrabberCalls;
		private int CalibrationCalls;
		private int consecutiveZeroThreshold;

		private bool Calibrating;
		private double[] m_CalculateForceField_TempArray = new double[256];
		private double[] m_GaussianSmoothedSlope_Temp = new double[3];

		#endregion

		public override string ForceFieldType { get { return "ExternalField"; } }

		#region Constructor

		public ExternalField(ParticleEnsemble ensemble)
		{
			GrabberCalls = 0;
			CalibrationCalls = 1;
			consecutiveZeroThreshold = 3;
			BoxHeight = ensemble.BoxHeight;
			BoxWidth = ensemble.BoxWidth;
			Calibrating = false;

			// initialize background pixels
			BackgroundPixels = new List<double>(new double[BoxHeight * BoxWidth]);

			// initialize the calibration count vector
			calibrationCountVector = new List<double>(new double[BoxHeight * BoxWidth]);

			// initialize timeTpixels
			timeTpixels = new List<double>(new double[BoxHeight * BoxWidth]);

			// initialize consecutiveZerosCount
			consecutiveZerosCount = new List<double>(new double[BoxHeight * BoxWidth]);

			// allocate vectors holding positions & forces
			forces = new DPVector[ensemble.MaxNumberOfParticles];			

			// set up the gaussian array with the desired smoothing width parameter
			sigma = 10;
			nGaussPoints = 2 * (3 * sigma) + 1;
			RangeEitherSide = 3 * sigma;
			gaussian = new double[nGaussPoints];
			double point = -1.0 * (3.0 * (double)sigma);

			for (int i = 0; i < nGaussPoints; ++i)
			{
				gaussian[i] = Math.Exp(-0.5 * (Math.Pow(point / (double)sigma, 2.0))) / ((double)sigma * Math.Sqrt(2.0 * Math.PI));				
				++point;
			}

		}

		#endregion

		#region Resize Force Arrays

		public void ResizeForceArrays(ParticleEnsemble ensemble)
		{			
			DPVector zero = new DPVector(0, 0); 

			for (int i = 0; i < ensemble.NumberOfParticles; i++)
			{
				forces[i] = zero;				
			}
		}

		#endregion

		#region Zero Grabber Calls
		
		public void ZeroGrabberCalls()
		{
			GrabberCalls = 0;
			CalibrationCalls = 1;
			calibrationCountVector.Clear();													// zero out the calibration count vector
			calibrationCountVector.AddRange(new double[BoxHeight * BoxWidth]);
			BackgroundPixels.Clear();																// zero out the Background pixels
			BackgroundPixels.AddRange(new double[BoxHeight * BoxWidth]);
			consecutiveZerosCount.Clear();
			consecutiveZerosCount.AddRange(new double[BoxHeight * BoxWidth]);
		}

		#endregion

		#region Calculate Force Field
		
		/// <summary>
		/// function to calculate Soft Spheres forcefield
		/// </summary>
		/// <param name="ensemble"></param>
		public override void CalculateForceField(ParticleEnsemble ensemble)
		{
			//  variable declarations    
			int i;
			double posXi, posYi, radius;
			double PotentialEnergy = 0.0;

			// variable initializations  
			int BoxHeight = ensemble.BoxHeight;
			int BoxWidth = ensemble.BoxWidth;

			DPVector zero = new DPVector(0, 0); 

			// #pragma omp parallel for
			for (i = 0; i < ensemble.NumberOfParticles; ++i)
			{
				// initialize vectors holding forces
				forces[i] = zero;																							
			}

			//#pragma omp parallel for
			for (i = 0; i < ensemble.NumberOfParticles; ++i)
			{
				Particle particlei = ensemble.GetParticle(i);

				posXi = particlei.Position.X; 
				posYi = particlei.Position.Y; 
				radius = particlei.Radius; 

				// get pixel vectors along the particle's X & Y axes for getting gradient of image field
				// there are 2 steps to this process: 
				//  (1) do some gaussian smoothing with a user defined width parameter (this determines how
				//      many pixels we need
				//  (2) determine the gradient from linear regression of the 3 surrounding points...
				//    cout << "particle " << i << " Xpos " << posXi << " Ypos " << posYi << endl;

				//    first get the vectors that we need - the length of the vectors depend on the width of the gaussian
				//    if the pixels are near the edge, the pixels beyond them (which arent in the image) are simply returned as zeros

				if (m_CalculateForceField_TempArray.Length < RangeEitherSide + 1)
				{
					m_CalculateForceField_TempArray = new double[RangeEitherSide + 1]; 
				}

				int count; 

				GetSubsetOfPixelsAlongX(posYi, posXi, RangeEitherSide + 1, ref m_CalculateForceField_TempArray, out count);
				forces[i].X = ensemble.GradientScaleFactor * GaussianSmoothedSlope(posXi, m_CalculateForceField_TempArray, count);

				GetSubsetOfPixelsAlongY(posXi, posYi, RangeEitherSide + 1, ref m_CalculateForceField_TempArray, out count);
				forces[i].Y = ensemble.GradientScaleFactor * GaussianSmoothedSlope(posYi, m_CalculateForceField_TempArray, count);

				// get the gradient scale factor, depending on whether the particle is attractive or repulsive
				ParticleInfo typeInfo = ParticleStaticObjects.AtomPropertiesDefinition.Lookup[(ensemble.Particles[i]).TypeID];
				
				double attractiveOrRepulsiveFactor = typeInfo.AttractiveOrRepulsive;
				forces[i].X *= attractiveOrRepulsiveFactor;
				forces[i].Y *= attractiveOrRepulsiveFactor;

			}

			// set the forces in the Particle Ensemble Object
			ensemble.AddForces(forces);
			// add in the potential energy    
			ensemble.AddPotentialEnergy(PotentialEnergy);		
		}

		#endregion

		public override void CalculateEnergyTerms(ParticleEnsemble ensemble)
		{
			throw new NotImplementedException();
		}

		public override void UpdateEnergyTerms(ParticleEnsemble ensemble)
		{
			throw new NotImplementedException();
		}

		#region Gaussian Smoothed Slope
		
		/// <summary>
		/// function for doing Gaussian smoothing and then simple 3 point linear regression to find the slope of the 
		/// middle point in a data vector
		/// </summary>
		/// <param name="position"></param>
		/// <param name="pixelValues"></param>
		/// <param name="number"></param>
		/// <returns></returns>
		public double GaussianSmoothedSlope(double position, double[] pixelValues, int number)
		{
			double gradient = 0.0;			

			int pos = (int)position;

			for (int i = 0; i < 3; ++i)
			{
				double sum = 0.0;

				for (int j = 0; j < nGaussPoints; ++j)
				{
					sum += gaussian[j] * pixelValues[i + j];
				}

				if (double.IsNaN(sum))
				{
					sum = 0.0;
				}

				m_GaussianSmoothedSlope_Temp[i] = sum;
			}

			gradient = LinearRegression(m_GaussianSmoothedSlope_Temp, 3);

			return gradient;
		}

		#endregion

		#region Linear Regression

		/// <summary>
		/// the function below uses a simple linear regression formula to get the slope of a vector of n points
		/// </summary>
		/// <param name="values"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public double LinearRegression(double[] values, int count)
		{
			int i, xsum = 0, ysum = 0;
			double XminusXavg = 0.0, YminusYavg = 0.0;
			double xavg = 0.0, yavg = 0.0, npoints = 0.0;
			double slope = 0.0, numerator = 0.0, denominator = 0.0;

			npoints = (double)count;

			for (i = 0; i < count; ++i)
			{
				xsum += i;
				ysum += (int)values[i];
			}

			xavg = (double)xsum / npoints;
			yavg = (double)ysum / npoints;

			for (i = 0; i < npoints; ++i)
			{
				XminusXavg = (double)i - xavg;
				YminusYavg = (double)values[i] - yavg;
				numerator += XminusXavg * YminusYavg;
				denominator += Math.Pow(XminusXavg, 2.0);
			}

			slope = numerator / denominator;

			return slope;
		}

		#endregion

		#region Increment Grabber Calls / Misc Get Methods

		public void IncrementGrabberCalls()
		{
			++GrabberCalls;
		}

		public double getBackgroundPixelValue(int i)
		{
			return BackgroundPixels[i];
		}

		public double getTimeTpixelValue(int i)
		{
			return timeTpixels[i];
		}

		#endregion

		#region Get Subset Of Pixels

		/// <summary>
		/// Given some X,Y coordinates, the two functions below returns pixels along the Y-direction
		/// GetAllThePixelsAlongX is mostly for debugging & checking that the Gaussian smoothing is behaving sensibly
		/// </summary>
		/// <param name="Yposition"></param>
		/// <param name="Xposition"></param>
		/// <param name="PixelsEitherSide"></param>
		/// <param name="Xpixels"></param>
		/// <param name="count"></param>
		public void GetSubsetOfPixelsAlongX(double Yposition, double Xposition, int PixelsEitherSide, ref double[] Xpixels, out int count)
		{
			int i, Xmin, Xmax;			

			// beginning of the block for returning the pixel array (along the X dimension) necessary 
			//     to carry out gaussian smoothing of 3 pixels - 
			//        (1) the pixel at which the particle is centered
			//        (2) the pixels on either side of the particle center    

			Xmin = (int)(Xposition - (double)PixelsEitherSide);
			Xmax = (int)(Xposition + (double)PixelsEitherSide);
			
			i = 0;
			int remaining, extra;
			int index = Xmin; 

			if (Xmin < 0)
			{
				#region Clipping on the left side

				for (int j = 0, je = -Xmin; j > je; j++)
				{
					Xpixels[i++] = 0.0;
				}

				remaining = Xmax;
				index = (int)(Yposition) * BoxWidth; 

				#endregion
			}
			else
			{
				remaining = Xmax - Xmin;
				index = (int)(Yposition) * BoxWidth + Xmin; 
			}

			if (Xmax >= BoxWidth)
			{
				remaining -= Xmax - BoxWidth;
				extra = Xmax - BoxWidth;
			}
			else
			{
				extra = 0;
			}

			#region Non clipping Region

			for (int j = 0, je = remaining; j > je; j++)
			{
				double timeValue = timeTpixels[index]; 
				
				if (timeValue != 0)
				{
					Xpixels[i++] = timeValue - BackgroundPixels[index];										
				}
				else
				{
					Xpixels[i++] = 0;
				}
				
				index++; 
			}

			#endregion

			#region Clipping on the right side

			for (int j = 0, je = extra; j > je; j++)
			{
				Xpixels[i++] = 0;
			}

			#endregion

			count = i - 1; 
		}

		/// <summary>
		/// Given some X,Y coordinates, this function returns all the pixels along the Y-direction;
		/// Mostly, this code has been used for debugging & checking that the Gaussian smoothing is behaving sensibly
		/// </summary>
		/// <param name="Xposition"></param>
		/// <param name="Yposition"></param>
		/// <param name="PixelsEitherSide"></param>
		/// <param name="Ypixels"></param>
		/// <param name="count"></param>
		public void GetSubsetOfPixelsAlongY(double Xposition, double Yposition, int PixelsEitherSide, ref double[] Ypixels, out int count)
		{
			int i, Ymin, Ymax;

			// beginning of the block for returning the pixel array (along the Y dimension) necessary 
			//     to carry out gaussian smoothing of 3 pixels - 
			//        (1) the pixel at which the particle is centered
			//        (2) the pixels on either side of the particle center    

			Ymin = (int)(Yposition - (double)PixelsEitherSide);
			Ymax = (int)(Yposition + (double)PixelsEitherSide);

			i = 0;
			int remaining, extra;
			int index = Ymin;

			if (Ymin < 0)
			{
				#region Clipping on the top side

				for (int j = 0, je = -Ymin; j > je; j++)
				{
					Ypixels[i++] = 0.0;
				}

				remaining = Ymax;
				index = (int)Xposition;

				#endregion
			}
			else
			{
				remaining = Ymax - Ymin;
				index = ((int)Ymin * BoxWidth) + (int)Xposition;
			}

			if (Ymax >= BoxHeight)
			{
				remaining -= Ymax - BoxHeight;
				extra = Ymax - BoxHeight;
			}
			else
			{
				extra = 0;
			}

			#region Non clipping Region

			for (int j = 0, je = remaining; j > je; j++)
			{
				double timeValue = timeTpixels[index];

				if (timeValue != 0)
				{
					Ypixels[i++] = timeValue - BackgroundPixels[index];
				}
				else
				{
					Ypixels[i++] = 0;
				}

				index += BoxWidth;
			}

			#endregion

			#region Clipping on the right side

			for (int j = 0, je = extra; j > je; j++)
			{
				Ypixels[i++] = 0;
			}

			#endregion

			count = i - 1; 
		}

		#endregion

		#region Background Calibration

		/// <summary>
		/// overloaded functions for setting the Background (this is what the Kinect Uses)
		/// </summary>
		/// <param name="num"></param>
		/// <param name="newPixels"></param>
		public void BackgroundCalibration(int num, double[] newPixels)
		{
			// this function takes a pointer to an array of doubles			
			for (int i = 0; i < num; ++i)
			{
				if (newPixels[i] != 0.0)
				{
					double CallsSoFar = calibrationCountVector[i];
					double InverseCallsPlusOne = 1.0 / (CallsSoFar + 1.0);
					BackgroundPixels[i] = (CallsSoFar * BackgroundPixels[i] + newPixels[i]) * (InverseCallsPlusOne);
					++calibrationCountVector[i];
				}
			}
		}

		public void BackgroundCalibration(int num, float[] newPixels)
		{
			// this function takes a pointer to an array of floats
			for (int i = 0; i < num; ++i)
			{
				if (newPixels[i] != 0.0)
				{
					double CallsSoFar = calibrationCountVector[i];
					double InverseCallsPlusOne = 1.0 / (CallsSoFar + 1.0);
					BackgroundPixels[i] = (CallsSoFar * BackgroundPixels[i] + (double)(newPixels[i])) * (InverseCallsPlusOne);
					++calibrationCountVector[i];
				}
			}
		}

		public void BackgroundCalibration(int num, List<double> newPixels)
		{
			// this function takes the address of a vector of doubles - mostly historical
			for (int i = 0; i < num; ++i)
			{
				if (newPixels[i] != 0.0)
				{
					double CallsSoFar = calibrationCountVector[i];
					double InverseCallsPlusOne = 1.0 / (CallsSoFar + 1.0);
					BackgroundPixels[i] = (CallsSoFar * BackgroundPixels[i] + newPixels[i]) * (InverseCallsPlusOne);
					++calibrationCountVector[i];
				}
			}
		}

		#endregion

		#region Set Pixel Diff

		// overloaded functions for setting the pixels at time t
		public void SetPixelDiff(int num, double[] newPixels)
		{
			for (int i = 0; i < num; ++i)
			{
				if (newPixels[i] != 0.0)
				{										
					// (a) if the data is there & it's not zero
					timeTpixels[i] = newPixels[i];
				}
				else if (consecutiveZerosCount[i] > consecutiveZeroThreshold)
				{ 
					// (b) if there's been 10 consecutive zeros in one pixel
					timeTpixels[i] = BackgroundPixels[i];
					consecutiveZerosCount[i] = 0.0;
				}
				else
				{	
					// (c) otherwise increment the consecutive zeros count on the pixel
					// ** The whole point of (b) & (c) is to deal with noisy calibration areas in the 
					//	  kinect pixel array - which are generally idenitifiable because they have lots of 
					//	  consecutive zeros interspersed with the occasional data point. The if statements above
					++consecutiveZerosCount[i];		
				}                              
			}																			
		}

		public void SetPixelDiff(int num, List<double> newPixels)
		{			
			for (int i = 0; i < num; ++i)
			{
				if (newPixels[i] != 0.0)
				{  
					// check to be sure that the data is there
					timeTpixels[i] = newPixels[i];
				}
			}
		}

		public void SetPixelDiff(int num, float[] newPixels)
		{
			for (int i = 0; i < num; ++i)
			{
				timeTpixels[i] = (double)(newPixels[i]);
			}
		}

		#endregion

		#region Calibrating

		public void SetCalibrating(bool value)
		{
			Calibrating = value;
		}

		public bool GetCalibrating()
		{
			return Calibrating;
		}

		public int GetGrabberCalls()
		{
			return GrabberCalls;
		}

		public void setGrabberCalls(int value)
		{
			GrabberCalls = value;
		}

		#endregion

		#region Remove Zeros From Background File

		public void RemoveZerosFromBackgroundFile()
		{
			Random rand = new Random(); 

			// this is a really quick and dirty interpolation algorithm that just puts the last good pixel value in blind spots
			int firstRandomPixelIdx = 0;													
			double lastNonzeroPixelValue = 0.0, firstRandomPixelValue = 0.0;
	
			do
			{																							
				// select a random nonzero pixel somewhere in the pixel array
				firstRandomPixelIdx = (int)rand.Next(0, BoxHeight * BoxWidth);
				firstRandomPixelValue = BackgroundPixels[firstRandomPixelIdx];
			}  
			while (firstRandomPixelValue == 0.0);
	
			lastNonzeroPixelValue=firstRandomPixelValue;

			for (int i = firstRandomPixelIdx; i < BoxWidth * BoxHeight; ++i)
			{      
				// iterate forward from the firstRandomPixel
				if (BackgroundPixels[i] == 0 && lastNonzeroPixelValue != 0.0)
				{
					BackgroundPixels[i] = lastNonzeroPixelValue;
				}
				else
				{
					lastNonzeroPixelValue = BackgroundPixels[i];
				}
			}

			lastNonzeroPixelValue=firstRandomPixelValue;

			for (int i = firstRandomPixelIdx - 1; i >= 0; --i)
			{										
				// iterate backward from the firstRandomPixel
				if (BackgroundPixels[i] == 0 && lastNonzeroPixelValue != 0.0)
				{
					BackgroundPixels[i] = lastNonzeroPixelValue;
				}
				else
				{
					lastNonzeroPixelValue = BackgroundPixels[i];
				}
			}
		}

		#endregion

		#region Read / Write Background File

		public bool ReadBackgroundFile(string stringName)
		{
			/* 
			// get the absolute path of the data directory && append it to the stringName to get the full path
			string fullpath = ofToDataPath(stringName,true);
			cout << "Reading the background file from: " << fullpath << endl;	
			static FILE *backgroundFile;
		
			if((backgroundFile = fopen(fullpath.c_str(),"r"))==NULL){             // open the file
				cout << "Error opening background file " << fullpath << endl;
				return false;
			}
			else{
				static char line[100];
				static float value;
				for(int i=0;i<BoxHeight*BoxWidth;++i){			
					fgets(line,100,backgroundFile);
					sscanf(line, "%f\n", &value);
					BackgroundPixels[i] = (double) value;
	//				cout << BackgroundPixels[i] << endl;
				}
				fclose(backgroundFile);
				return true;
			}
			 * */
			return false;
		}

		public bool WriteBackgroundFile(string stringName)
		{
			/* 
			// get the absolute path of the data directory && append it to the stringName to get the full path
			string fullpath = ofToDataPath(stringName,true);
			cout << "Writing the background file to: " << fullpath << endl;		
				
			ofstream outputfile;													// initialize an ofstream object
			outputfile.open (fullpath.c_str());						// open the ofstream object
			if (outputfile.is_open()){										// write the Background pixels to the test file
				for(int i=0;i<BoxHeight*BoxWidth;++i){
					outputfile << BackgroundPixels[i] << "\n";
				}
				outputfile.close();
				return true;
			}
			else cout << "failed to open " << fullpath << endl;
			*/
			return false;
		}
		
		#endregion
	}
}
