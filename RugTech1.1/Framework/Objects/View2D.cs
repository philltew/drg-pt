using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;

namespace RugTech1.Framework.Objects
{
	public class View2D
	{
		public Viewport Viewport;

		public Vector2 PixelSize;
		public Vector2 ViewportOffset;
		public Vector2 WindowSize;

		public View2D(System.Drawing.Rectangle activeRegion, int windowWidth, int windowHeight)
		{
			Resize2D(activeRegion, windowWidth, windowHeight);
		}

		public virtual void Resize(System.Drawing.Rectangle activeRegion, int windowWidth, int windowHeight)
		{
			Resize2D(activeRegion, windowWidth, windowHeight);
		}

		private void Resize2D(System.Drawing.Rectangle activeRegion, int windowWidth, int windowHeight)
		{
			Viewport = new Viewport(activeRegion.X, activeRegion.Y, activeRegion.Width, activeRegion.Height);
			PixelSize = new Vector2(2f / (activeRegion.Width), 2f / (activeRegion.Height));
			WindowSize = new Vector2((float)windowWidth, (float)windowHeight);

			float leftOffset = (float)activeRegion.Left;
			float rightOffset = (float)(windowWidth - activeRegion.Right);

			float topOffset = (float)activeRegion.Top;
			float bottomOffset = (float)(windowHeight - activeRegion.Bottom);

			ViewportOffset = new Vector2((rightOffset - leftOffset) * 0.5f, (bottomOffset - topOffset) * 0.5f);
			
		}
	}
}
