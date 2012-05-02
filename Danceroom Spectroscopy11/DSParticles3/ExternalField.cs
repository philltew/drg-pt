using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticles3
{
	public class ExternalField : ForceField
	{
		#region Private Members
		
		private double[] xforces;
		private double[] yforces;

		// stuff for gaussian smoothing
		private double[] gaussian;
		private int sigma, nGaussPoints, RangeEitherSide;

		// pixels for the background and at time t
		//private double[] BackgroundPixels;						// the background Pixels
		//private double[] calibrationCountVector;			// count of how many pixels went into the calibration average
		//private double[] timeTpixels;								// the time dependent Pixels
		//private double[] consecutiveZerosCount;      // the count of how many consecutive zeros there are

		//private int BoxHeight, BoxWidth;
		//private int GrabberCalls;
		//private int CalibrationCalls;
		//private int consecutiveZeroThreshold;

		//private bool Calibrating;

		private double[] m_GaussianSmoothedSlope_Temp = new double[3];
		private double[] m_CalculateForceField_TempArray = new double[256];

		private IFieldDataSource m_Source;

		#endregion

		#region Public Properties

		public override string ForceFieldType { get { return "ExternalField"; } }

		public IFieldDataSource Source
		{
			get { return m_Source; }
			set { m_Source = value; }
		} 

		/* 
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
		 */

		#endregion

		#region Constructor

		public ExternalField(ParticleEnsemble ensemble, IFieldDataSource source)
		{
			m_Source = source; 

			//GrabberCalls = 0;
			//CalibrationCalls = 1;
			//consecutiveZeroThreshold = 3;			
			//BoxWidth = ensemble.BoxWidth;
			//BoxHeight = ensemble.BoxHeight;
			//Calibrating = false;

			// initialize background pixels
			//BackgroundPixels = new double[BoxHeight * BoxWidth];

			// initialize the calibration count vector
			//calibrationCountVector = new double[BoxHeight * BoxWidth];

			// initialize timeTpixels
			//timeTpixels = new double[BoxHeight * BoxWidth];

			// initialize consecutiveZerosCount
			//consecutiveZerosCount = new double[BoxHeight * BoxWidth];

			// allocate vectors holding positions & forces
			xforces = new double[ensemble.MaxNumberOfParticles];
			yforces = new double[ensemble.MaxNumberOfParticles];

			// set up the gaussian array with the desired smoothing width parameter
			sigma = 10;
			nGaussPoints = 2 * (3 * sigma) + 1;
			RangeEitherSide = 3 * sigma;
			gaussian = new double[nGaussPoints];
			double point = -1.0 * (3.0 * (double)sigma);
			for (int i = 0; i < nGaussPoints; ++i)
			{
#if !UsePOW
				gaussian[i] = Math.Exp(-0.5 * (Math.Pow(point / (double)sigma, 2.0))) / ((double)sigma * Math.Sqrt(2.0 * Math.PI));
#else
				gaussian[i] = Math.Exp(-0.5 * (MathHelper.Pow2(point / (double)sigma))) / ((double)sigma * Math.Sqrt(2.0 * Math.PI));
#endif
				//    cout << point << " " << gaussian[i] << endl;
				++point;
			}
		}

		#endregion

		#region Resize Force Arrays

		public void ResizeForceArrays(ParticleEnsemble ensemble)
		{
			// clear out the forces vectors
			//xforces.Clear();
			//yforces.Clear();

			// allocate vectors holding positions & forces
			xforces = new double[ensemble.NumberOfParticles];
			yforces = new double[ensemble.NumberOfParticles];
		}

		#endregion

		/* 
		#region Zero Grabber Calls

		public void ZeroGrabberCalls()
		{
			GrabberCalls = 0;
			CalibrationCalls = 1;
			//calibrationCountVector.Clear();													// zero out the calibration count vector
			calibrationCountVector = new double[BoxHeight * BoxWidth];
			//BackgroundPixels.Clear();																// zero out the Background pixels
			BackgroundPixels = new double[BoxHeight * BoxWidth];
			//consecutiveZerosCount.Clear();
			consecutiveZerosCount = new double[BoxHeight * BoxWidth];
		}

		#endregion
		*/ 

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
			int BoxWidth = ensemble.BoxWidth;
			int BoxHeight = ensemble.BoxHeight;			

			// #pragma omp parallel for
			for (i = 0; i < ensemble.NumberOfParticles; ++i)
			{
				// initialize vectors holding forces
				xforces[i] = 0.0;																			// HERE'S THE PROBLEM - THE INDEX WILL OVERRUN THE VECTOR SIZE!!!
				yforces[i] = 0.0;
			}

			//#pragma omp parallel for
			for (i = 0; i < ensemble.NumberOfParticles; ++i)
			{
				posXi = ensemble.GetXParticlePosition(i);
				posYi = ensemble.GetYParticlePosition(i);
				radius = ensemble.GetParticleRadius(i);

				// get pixel vectors along the particle's X & Y axes for getting gradient of image field
				// there are 2 steps to this process: 
				//  (1) do some gaussian smoothing with a user defined width parameter (this determines how
				//      many pixels we need
				//  (2) determine the gradient from linear regression of the 3 surrounding points...
				//    cout << "particle " << i << " Xpos " << posXi << " Ypos " << posYi << endl;

				//    first get the vectors that we need - the length of the vectors depend on the width of the gaussian
				//    if the pixels are near the edge, the pixels beyond them (which arent in the image) are simply returned as zeros

				//    vector < double > AllThePixelsAlongX = pParticleSet->GetAllThePixelsAlongX(posYi,posXi,RangeEitherSide);
				//    xforces[i] = pParticleSet->GetGradientScaleFactor()*GaussianSmoothedSlope(posXi,AllThePixelsAlongX);
				//    cout << "Xposition " << posXi << endl;

				if (m_CalculateForceField_TempArray.Length < RangeEitherSide + 1)
				{
					m_CalculateForceField_TempArray = new double[RangeEitherSide + 1];
				}

				int count;				

				//List<double> SubsetOfPixelsAlongX = GetSubsetOfPixelsAlongX(posYi, posXi, RangeEitherSide + 1);
				GetSubsetOfPixelsAlongX(posXi, posYi, RangeEitherSide + 1, ref m_CalculateForceField_TempArray, out count);

				//    for(int kk=0; kk<SubsetOfPixelsAlongX.size(); ++kk){
				//      cout << kk << " " << SubsetOfPixelsAlongX[kk] << endl;      
				//    }

				//    cout << "Xposition " << posXi << endl;
				//    for(int kk=1;kk<SubsetOfPixelsAlongX.size();++kk){cout << kk << " " << SubsetOfPixelsAlongX[kk] << endl;}
				//xforces[i] = ensemble.GetGradientScaleFactor() * GaussianSmoothedSlope(posXi, SubsetOfPixelsAlongX);
				xforces[i] = ensemble.GradientScaleFactor * GaussianSmoothedSlope(posXi, m_CalculateForceField_TempArray, count);

				//    vector < double > AllThePixelsAlongY = pParticleSet->GetAllThePixelsAlongY(posXi,posYi,RangeEitherSide);
				//    cout << "Yposition " << posYi << endl;
				//    for(int kk=0;kk<AllThePixelsAlongY.size();++kk){cout << kk << " " << AllThePixelsAlongY[kk] << endl;}
				//    yforces[i] = pParticleSet->GetGradientScaleFactor()*GaussianSmoothedSlope(posYi,AllThePixelsAlongY);

				//List<double> SubsetOfPixelsAlongY = GetSubsetOfPixelsAlongY(posXi, posYi, RangeEitherSide + 1);
				GetSubsetOfPixelsAlongY(posXi, posYi, RangeEitherSide + 1, ref m_CalculateForceField_TempArray, out count);
				//    cout << "Yposition " << endl;
				//yforces[i] = ensemble.GetGradientScaleFactor() * GaussianSmoothedSlope(posYi, SubsetOfPixelsAlongY);
				yforces[i] = ensemble.GradientScaleFactor * GaussianSmoothedSlope(posYi, m_CalculateForceField_TempArray, count);
				//    cout << "yforces[i] " << i << " " << yforces[i] << endl;    

				// get the gradient scale factor, depending on whether the particle is attractive or repulsive

				AtomicInfo typeInfo = ParticleStaticObjects.AtomPropertiesDefinition.Lookup[(ensemble.Particles[i]).TypeID];
				double attractiveOrRepulsiveFactor = typeInfo.AttractiveOrRepulsive;
				xforces[i] *= attractiveOrRepulsiveFactor;
				yforces[i] *= attractiveOrRepulsiveFactor;

			}


			ensemble.AddXForces(xforces);		  // set the forces in the Particle Ensemble Object
			ensemble.AddYForces(yforces);			// set the potential energy
			ensemble.AddPotentialEnergy(PotentialEnergy);		// add in the potential energy    

		}

		#endregion
		
		#region Energy Terms (Defunct!)
		
		public override void CalculateEnergyTerms(ParticleEnsemble ensemble)
		{
			throw new NotImplementedException();
		}

		public override void UpdateEnergyTerms(ParticleEnsemble ensemble)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Field Pixel Values with Smoothing

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Xposition"></param>
		/// <param name="Yposition"></param>
		/// <param name="PixelsEitherSide"></param>
		/// <param name="Xpixels"></param>
		/// <param name="count"></param>
		private void GetSubsetOfPixelsAlongX(double Xposition, double Yposition, int PixelsEitherSide, ref double[] Xpixels, out int count)
		{
			if (m_Source != null)
			{
				m_Source.GetSubsetOfPixelsAlongX(Xposition, Yposition, PixelsEitherSide, ref Xpixels, out count);
			}
			else
			{
				count = 0;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Xposition"></param>
		/// <param name="Yposition"></param>
		/// <param name="PixelsEitherSide"></param>
		/// <param name="Ypixels"></param>
		/// <param name="count"></param>
		private void GetSubsetOfPixelsAlongY(double Xposition, double Yposition, int PixelsEitherSide, ref double[] Ypixels, out int count)
		{
			if (m_Source != null)
			{
				m_Source.GetSubsetOfPixelsAlongY(Xposition, Yposition, PixelsEitherSide, ref Ypixels, out count);
			}
			else
			{
				count = 0; 
			}
		}

		#region Gaussian Smoothing

		/// <summary>
		/// function for doing Gaussian smoothing and then simple 3 point linear regression to find the slope of the 
		/// middle point in a data vector
		/// </summary>
		/// <param name="Position"></param>
		/// <param name="pixelValues"></param>
		/// <param name="number"></param>
		/// <returns></returns>
		private double GaussianSmoothedSlope(double Position, double[] pixelValues, int number)
		{
			int i, j, ctr;
			double gradient = 0.0, sum = 0.0;

			int pos = (int)Position;

			for (i = 0; i < 3; ++i)
			{
				sum = 0.0;

				for (j = 0; j < nGaussPoints; ++j)
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

		/// <summary>
		/// the function below uses a simple linear regression formula to get the slope of a vector of n points
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		private double LinearRegression(double[] yvalues, int count)
		{
			int i, xsum = 0, ysum = 0;
			double XminusXavg = 0.0, YminusYavg = 0.0;
			double xavg = 0.0, yavg = 0.0, npoints = 0.0;
			double slope = 0.0, numerator = 0.0, denominator = 0.0;

			npoints = (double)count;

			for (i = 0; i < count; ++i)
			{
				//    cout << i << " " << yvalues[i] << endl;
				xsum += i;
				ysum += (int)yvalues[i];
			}

			xavg = (double)xsum / npoints;
			yavg = (double)ysum / npoints;

			for (i = 0; i < npoints; ++i)
			{
				XminusXavg = (double)i - xavg;
				YminusYavg = (double)yvalues[i] - yavg;
				numerator += XminusXavg * YminusYavg;
#if !UsePOW
				denominator += Math.Pow(XminusXavg, 2.0);
#else
				denominator += MathHelper.Pow2(XminusXavg);
#endif
			}

			slope = numerator / denominator;

			return slope;
		}

		#endregion

		#endregion

		#region Modfifing the field 
		
		/* 
		#region Background Calibration

		/// <summary>
		/// overloaded functions for setting the Background (this is what the Kinect Uses)
		/// </summary>
		/// <param name="num"></param>
		/// <param name="newPixels"></param>
		public void BackgroundCalibration(int num, double[] newPixels)
		{
			// this function takes a pointer to an array of doubles
			//	int pixToPrint = 640*123+275;  // debug print-outs
			for (int i = 0; i < num; ++i)
			{
				if (newPixels[i] != 0.0)
				{
					double CallsSoFar = calibrationCountVector[i];
					double InverseCallsPlusOne = 1.0 / (CallsSoFar + 1.0);
					BackgroundPixels[i] = (CallsSoFar * BackgroundPixels[i] + newPixels[i]) * (InverseCallsPlusOne);
					++calibrationCountVector[i];
				}

				// debug print-outs
				//		if(i==pixToPrint){cout << "pixel " << i << " " << calibrationCountVector[i] << " " << newPixels[i] << " " << BackgroundPixels[i] << " " << endl;}
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
		*/ 

		/* 
		#region Pixel Diff
		
		// overloaded functions for setting the pixels at time t
		public void SetPixelDiff(int num, double[] newPixels)
		{
					//	if(newPixels != NULL){
		//		timeTpixels = newPixels;   // aim the timeTpixels pointer at the newPixels pointer to avoid overhead of element-by-element copying
		//	}
			for (int i = 0; i < num; ++i)
			{
				//if (newPixels[i] && newPixels[i] != 0.0)
				if (newPixels[i] != 0.0)
				{										// (a) if the data is there & it's not zero
					timeTpixels[i] = newPixels[i];
				}
				else if (consecutiveZerosCount[i] > consecutiveZeroThreshold)
				{ // (b) if there's been 10 consecutive zeros in one pixel
					timeTpixels[i] = BackgroundPixels[i];
					consecutiveZerosCount[i] = 0.0;
				}
				else
				{													// (c) otherwise increment the consecutive zeros count on the pixel
					++consecutiveZerosCount[i];		// ** The whole point of (b) & (c) is to deal with noisy calibration areas in the 
				}                               //		kinect pixel array - which are generally idenitifiable because they have lots of 
			}																//		consecutive zeros interspersed with the occasional data point. The if statements above
			
		}

		public void SetPixelDiff(int num, List<double> newPixels)
		{
			//    #pragma omp parallel for   // use openmp to parallelize these loops
			for (int i = 0; i < num; ++i)
			{
				if (newPixels[i] != 0.0)
				{  // check to be sure that the data is there
					timeTpixels[i] = newPixels[i];
				}
			}
		}

		public void SetPixelDiff(int num, float[] newPixels)
		{
			// if (newPixels != NULL)
			{
				for (int i = 0; i < num; ++i)
				{
					timeTpixels[i] = (double)(newPixels[i]);
				}
			}
		}

		#endregion
		*/ 

		#endregion

		#region Read / Write Background file
		
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

		/* 
		#region Remove Zeros From Background File

		public void RemoveZerosFromBackgroundFile()
		{
			Random rand = new Random();

			// this is a really quick and dirty interpolation algorithm that just puts the last good pixel value 
			int ctr = 0, firstRandomPixelIdx = 0;													// in blind spots
			double lastNonzeroPixelValue = 0.0, firstRandomPixelValue = 0.0;

			do
			{																							// select a random nonzero pixel somewhere in the pixel array
				firstRandomPixelIdx = (int)rand.Next(0, BoxHeight * BoxWidth);
				firstRandomPixelValue = BackgroundPixels[firstRandomPixelIdx];
			}
			while (firstRandomPixelValue == 0.0);

			lastNonzeroPixelValue = firstRandomPixelValue;

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

			lastNonzeroPixelValue = firstRandomPixelValue;

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
		*/ 

		#endregion
	}
}