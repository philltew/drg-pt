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
	public class TextButton : UiControlBase
	{
		private string m_Text = "";
		private FontType m_FontType = FontType.Small;
		private ContentAlignment m_TextAlign = ContentAlignment.MiddleCenter;
		private System.Windows.Forms.Padding m_Padding = new System.Windows.Forms.Padding(0);
		private bool m_FixedSize = false;

		public string Text { get { return m_Text; } set { m_Text = value; } }

		public FontType FontType { get { return m_FontType; } set { m_FontType = value; } }

		public ContentAlignment TextAlign { get { return m_TextAlign; } set { m_TextAlign = value; } }

		public System.Windows.Forms.Padding Padding { get { return m_Padding; } set { m_Padding = value; } }

		public bool FixedSize { get { return m_FixedSize; } set { m_FixedSize = value; } }

		public event EventHandler Click;

		public TextButton()
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
		}

		public override void GetTotalElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			int textIndexCount, textTriangleCount;
			TextRenderHelper.GetTotalElementCounts(m_Text, out textIndexCount, out textTriangleCount);

			TriangleVerts += textTriangleCount;
			TriangleIndices += textIndexCount;
		}

		public override void WriteVisibleElements(View3D view, RectangleF ClientBounds, ref RectangleF RemainingBounds, SlimDX.DataStream LineVerts, ref int LineVertsCount, SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			SizeF stringSize = TextRenderHelper.MessureString(m_Text, m_FontType, view, 1f);

			if (m_FixedSize == false)
			{
				this.Size = new System.Drawing.Size(m_Padding.Horizontal + (int)stringSize.Width, m_Padding.Vertical + (int)stringSize.Height);
			}

			m_Bounds = UiStyleHelper.LayoutControlBounds(RemainingBounds, Location, Size, Anchor, Docking, out RemainingBounds);

			float x, y;
			float px = view.PixelSize.X;
			float py = view.PixelSize.Y;

			PointF textLocation = UiStyleHelper.LayoutTextBounds(Bounds, stringSize, TextAlign, Padding);

			UiStyleHelper.CovertToVertCoords_Relitive(textLocation, view.PixelSize, out x, out y);

			Color4 textColor = UiStyleHelper.GetControlBackColor(this, DisplayMode.Auto);

			TextRenderHelper.WriteString(view, Bounds,
											m_Text, m_FontType, new Vector3(x, y, ZIndexForLines_Float), 1f, textColor,
											TriangleVerts, ref TriangleVertsCount,
											TriangleIndices, ref TriangleIndicesCount);
		}
	}
}
