using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS.Kinect;
using System.Drawing;
using DSParticles3;

namespace DS.Simulation
{
	public enum CompositeFieldEdgeMode { Reflect, Wrap }
	/// <summary>
	/// The image from N cameras. Each camera image is Rotated, Translated and clipped to form a single composite image
	/// </summary>
	partial class CompositeFieldImage : IFieldDataSource
	{
		#region Private and internal Members
		
		internal List<KinectFieldImage> Images = new List<KinectFieldImage>();
		private List<OverlapRegion> OverlapRegions = new List<OverlapRegion>();

		private Rectangle m_Bounds;

		private CompositeFieldEdgeMode m_VerticleEdgeMode = CompositeFieldEdgeMode.Reflect;
		private CompositeFieldEdgeMode m_HorizontalEdgeMode = CompositeFieldEdgeMode.Reflect;
		
		#endregion

		#region Public Properties
		
		public Rectangle Bounds { get { return m_Bounds; } set { m_Bounds = value; } }

		#endregion

		public void RegisterDevice(KinectDevice device)
		{
			KinectFieldImage image = new KinectFieldImage(this, device);

			Images.Add(image);
		}

		/// <summary>
		/// Update any images that need it
		/// </summary>
		/// <returns>true if any images were updated</returns>
		public bool Update()
		{
			bool result = false; 

			foreach (KinectFieldImage image in Images)
			{
				result |= image.Update();				
			}

			return result;
		}

		public void RecalculateComposite()
		{
			RecalculateOverlapRegions();

			RecalculateBlendingMaps();

			RecalculateClippingMaps();

			RecalculateAbsAxis(); 
		}

		private void RecalculateOverlapRegions()
		{
			OverlapRegions.Clear(); 			

			for (int i = 0; i < Images.Count; i++)
			{
				KinectFieldImage image = Images[i];				

				float x, y, w, h;

				x = image.X; 
				y = image.Y;
				w = image.Width;
				h = image.Height;

				Images[i].AbsBounds = new RectangleF(x, y, w, h);
			}

			for (int i = 0; i < Images.Count; i++)
			{
				for (int j = i + 1; j < Images.Count; j++)
				{
					if (Images[i].AbsBounds.IntersectsWith(Images[j].AbsBounds) == true)
					{
						RectangleF intersect = Images[i].AbsBounds;
						intersect.Intersect(Images[j].AbsBounds);

						OverlapRegion region = new OverlapRegion(intersect, Images[i], Images[i].AbsBounds, Images[j], Images[j].AbsBounds);

						OverlapRegions.Add(region); 
					}
				}
			}
		}

		private void RecalculateBlendingMaps()
		{
			foreach (KinectFieldImage image in Images)
			{
				image.Reset();
			}

			foreach (OverlapRegion region in OverlapRegions)
			{
				region.Apply(); 
			}
		}

		private void RecalculateClippingMaps()
		{

		}

		private void RecalculateAbsAxis()
		{

		}

		#region IFieldDataSource Members

		public int Width
		{
			get { return m_Bounds.Width; }
		}

		public int Height
		{
			get { return m_Bounds.Height; }
		}

		public double GetValueAt(int x, int y)
		{
			// get the postion of the particle in absolute space
			double absX = m_Bounds.X + x;
			double absY = m_Bounds.Y + y;

			double value = 0; 

			foreach (KinectFieldImage image in Images)
			{
				// get the subset for that region
				value += image.GetValueAt(absX, absY);
			}

			return value; 
		}

		public void GetSubsetOfPixelsAlongX(double Xposition, double Yposition, int PixelsEitherSide, ref double[] Xpixels, out int count)
		{
			// set all the values to zero
			for (int i = 0; i < Xpixels.Length; i++)
			{
				Xpixels[i] = 0;
			}

			// get the postion of the particle in absolute space
			double absX = m_Bounds.X + Xposition;
			double absY = m_Bounds.Y + Yposition;

			// get the min and max values
			int absXmin = (int)(absX - (double)PixelsEitherSide);
			int absXmax = (int)(absX + (double)PixelsEitherSide);

			// build a rectangle for the region we will require
			RectangleF absRect = new RectangleF(absXmin, (float)absY, absXmax - absXmin, 1);

			// for all images 
			foreach (KinectFieldImage image in Images)
			{
				// if the image bounds intersects the region 
				if (image.AbsBounds.IntersectsWith(absRect) == true)
				{
					// get the subset for that region
					image.GetSubsetOfPixelsAlongX(absX, absY, PixelsEitherSide, ref Xpixels);
				}
			}

			count = PixelsEitherSide - 1; 
		}

		public void GetSubsetOfPixelsAlongY(double Xposition, double Yposition, int PixelsEitherSide, ref double[] Ypixels, out int count)
		{
			// set all the values to zero
			for (int i = 0; i < Ypixels.Length; i++)
			{
				Ypixels[i] = 0;
			}

			// get the postion of the particle in absolute space
			double absX = m_Bounds.X + Xposition;
			double absY = m_Bounds.Y + Yposition;

			// get the min and max values
			int absYmin = (int)(absY - (double)PixelsEitherSide);
			int absYmax = (int)(absY + (double)PixelsEitherSide);

			// build a rectangle for the region we will require
			RectangleF absRect = new RectangleF((float)absX, absYmin, 1, absYmax - absYmin);

			// for all images 
			foreach (KinectFieldImage image in Images)
			{
				// if the image bounds intersects the region 
				if (image.AbsBounds.IntersectsWith(absRect) == true)
				{
					// get the subset for that region
					image.GetSubsetOfPixelsAlongY(absX, absY, PixelsEitherSide, ref Ypixels);
				}
			}

			count = PixelsEitherSide - 1; 
		}

		#endregion

	}
}
