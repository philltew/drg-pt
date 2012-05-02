using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimDX;

namespace DS.Simulation
{
	partial class CompositeFieldImage
	{
		class OverlapRegion
		{
			private RectangleF m_AbsoluteBounds;

			private KinectFieldImage m_ImageA;
			private RectangleF m_BoundsA;

			private KinectFieldImage m_ImageB;
			private RectangleF m_BoundsB;

			public OverlapRegion(RectangleF absBounds, KinectFieldImage imageA, RectangleF boundsA, KinectFieldImage imageB, RectangleF boundsB)
			{
				m_AbsoluteBounds = absBounds; 

				m_ImageA = imageA;
				m_BoundsA = boundsA;

				m_ImageB = imageB;
				m_BoundsB = boundsB;
			}

			public void Apply()
			{
				Vector2 centerA = new Vector2(m_BoundsA.Left + (m_BoundsA.Width * 0.5f), m_BoundsA.Top + (m_BoundsA.Height * 0.5f));
				Vector2 centerB = new Vector2(m_BoundsB.Left + (m_BoundsB.Width * 0.5f), m_BoundsB.Top + (m_BoundsB.Height * 0.5f));				

				Vector2 vectorFromAtoB = centerB - centerA;

				RectangleF regionRelitiveBoundsA = m_AbsoluteBounds;
				regionRelitiveBoundsA.Offset(-m_BoundsA.X, -m_BoundsA.Y);
				FillRectangle(m_ImageA, regionRelitiveBoundsA, vectorFromAtoB);

				RectangleF regionRelitiveBoundsB = m_AbsoluteBounds;
				regionRelitiveBoundsB.Offset(-m_BoundsB.X, -m_BoundsB.Y);
				FillRectangle(m_ImageB, regionRelitiveBoundsB, vectorFromAtoB * -1); 
			}

			private void FillRectangle(KinectFieldImage image, RectangleF rectangle, Vector2 direction)
			{
				int yOffset = (int)rectangle.Top;
				int xOffset = (int)rectangle.Left;

				float diff = rectangle.Width;

				float rowStartValue, inc, value; 

				if (direction.X > 0)
				{
					rowStartValue = 1;

					inc = -1f / diff; 
				}
				else
				{
					rowStartValue = 0;

					inc = 1f / diff; 
				}				

				for (int y = 0; y < rectangle.Height; y++)
				{
					int rowOffset = ((yOffset + y) * image.Stride) + xOffset;

					value = rowStartValue;

					for (int x = 0; x < rectangle.Width; x++)
					{
						image.BlendingMap[rowOffset + x] *= value > 1 ? 1 : value < 0 ? 0 : value;

						value += inc; 
					}
				}
			}
		}
	}
}
