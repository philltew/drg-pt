using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS.Kinect;
using System.Drawing;
using DSParticles3;

namespace DS.Simulation
{
	partial class CompositeFieldImage
	{
		/// <summary>
		/// The image from a single kinect camera, also holds the information about its rotation ad translation within the composite image
		/// </summary>
		internal class KinectFieldImage
		{
			#region Private Members
			
			/// <summary>
			/// The abs bounds of the image inside the composite
			/// </summary>
			private RectangleF m_AbsBounds;
			
			/// <summary>
			/// A map of blending regions
			/// </summary>
			private double[] m_BlendingMap;
			
			/// <summary>
			/// A map of clipping regions   
			/// </summary>
			private double[] m_DepthClippingMap;

			/// <summary>
			/// The actual data in the field at the present time
			/// </summary>
			private double[] m_FieldData;

			/// <summary>
			/// Rotaion Angle
			/// </summary>
			private double m_RotaionAngle;

			/// <summary>
			/// Position of the center within a composite image
			/// </summary>
			private float m_X, m_Y;

			/// <summary>
			/// the size of the image in pixels
			/// </summary>
			private int m_Width, m_Height, m_Stride;

			/// <summary>
			/// The composite that this image belongs to
			/// </summary>
			private CompositeFieldImage m_Parent;

			/// <summary>
			/// The device we are watching 
			/// </summary>
			private KinectDevice m_Device;

			#endregion

			#region Public Properties
			
			/// <summary>
			/// A map of blending regions
			/// </summary>
			public double[] BlendingMap { get { return m_BlendingMap; } }

			/// <summary>
			/// A map of clipping regions   
			/// </summary>
			public double[] DepthClippingMap { get { return m_DepthClippingMap; } }

			/// <summary>
			/// The actual data in the field at the present time
			/// </summary>
			public double[] FieldData { get { return m_FieldData; } }

			/// <summary>
			/// Rotaion Angle
			/// </summary>
			public double RotaionAngle { get { return m_RotaionAngle; } set { m_RotaionAngle = value; } }

			/// <summary>
			/// X Position of the center within a composite image
			/// </summary>
			public float X { get { return m_X; } set { m_X = value; } }

			/// <summary>
			/// Y Position of the center within a composite image
			/// </summary>
			public float Y { get { return m_Y; } set { m_Y = value; } }

			/// <summary>
			/// the width of the image in pixels
			/// </summary>
			public int Width { get { return m_Width; } }

			/// <summary>
			/// the Stride is the width of the image buffer in pixels
			/// </summary>
			public int Stride { get { return m_Stride; } }

			/// <summary>
			/// the height of the image in pixels
			/// </summary>
			public int Height { get { return m_Height; } }

			public bool HasNewDepthFrame { get { return m_Device.HasNewDepthFrame; } }

			public KinectDevice Device { get { return m_Device; } }

			public RectangleF AbsBounds { get { return m_AbsBounds; } set { m_AbsBounds = value; } }

			#endregion

			#region Constuctor
			
			public KinectFieldImage(CompositeFieldImage owner, KinectDevice device)
			{
				// assign the parent 
				m_Parent = owner;
				// assign the device
				m_Device = device;

				// calculate size from the resilution of the device
				m_Stride = KinectHelper.GetWidthForResolution(m_Device.DepthResolution);
				m_Width = m_Stride - 8; 
				m_Height = KinectHelper.GetHeightForResolution(m_Device.DepthResolution);

				int size = m_Stride * m_Height;

				// create array for the feild data
				m_FieldData = new double[size];

				// create arrays for blending and clip maps 
				m_BlendingMap = new double[size];
			
				m_DepthClippingMap = new double[size];
			}

			#endregion

			#region Update the Image from the filtered data

			public bool Update()
			{
				if (m_Device.HasNewDepthFrame == true)
				{
					m_Device.Filter.RemoveBackground(m_Device.DepthBuffer, m_FieldData,
													 ArtworkStaticObjects.Options.Kinect.NearClippingPlane, ArtworkStaticObjects.Options.Kinect.FarClippingPlane,
													 ArtworkStaticObjects.Options.Kinect.NoiseTolerance);

					return true;
				}
				else
				{
					return false; 
				}
			}

			#endregion

			#region Reset Blending Map
			
			public void Reset()
			{
				int i = 0; 

				for (int y = 0; y < m_Height; y++)
				{
					for (int x = 0; x < m_Stride; x++)
					{
						if (y < 8 || x >= m_Width)
						{
							m_BlendingMap[i++] = 0;
						}
						else
						{
							m_BlendingMap[i++] = 1;
						}
					}
				}

				Array.Clear(m_DepthClippingMap, 0, m_DepthClippingMap.Length);				
			}

			#endregion

			#region Get Value / Get Value Range
			
			public double GetValueAt(double absX, double absY)
			{
				int relitiveX = (int)(absX - m_AbsBounds.X);
				int relitiveY = (int)(absY - m_AbsBounds.Y);

				if (relitiveX < 0 || relitiveY < 0)
				{
					return 0; 
				}
				else if (relitiveX >= m_Width || relitiveY >= m_Height)
				{
					return 0;
				}
				else
				{
					int index = (relitiveY * m_Stride) + relitiveX;

					return (m_FieldData[index] * m_BlendingMap[index]); 
				}
			}

			public void GetSubsetOfPixelsAlongX(double absX, double absY, int PixelsEitherSide, ref double[] Xpixels)
			{
				int relitiveX, relitiveY; 

				relitiveX = (int)(absX - m_AbsBounds.X);
				relitiveY = (int)(absY - m_AbsBounds.Y); 

				int i, Xmin, Xmax;

				// beginning of the block for returning the pixel array (along the Y dimension) necessary 
				//     to carry out gaussian smoothing of 3 pixels - 
				//        (1) the pixel at which the particle is centered
				//        (2) the pixels on either side of the particle center    

				Xmin = (int)(relitiveX - (double)PixelsEitherSide);
				Xmax = (int)(relitiveX + (double)PixelsEitherSide);

				i = 0;
				int remaining, extra;
				int index = Xmin;

				if (Xmin < 0)
				{
					#region Clipping on the left side

					i += -Xmin; 

					remaining = Xmax;
					index = (int)(relitiveY) * m_Stride;

					#endregion
				}
				else
				{
					remaining = Xmax - Xmin;
					index = (int)(relitiveY) * m_Stride + Xmin;
				}

				if (Xmax >= m_Stride)
				{
					remaining -= Xmax - m_Stride;
					extra = Xmax - m_Stride;
				}
				else
				{
					extra = 0;
				}

				#region Non clipping Region

				for (int j = 0, je = remaining; j < je; j++)
				{
					Xpixels[i] = Xpixels[i] + (m_FieldData[index] * m_BlendingMap[index]);

					i++; 
					index++;
				}

				#endregion
			}

			public void GetSubsetOfPixelsAlongY(double absX, double absY, int PixelsEitherSide, ref double[] Ypixels)
			{
				int relitiveX, relitiveY;

				relitiveX = (int)(absX - m_AbsBounds.X);
				relitiveY = (int)(absY - m_AbsBounds.Y); 

				int i, Ymin, Ymax;

				// beginning of the block for returning the pixel array (along the Y dimension) necessary 
				//     to carry out gaussian smoothing of 3 pixels - 
				//        (1) the pixel at which the particle is centered
				//        (2) the pixels on either side of the particle center    

				Ymin = (int)(relitiveY - (double)PixelsEitherSide);
				Ymax = (int)(relitiveY + (double)PixelsEitherSide);

				i = 0;
				int remaining, extra;
				int index = Ymin;

				if (Ymin < 0)
				{
					#region Clipping on the top side

					i += -Ymin; 

					remaining = Ymax;
					index = (int)relitiveX;

					#endregion
				}
				else
				{
					remaining = Ymax - Ymin;
					index = ((int)Ymin * m_Stride) + (int)relitiveX;
				}

				if (Ymax >= m_Height)
				{
					remaining -= Ymax - m_Height;
					extra = Ymax - m_Height;
				}
				else
				{
					extra = 0;
				}

				#region Non clipping Region

				for (int j = 0, je = remaining; j < je; j++)
				{
					Ypixels[i] = Ypixels[i] + (m_FieldData[index] * m_BlendingMap[index]);
					
					i++; 

					index += m_Stride;
				}

				#endregion
			}

			#endregion

			#region Copy to image buffer helpers

			public void CopyBlendMap32(byte[] colorBuffer, System.Drawing.Color color)
			{
				byte r = color.R, g = color.G, b = color.B;

				for (int i = 0, i32 = 0; i < m_BlendingMap.Length; i++, i32 += 4)
				{
					colorBuffer[i32 + 0] = r;
					colorBuffer[i32 + 1] = g;
					colorBuffer[i32 + 2] = b;
					colorBuffer[i32 + 3] = (byte)(m_BlendingMap[i] * 255f);
				}
			}

			public void CopyClipMap32(byte[] colorBuffer, System.Drawing.Color color)
			{
				byte r = color.R, g = color.G, b = color.B;

				for (int i = 0, i32 = 0; i < m_BlendingMap.Length; i++, i32 += 4)
				{
					colorBuffer[i32 + 0] = r;
					colorBuffer[i32 + 1] = g;
					colorBuffer[i32 + 2] = b;
					colorBuffer[i32 + 3] = (byte)(m_DepthClippingMap[i] * 255f);
				}
			}

			public void CopyIdentify32(byte[] colorBuffer, System.Drawing.Color color)
			{
				double factor = 1f / 255f;
				double r = factor * (double)color.R, g = factor * (double)color.G, b = factor * (double)color.B;

				for (int i = 0, i32 = 0; i < m_BlendingMap.Length; i++, i32 += 4)
				{
					double intesity = m_FieldData[i];

					colorBuffer[i32 + 0] = (byte)(r * intesity);
					colorBuffer[i32 + 1] = (byte)(g * intesity);
					colorBuffer[i32 + 2] = (byte)(b * intesity);
					colorBuffer[i32 + 3] = (byte)(m_BlendingMap[i] * 255f);
				}
			}

			public void CopyFinal32(byte[] colorBuffer, System.Drawing.Color color)
			{
				for (int i = 0, i32 = 0; i < m_BlendingMap.Length; i++, i32 += 4)
				{
					double intesity = m_FieldData[i];

					colorBuffer[i32 + 0] = (byte)(intesity);
					colorBuffer[i32 + 1] = (byte)(intesity);
					colorBuffer[i32 + 2] = (byte)(intesity);
					colorBuffer[i32 + 3] = (byte)(m_BlendingMap[i] * 255f);
				}
			}

			public void CopyBlendMapFloat(float[] buffer)
			{
				for (int i = 0; i < m_BlendingMap.Length; i++)
				{
					buffer[i] = (float)m_BlendingMap[i];
				}
			}

			public void CopyClipMapFloat(float[] buffer)
			{
				for (int i = 0; i < m_DepthClippingMap.Length; i++)
				{
					buffer[i] = (float)m_DepthClippingMap[i];
				}
			}

			public void CopyFinalFloat(float[] buffer)
			{
				for (int i = 0; i < m_FieldData.Length; i++)
				{
					buffer[i] = ((float)m_BlendingMap[i] * (float)m_FieldData[i]) * 0.00390625f;
				}
			}

			#endregion
		}
	}
}
