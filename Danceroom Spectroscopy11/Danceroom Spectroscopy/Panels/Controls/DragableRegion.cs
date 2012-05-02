using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using RugTech1.Framework.Data;
using RugTech1.Framework.Objects.Text;
using SlimDX;
using RugTech1.Framework.Objects.UI;
using RugTech1.Framework.Objects;

namespace DS.Panels
{
	public class DragableRegion : UiControlBase
	{
		private string m_Text = "";
		private FontType m_FontType = FontType.Small;
		private Vector2 m_MouseOffset;
		private bool m_Moving = false;
		
		public string Text { get { return m_Text; } set { m_Text = value; } }

		public FontType FontType { get { return m_FontType; } set { m_FontType = value; } }

		public event EventHandler MoveEnded;
		public event EventHandler Moving; 

		public DragableRegion()
		{
			InteractionType = ControlInteractionType.Click | ControlInteractionType.Drag;

			this.ShowBackground = false;
			this.ShowBorder = true; 
		}

		public override void DoMouseInteraction(MouseState mouseState, System.Windows.Forms.MouseButtons mouseButtons, Vector2 mousePos, out bool shouldUpdate)
		{
			
			shouldUpdate = false;

			switch (mouseState)
			{
				case MouseState.ClickEnd:
					if (Bounds.Contains(mousePos.X, mousePos.Y))
					{
						DoClick();
						shouldUpdate = true;
					}
					break;
				case MouseState.DragStart:
					DoDragStart(mousePos);
					shouldUpdate = true;
					break;
				case MouseState.Dragging:					
					DoDragging(mousePos);
					shouldUpdate = true;					
					break;
				case MouseState.DragEnd:
					DoDragEnd(mousePos);
					break; 
				default:
					base.DoMouseInteraction(mouseState, mouseButtons, mousePos, out shouldUpdate);
					break;
			}			
		}
		
		private void DoDragStart(Vector2 mousePos)
		{
			if (Bounds.Contains(mousePos.X, mousePos.Y) == true) 
			{
				m_MouseOffset = new Vector2(mousePos.X - Bounds.X, mousePos.Y - Bounds.Y);

				m_Moving = true; 

			}
		}

		private void DoDragging(Vector2 mousePos)
		{
			if (m_Moving == true)
			{
				Location = new Point((int)(mousePos.X - m_MouseOffset.X), (int)(mousePos.Y - m_MouseOffset.Y));

				if (Moving != null)
				{
					Moving(this, EventArgs.Empty); 
				}
			}
		}

		private void DoDragEnd(Vector2 mousePos)
		{
			if (m_Moving == true)
			{
				if (MoveEnded != null)
				{
					MoveEnded(this, EventArgs.Empty);
				}
			}

			m_Moving = false;
		}

		public void DoClick()
		{
			
		}

		public override void EndPick(SlimDX.Vector2 mousePos, PickType pickType, UiControl control)
		{
			switch (pickType)
			{
				case PickType.None:
					break;
				case PickType.Hover:
					break;
				case PickType.Focus:
					break;
				case PickType.UnHover:
					break;
				case PickType.UnFocus:
					break;
				default:
					break;
			}

			base.EndPick(mousePos, pickType, control);

			/* 
			switch (pickType)
			{
				case PickType.Focus:
					DoClick();					
					break;
				default:
					break;
			}
			 * */ 
		}

		public override void GetTotalElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			base.GetTotalElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);

			int textIndexCount, textTriangleCount;
			TextRenderHelper.GetTotalElementCounts(m_Text, out textIndexCount, out textTriangleCount);

			TriangleVerts += textTriangleCount;
			TriangleIndices += textIndexCount;
		}

		public override void WriteVisibleElements(View3D view, RectangleF ClientBounds, ref RectangleF RemainingBounds, SlimDX.DataStream LineVerts, ref int LineVertsCount, SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			base.WriteVisibleElements(view, ClientBounds, ref RemainingBounds,
										LineVerts, ref LineVertsCount,
										LinesIndices, ref LinesIndicesCount,
										TriangleVerts, ref TriangleVertsCount,
										TriangleIndices, ref TriangleIndicesCount);

			float x, y, w, h;
			float px = view.PixelSize.X;
			float py = view.PixelSize.Y;

			SizeF stringSize = TextRenderHelper.MessureString(m_Text, m_FontType, view, 1f);

			UiStyleHelper.CovertToVertCoords(Bounds, view.WindowSize, view.PixelSize, out x, out y, out w, out h);

			Color4 textColor = UiStyleHelper.GetControlTextColor(this, DisplayMode.Normal);

			TextRenderHelper.WriteString(view, Bounds,
													m_Text, m_FontType, new Vector3(px * (Size.Width - stringSize.Width) * 0.5f, -py * (Size.Height - stringSize.Height) * 0.5f, ZIndexForLines_Float), 1f, textColor,
													TriangleVerts, ref TriangleVertsCount,
													TriangleIndices, ref TriangleIndicesCount);
		}
	}
}
