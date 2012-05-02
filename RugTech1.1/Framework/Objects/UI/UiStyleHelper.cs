using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX;

namespace RugTech1.Framework.Objects.UI
{
	public enum DisplayMode { Auto, Normal, Focused, Hovering, Open }

	public static class UiStyleHelper
	{
		#region Layout Control Bounds
		
		public static RectangleF LayoutControlBounds(RectangleF ParentBounds, 
													 Point Location, Size Size, 
													 AnchorStyles Anchor, DockStyle Docking,
													 out RectangleF RemainingBounds) 
		{
			float x, y, w, h;

			switch (Docking)
			{
				case DockStyle.Top:
					x = ParentBounds.X;
					y = ParentBounds.Y;
					w = ParentBounds.Width;
					h = Size.Height;

					RemainingBounds = new RectangleF(ParentBounds.X, ParentBounds.Y + Size.Height, ParentBounds.Width, ParentBounds.Height - Size.Height); 
					break;
				case DockStyle.Bottom:
					x = ParentBounds.X;
					y = ParentBounds.Bottom - Size.Height;
					w = ParentBounds.Width;
					h = Size.Height;

					RemainingBounds = new RectangleF(ParentBounds.X, ParentBounds.Y, ParentBounds.Width, ParentBounds.Height - Size.Height); 
					break;
				case DockStyle.Left:
					x = ParentBounds.X;
					y = ParentBounds.Y;
					w = Size.Width;
					h = ParentBounds.Height;

					RemainingBounds = new RectangleF(ParentBounds.X + Size.Width, ParentBounds.Y, ParentBounds.Width - Size.Width, ParentBounds.Height); 
					break;
				case DockStyle.Right:
					x = ParentBounds.Right - Size.Width;
					y = ParentBounds.Y;
					w = Size.Width;
					h = ParentBounds.Height;

					RemainingBounds = new RectangleF(ParentBounds.X, ParentBounds.Y, ParentBounds.Width - Size.Width, ParentBounds.Height); 
					break;
				case DockStyle.Fill:
					x = ParentBounds.X;
					y = ParentBounds.Y;
					w = ParentBounds.Width;
					h = ParentBounds.Height;

					RemainingBounds = ParentBounds; 
					break;
				default:
					x = ParentBounds.X + Location.X;
					y = ParentBounds.Y + Location.Y;
					w = Size.Width;
					h = Size.Height;

					RemainingBounds = ParentBounds; 
					break;
			}

			RectangleF bounds = new RectangleF(x, y, w, h);

			return bounds;
		}

		public static PointF LayoutTextBounds(RectangleF Bounds, SizeF stringSize, ContentAlignment TextAlign, Padding Padding)
		{
			float width = Bounds.Width - Padding.Horizontal;
			float height = Bounds.Height - Padding.Vertical; 

			switch (TextAlign)
			{
				case ContentAlignment.BottomCenter:
					return new PointF(Padding.Left + ((width - stringSize.Width) * 0.5f), (height - (stringSize.Height + Padding.Bottom))); 
				case ContentAlignment.BottomLeft:
					return new PointF(Padding.Left, (height - (stringSize.Height + Padding.Bottom))); 
				case ContentAlignment.BottomRight:
					return new PointF((Bounds.Width - (stringSize.Width + Padding.Right)), (height - (stringSize.Height + Padding.Bottom))); 
				case ContentAlignment.MiddleCenter:
					return new PointF(Padding.Left + ((width - stringSize.Width) * 0.5f), Padding.Top + ((height - stringSize.Height) * 0.5f)); 					
				case ContentAlignment.MiddleLeft:
					return new PointF(Padding.Left, Padding.Top + ((height - stringSize.Height) * 0.5f)); 
				case ContentAlignment.MiddleRight:
					return new PointF((Bounds.Width - stringSize.Width), Padding.Top + ((height - stringSize.Height) * 0.5f)); 
				case ContentAlignment.TopCenter:
					return new PointF(Padding.Left + ((width - stringSize.Width) * 0.5f), Padding.Top); 
				case ContentAlignment.TopLeft:
					return new PointF(Padding.Left, Padding.Top);
				case ContentAlignment.TopRight:
					return new PointF((Bounds.Width - stringSize.Width), Padding.Top);
				default:
					return new PointF(Padding.Left + ((width - stringSize.Width) * 0.5f), Padding.Top + ((height - stringSize.Height) * 0.5f));
			}	
		 
			/* 
			switch (TextAlign)
			{
				case ContentAlignment.BottomCenter:
					return new PointF(Bounds.X + (Padding.Left + ((width - stringSize.Width) * 0.5f)), Bounds.Y + (height - (stringSize.Height + Padding.Bottom))); 
				case ContentAlignment.BottomLeft:
					return new PointF(Bounds.X + (Padding.Left), Bounds.Y + (height - (stringSize.Height + Padding.Bottom))); 
				case ContentAlignment.BottomRight:
					return new PointF(Bounds.X + ((Bounds.Width - (stringSize.Width + Padding.Right))), Bounds.Y + (height - (stringSize.Height + Padding.Bottom))); 
				case ContentAlignment.MiddleCenter:
					return new PointF(Bounds.X + (Padding.Left + ((width - stringSize.Width) * 0.5f)), Bounds.Y + (Padding.Top + ((height - stringSize.Height) * 0.5f))); 					
				case ContentAlignment.MiddleLeft:
					return new PointF(Bounds.X + (Padding.Left), Bounds.Y + (Padding.Top + ((height - stringSize.Height) * 0.5f))); 
				case ContentAlignment.MiddleRight:
					return new PointF(Bounds.X + ((Bounds.Width - stringSize.Width)), Bounds.Y + (Padding.Top + ((height - stringSize.Height) * 0.5f))); 
				case ContentAlignment.TopCenter:
					return new PointF(Bounds.X + (Padding.Left + ((width - stringSize.Width) * 0.5f)), Bounds.Y + (Padding.Top)); 
				case ContentAlignment.TopLeft:
					return new PointF(Bounds.X + (Padding.Left), Bounds.Y + (Padding.Top));
				case ContentAlignment.TopRight:
					return new PointF(Bounds.X + ((Bounds.Width - stringSize.Width)), Bounds.Y + (Padding.Top));
				default:
					return new PointF(Bounds.X + (Padding.Left + ((width - stringSize.Width) * 0.5f)), Bounds.Y + (Padding.Top + ((height - stringSize.Height) * 0.5f)));
			}	
			*/
		}

		#endregion

		#region Covert To Vert Coords

		public static void CovertToVertCoords(RectangleF Bounds, Vector2 WindowSize, Vector2 PixelSize, out float x, out float y, out float w, out float h)
		{
			x = (Bounds.X - (WindowSize.X / 2)) * PixelSize.X;
			y = -(Bounds.Y - (WindowSize.Y / 2)) * PixelSize.Y;
			w = Bounds.Width * PixelSize.X;
			h = -Bounds.Height * PixelSize.Y;
		}

		public static void CovertToVertCoords(PointF Location, Vector2 WindowSize, Vector2 PixelSize, out float x, out float y)
		{
			x = (Location.X - (WindowSize.X / 2)) * PixelSize.X;
			y = -(Location.Y - (WindowSize.Y / 2)) * PixelSize.Y;
		}

		public static void CovertToVertCoords(float px, float py, Vector2 WindowSize, Vector2 PixelSize, out float x, out float y)
		{
			x = (px - (WindowSize.X / 2)) * PixelSize.X;
			y = -(py - (WindowSize.Y / 2)) * PixelSize.Y;
		}

		public static void CovertToVertCoords_Relitive(RectangleF Bounds, Vector2 PixelSize, out float x, out float y, out float w, out float h)
		{
			x = Bounds.X * PixelSize.X;
			y = -Bounds.Y * PixelSize.Y;
			w = Bounds.Width * PixelSize.X;
			h = -Bounds.Height * PixelSize.Y;
		}

		public static void CovertToVertCoords_Relitive(PointF Location, Vector2 PixelSize, out float x, out float y)
		{
			x = Location.X * PixelSize.X;
			y = -Location.Y * PixelSize.Y;
		}

		public static void CovertToVertCoords_Relitive(float px, float py, Vector2 PixelSize, out float x, out float y)
		{
			x = px * PixelSize.X;
			y = -py * PixelSize.Y;
		}

		#endregion

		#region Color Styles 

		public static SlimDX.Color4 GetControlLineColor(UiControl control)
		{
			return new Color4(0.5f, 1f, 1f, 1f);
		}

		public static SlimDX.Color4 GetControlBackColor(UiControl control, DisplayMode mode)
		{
			if (mode == DisplayMode.Hovering || (mode == DisplayMode.Auto && control.IsHovering == true))
			{
				return new Color4(0.75f, 0f, 0.33f, 0f);
			}
			else if (mode == DisplayMode.Focused || (mode == DisplayMode.Auto && control.IsFocused == true))
			{				
				return new Color4(0.75f, 0.4f, 0.4f, 0.4f);
			}
			else if (mode == DisplayMode.Open)
			{
				return new Color4(1f, 1f, 1f, 1f);
			}
			else
			{
				return new Color4(0.75f, 0.1f, 0.1f, 0.1f); 
			}
		}

		public static SlimDX.Color4 GetControlTextColor(UiControl control, DisplayMode mode)
		{
			if (mode == DisplayMode.Hovering || (mode == DisplayMode.Auto && control.IsHovering == true))
			{
				return new Color4(1f, 1f, 1f, 1f); 
			}
			else if (mode == DisplayMode.Focused || (mode == DisplayMode.Auto && control.IsFocused == true))
			{
				return new Color4(1f, 0f, 0f, 0f);
			}
			else if (mode == DisplayMode.Open)
			{
				return new Color4(1f, 0f, 0f, 0f); 
			}
			else
			{
				return new Color4(1f, 1f, 1f, 1f);
			}
		}

		#endregion
	}
}