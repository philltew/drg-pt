using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticlesOpt2
{
	public class ExternalField : ForceField
	{
		public const int MaxParticles = 1024;

		private DPVector[] forces = new DPVector[MaxParticles];		

		// stuff for gaussian smoothing
		private double[] gaussian;
		private int sigma, nGaussPoints, RangeEitherSide;

		// pixels for the background and at time t
		private double[] BackgroundPixels;						// the background Pixels
		private double[] calibrationCountVector;			// count of how many pixels went into the calibration average
		private double[] timeTpixels;								// the time dependent Pixels
		private double[] consecutiveZerosCount;      // the count of how many consecutive zeros there are

		private int BoxHeight, BoxWidth;
		private int GrabberCalls;
		private int CalibrationCalls;
		private int consecutiveZeroThreshold;

		private bool Calibrating;

		private double[] m_CalculateForceField_TempArray = new double[256];

		public override string ForceFieldType { get { return "ExternalField"; } }

		public ExternalField(ParticleEnsemble ensemble)
		{
			GrabberCalls = 0;
			CalibrationCalls = 1;
			consecutiveZeroThreshold = 3;
			BoxHeight = ensemble.GetBoxHeight();
			BoxWidth = ensemble.GetBoxWidth();
			Calibrating = false;

			// initialize background pixels
			BackgroundPixels = new double[BoxHeight * BoxWidth];

			// initialize the calibration count vector
			calibrationCountVector = new double[BoxHeight * BoxWidth];

			// initialize timeTpixels
			timeTpixels = new double[BoxHeight * BoxWidth];

			// initialize consecutiveZerosCount
			consecutiveZerosCount = new double[BoxHeight * BoxWidth];

			// allocate vectors holding positions & forces
			//xforces = new List<double>(new double[ensemble.GetNumberOfParticles()]);
			//yforces = new List<double>(new double[ensemble.GetNumberOfParticles()]);

			// set up the gaussian array with the desired smoothing width parameter
			sigma = 10;
			nGaussPoints = 2 * (3 * sigma) + 1;
			RangeEitherSide = 3 * sigma;
			gaussian = new double[nGaussPoints];
			double point = -1.0 * (3.0 * (double)sigma);
			for (int i = 0; i < nGaussPoints; ++i)
			{
				gaussian[i] = Math.Exp(-0.5 * (Math.Pow(point / (double)sigma, 2.0))) / ((double)sigma * Math.Sqrt(2.0 * Math.PI));
				//    cout << point << " " << gaussian[i] << endl;
				++point;
			}

		}

		public void ResizeForceArrays(ParticleEnsemble ensemble)
		{
			// clear out the forces vectors
			//xforces.Clear();
			//yforces.Clear();

			// allocate vectors holding positions & forces
			//xforces.AddRange(new double[ensemble.GetNumberOfParticles()]);
			//yforces.AddRange(new double[ensemble.GetNumberOfParticles()]);

			for (int i = 0; i < MaxParticles; i++)
			{
				forces[i] = new DPVector(0, 0);
			}
		}

		public void ZeroGrabberCalls()
		{
			GrabberCalls = 0;
			CalibrationCalls = 1;

			for (int i = 0, ie = BoxHeight * BoxWidth; i < ie; i++)
			{
				calibrationCountVector[i] = 0;
				BackgroundPixels[i] = 0;
				consecutiveZerosCount[i] = 0;
			}
		}		

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
			int BoxHeight = ensemble.GetBoxHeight();
			int BoxWidth = ensemble.GetBoxWidth();

			// #pragma omp parallel for
			for (i = 0; i < ensemble.GetNumberOfParticles(); ++i)
			{
				// initialize vectors holding forces
				forces[i] = new DPVector(0, 0);																			// HERE'S THE PROBLEM - THE INDEX WILL OVERRUN THE VECTOR SIZE!!!			
			}

			//#pragma omp parallel for
			for (i = 0; i < ensemble.GetNumberOfParticles(); ++i)
			{
				Particle particle = ensemble.GetParticle(i);
				
				posXi = particle.Position.X; //ensemble.GetXParticlePosition(i);
				posYi = particle.Position.Y; //ensemble.GetYParticlePosition(i);
				radius = particle.Radius; //ensemble.GetParticleRadius(i);

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
				DPVector newForce = new DPVector(); 

				GetSubsetOfPixelsAlongX(posYi, posXi, RangeEitherSide + 1, ref m_CalculateForceField_TempArray, out count);
				//    for(int kk=0; kk<SubsetOfPixelsAlongX.size(); ++kk){
				//      cout << kk << " " << SubsetOfPixelsAlongX[kk] << endl;      
				//    }

				//    cout << "Xposition " << posXi << endl;
				//    for(int kk=1;kk<SubsetOfPixelsAlongX.size();++kk){cout << kk << " " << SubsetOfPixelsAlongX[kk] << endl;}
				newForce.X = ensemble.GetGradientScaleFactor() * GaussianSmoothedSlope(posXi, m_CalculateForceField_TempArray, count);

				//    vector < double > AllThePixelsAlongY = pParticleSet->GetAllThePixelsAlongY(posXi,posYi,RangeEitherSide);
				//    cout << "Yposition " << posYi << endl;
				//    for(int kk=0;kk<AllThePixelsAlongY.size();++kk){cout << kk << " " << AllThePixelsAlongY[kk] << endl;}
				//    yforces[i] = pParticleSet->GetGradientScaleFactor()*GaussianSmoothedSlope(posYi,AllThePixelsAlongY);


				GetSubsetOfPixelsAlongY(posXi, posYi, RangeEitherSide + 1, ref m_CalculateForceField_TempArray, out count);
				//List<double> SubsetOfPixelsAlongY = GetSubsetOfPixelsAlongY(posXi, posYi, RangeEitherSide + 1);
				//    cout << "Yposition " << endl;
				newForce.Y = ensemble.GetGradientScaleFactor() * GaussianSmoothedSlope(posYi, m_CalculateForceField_TempArray, count);
				//    cout << "yforces[i] " << i << " " << yforces[i] << endl;    

				// get the gradient scale factor, depending on whether the particle is attractive or repulsive

				ParticleInfo typeInfo = ParticleStaticObjects.AtomPropertiesDefinition.Lookup[particle.TypeID];
				double attractiveOrRepulsiveFactor = typeInfo.AttractiveOrRepulsive;
				newForce.X *= attractiveOrRepulsiveFactor;
				newForce.Y *= attractiveOrRepulsiveFactor;

				forces[i] = newForce;
			}


			ensemble.AddForces(forces);		  // set the forces in the Particle Ensemble Object			
			ensemble.AddPotentialEnergy(PotentialEnergy);		// add in the potential energy    

		}

		public override void CalculateEnergyTerms(ParticleEnsemble ensemble)
		{
			throw new NotImplementedException();
		}

		public override void UpdateEnergyTerms(ParticleEnsemble ensemble)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// function for doing Gaussian smoothing and then simple 3 point linear regression to find the slope of the 
		/// middle point in a data vector
		/// </summary>
		/// <param name="Position"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public double GaussianSmoothedSlope(double Position, List<double> pixelValues)
		{

			int i, j, ctr;
			double gradient = 0.0, sum = 0.0;
			List<double> ThreePoints = new List<double>();

			int pos = (int)Position;

			// below is the block for smoothing the entire 1d pixel array in each dimension
			// this was used for debugging & requires all the values in either the x or y direction to be passed in 
			// the method for doing this is on the ParticleEnsemble class - smoothing all the data is very computationally expensive
			/*
			//  cout << " convolution " << endl;
			for(i=30;i<(pixelValues.size()-30);++i){
			sum = 0.0;
			for(j=1;j<nGaussPoints;++j){
			sum += gaussian[j]*pixelValues[i-RangeEitherSide+j];
			}
			if(isnan(sum)){sum=0.0;}
			//    cout << i << " " << sum << endl;
			if(i>=(pos+29) && i<=(pos+31)){
			ThreePoints.push_back(sum);            
			}
			}
			*/
			// convolution to get 3 smoothed data points for finding gradients

			if (pixelValues.Count != 63)
			{
				// cout << "convolution Pixel Vector size " << pixelValues.size() << endl;
				for (int kk = 0; kk <= pixelValues.Count; ++kk)
				{
					// cout << kk << " " << pixelValues[kk] << endl;
				}
			}

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

				ThreePoints.Add(sum);
			}

			gradient = LinearRegression(ThreePoints);

			return gradient;
		}

		double[] m_GaussianSmoothedSlope_Temp = new double[3]; 

		public double GaussianSmoothedSlope(double Position, double[] pixelValues, int number)
		{

			int i, j, ctr;
			double gradient = 0.0, sum = 0.0;
			//List<double> ThreePoints = new List<double>();

			int pos = (int)Position;

			// below is the block for smoothing the entire 1d pixel array in each dimension
			// this was used for debugging & requires all the values in either the x or y direction to be passed in 
			// the method for doing this is on the ParticleEnsemble class - smoothing all the data is very computationally expensive
			/*
			//  cout << " convolution " << endl;
			for(i=30;i<(pixelValues.size()-30);++i){
			sum = 0.0;
			for(j=1;j<nGaussPoints;++j){
			sum += gaussian[j]*pixelValues[i-RangeEitherSide+j];
			}
			if(isnan(sum)){sum=0.0;}
			//    cout << i << " " << sum << endl;
			if(i>=(pos+29) && i<=(pos+31)){
			ThreePoints.push_back(sum);            
			}
			}
			*/
			// convolution to get 3 smoothed data points for finding gradients

			//if (pixelValues.Count != 63)
			//{
				// cout << "convolution Pixel Vector size " << pixelValues.size() << endl;
				//for (int kk = 0; kk <= pixelValues.Count; ++kk)
				//{
					// cout << kk << " " << pixelValues[kk] << endl;
				//}
			//}

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
		public double LinearRegression(List<double> yvalues)
		{
			int i, xsum = 0, ysum = 0;
			double XminusXavg = 0.0, YminusYavg = 0.0;
			double xavg = 0.0, yavg = 0.0, npoints = 0.0;
			double slope = 0.0, numerator = 0.0, denominator = 0.0;

			npoints = (double)yvalues.Count;

			for (i = 0; i < yvalues.Count; ++i)
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
				denominator += Math.Pow(XminusXavg, 2.0);
			}

			slope = numerator / denominator;

			return slope;
		}

		/// <summary>
		/// the function below uses a simple linear regression formula to get the slope of a vector of n points
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		public double LinearRegression(double[] yvalues, int count)
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
				denominator += Math.Pow(XminusXavg, 2.0);
			}

			slope = numerator / denominator;

			return slope;
		}

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

		/// <summary>
		/// Given some X,Y coordinates, the two functions below returns pixels along the Y-direction
		/// GetAllThePixelsAlongX is mostly for debugging & checking that the Gaussian smoothing is behaving sensibly
		/// </summary>
		/// <param name="Yposition"></param>
		/// <param name="Xposition"></param>
		/// <param name="PixelsEitherSide"></param>
		/// <returns></returns>
		public List<double> GetAllThePixelsAlongX(double Yposition, double Xposition, int PixelsEitherSide)
		{
			int i, FirstPixel, LastPixel;
			List<double> Xpixels = new List<double>();

			FirstPixel = (int)(Yposition) * BoxWidth;       // this gives the start index of the pixels
			LastPixel = (int)(Yposition + 1) * BoxWidth;

			for (i = 0; i <= PixelsEitherSide; ++i)
			{
				Xpixels.Add(0.0);
			}

			for (i = FirstPixel; i < LastPixel; ++i)
			{
				//    if(timeTpixels!=NULL){
				if (timeTpixels[i] != 0.0)
				{
					Xpixels.Add(timeTpixels[i] - BackgroundPixels[i]);
				}   // no background subtraction
				else
				{
					Xpixels.Add(0.0);
				}
				//    }
				//    else{Xpixels.push_back(0.0);}
			}

			for (i = 0; i < PixelsEitherSide; ++i)
			{
				Xpixels.Add(0.0);
			}

			return Xpixels;
		}

		public void GetSubsetOfPixelsAlongX(double Yposition, double Xposition, int PixelsEitherSide, ref double[] Xpixels, out int count)
		{
			int i, Xmin, Xmax;			

			// beginning of the block for returning the pixel array (along the Y dimension) necessary 
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

		public List<double> GetSubsetOfPixelsAlongX(double Yposition, double Xposition, int PixelsEitherSide)
		{
			int i, Xmin, Xmax, FirstPixel, LastPixel;
			List<double> Xpixels = new List<double>();

			// beginning of the block for returning the pixel array (along the Y dimension) necessary 
			//     to carry out gaussian smoothing of 3 pixels - 
			//        (1) the pixel at which the particle is centered
			//        (2) the pixels on either side of the particle center    

			Xmin = (int)(Xposition - (double)PixelsEitherSide);
			Xmax = (int)(Xposition + (double)PixelsEitherSide);

			int FirstPixelArrayPosition = (int)(Yposition) * BoxWidth + Xmin;     // this gives the start index of pixels
			int valuesAddedSoFar = 0;           // this variable keeps track of how many pixels have been added to vector Xpixels

			for (i = Xmin; i <= Xmax; ++i)
			{
				if (i < 0)
				{
					Xpixels.Add(0.0);
					//      Xpixels.push_back( timeTpixels[FirstPixelArrayPosition] - BackgroundPixels[FirstPixelArrayPosition] );
					//      cout << "i less than zero " << valuesAddedSoFar << " " << Xpixels[valuesAddedSoFar] << endl;
				}
				else if (i >= BoxWidth)
				{
					Xpixels.Add(0.0);
					// Xpixels.push_back( timeTpixels[FirstPixelArrayPosition] - BackgroundPixels[FirstPixelArrayPosition] );
					// cout << "i greater than boxwidth " << valuesAddedSoFar << " " << Xpixels[valuesAddedSoFar] << endl;
				}
				else
				{
					//      if(timeTpixels!=NULL){  // if the array has been initialized take the difference wrt the Background pixels
					if (timeTpixels[valuesAddedSoFar + FirstPixelArrayPosition] != 0)
					{
						Xpixels.Add(timeTpixels[valuesAddedSoFar + FirstPixelArrayPosition] - BackgroundPixels[valuesAddedSoFar + FirstPixelArrayPosition]);
						//          cout << "adding stuff from timeTpixels " << valuesAddedSoFar << " " << Xpixels[valuesAddedSoFar] << endl;
					}
					else
					{
						Xpixels.Add(0.0);
						//          cout << "timeTpixels element doesnt exist " << valuesAddedSoFar << " " << Xpixels[valuesAddedSoFar] << endl;
					}
					//      }
					//      else{
					//        Xpixels.push_back(0.0);
					//        cout << "timeTpixels is NULL " << valuesAddedSoFar << " " << Xpixels[valuesAddedSoFar] << endl;
					//      }
				}

				++valuesAddedSoFar;
			}

			int AnyExtraOrMissingPixelsFromRoundingErrors = (PixelsEitherSide * 2 + 1) - Xpixels.Count;

			if (AnyExtraOrMissingPixelsFromRoundingErrors > 0)
			{
				for (i = 0; i < (AnyExtraOrMissingPixelsFromRoundingErrors); ++i)
				{
					Xpixels.Add(0.0);
				}
			}
			else if (AnyExtraOrMissingPixelsFromRoundingErrors < 0)
			{
				for (i = 0; i > (AnyExtraOrMissingPixelsFromRoundingErrors); --i)
				{
					//Xpixels.pop_back();
					Xpixels.RemoveAt(Xpixels.Count - 1);
				}
			}

			return Xpixels;
		}

		/// <summary>
		/// Given some X,Y coordinates, this function returns all the pixels along the Y-direction;
		/// Mostly, this code has been used for debugging & checking that the Gaussian smoothing is behaving sensibly
		/// </summary>
		/// <param name="Xposition"></param>
		/// <param name="Yposition"></param>
		/// <param name="PixelsEitherSide"></param>
		/// <returns></returns>
		public List<double> GetAllThePixelsAlongY(double Xposition, double Yposition, int PixelsEitherSide)
		{

			int i, FirstPixel, LastPixel;
			List<double> Ypixels = new List<double>();                    // pointer to a vector of ints  

			FirstPixel = (int)Xposition;                // this gives the start index of the pixels
			LastPixel = (int)(((double)BoxHeight - 1.0) * (double)BoxWidth + Xposition);

			for (i = 0; i < PixelsEitherSide; ++i)
			{
				Ypixels.Add(0.0);
			}

			for (i = FirstPixel; i <= LastPixel; i += BoxWidth)
			{
				//    if(timeTpixels!=NULL){
				if (timeTpixels[i] != 0)
				{
					Ypixels.Add(timeTpixels[i] - BackgroundPixels[i]);   // no background subtraction
					//    cout << "i " << i << endl;
				}
				else
				{
					Ypixels.Add(0.0);
				}
				//    }
				//    else{Ypixels.push_back(0.0);}
			}
			for (i = 0; i < PixelsEitherSide; ++i)
			{
				Ypixels.Add(0.0);
			}

			return Ypixels;
		}

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

		public List<double> GetSubsetOfPixelsAlongY(double Xposition, double Yposition, int PixelsEitherSide)
		{

			int i, FirstPixel, LastPixel, Ymin, Ymax;
			List<double> Ypixels = new List<double>();                    // pointer to a vector of ints  

			// beginning of the block for returning the pixel array (along the Y dimension) necessary 
			//     to carry out gaussian smoothing of 3 pixels - 
			//        (1) the pixel at which the particle is centered
			//        (2) the pixels on either side of the particle center

			Ymin = (int)(Yposition - (double)PixelsEitherSide);
			Ymax = (int)(Yposition + (double)PixelsEitherSide);

			FirstPixel = Ymin * BoxWidth + (int)Xposition;           // this gives the start index of pixels
			LastPixel = Ymax * BoxWidth + (int)Xposition;           // this gives the finish index of pixels

			for (i = FirstPixel; i <= LastPixel; i += BoxWidth)
			{
				if (i < 0)
				{
					Ypixels.Add(0.0);
				}
				else if (i >= (BoxHeight * BoxWidth))
				{
					Ypixels.Add(0.0);
				}
				else
				{
					//      if(timeTpixels!=NULL){      // check if the pointer has been initialized 
					if (timeTpixels[i] != 0.0) 
					{ 
						Ypixels.Add(timeTpixels[i] - BackgroundPixels[i]); 
					}   // take the difference wrt the background pixels
					else 
					{
						Ypixels.Add(0.0); 
					}
					//      }
					//      else{Ypixels.push_back(0.0);}
				}
			}

			int AnyExtraOrMissingPixelsFromRoundingErrors = (PixelsEitherSide * 2 + 1) - Ypixels.Count;

			if (AnyExtraOrMissingPixelsFromRoundingErrors > 0)
			{
				for (i = 0; i < (AnyExtraOrMissingPixelsFromRoundingErrors); ++i)
				{
					Ypixels.Add(0.0);
				}
			}
			else if (AnyExtraOrMissingPixelsFromRoundingErrors < 0)
			{
				for (i = 0; i > (AnyExtraOrMissingPixelsFromRoundingErrors); --i)
				{
					// Ypixels.pop_back();
					Ypixels.RemoveAt(Ypixels.Count - 1);
				}
			}

			/*  
			 for(i=0;i<Ypixels.size();++i){
			 cout << i << " " << Ypixels[i] << endl; 
			 }  
			 */
			return Ypixels;
		}

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

		/* 

		// overloaded functions for setting the pixels at time t
		void ExternalField::SetPixelDiff(int num, double *newPixels){
		//	if(newPixels != NULL){
		//		timeTpixels = newPixels;   // aim the timeTpixels pointer at the newPixels pointer to avoid overhead of element-by-element copying
		//	}
			for(int i=0;i<num;++i){
				if(newPixels[i] && newPixels[i] != 0.0){										// (a) if the data is there & it's not zero
					timeTpixels[i] = newPixels[i];
				}
				else if(consecutiveZerosCount[i]>consecutiveZeroThreshold){ // (b) if there's been 10 consecutive zeros in one pixel
					timeTpixels[i] = BackgroundPixels[i];
					consecutiveZerosCount[i] = 0.0;
				}
				else {													// (c) otherwise increment the consecutive zeros count on the pixel
					++consecutiveZerosCount[i];		// ** The whole point of (b) & (c) is to deal with noisy calibration areas in the 
				}                               //		kinect pixel array - which are generally idenitifiable because they have lots of 
			}																	//		consecutive zeros interspersed with the occasional data point. The if statements above
		}																		//		set them equal to the background if there has been consecutive noisy data

		void ExternalField::SetPixelDiff(int num, vector <double> &newPixels){
			//    #pragma omp parallel for   // use openmp to parallelize these loops
			for(int i=0;i<num;++i){
				if(newPixels[i]){  // check to be sure that the data is there
					timeTpixels[i] = newPixels[i];
				}
			}
		}

		void ExternalField::SetPixelDiff(int num, float *newPixels){
			if(newPixels != NULL){
				for(int i=0;i<num;++i){
					timeTpixels[i] = (double) (newPixels[i]);
				}
			}
		}  
		 */
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
	}
}

/* 

/*
 *  ExternalField.cpp
 *  MyFirstTest
 *
 *  Created by David Glowacki on 09/01/2011.
 *  Copyright 2011 University of Bristol. All rights reserved.
 *
 * /

#include "ExternalField.h"
#include "ParticleEnsemble.h"
#include "ParticlePalette.h"
#include <cmath>
#include <iostream>
#include <vector>
//#include <omp.h>

// constructor
ExternalField::ExternalField(ParticleEnsemble* pParticleSet){
  
	GrabberCalls = 0;
  CalibrationCalls = 1;
	consecutiveZeroThreshold = 3;
  BoxHeight = pParticleSet->GetBoxHeight();
	BoxWidth = pParticleSet->GetBoxWidth();
	Calibrating = false;
	
	// initialize background pixels
  BackgroundPixels.assign(BoxHeight*BoxWidth,0.0);
	
	// initialize the calibration count vector
	calibrationCountVector.assign(BoxHeight*BoxWidth,0.0);
	
	// initialize timeTpixels
  timeTpixels.assign(BoxHeight*BoxWidth,0.0);
	
	// initialize consecutiveZerosCount
	consecutiveZerosCount.assign(BoxHeight*BoxWidth,0.0);
	
	// allocate vectors holding positions & forces
	xforces.assign(pParticleSet->GetNumberOfParticles(),0.0);
	yforces.assign(pParticleSet->GetNumberOfParticles(),0.0);
  
  // set up the gaussian array with the desired smoothing width parameter
  sigma=10;
  nGaussPoints = 2*(3*sigma)+1;
  RangeEitherSide = 3*sigma;
  gaussian.assign(nGaussPoints,0.0);
  double point = -1.0*(3.0*(double)sigma);
  for(int i=0;i<nGaussPoints;++i){
    gaussian[i] = exp(-0.5*(pow(point/(double)sigma,2.0)))/((double)sigma*sqrt(2.0*PI));
		//    cout << point << " " << gaussian[i] << endl;
    ++point;
  }
  
}

void ExternalField::resizeForceArrays(ParticleEnsemble* pParticleSet) {
	// clear out the forces vectors
	xforces.clear(); 
	yforces.clear(); 

	// allocate vectors holding positions & forces
	xforces.assign(pParticleSet->GetNumberOfParticles(),0.0);
	yforces.assign(pParticleSet->GetNumberOfParticles(),0.0);
}

void ExternalField::ZeroGrabberCalls(){
	GrabberCalls=0;
	CalibrationCalls=1;
	calibrationCountVector.clear();													// zero out the calibration count vector
	calibrationCountVector.assign(BoxHeight*BoxWidth,0.0);
	BackgroundPixels.clear();																// zero out the Background pixels
	BackgroundPixels.assign(BoxHeight*BoxWidth,0.0);	
	consecutiveZerosCount.clear();
	consecutiveZerosCount.assign(BoxHeight*BoxWidth,0.0);
}

// force field calculator
void ExternalField::calculateForceField(ParticleEnsemble* pParticleSet) {
  
	//  variable declarations    
	int i;
	double posXi, posYi, radius;
	double PotentialEnergy(0.0);
  
  //variable initializations
  
  int BoxHeight = pParticleSet->GetBoxHeight();
  int BoxWidth = pParticleSet->GetBoxWidth();
	
#pragma omp parallel for
  for(i=0;i<pParticleSet->GetNumberOfParticles();++i){	  // initialize vectors holding forces
    xforces[i]=0.0;																			// HERE'S THE PROBLEM - THE INDEX WILL OVERRUN THE VECTOR SIZE!!!
    yforces[i]=0.0;
  }
  
#pragma omp parallel for
	for(i=0;i<pParticleSet->GetNumberOfParticles();++i){
    posXi = pParticleSet->GetXParticlePosition(i);
		posYi = pParticleSet->GetYParticlePosition(i);
    radius = pParticleSet->GetParticleRadius(i);
    
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
		
    vector < double > SubsetOfPixelsAlongX = GetSubsetOfPixelsAlongX(posYi,posXi,RangeEitherSide+1); 
		//    for(int kk=0; kk<SubsetOfPixelsAlongX.size(); ++kk){
		//      cout << kk << " " << SubsetOfPixelsAlongX[kk] << endl;      
		//    }
    
		//    cout << "Xposition " << posXi << endl;
		//    for(int kk=1;kk<SubsetOfPixelsAlongX.size();++kk){cout << kk << " " << SubsetOfPixelsAlongX[kk] << endl;}
    xforces[i] = pParticleSet->GetGradientScaleFactor()*GaussianSmoothedSlope(posXi,SubsetOfPixelsAlongX);
    
		//    vector < double > AllThePixelsAlongY = pParticleSet->GetAllThePixelsAlongY(posXi,posYi,RangeEitherSide);
		//    cout << "Yposition " << posYi << endl;
		//    for(int kk=0;kk<AllThePixelsAlongY.size();++kk){cout << kk << " " << AllThePixelsAlongY[kk] << endl;}
		//    yforces[i] = pParticleSet->GetGradientScaleFactor()*GaussianSmoothedSlope(posYi,AllThePixelsAlongY);
    
    vector < double > SubsetOfPixelsAlongY = GetSubsetOfPixelsAlongY(posXi,posYi,RangeEitherSide+1);
		//    cout << "Yposition " << endl;
    yforces[i] = pParticleSet->GetGradientScaleFactor()*GaussianSmoothedSlope(posYi,SubsetOfPixelsAlongY);
		//    cout << "yforces[i] " << i << " " << yforces[i] << endl;    
		
		// get the gradient scale factor, depending on whether the particle is attractive or repulsive
		
		ParticleInfo typeInfo = AtomPropertiesDefinition.Lookup[(pParticleSet->pParticleVector[i])->TypeID];
		double attractiveOrRepulsiveFactor = typeInfo.AttractiveOrRepulsive;
		xforces[i] *= attractiveOrRepulsiveFactor;
		yforces[i] *= attractiveOrRepulsiveFactor;
		
  }
		
	
  pParticleSet->AddXForces(xforces);		  // set the forces in the Particle Ensemble Object
	pParticleSet->AddYForces(yforces);			// set the potential energy
	pParticleSet->AddPotentialEnergy(PotentialEnergy);		// add in the potential energy    
	
}

// function for doing Gaussian smoothing and then simple 3 point linear regression to find the slope of the 
// middle point in a data vector
double ExternalField::GaussianSmoothedSlope(double Position, vector< double > pixelValues){
  
  int i, j, ctr;
  double gradient(0.0), sum(0.0);
  vector< double >ThreePoints;
  
  int pos = (int) Position;
  
	// below is the block for smoothing the entire 1d pixel array in each dimension
	// this was used for debugging & requires all the values in either the x or y direction to be passed in 
	// the method for doing this is on the ParticleEnsemble class - smoothing all the data is very computationally expensive
	/*
	 //  cout << " convolution " << endl;
	 for(i=30;i<(pixelValues.size()-30);++i){
	 sum = 0.0;
	 for(j=1;j<nGaussPoints;++j){
	 sum += gaussian[j]*pixelValues[i-RangeEitherSide+j];
	 }
	 if(isnan(sum)){sum=0.0;}
	 //    cout << i << " " << sum << endl;
	 if(i>=(pos+29) && i<=(pos+31)){
	 ThreePoints.push_back(sum);            
	 }
	 }
	 * /
  // convolution to get 3 smoothed data points for finding gradients
	
  if(pixelValues.size() != 63){
    cout << "convolution Pixel Vector size " << pixelValues.size() << endl;
    for(int kk=0; kk<=pixelValues.size(); ++kk){
      cout << kk << " " << pixelValues[kk] << endl;
    }
  }
  
  for(i=0;i<3;++i){
    sum = 0.0;
    for(j=0;j<nGaussPoints;++j){
      sum += gaussian[j]*pixelValues[i+j];
    }
    if(isnan(sum)){sum=0.0;}
    ThreePoints.push_back(sum);
  }
	
	
  gradient = LinearRegression(ThreePoints);
	
  return gradient;
}

// the function below uses a simple linear regression formula to get the slope of a vector of n points
double ExternalField::LinearRegression(vector< double > yvalues){
  
  int i, xsum(0), ysum(0);
  double XminusXavg(0.0), YminusYavg(0.0);
  double xavg(0.0), yavg(0.0), npoints(0.0);
  double slope(0.0), numerator(0.0), denominator(0.0);  
  
  npoints = (double) yvalues.size();
  
  for(i=0;i<yvalues.size();++i){ 
		//    cout << i << " " << yvalues[i] << endl;
    xsum += i;
    ysum += yvalues[i];
  }
  
  xavg = (double) xsum / npoints;
  yavg = (double) ysum / npoints;
  
  for(i=0;i<npoints;++i){
    XminusXavg = (double)i - xavg;
    YminusYavg = (double)yvalues[i] - yavg;
    numerator += XminusXavg*YminusYavg;
    denominator += pow(XminusXavg,2.0);
  }  
  
  slope = numerator/denominator;
	
  return slope;
  
}

// this code is for returning pixels for the entire row (see GaussianSmoothedSlope on ExternalField class for addt'l notes)
vector< double > ExternalField::GetAllThePixelsAlongX(double Yposition, double Xposition, int PixelsEitherSide)
{  
  int i, FirstPixel, LastPixel;
  vector< double > Xpixels;   
  
  FirstPixel = (int) (Yposition) * BoxWidth;       // this gives the start index of the pixels
  LastPixel  = (int) (Yposition+1) * BoxWidth;
  
  for(i=0;i<=PixelsEitherSide;++i){Xpixels.push_back(0.0);}
  
  for(i=FirstPixel;i<LastPixel;++i){
//    if(timeTpixels!=NULL){
      if(timeTpixels[i]){Xpixels.push_back(timeTpixels[i]-BackgroundPixels[i]);}   // no background subtraction
      else{Xpixels.push_back(0.0);}      
//    }
//    else{Xpixels.push_back(0.0);}
  }
  
  for(i=0;i<PixelsEitherSide;++i){Xpixels.push_back(0.0);}
	
  return Xpixels;
}

vector< double > ExternalField::GetSubsetOfPixelsAlongX(double Yposition, double Xposition, int PixelsEitherSide)
{
  int i, Xmin, Xmax, FirstPixel, LastPixel;
  vector< double > Xpixels;
  
  // beginning of the block for returning the pixel array (along the Y dimension) necessary 
  //     to carry out gaussian smoothing of 3 pixels - 
  //        (1) the pixel at which the particle is centered
  //        (2) the pixels on either side of the particle center    
  
  Xmin = (int)(Xposition - (double)PixelsEitherSide);
  Xmax = (int)(Xposition + (double)PixelsEitherSide);
  
  int FirstPixelArrayPosition= (int)(Yposition) * BoxWidth + Xmin;     // this gives the start index of pixels
  int valuesAddedSoFar(0);           // this variable keeps track of how many pixels have been added to vector Xpixels
	
  for(i=Xmin;i<=Xmax;++i){
    if(i<0){
      Xpixels.push_back(0.0);
//      Xpixels.push_back( timeTpixels[FirstPixelArrayPosition] - BackgroundPixels[FirstPixelArrayPosition] );
			//      cout << "i less than zero " << valuesAddedSoFar << " " << Xpixels[valuesAddedSoFar] << endl;
    }
    
    else if(i>=BoxWidth){
      Xpixels.push_back(0.0);
//      Xpixels.push_back( timeTpixels[FirstPixelArrayPosition] - BackgroundPixels[FirstPixelArrayPosition] );
			//      cout << "i greater than boxwidth " << valuesAddedSoFar << " " << Xpixels[valuesAddedSoFar] << endl;
    }
    
    else{
//      if(timeTpixels!=NULL){  // if the array has been initialized take the difference wrt the Background pixels
        if(timeTpixels[valuesAddedSoFar+FirstPixelArrayPosition]){
          Xpixels.push_back(timeTpixels[valuesAddedSoFar+FirstPixelArrayPosition]-BackgroundPixels[valuesAddedSoFar+FirstPixelArrayPosition]);
					//          cout << "adding stuff from timeTpixels " << valuesAddedSoFar << " " << Xpixels[valuesAddedSoFar] << endl;
        }
        else{
          Xpixels.push_back(0.0);
					//          cout << "timeTpixels element doesnt exist " << valuesAddedSoFar << " " << Xpixels[valuesAddedSoFar] << endl;
        }
//      }
//      else{
//        Xpixels.push_back(0.0);
				//        cout << "timeTpixels is NULL " << valuesAddedSoFar << " " << Xpixels[valuesAddedSoFar] << endl;
//      }
    }
    ++valuesAddedSoFar;
  }
  
  int AnyExtraOrMissingPixelsFromRoundingErrors = (PixelsEitherSide*2+1) - Xpixels.size();
  
  if(AnyExtraOrMissingPixelsFromRoundingErrors>0){
    for(i=0;i<(AnyExtraOrMissingPixelsFromRoundingErrors);++i){
      Xpixels.push_back(0.0);      
    }
  }  
  else if(AnyExtraOrMissingPixelsFromRoundingErrors<0){
    for(i=0;i>(AnyExtraOrMissingPixelsFromRoundingErrors);--i){
      Xpixels.pop_back();      
    }
  }
	
  return Xpixels;
}

// The code below was used to return pixels for the entire row (see GaussianSmoothedSlope on ExternalField class for addt'l notes)
vector< double > ExternalField::GetAllThePixelsAlongY(double Xposition, double Yposition, int PixelsEitherSide)
{
  
  int i, FirstPixel, LastPixel;
  vector< double > Ypixels;                    // pointer to a vector of ints  
  
  FirstPixel = (int)Xposition;                // this gives the start index of the pixels
  LastPixel  = (int)(((double)BoxHeight - 1.0) * (double)BoxWidth + Xposition);
  
  for(i=0;i<PixelsEitherSide;++i){
    Ypixels.push_back(0.0);
  }
  for(i=FirstPixel;i<=LastPixel;i+=BoxWidth){
//    if(timeTpixels!=NULL){
      if(timeTpixels[i]){
        Ypixels.push_back(timeTpixels[i]-BackgroundPixels[i]);   // no background subtraction
        //    cout << "i " << i << endl;
      }
      else{Ypixels.push_back(0.0);}      
//    }
//    else{Ypixels.push_back(0.0);}
  }
  for(i=0;i<PixelsEitherSide;++i){
    Ypixels.push_back(0.0);
  }
  
  return Ypixels;
}

vector< double > ExternalField::GetSubsetOfPixelsAlongY(double Xposition, double Yposition, int PixelsEitherSide)
{
  
  int i, FirstPixel, LastPixel, Ymin, Ymax;
  vector< double > Ypixels;                   // pointer to a vector of ints  
  
  // beginning of the block for returning the pixel array (along the Y dimension) necessary 
  //     to carry out gaussian smoothing of 3 pixels - 
  //        (1) the pixel at which the particle is centered
  //        (2) the pixels on either side of the particle center
  
  Ymin = (int)(Yposition - (double)PixelsEitherSide);
  Ymax = (int)(Yposition + (double)PixelsEitherSide);
  
  FirstPixel= Ymin*BoxWidth+(int)Xposition;           // this gives the start index of pixels
  LastPixel = Ymax*BoxWidth+(int)Xposition;           // this gives the finish index of pixels
  
  for(i=FirstPixel;i<=LastPixel;i+=BoxWidth){
    if(i<0){
      Ypixels.push_back(0.0);
    }
    else if(i>=(BoxHeight*BoxWidth)){
      Ypixels.push_back(0.0);
    }
    else{
//      if(timeTpixels!=NULL){      // check if the pointer has been initialized 
        if(timeTpixels[i]){Ypixels.push_back(timeTpixels[i]-BackgroundPixels[i]);}   // take the difference wrt the background pixels
        else{Ypixels.push_back(0.0);}      
//      }
//      else{Ypixels.push_back(0.0);}
    }
  }
	
  int AnyExtraOrMissingPixelsFromRoundingErrors = (PixelsEitherSide*2+1) - Ypixels.size();
  
  if(AnyExtraOrMissingPixelsFromRoundingErrors>0){
    for(i=0;i<(AnyExtraOrMissingPixelsFromRoundingErrors);++i){
      Ypixels.push_back(0.0);      
    }
  }
  else if(AnyExtraOrMissingPixelsFromRoundingErrors<0){
    for(i=0;i>(AnyExtraOrMissingPixelsFromRoundingErrors);--i){
      Ypixels.pop_back();      
    }
  }
  
	/*  
	 for(i=0;i<Ypixels.size();++i){
	 cout << i << " " << Ypixels[i] << endl; 
	 }  
	 * /    
  return Ypixels;
}

// overloaded functions for setting the Background (this is what the Kinect Uses)
void ExternalField::BackgroundCalibration(int num, double *newPixels){      // this function takes a pointer to an array of doubles
//	int pixToPrint = 640*123+275;  // debug print-outs
	for(int i=0;i<num;++i){
		if(newPixels[i] != 0.0){
			double CallsSoFar = calibrationCountVector[i];
			double InverseCallsPlusOne = 1.0/(CallsSoFar + 1.0);
			BackgroundPixels[i] = (CallsSoFar*BackgroundPixels[i] + newPixels[i])*(InverseCallsPlusOne);
			++calibrationCountVector[i];
		}
		// debug print-outs
//		if(i==pixToPrint){cout << "pixel " << i << " " << calibrationCountVector[i] << " " << newPixels[i] << " " << BackgroundPixels[i] << " " << endl;}
	}
}

void ExternalField::BackgroundCalibration(int num, float *newPixels){     // this function takes a pointer to an array of floats
	for(int i=0;i<num;++i){
		if(newPixels[i] != 0.0){		
			double CallsSoFar = calibrationCountVector[i];
			double InverseCallsPlusOne = 1.0/(CallsSoFar + 1.0);
			BackgroundPixels[i] = (CallsSoFar*BackgroundPixels[i] + (double) (newPixels[i]))*(InverseCallsPlusOne);
			++calibrationCountVector[i];
		}
	}
}

void ExternalField::BackgroundCalibration(int num, vector <double> &newPixels){  // this function takes the address of a vector of doubles - mostly historical
	for(int i=0;i<num;++i){
		if(newPixels[i] != 0.0){		
			double CallsSoFar = calibrationCountVector[i];
			double InverseCallsPlusOne = 1.0/(CallsSoFar + 1.0);
			BackgroundPixels[i] = (CallsSoFar*BackgroundPixels[i] + newPixels[i])*(InverseCallsPlusOne);
			++calibrationCountVector[i];
		}
	}
}

void ExternalField::removeZerosFromBackgroundFile(){   // this is a really quick and dirty interpolation algorithm that just puts the last good pixel value 
  int ctr(0), firstRandomPixelIdx(0);													// in blind spots
	double lastNonzeroPixelValue(0.0), firstRandomPixelValue(0.0);
	
	do{																							// select a random nonzero pixel somewhere in the pixel array
		firstRandomPixelIdx = (int) ofRandom(0, BoxHeight*BoxWidth);
		firstRandomPixelValue = BackgroundPixels[firstRandomPixelIdx];
	}  
	while(firstRandomPixelValue==0.0);
	
	lastNonzeroPixelValue=firstRandomPixelValue;
	
	for(int i=firstRandomPixelIdx; i<BoxWidth*BoxHeight; ++i){      // iterate forward from the firstRandomPixel
		if(BackgroundPixels[i]==0 && lastNonzeroPixelValue != 0.0){
			BackgroundPixels[i]=lastNonzeroPixelValue;
		}
		else{
			lastNonzeroPixelValue=BackgroundPixels[i];
		}
	}

	lastNonzeroPixelValue=firstRandomPixelValue;
	
	for(int i=firstRandomPixelIdx-1; i>=0; --i){										// iterate backward from the firstRandomPixel
		if(BackgroundPixels[i]==0 && lastNonzeroPixelValue != 0.0){
			BackgroundPixels[i]=lastNonzeroPixelValue;
		}
		else{
			lastNonzeroPixelValue=BackgroundPixels[i];
		}
	}
	
}

// overloaded functions for setting the pixels at time t
void ExternalField::SetPixelDiff(int num, double *newPixels){
//	if(newPixels != NULL){
//		timeTpixels = newPixels;   // aim the timeTpixels pointer at the newPixels pointer to avoid overhead of element-by-element copying
//	}
	for(int i=0;i<num;++i){
		if(newPixels[i] && newPixels[i] != 0.0){										// (a) if the data is there & it's not zero
			timeTpixels[i] = newPixels[i];
		}
		else if(consecutiveZerosCount[i]>consecutiveZeroThreshold){ // (b) if there's been 10 consecutive zeros in one pixel
			timeTpixels[i] = BackgroundPixels[i];
			consecutiveZerosCount[i] = 0.0;
		}
		else {													// (c) otherwise increment the consecutive zeros count on the pixel
			++consecutiveZerosCount[i];		// ** The whole point of (b) & (c) is to deal with noisy calibration areas in the 
		}                               //		kinect pixel array - which are generally idenitifiable because they have lots of 
	}																	//		consecutive zeros interspersed with the occasional data point. The if statements above
}																		//		set them equal to the background if there has been consecutive noisy data

void ExternalField::SetPixelDiff(int num, vector <double> &newPixels){
	//    #pragma omp parallel for   // use openmp to parallelize these loops
	for(int i=0;i<num;++i){
		if(newPixels[i]){  // check to be sure that the data is there
			timeTpixels[i] = newPixels[i];
		}
	}
}

void ExternalField::SetPixelDiff(int num, float *newPixels){
	if(newPixels != NULL){
		for(int i=0;i<num;++i){
			timeTpixels[i] = (double) (newPixels[i]);
		}
	}
}  
*/