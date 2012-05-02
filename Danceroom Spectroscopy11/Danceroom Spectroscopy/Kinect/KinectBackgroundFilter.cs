using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1;
using System.IO;

namespace DS.Kinect
{
	class KinectBackgroundFilter
	{
		const ushort MagicNumber = 0xF2F4;		

		#region Static Constructor and Members

		static double[] BlendFactors;

		static KinectBackgroundFilter()
		{
			BlendFactors = new double[9];
			double standardDeviation = 2;
			int i = 0; 

			for (int y = 0; y < 3; y++)
			{
				for (int x = 0; x < 3; x++)
				{
					BlendFactors[i++] = (1 / (2 * Math.PI * standardDeviation * standardDeviation)) * Math.Exp(-((x * x) + (y * y) / (2 * standardDeviation * standardDeviation)));
				}
			}
		}

		#endregion

		#region Filter Header

		struct FilterHeader
		{
			public ushort MagicNumber;
			public int Width;
			public int Height; 
		}

		#endregion

		#region Private Members
		
		private int m_Width;
		private int m_Height;
		private int m_Size; 

		private double[] m_BackgroundData;
		private short[] m_DataPointCount;
		private short m_GrabberCalls;

		private double[] m_TestImage;
		private bool m_UseTestImage;

		#endregion

		#region Public Properties

		public int Width { get { return m_Width; } }
		public int Height { get { return m_Height; } }
		public int Size { get { return m_Size; } }

		public double[] BackgroundData { get { return m_BackgroundData; } }
		public short[] DataPointCount { get { return m_DataPointCount; } }

		public short GrabberCalls { get { return m_GrabberCalls; } }
			
		public bool UseTestImage { get { return m_UseTestImage; } set { m_UseTestImage = value; } }

		public double[] TestImage { get { return m_TestImage; } }

		#endregion

		public KinectBackgroundFilter(int width, int height)
		{
			Resize(width, height); 
		}

		#region Resize
		
		public void Resize(int width, int height)
		{
			m_Width = width;
			m_Height = height;
			m_Size = m_Width * m_Height;

			m_GrabberCalls = 0; 

			m_BackgroundData = new double[m_Size];
			m_DataPointCount = new short[m_Size];

			m_TestImage = new double[m_Size];

			KinectHelper.FillTestBuffer(m_TestImage, "~/Assets/TestBuffer0_" + m_Width + "x" + m_Height + ".png");

			m_UseTestImage = false; 
		}

		#endregion

		#region Clear

		public void Clear()
		{
			m_GrabberCalls = 0; 

			for (int i = 0; i < m_Size; i++)
			{
				m_BackgroundData[i] = 0; 
			}

			for (int i = 0; i < m_Size; i++)
			{
				m_DataPointCount[i] = 0;
			}
		}

		#endregion

		#region Remove Background

		public void RemoveBackground(short[] source, double[] dest, short minValue, short maxValue, double noiseTolerance)
		{
			if (m_UseTestImage == false)
			{
				double max = maxValue - minValue;

				if (max < 1)
				{
					max = 1;
				}

				double maxMod = 1 / max;

				for (int i = 0, ie = m_Size; i < ie; i++)
				{
					double pixel = m_BackgroundData[i];
					double value = (double)source[i];

					if (value == 0)
					{
						dest[i] = 0;
					}
					else if (pixel > value + noiseTolerance)
					{
						value -= minValue;

						if (value > max)
						{
							dest[i] = 0;
						}
						else if (value <= 0)
						{
							dest[i] = 255;
						}
						else
						{
							//double intensity = 255 - (255 * value / max);
							double intensity = 255 - (255 * (value * maxMod));

							dest[i] = intensity;
						}
					}
					else
					{
						dest[i] = 0;
					}
				}
			}
			else
			{
				for (int i = 0, ie = m_Size; i < ie; i++)
				{
					dest[i] = m_TestImage[i]; 
				}
			}
		}

		#endregion

		#region Grab Gackground

		public void GrabBackground(short[] data)
		{
			for (int i = 0, ie = m_Size; i < ie; i++)
			{
				short pixel = data[i];
				
				if (pixel != 0)
				{
					double pixelDouble = (double)pixel;
					double CallsSoFar = (double)m_DataPointCount[i];
					double InverseCallsPlusOne = 1.0 / (CallsSoFar + 1.0);

					if (CallsSoFar > 0)
					{
						if (m_BackgroundData[i] > pixelDouble)
						{
							m_BackgroundData[i] = pixelDouble;
						}
					}
					else
					{
						m_BackgroundData[i] = pixelDouble; 
					}

					//m_BackgroundData[i] = (CallsSoFar * m_BackgroundData[i] + pixelDouble) * (InverseCallsPlusOne);
					
					m_DataPointCount[i]++;
				}
			}

			m_GrabberCalls++; 
		}

		#endregion

		#region Get Images
		
		#region Get Background Image

		public void GetBackgroundImage(byte[] destBuffer32, short minValue, short maxValue)
		{
			if (m_UseTestImage == false)
			{
				int max = maxValue - minValue;

				if (max < 1)
				{
					max = 1;
				}

				for (int i = 0, i32 = 0, ie = m_Size; i < ie; i++, i32 += 4)
				{
					short count = m_DataPointCount[i];
					double pixel = m_BackgroundData[i];

					if (count == 0)
					{
						destBuffer32[i32 + KinectHelper.RED_IDX] = 255;
						destBuffer32[i32 + KinectHelper.GREEN_IDX] = 0;
						destBuffer32[i32 + KinectHelper.BLUE_IDX] = 0;
						destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 255;
					}
					else if (pixel <= 0)
					{
						destBuffer32[i32 + KinectHelper.RED_IDX] = 0;
						destBuffer32[i32 + KinectHelper.GREEN_IDX] = 0;
						destBuffer32[i32 + KinectHelper.BLUE_IDX] = 0;
						destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 255;
					}
					else
					{
						pixel -= minValue;

						if (pixel <= 0)
						{
							destBuffer32[i32 + KinectHelper.RED_IDX] = 255;
							destBuffer32[i32 + KinectHelper.GREEN_IDX] = 0;
							destBuffer32[i32 + KinectHelper.BLUE_IDX] = 255;
							destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 255;
						}
						else if (pixel > max)
						{
							destBuffer32[i32 + KinectHelper.RED_IDX] = 0;
							destBuffer32[i32 + KinectHelper.GREEN_IDX] = 255;
							destBuffer32[i32 + KinectHelper.BLUE_IDX] = 0;
							destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 255;
						}
						else
						{
							byte intensity = (byte)(255 - (255 * pixel / max));

							destBuffer32[i32 + KinectHelper.RED_IDX] = intensity;
							destBuffer32[i32 + KinectHelper.GREEN_IDX] = intensity;
							destBuffer32[i32 + KinectHelper.BLUE_IDX] = intensity;
							destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 255;
						}
					}
				}

			}
			else
			{
				for (int i = 0, i32 = 0, ie = m_Size; i < ie; i++, i32 += 4)
				{
					//dest[i] = m_TestImage[i]; 
					byte intensity = (byte)(255 - (byte)m_TestImage[i]);

					destBuffer32[i32 + KinectHelper.RED_IDX] = intensity;
					destBuffer32[i32 + KinectHelper.GREEN_IDX] = intensity;
					destBuffer32[i32 + KinectHelper.BLUE_IDX] = intensity;
					destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 255;
				}
			}
		}

		#endregion

		#region Get Background Canceled Image

		public void GetBackgroundCanceledImage(short[] source, byte[] destBuffer32, short minValue, short maxValue, double noiseTolerance, bool highlight)
		{
			if (m_UseTestImage == false)
			{
				int max = maxValue - minValue;

				if (max < 1)
				{
					max = 1;
				}

				if (highlight == true)
				{
					for (int i = 0, i32 = 0, ie = m_Size; i < ie; i++, i32 += 4)
					{
						double pixel = m_BackgroundData[i];
						double value = (double)source[i];

						if (value == 0)
						{
							destBuffer32[i32 + KinectHelper.RED_IDX] = 0;
							destBuffer32[i32 + KinectHelper.GREEN_IDX] = 0;
							destBuffer32[i32 + KinectHelper.BLUE_IDX] = 0;
							destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 0;
						}
						else if (pixel > value + noiseTolerance)
						{
							value -= minValue;

							if (value <= 0)
							{
								destBuffer32[i32 + KinectHelper.RED_IDX] = 255;
								destBuffer32[i32 + KinectHelper.GREEN_IDX] = 0;
								destBuffer32[i32 + KinectHelper.BLUE_IDX] = 255;
								destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 255;
							}
							else if (value > max)
							{
								destBuffer32[i32 + KinectHelper.RED_IDX] = 0;
								destBuffer32[i32 + KinectHelper.GREEN_IDX] = 255;
								destBuffer32[i32 + KinectHelper.BLUE_IDX] = 0;
								destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 255;
							}
							else
							{
								byte intensity = (byte)(255 - (255 * value / max));

								destBuffer32[i32 + KinectHelper.RED_IDX] = intensity;
								destBuffer32[i32 + KinectHelper.GREEN_IDX] = intensity;
								destBuffer32[i32 + KinectHelper.BLUE_IDX] = intensity;
								destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 255;
							}
						}
						else
						{
							destBuffer32[i32 + KinectHelper.RED_IDX] = 0;
							destBuffer32[i32 + KinectHelper.GREEN_IDX] = 0;
							destBuffer32[i32 + KinectHelper.BLUE_IDX] = 0;
							destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 0;
						}
					}
				}
				else
				{
					for (int i = 0, i32 = 0, ie = m_Size; i < ie; i++, i32 += 4)
					{
						double pixel = m_BackgroundData[i];
						double value = (double)source[i];

						if (value == 0)
						{
							destBuffer32[i32 + KinectHelper.RED_IDX] = 0;
							destBuffer32[i32 + KinectHelper.GREEN_IDX] = 0;
							destBuffer32[i32 + KinectHelper.BLUE_IDX] = 0;
							destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 0;
						}
						else if (pixel > value + noiseTolerance)
						{
							value -= minValue;

							if (value > max)
							{
								destBuffer32[i32 + KinectHelper.RED_IDX] = 0;
								destBuffer32[i32 + KinectHelper.GREEN_IDX] = 0;
								destBuffer32[i32 + KinectHelper.BLUE_IDX] = 0;
								destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 0;
							}
							else if (value <= 0)
							{
								destBuffer32[i32 + KinectHelper.RED_IDX] = 255;
								destBuffer32[i32 + KinectHelper.GREEN_IDX] = 255;
								destBuffer32[i32 + KinectHelper.BLUE_IDX] = 255;
								destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 255;
							}
							else
							{
								byte intensity = (byte)(255 - (255 * value / max));

								destBuffer32[i32 + KinectHelper.RED_IDX] = intensity;
								destBuffer32[i32 + KinectHelper.GREEN_IDX] = intensity;
								destBuffer32[i32 + KinectHelper.BLUE_IDX] = intensity;
								destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 255;
							}
						}
						else
						{
							destBuffer32[i32 + KinectHelper.RED_IDX] = 0;
							destBuffer32[i32 + KinectHelper.GREEN_IDX] = 0;
							destBuffer32[i32 + KinectHelper.BLUE_IDX] = 0;
							destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 0;
						}
					}
				}
			}
			else
			{
				for (int i = 0, i32 = 0, ie = m_Size; i < ie; i++, i32 += 4)
				{
					//dest[i] = m_TestImage[i]; 
					byte intensity = (byte)(255 - (byte)m_TestImage[i]);

					destBuffer32[i32 + KinectHelper.RED_IDX] = intensity;
					destBuffer32[i32 + KinectHelper.GREEN_IDX] = intensity;
					destBuffer32[i32 + KinectHelper.BLUE_IDX] = intensity;
					destBuffer32[i32 + KinectHelper.ALPHA_IDX] = 255;
				}
			}
		}

		#endregion

		#endregion

		#region Fill In Missing Data Points

		public void FillInMissingDataPoints()
		{
			#region Interpolate and fill values along x axis
			
			int i = 0;
			for (int y = 0; y < m_Height; y++)
			{
				int lastValidIndex = -1;

				for (int x = 0; x < m_Width; x++)
				{
					if (m_DataPointCount[i] == 0)
					{
						// do nothing 
					}
					else if (lastValidIndex != -1 && i - lastValidIndex > 1)
					{
						// we have a gap to fill 						
						DoLinearInterpolationBetween(lastValidIndex, i); 

						lastValidIndex = i; 
					}
					// no valid background data has been captured on this scanline yet
					else if (lastValidIndex == -1)
					{
						FillValueBetween(y * m_Width, i, m_BackgroundData[i]); 

						lastValidIndex = i; 
					}
					else 
					{
						lastValidIndex = i; 
					}

					i++; 
				}

				if (lastValidIndex == -1)
				{
					// no data has been recorded on this row at all?! 
				}
				else if ((m_Width * (y + 1)) - lastValidIndex > 1)
				{
					FillValueBetween(lastValidIndex, (m_Width * (y + 1)) - 1, m_BackgroundData[lastValidIndex]); 
				}
			}

			#endregion

			#region  Interpolate values along y axis
			
			i = 0;

			for (int x = 0; x < m_Width; x++)
			{
				int lastValidY = -1;
				
				for (int y = 0; y < m_Height; y++)
				{
					i = (y * m_Width) + x; 

					if (m_DataPointCount[i] == 0)
					{
						// do nothing 
					}
					else if (lastValidY != -1 && y - lastValidY > 1)
					{
						// we have a gap to fill 						
						DoLinearInterpolationBetweenY(x, lastValidY, y);

						lastValidY = y;
					}
					// no valid background data has been captured on this scanline yet
					else if (lastValidY == -1)
					{
						//FillValueBetweenY(x, 0, y, m_BackgroundData[i]);
						DoLinearInterpolationBetweenY(x, 0, y);

						lastValidY = y;
					}
					else
					{
						lastValidY = y;
					}
				}

				if (lastValidY == -1)
				{
					// no data has been recorded on this row at all?! 
				}
				else if (m_Height - lastValidY > 1)
				{
					//FillValueBetweenY(x, lastValidY, m_Height - 1, m_BackgroundData[(lastValidY * m_Width) + x]);
					DoLinearInterpolationBetweenY(x, lastValidY, m_Height - 1);
				}
			}

			#endregion

			#region Blend final values

			double[] tempBuffer = new double[m_BackgroundData.Length];
			
			m_BackgroundData.CopyTo(tempBuffer, 0);

			for (int y = 1; y < m_Height - 1; y++)
			{
				for (int x = 1; x < m_Width - 1; x++)
				{
					int index = (y * m_Width) + x;

					if (m_DataPointCount[index] == 0)
					{
						BlendPixel(x, y, tempBuffer);
					}
				}
			}

			#endregion

			#region Set all values so that they recived at least 1 data point

			i = 0;
			for (int y = 0; y < m_Height; y++)
			{
				for (int x = 0; x < m_Width; x++)
				{
					if (m_DataPointCount[i] == 0)
					{
						m_DataPointCount[i] = 1; 
					}

					i++; 
				}
			}

			#endregion
		}

		#endregion		

		#region Backgroud Filling
		
		private void BlendPixel(int x, int y, double[] source)
		{
			//double total = 0;

			//if (x > 3)
			//{
			//	
			//}			
		}

		private void DoLinearInterpolationBetween(int index1, int index2)
		{
			int pixelsToFill = index2 - index1;
			double startValue = m_BackgroundData[index1];
			double endValue = m_BackgroundData[index2];
			double inc = (endValue - startValue) / (double)pixelsToFill;
			double currentValue = startValue;

			for (int s = 0; s < pixelsToFill; s++)
			{
				m_BackgroundData[index1 + s] = currentValue;
				currentValue += inc;
			}
		}

		private void FillValueBetween(int index1, int index2, double value)
		{
			int pixelsToFill = index2 - index1;

			for (int s = 0; s < pixelsToFill; s++)
			{
				m_BackgroundData[index1 + s] = value;
			}
		}

		private void DoLinearInterpolationBetweenY(int x, int yStart, int yEnd)
		{
			int pixelsToFill = yEnd - yStart;
			double startValue = m_BackgroundData[(yStart * m_Width) + x];
			double endValue = m_BackgroundData[(yEnd * m_Width) + x];
			double inc = (endValue - startValue) / (double)pixelsToFill;
			double currentValue = startValue;

			for (int s = 0; s < pixelsToFill; s++)
			{
				int index = ((yStart + s) * m_Width) + x;

				m_BackgroundData[index] = (m_BackgroundData[index] + currentValue) * 0.5;

				currentValue += inc;
			}
		}

		private void FillValueBetweenY(int x, int yStart, int yEnd, double value)
		{
			int pixelsToFill = yEnd - yStart;

			for (int s = 0; s < pixelsToFill; s++)
			{
				int index = ((yStart + s) * m_Width) + x;

				m_BackgroundData[index] = (m_BackgroundData[index] + value) * 0.5;
			}
		}

		#endregion

		#region Write / Read

		public void Write(string filePath)
		{
			using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				FilterHeader header = new FilterHeader();

				header.MagicNumber = MagicNumber;
				header.Width = Width;
				header.Height = Height;

				StructHelper.WriteStructure(writer, header);

				for (int i = 0, ie = Size; i < ie; i++)
				{
					short dataPointCount = DataPointCount[i];

					writer.Write(dataPointCount);

					if (dataPointCount > 0)
					{
						writer.Write(BackgroundData[i]);
					}
				}
			}
		}

		public static KinectBackgroundFilter Read(string filePath)
		{
			using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			using (BinaryReader reader = new BinaryReader(stream))
			{
				FilterHeader header = StructHelper.ReadStructure<FilterHeader>(reader);

				if (header.MagicNumber != MagicNumber)
				{
					throw new Exception("Exception while reading background data."); 
				}

				KinectBackgroundFilter filter = new KinectBackgroundFilter(header.Width, header.Height);

				for (int i = 0, ie = filter.Size; i < ie; i++)
				{
					short dataPointCount = reader.ReadInt16();

					filter.DataPointCount[i] = dataPointCount;

					if (dataPointCount > 0)
					{
						filter.BackgroundData[i] = reader.ReadDouble();
					}
					else
					{
						filter.BackgroundData[i] = 0; 
					}
				}

				return filter; 
			}
		}

		#endregion
	}
}
