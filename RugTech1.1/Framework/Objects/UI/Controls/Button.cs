using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using RugTech1.Framework.Data;
using RugTech1.Framework.Objects.Text;
using SlimDX;

namespace RugTech1.Framework.Objects.UI.Controls
{
	public class Button : UiControlBase
	{
		private string m_Text = "";
		private FontType m_FontType = FontType.Small; 

		public string Text { get { return m_Text; } set { m_Text = value; } }

		public FontType FontType { get { return m_FontType; } set { m_FontType = value; } }

		public event EventHandler Click;

		public Button()
		{
			InteractionType = ControlInteractionType.Click; // | ControlInteractionType.Drag; 
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
				default:
					base.DoMouseInteraction(mouseState, mouseButtons, mousePos, out shouldUpdate);
					break;
			}			
		}

		public void DoClick()
		{
			if (Click != null)
			{
				Click(this, EventArgs.Empty);
			}
		}

		public override void EndPick(SlimDX.Vector2 mousePos, PickType pickType, UiControl control)
		{
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

			Color4 textColor = UiStyleHelper.GetControlTextColor(this, DisplayMode.Auto);

			TextRenderHelper.WriteString(view, Bounds,
													m_Text, m_FontType, new Vector3(px * (Size.Width - stringSize.Width) * 0.5f, -py * (Size.Height - stringSize.Height) * 0.5f, ZIndexForLines_Float), 1f, textColor,
													TriangleVerts, ref TriangleVertsCount,
													TriangleIndices, ref TriangleIndicesCount);
		}
	}
}
