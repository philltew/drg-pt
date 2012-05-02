using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RugTech1.Framework.Capture
{
	public interface IVideoWriter
	{
		int FrameRate { get; set; }

		void Open(string filePath, int width, int height);

		void Close();		

		void AddFrame(byte[] data);
	}
}
