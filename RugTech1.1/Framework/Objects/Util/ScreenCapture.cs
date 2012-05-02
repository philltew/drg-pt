using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;

namespace RugTech1.Framework.Objects.Util
{
	public class ScreenCapture : IResourceManager
	{
		private bool m_Disposed = true;
		private Texture2D m_CaptureTexture;
		private SlimDX.DXGI.Surface m_CaptureSurface; 
		private Texture2DDescription m_Description;
		private int m_PixelSizeInBytes;
		private byte[] m_ScanBytes;
		private byte[] m_Bytes;
		private System.Drawing.Bitmap m_Bitmap; 

		public byte[] Bytes { get { return m_Bytes; } } 

		public Texture2DDescription Description { get { return m_Description; } set { m_Description = value; } }
		public int PixelSizeInBytes { get { return m_PixelSizeInBytes; } set { m_PixelSizeInBytes = value; } }
		public System.Drawing.Bitmap Bitmap { get { return m_Bitmap; } }

		public void CopyFromRenderTarget(Resource resource, byte[] destination)
		{
			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext; 

			context.CopyResource(resource, m_CaptureTexture);

			try
			{				
				//context.MapSubresource(m_CaptureTexture, 0, m_CaptureTexture. )
				// now get the data out of the texture
				DataRectangle data = m_CaptureSurface.Map(SlimDX.DXGI.MapFlags.Read);
				
				// and convert it to a byte array
				ExtractByteArray(data.Data.DataPointer, destination, data.Pitch, m_Description.Width, m_Description.Height, m_PixelSizeInBytes);
			}
			catch (Exception ex)
			{				
				Exception newException = new Exception("Error during FastMap Rendering", ex);
				throw newException;
			}
			finally
			{
				m_CaptureSurface.Unmap();
			}
		}

		public void SaveToImage()
		{
			BitmapData data = m_Bitmap.LockBits(new System.Drawing.Rectangle(0, 0, m_Description.Width, m_Description.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
			int height = m_Description.Height;

			IntPtr ptr = data.Scan0;
			int source = 0;
			int scanSize = m_Description.Width * m_PixelSizeInBytes;

			for (int y = 0; y < height; y++)
			{
				Marshal.Copy(m_Bytes, source, ptr, scanSize);

				ptr = IntPtr.Add(ptr, data.Stride);
				source += scanSize;
			}

			m_Bitmap.UnlockBits(data);
		}

		public void SaveToImage(string fileName)
		{
			SaveToImage();

			m_Bitmap.Save(fileName, ImageFormat.Png); 
		}

		private void ExtractByteArray(IntPtr source, byte[] destination, int pitch, int width, int height, int pixelSizeInBytes)
		{
			// Declare an array to hold the bytes of the bitmap.
			IntPtr ptr = source; 
			int dest = 0;
			int scanSize = width * 4; // pixelSizeInBytes; 
			int destScanSize = width * pixelSizeInBytes; 

			// Copy the RGB values into the array.
			for (int y = 0; y < height; y++)
			{
				Marshal.Copy(ptr, m_ScanBytes, 0, scanSize);

				int d = dest;
				for (int x = 0; x < scanSize; x += 4)
				{
					byte r = m_ScanBytes[x + 0];
					byte g = m_ScanBytes[x + 1];
					byte b = m_ScanBytes[x + 2];

					m_Bytes[d + 0] = b;
					m_Bytes[d + 1] = g;
					m_Bytes[d + 2] = r;

					d += m_PixelSizeInBytes; 
				}

				ptr = IntPtr.Add(ptr, pitch);
				dest += destScanSize; 
			}
		}

		#region IResourceManager Members

		public bool Disposed
		{
			get { return m_Disposed; }
		}

		public void LoadResources()
		{
			if (m_Disposed == true)
			{
				m_ScanBytes = new byte[m_Description.Width * 4];
				m_Bytes = new byte[m_Description.Width * m_Description.Height * m_PixelSizeInBytes];
				m_CaptureTexture = new Texture2D(GameEnvironment.Device, m_Description);
				m_CaptureSurface = m_CaptureTexture.AsSurface(); 
				m_Bitmap = new System.Drawing.Bitmap(m_Description.Width, m_Description.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
				
				m_Disposed = false;
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				m_CaptureTexture.Dispose();
				m_CaptureSurface.Dispose(); 
				m_Bitmap.Dispose(); 
				m_Disposed = true;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			UnloadResources(); 
		}

		#endregion


	}
}
