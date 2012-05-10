using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1;
using System.Drawing.Imaging;
using System.Drawing;

namespace DS.Kinect
{
	public enum KinectCopyMode { WithIdent, BackgroundElimination, NoIdent }

	public static class KinectHelper
	{
		public const int BLUE_IDX = 2;
		public const int GREEN_IDX = 1;
		public const int RED_IDX = 0;
		public const int ALPHA_IDX = 3;

		#region Copy Kinect Buffer
		
		public static void CopyKinectBuffer(byte[] sourceBuffer, short[] destBuffer, KinectCopyMode mode, out bool IsTrackingAPlayer)
		{
			bool isTrackingAPlayer = false;

			if (mode == KinectCopyMode.NoIdent)
			{
				for (int source = 0, dest = 0; source < sourceBuffer.Length && dest < destBuffer.Length; source += 2, dest++)
				{
					int realDepth = (sourceBuffer[source + 1] << 8) | (sourceBuffer[source]);

					destBuffer[dest] = (short)realDepth;				
				}
			}
			else if (mode == KinectCopyMode.WithIdent)
			{
				for (int source = 0, dest = 0; source < sourceBuffer.Length && dest < destBuffer.Length; source += 2, dest++)
				{
					int player = sourceBuffer[source] & 0x07;
					int realDepth = (sourceBuffer[source + 1] << 5) | (sourceBuffer[source] >> 3);

					destBuffer[dest] = (short)realDepth;
					isTrackingAPlayer = isTrackingAPlayer || player != 0;
				}
			}
			else
			{
				for (int source = 0, dest = 0; source < sourceBuffer.Length && dest < destBuffer.Length; source += 2, dest++)
				{
					int player = sourceBuffer[source] & 0x07;
					int realDepth = (sourceBuffer[source + 1] << 5) | (sourceBuffer[source] >> 3);

					if (player > 0)
					{
						destBuffer[dest] = (short)realDepth;
						isTrackingAPlayer = true;
					}
					else
					{
						destBuffer[dest] = -1; 
					}
				}
			}

			IsTrackingAPlayer = isTrackingAPlayer;
		}

		public static void CopyKinectBuffer(byte[] sourceBuffer, short[] destBuffer, byte[] identBuffer, KinectCopyMode mode, out bool IsTrackingAPlayer)
		{
			bool isTrackingAPlayer = false;

			if (mode == KinectCopyMode.WithIdent)
			{
				for (int source = 0, dest = 0; source < sourceBuffer.Length && dest < destBuffer.Length; source += 2, dest++)
				{
					int player = sourceBuffer[source] & 0x07;
					int realDepth = (sourceBuffer[source + 1] << 5) | (sourceBuffer[source] >> 3);

					destBuffer[dest] = (short)realDepth;
					identBuffer[dest] = (byte)player;

					isTrackingAPlayer = isTrackingAPlayer || player != 0;
				}
			}
			else
			{
				for (int source = 0, dest = 0; source < sourceBuffer.Length && dest < destBuffer.Length; source += 2, dest++)
				{
					int player = sourceBuffer[source] & 0x07;
					int realDepth = (sourceBuffer[source + 1] << 5) | (sourceBuffer[source] >> 3);

					if (player > 0)
					{
						destBuffer[dest] = (short)realDepth;
						identBuffer[dest] = (byte)player;

						isTrackingAPlayer = true; 
					}
					else
					{
						destBuffer[dest] = -1;
						identBuffer[dest] = 0; 
					}
				}
			}

			IsTrackingAPlayer = isTrackingAPlayer;
		}

		/* 
		public static void CopyColorBuffer(Microsoft.Research.Kinect.Nui.ImageFrame imageFrame, byte[] dest)
		{
			byte[] source = imageFrame.Image.Bits;

			if (imageFrame.Image.BytesPerPixel == 4)
			{
				int s = 0;
				int d = 0;
				byte r, g, b, a; 

				for (int i = 0, ie = imageFrame.Image.Width * imageFrame.Image.Height; i < ie; i++)
				{
					b = source[s++];					
					g = source[s++];
					r = source[s++];
					a = source[s++];
 
					dest[d++] = r;
					dest[d++] = g;
					dest[d++] = b;
					dest[d++] = a;
				}
			}
			else if (imageFrame.Image.BytesPerPixel == 3)
			{
				int s = 0;
				int d = 0;
				byte r, g, b;

				for (int i = 0, ie = imageFrame.Image.Width * imageFrame.Image.Height; i < ie; i++)
				{
					b = source[s++];
					g = source[s++];
					r = source[s++];

					dest[d++] = r;
					dest[d++] = g;
					dest[d++] = b;
					dest[d++] = 255;
				}
			}
			else
			{
				throw new Exception("Unsuppprted video frame format '" + imageFrame.Image.BytesPerPixel + "'");
			}
		}
		*/ 

		#endregion

		#region Scale Buffer For Display

		public static void ScaleBufferForDisplay8(short[] sourceBuffer, byte[] destBuffer8)
		{
			for (int i = 0; i < sourceBuffer.Length; i++)
			{
				short value = sourceBuffer[i];

				if (value == -1)
				{
					destBuffer8[i] = 0;
				}
				else
				{
					destBuffer8[i] = (byte)(255 * value / 0x0fff);
				}
			}
		}

		public static void ScaleBufferForDisplay32(short[] sourceBuffer, byte[] destBuffer32)
		{
			for (int i = 0, i32 = 0; i < sourceBuffer.Length; i++, i32 += 4)
			{
				short value = sourceBuffer[i];

				if (value == -1)
				{
					destBuffer32[i32 + RED_IDX] = 0;
					destBuffer32[i32 + GREEN_IDX] = 0;
					destBuffer32[i32 + BLUE_IDX] = 0;
					destBuffer32[i32 + ALPHA_IDX] = 255;
				}
				else
				{
					// byte intensity = (byte)(255 - (255 * sourceBuffer[i] / 0x0fff));
					byte intensity = (byte)(255 - (255 * value / 0x0fff));

					destBuffer32[i32 + RED_IDX] = intensity;
					destBuffer32[i32 + GREEN_IDX] = intensity;
					destBuffer32[i32 + BLUE_IDX] = intensity;
					destBuffer32[i32 + ALPHA_IDX] = 255;
				}
			}
		}

		public static void ScaleBufferForDisplay32(short[] sourceBuffer, byte[] destBuffer32, short minValue, short maxValue)
		{
			int max = maxValue - minValue;

			if (max < 1)
			{
				max = 1; 
			}

			for (int i = 0, i32 = 0; i < sourceBuffer.Length; i++, i32 += 4)
			{
				int value = sourceBuffer[i];

				if (value <= 0)
				{
					destBuffer32[i32 + RED_IDX] = 0;
					destBuffer32[i32 + GREEN_IDX] = 0;
					destBuffer32[i32 + BLUE_IDX] = 0;
					destBuffer32[i32 + ALPHA_IDX] = 255;
				}
				else
				{
					value -= minValue;

					if (value <= 0)
					{
						destBuffer32[i32 + RED_IDX] = 255;
						destBuffer32[i32 + GREEN_IDX] = 0;
						destBuffer32[i32 + BLUE_IDX] = 255;
						destBuffer32[i32 + ALPHA_IDX] = 255;
					}
					else if (value > max)
					{
						destBuffer32[i32 + RED_IDX] = 0;
						destBuffer32[i32 + GREEN_IDX] = 255;
						destBuffer32[i32 + BLUE_IDX] = 0;
						destBuffer32[i32 + ALPHA_IDX] = 255;
					}
					else 
					{
						// byte intensity = (byte)(255 - (255 * sourceBuffer[i] / 0x0fff));
						byte intensity = (byte)(255 - (255 * value / max));

						destBuffer32[i32 + RED_IDX] = intensity;
						destBuffer32[i32 + GREEN_IDX] = intensity;
						destBuffer32[i32 + BLUE_IDX] = intensity;
						destBuffer32[i32 + ALPHA_IDX] = 255;
					}
				}
			}
		}

		public static void ScaleBufferForDisplay32(short[] sourceBuffer, byte[] identBuffer, byte[] destBuffer32)
		{
			for (int i = 0, i32 = 0; i < sourceBuffer.Length; i++, i32 += 4)
			{
				byte player = identBuffer[i];
				byte intensity = (byte)(255 * sourceBuffer[i] / 0x0fff); 

				destBuffer32[i32 + RED_IDX] = 0;
				destBuffer32[i32 + GREEN_IDX] = 0; 
				destBuffer32[i32 + BLUE_IDX] = 0;
				destBuffer32[i32 + ALPHA_IDX] = 255; 

				// choose different display colors based on player
				switch (player)
				{
					case 0:
						destBuffer32[i32 + RED_IDX] = (byte)(intensity / 2);
						destBuffer32[i32 + GREEN_IDX] = (byte)(intensity / 2);
						destBuffer32[i32 + BLUE_IDX] = (byte)(intensity / 2);
						break;
					case 1:
						destBuffer32[i32 + RED_IDX] = intensity;
						break;
					case 2:
						destBuffer32[i32 + GREEN_IDX] = intensity;
						break;
					case 3:
						destBuffer32[i32 + RED_IDX] = (byte)(intensity / 4);
						destBuffer32[i32 + GREEN_IDX] = (byte)(intensity);
						destBuffer32[i32 + BLUE_IDX] = (byte)(intensity);
						break;
					case 4:
						destBuffer32[i32 + RED_IDX] = (byte)(intensity);
						destBuffer32[i32 + GREEN_IDX] = (byte)(intensity);
						destBuffer32[i32 + BLUE_IDX] = (byte)(intensity / 4);
						break;
					case 5:
						destBuffer32[i32 + RED_IDX] = (byte)(intensity);
						destBuffer32[i32 + GREEN_IDX] = (byte)(intensity / 4);
						destBuffer32[i32 + BLUE_IDX] = (byte)(intensity);
						break;
					case 6:
						destBuffer32[i32 + RED_IDX] = (byte)(intensity / 2);
						destBuffer32[i32 + GREEN_IDX] = (byte)(intensity / 2);
						destBuffer32[i32 + BLUE_IDX] = (byte)(intensity);
						break;
					case 7:
						destBuffer32[i32 + RED_IDX] = (byte)(255 - intensity);
						destBuffer32[i32 + GREEN_IDX] = (byte)(255 - intensity);
						destBuffer32[i32 + BLUE_IDX] = (byte)(255 - intensity);
						break;
				}
			}
		}

		#endregion

		/* 
		 // 0x3FFF;
			short depthValue = 8000;

			byte newValue1 = (byte)(0x80 | (byte)((depthValue & 0x3f00) >> 8));
			byte newValue2 = (byte)(depthValue & 0xFF);

			bool flag = (newValue1 & 0x80) == 128;
			short deDepthValue = (short)((newValue1 & 0x3F) << 8 | newValue2);

			RC.WriteLine(flag + " " + deDepthValue.ToString()); 

			newValue1 = (byte)((byte)((depthValue & 0x3f00) >> 8));
			newValue2 = (byte)(depthValue & 0xFF);

			flag = (newValue1 & 0x80) == 128;
			deDepthValue = (short)((newValue1 & 0x3F) << 8 | newValue2);

			RC.WriteLine(flag + " " + deDepthValue.ToString());
		 */ 

		public static void CompressDepthImage(int width, int height, int detail, short[] depthImage, byte[] buffer, out int byteCount)
		{
			int srcIndex = 0;
			int destIndex = 0;
			int currentZeroCount = 0; 

			for (int p = 0, pe = width * height; p < pe; p++)
			{
				short value = depthImage[srcIndex++];

				if (value == -1)
				{
					currentZeroCount++;
				}
				else
				{
					if (currentZeroCount > 0)
					{
						buffer[destIndex++] = (byte)(0x80 | (byte)((currentZeroCount & 0x7f00) >> 8));
						buffer[destIndex++] = (byte)(currentZeroCount & 0xFF);
						currentZeroCount = 0; 
					}

					buffer[destIndex++] = (byte)((currentZeroCount & 0x7f00) >> 8);
					buffer[destIndex++] = (byte)((currentZeroCount & 0x00FF));					
				}
			}

			if (currentZeroCount > 0)
			{
				buffer[destIndex++] = (byte)(0x80 | (byte)((currentZeroCount & 0x7f00) >> 8));
				buffer[destIndex++] = (byte)(currentZeroCount & 0xFF);
				currentZeroCount = 0;
			}

			byteCount = destIndex;
		}

		public static void CompressDepthImage(int width, int height, int detail, short[] depthImage, byte[] buffer, int start, out int byteCount)
		{
			int srcIndex = 0;
			int destIndex = start;
			int currentZeroCount = 0;

			for (int p = 0, pe = width * height; p < pe; p++)
			{
			//for (int y = 0, ye = height; y < ye; y++)
			//{
				//for (int x = 0, xe = width; x < xe; x++)
				//{
				short value = depthImage[srcIndex++];

				if (value == -1)
				{
					if (currentZeroCount == 0x7FFE)
					{
						buffer[destIndex++] = (byte)(0x80 | (byte)((currentZeroCount & 0x7f00) >> 8));
						buffer[destIndex++] = (byte)(currentZeroCount & 0xFF);
						currentZeroCount = 0;
					}
					else
					{
						currentZeroCount++;
					}
				}
				else
				{
					if (currentZeroCount > 0)
					{
						buffer[destIndex++] = (byte)(0x80 | (byte)((currentZeroCount & 0x7f00) >> 8));
						buffer[destIndex++] = (byte)(currentZeroCount & 0xFF);
						currentZeroCount = 0;
					}

					buffer[destIndex++] = (byte)((value & 0x7f00) >> 8);
					buffer[destIndex++] = (byte)((value & 0x00FF));
				}
				//}				
			}

			if (currentZeroCount > 0)
			{
				buffer[destIndex++] = (byte)(0x80 | (byte)((currentZeroCount & 0x7f00) >> 8));
				buffer[destIndex++] = (byte)(currentZeroCount & 0xFF);
				currentZeroCount = 0;
			}

			byteCount = destIndex - start;
		}

		public static void DecompressDepthImage(int width, int height, int detail, short[] depthImage, byte[] buffer, int start, int byteCount)
		{
			int srcIndex = start;
			int destIndex = 0;

			for (int p = 0, pe = byteCount / 2; p < pe; p++)
			{
				byte value1 = buffer[srcIndex++];
				byte value2 = buffer[srcIndex++];

				if ((value1 & 0x80) == 0x80)
				{
					int zeroCount = ((value1 & 0x7F) << 8 | value2);

					for (int i = 0; i < zeroCount; i++)
					{
						depthImage[destIndex++] = -1;
					}
				}
				else
				{
					depthImage[destIndex++] = (short)((value1 & 0x7F) << 8 | value2);
				}
			}
		}

		public static int GetSizeForResolution(ImageResolution imageResolution)
		{
			switch (imageResolution)
			{
				case ImageResolution.Invalid:
					return 0; 
				case ImageResolution.Resolution1280x1024:
					return 1280 * 1024;
				case ImageResolution.Resolution640x480:
					return 640 * 480;
				case ImageResolution.Resolution320x240:
					return 320 * 240; 
				case ImageResolution.Resolution80x60:
					return 80 * 60; 
				default:
					return 0; 
			}
		}

		public static int GetWidthForResolution(ImageResolution imageResolution)
		{
			switch (imageResolution)
			{
				case ImageResolution.Invalid:
					return 0;
				case ImageResolution.Resolution1280x1024:
					return 1280;
				case ImageResolution.Resolution640x480:
					return 640;
				case ImageResolution.Resolution320x240:
					return 320;
				case ImageResolution.Resolution80x60:
					return 80;
				default:
					return 0;
			}
		}

		public static int GetHeightForResolution(ImageResolution imageResolution)
		{
			switch (imageResolution)
			{
				case ImageResolution.Invalid:
					return 0;
				case ImageResolution.Resolution1280x1024:
					return 1024;
				case ImageResolution.Resolution640x480:
					return 480;
				case ImageResolution.Resolution320x240:
					return 240;
				case ImageResolution.Resolution80x60:
					return 60;
				default:
					return 0;
			}
		}

		public static void FillTestBuffer(double[] buffer, string path)
		{
			string filePath = Helper.ResolvePath(path);

			using (Bitmap bmp = (Bitmap)Bitmap.FromFile(filePath))
			{
				BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb); // bmp.PixelFormat);

				unsafe
				{	
					int i = 0; 
					for (int y = 0; y < bmd.Height; y++)
					{
						uint* row = (uint*)((byte*)bmd.Scan0 + (y * bmd.Stride));

						for (int x = 0; x < bmd.Width; x++)
						{
							buffer[i++] = (double)((row[x] & 0x00ff0000) >> 16);
						}
					}
				}

				bmp.UnlockBits(bmd); 
			}
		}

		public static void FillTestBuffer(short[] buffer, string path)
		{
			string filePath = Helper.ResolvePath(path);

			using (Bitmap bmp = (Bitmap)Bitmap.FromFile(filePath))
			{
				BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb); // bmp.PixelFormat);

				unsafe
				{
					int i = 0;
					for (int y = 0; y < bmd.Height; y++)
					{
						uint* row = (uint*)((byte*)bmd.Scan0 + (y * bmd.Stride));

						for (int x = 0; x < bmd.Width; x++)
						{
							byte value = (byte)((row[x] & 0x00ff0000) >> 16);

							buffer[i++] = (short)(value * 31);
						}
					}
				}

				bmp.UnlockBits(bmd);
			}
		}
	}
}
