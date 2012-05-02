using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DS.Simulation
{
	class FFTScanner
	{
		public float[] PeaksFrequencyAndIntensity = new float[2];
		public int[] PeakLocations = new int[1];
		public double[] PeakIntensitys = new double[1];
		private int m_PeakCount = 1;
		public bool ShouldScan; 

		public void ScanForPeakFrequencyAndIntensity(double[] values, double[] freqs, float frameRate, float updateFreq)
		{
			if (ArtworkStaticObjects.Options.FFT.PeakCount != m_PeakCount)
			{
				m_PeakCount = ArtworkStaticObjects.Options.FFT.PeakCount; 
				PeaksFrequencyAndIntensity = new float[m_PeakCount * 2];
				PeakLocations = new int[m_PeakCount];
				PeakIntensitys = new double[m_PeakCount]; 
			}

			for (int i = 0; i < m_PeakCount; i++)
			{
				PeakLocations[i] = -1;
			}

			int lowest = 0;
			double lowestIntensity = values[0]; 
			bool rescanForLowest = false; 
			//int currentFullness = 0;
			double lastIntensity = values[0];
			int lastPeakLocation = -1;
			bool downwardSlope = false;
			double highestPeak = double.NegativeInfinity; 

			for (int i = 0; i < values.Length; i++)
			{
				double intensity = values[i];

				/* if (currentFullness < m_PeakCount)
				{
					PeakLocations[currentFullness] = i;
					PeakIntensitys[currentFullness] = intensity; 
					
					if (intensity < lowestIntensity)
					{
						lowestIntensity = intensity; 
						lowest = currentFullness; 
					}

					lastPeakLocation = i;
					lastIntensity = intensity;
					
					currentFullness++; 
				}
				else 
				{		
				 */ 
					if (rescanForLowest == true)
					{
						rescanForLowest = false; 

						int newLowest = -1;
						double newLowestIntensity = double.PositiveInfinity; 

						for (int j = 0; j < m_PeakCount; j++)
						{
							if (PeakLocations[j] == -1)
							{
								newLowest = j;
								newLowestIntensity = double.NegativeInfinity;
								break; 
							}
							else if (PeakIntensitys[j] < newLowestIntensity)
							{
								newLowest = j; 
								newLowestIntensity = PeakIntensitys[j]; 
							}
						}

						lowest = newLowest; 
						lowestIntensity = newLowestIntensity; 
					}

					if (intensity < lastIntensity && lastIntensity > lowestIntensity && downwardSlope == false)
					{						
						rescanForLowest = true;

						if (highestPeak < lastIntensity)
						{
							highestPeak = lastIntensity; 
						}

						PeakLocations[lowest] = lastPeakLocation;
						PeakIntensitys[lowest] = lastIntensity; 
					}

					downwardSlope = intensity < lastIntensity; 

					lastPeakLocation = i;
					lastIntensity = intensity;
				//}
			}

			double intencityScale = 1000 / highestPeak; 
			
			for (int i = 0, j = 0; i < m_PeakCount; i++)
			{
				if (PeakLocations[i] == -1)
				{
					PeaksFrequencyAndIntensity[j++] = 0;
					PeaksFrequencyAndIntensity[j++] = 0;
				}
				else
				{
					int n = PeakLocations[i];

					float frequency = frameRate / ((float)(values.Length - n + 1) * updateFreq);
					//float freqFromVector = (float)freqs[n]; 

					float intensity = (float)(PeakIntensitys[i] * intencityScale);

					PeaksFrequencyAndIntensity[j++] = frequency;
					PeaksFrequencyAndIntensity[j++] = intensity;
				}
			}
		}
	}
}
