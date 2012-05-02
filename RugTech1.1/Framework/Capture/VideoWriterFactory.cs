using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Win32.Avi;

namespace RugTech1.Framework.Capture
{
	public class VideoWriterFactory
	{
		public static void Initiate()
		{
#if !__monoCS__
			AVIWriter.Initiate();
#endif
		}

		public static void Shutdown()
		{
#if !__monoCS__
			AVIWriter.Shutdown();
#endif
		}

		public static IVideoWriter CreateVideoWriter()
		{
#if __monoCS__
			throw new NotImplementedException("Sorry, no cross platform video writer has been implemented yet");
#else
			return new UncompressedAVIWriter(); 
#endif
		}
	}
}
