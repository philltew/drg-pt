using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RugTech1.Win32.Avi
{
#if !__MonoCS__
	internal class AVIWriter : IDisposable
	{
		public static void Initiate()
		{
			Win32.AVIFileInit();
		}

		public static void Shutdown()
		{
			Win32.AVIFileExit();
		}

		private IntPtr	file;
		private IntPtr	stream;
		private IntPtr	streamCompressed;
		private IntPtr	buf = IntPtr.Zero;

		private int		width;
		private int		height;
		private int		stride;
		private string	codec = "DIB ";
		private int		quality = -1;
		private int		rate = 25;
		private int		position;

		// CurrentPosition property
		public int CurrentPosition
		{
			get { return position; }
		}
		// Width property
		public int Width
		{
			get
			{
				return (buf != IntPtr.Zero) ? width : 0;
			}
		}
		// Height property
		public int Height
		{
			get
			{
				return (buf != IntPtr.Zero) ? height : 0;
			}
		}
		// Codec property
		public string Codec
		{
			get { return codec; }
			set { codec = value; }
		}
		// Quality property
		public int Quality
		{
			get { return quality; }
			set { quality = value; }
		}

		// FrameRate property
		public int FrameRate
		{
			get { return rate; }
			set { rate = value; }
		}

		public AVIWriter()
		{		
		}

		public AVIWriter(string codec)
		{
			this.codec = codec;
		}

		// Desctructor
		~AVIWriter()
		{
			Dispose(false);
		}

		// Free all unmanaged resources
		public void Dispose()
		{
			Dispose(true);
			// Remove me from the Finalization queue 
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Dispose managed resources
				
				// there is nothing managed yet
			}

			Close();
		}

		// Create new AVI file
		public void Open(string fname, int width, int height)
		{
			// close previous file
			Close();

			// calculate stride
			stride = width * 3;
			int r = stride % 4;
			if (r != 0)
				stride += (4 - r);

			// create new file
			if (Win32.AVIFileOpen(out file, fname, Win32.OpenFileMode.Create | Win32.OpenFileMode.Write, IntPtr.Zero) != 0)
			{ 
				throw new ApplicationException("Failed opening file");
			}

			this.width = width;
			this.height = height;

			// describe new stream
			Win32.AVISTREAMINFO info = new Win32.AVISTREAMINFO();

			info.fccType	= Win32.mmioFOURCC("vids");
			info.fccHandler	= Win32.mmioFOURCC(codec);
			info.dwScale	= 1;
			info.dwRate		= rate;
			info.dwSuggestedBufferSize = stride * height;

			// create stream
			if (Win32.AVIFileCreateStream(file, out stream, ref info) != 0)
			{ 
				throw new ApplicationException("Failed creating stream");
			}

			// describe compression options
			Win32.AVICOMPRESSOPTIONS opts = new Win32.AVICOMPRESSOPTIONS();

			opts.fccHandler	= Win32.mmioFOURCC(codec);
			opts.dwQuality	= quality;

			//
			Win32.AVISaveOptions(stream, ref opts, IntPtr.Zero); // != 0)
			{
				// throw new ApplicationException("Failed creating compressed options");
			}
			
			// create compressed stream
			if (Win32.AVIMakeCompressedStream(out streamCompressed, stream, ref opts, IntPtr.Zero) != 0)
			{ 
				throw new ApplicationException("Failed creating compressed stream");
			}

			// describe frame format
			Win32.BITMAPINFOHEADER bih = new Win32.BITMAPINFOHEADER();

			bih.biSize		= Marshal.SizeOf(bih.GetType());
			bih.biWidth		= width;
			bih.biHeight	= height;
			bih.biPlanes	= 1;
			bih.biBitCount	= 24;
			bih.biSizeImage	= 0;
			bih.biCompression = 0; // BI_RGB

			// set frame format
			if (Win32.AVIStreamSetFormat(streamCompressed, 0, ref bih, Marshal.SizeOf(bih.GetType())) != 0)
			{ 
				throw new ApplicationException("Failed creating compressed stream");
			}

			// alloc unmanaged memory for frame
			buf = Marshal.AllocHGlobal(stride * height);

			position = 0;
		}

		// Close file
		public void Close()
		{
			// free unmanaged memory
			if (buf != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(buf);
				buf = IntPtr.Zero;
			}
			// release compressed stream
			if (streamCompressed != IntPtr.Zero)
			{
				Win32.AVIStreamRelease(streamCompressed);
				streamCompressed = IntPtr.Zero;
			}
			// release stream
			if (stream != IntPtr.Zero)
			{
				Win32.AVIStreamRelease(stream);
				stream = IntPtr.Zero;
			}
			// release file
			if (file != IntPtr.Zero)
			{
				Win32.AVIFileRelease(file);
				file = IntPtr.Zero;
			}
		}


		// Add new frame to the AVI file
		public void AddFrame(byte[] bytes)
		{
			// copy image data
			int dstStride = stride;

			IntPtr dest = buf;
			int srcStride = (width * 3);
			int src = (width * 3) * (height - 1); 
			
			for (int y = 0; y < height; y++)
			{
				Marshal.Copy(bytes, src, dest, srcStride);

				dest = IntPtr.Add(dest, dstStride);
				src -= srcStride;
			}

			// write to stream
			if (Win32.AVIStreamWrite(streamCompressed, position, 1, buf, stride * height, 0, IntPtr.Zero, IntPtr.Zero) != 0)
			{
				throw new ApplicationException("Failed adding frame");
			}

			position++;
		}

		// Add new frame to the AVI file
		public void AddFrame(System.Drawing.Bitmap bmp)
		{
			// check image dimension
			if ((bmp.Width != width) || (bmp.Height != height))
				throw new ApplicationException("Invalid image dimension");

			// lock bitmap data
			BitmapData	bmData = bmp.LockBits(
				new Rectangle(0, 0, width, height),
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			// copy image data
			int srcStride = bmData.Stride;
			int dstStride = stride;

			//int src = bmData.Scan0.ToInt32() + srcStride * (height - 1);
			//int dst = buf.ToInt32();
			IntPtr dest = buf; 
			IntPtr src = IntPtr.Add(bmData.Scan0, srcStride * (height - 1));
			byte[] singleScanLine = new byte[srcStride]; 

			for (int y = 0; y < height; y++)
			{
				Marshal.Copy(src, singleScanLine, 0, srcStride);
				Marshal.Copy(singleScanLine, 0, dest, srcStride);
				// Win32.CopyMemory(dest, src, dstStride);

				dest = IntPtr.Add(dest, dstStride);
				src = IntPtr.Subtract(src, srcStride);
			}

			// unlock bitmap data
			bmp.UnlockBits(bmData);

			// write to stream
			if (Win32.AVIStreamWrite(streamCompressed, position, 1, buf, stride * height, 0, IntPtr.Zero, IntPtr.Zero) != 0)
			{ 
				throw new ApplicationException("Failed adding frame");
			}

			position++;
		}
	}
#endif
}
