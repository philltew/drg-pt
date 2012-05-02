using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DS.Audio
{
	public class OscEventRecorder : IDisposable
	{
		private FileStream m_FileStream;
		private StreamWriter m_Writer; 
		private bool m_IsOpen; 

		public OscEventRecorder(string filePath)
		{
			m_FileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

			m_Writer = new StreamWriter(m_FileStream);
			
			m_Writer.AutoFlush = false;

			m_IsOpen = true; 
		}

		public void WriteEvent(int timeCode, params object[] args)
		{
			m_Writer.Write(timeCode.ToString());
			m_Writer.Write(",");

			foreach (object obj in args)
			{
				m_Writer.Write(" ");
				m_Writer.Write(obj.ToString());
			}

			m_Writer.WriteLine(";");
			m_Writer.Flush(); 
		}

		public void WriteEvent(int timeCode, float[] args)
		{
			m_Writer.Write(timeCode.ToString());
			m_Writer.Write(",");

			foreach (float obj in args)
			{
				m_Writer.Write(" ");
				m_Writer.Write(obj.ToString());
			}

			m_Writer.WriteLine(";");
			m_Writer.Flush();
		}


		public void Dispose()
		{
			if (m_IsOpen == true)
			{
				m_FileStream.Close();
				m_FileStream.Dispose(); 

				m_FileStream.Close();
				m_FileStream.Dispose();

				m_IsOpen = false; 
			}
		}
	}
}
