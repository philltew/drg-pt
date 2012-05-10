using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DS.Kinect;
using RugTech1;
using System.IO;
using System.Drawing.Imaging;

namespace BackgroundFillTest
{
	public partial class Form1 : Form
	{
		KinectBackgroundFilter m_Filter; 
		short[] m_TempBuffer;
		byte[] m_TempBuffer32; 
		Bitmap m_Bitmap; 

		public Form1()
		{
			// Setup filter
			m_Filter = new KinectBackgroundFilter(320, 240);

			// Setup buffers
			m_TempBuffer = new short[320 * 240];
			m_TempBuffer32 = new byte[320 * 240 * 4]; 

			// Create bitmap
			m_Bitmap = new Bitmap(320, 240, System.Drawing.Imaging.PixelFormat.Format32bppArgb); 

			InitializeComponent();			
		}
		
		private void Form1_Load(object sender, EventArgs e)
		{
			GetImageFromBackgroundData(); 
		}

		private void m_LoadButton_Click(object sender, EventArgs e)
		{			
			string path = Helper.ResolvePath(textBox1.Text);

			try
			{
				if (new FileInfo(path).Exists == false)
				{
					MessageBox.Show("The image file '" + textBox1.Text + "' does not exist.");
					return;
				}
			}
			catch
			{
				MessageBox.Show("The image file '" + textBox1.Text + "' does not exist.");
				return;
			}

			// clear the filter
			m_Filter.Clear();
			
			// copy the image file into a test buffer 
			KinectHelper.FillTestBuffer(m_TempBuffer, path);

			// put the temp buffer into the background filter 
			m_Filter.GrabBackground(m_TempBuffer);

			// show the image
			GetImageFromBackgroundData();
		}

		private void m_ProcessButton_Click(object sender, EventArgs e)
		{
			// process the background image (fill in missing dataand blend)
			m_Filter.FillInMissingDataPoints();

			// show the image
			GetImageFromBackgroundData();
		}

		private void GetImageFromBackgroundData()
		{
			SuspendLayout();

			pictureBox1.Image = null;

			BitmapData bmd = m_Bitmap.LockBits(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb); // bmp.PixelFormat);

			// get the background image 
			m_Filter.GetBackgroundImage(m_TempBuffer32, 1, 8001);

			unsafe
			{
				int s = 0;		
				for (int y = 0; y < bmd.Height; y++)
				{
					byte* row = ((byte*)bmd.Scan0 + (y * bmd.Stride));

					int d = 0; 
					for (int x = 0; x < bmd.Width; x++)
					{		
						byte r, g, b, a;

						r = m_TempBuffer32[s++];
						g = m_TempBuffer32[s++];
						b = m_TempBuffer32[s++];
						a = m_TempBuffer32[s++];
																
						row[d++] = b;
						row[d++] = g;
						row[d++] = r;
						row[d++] = a;		
					}
				}
			}

			m_Bitmap.UnlockBits(bmd);

			pictureBox1.Image = m_Bitmap; 

			ResumeLayout(true); 
		}

	}
}
