using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RugTech1.Framework.Objects.Text;
using SlimDX;

namespace RugTech1.Framework.Objects.UI.Controls
{
	public class TestLabel : UiControlBase
	{
		private string m_Text = "";
		private FontType m_FontType = FontType.Small;
		private bool m_FixedSize = false;
		private Color4 m_TextColor;
		private bool m_TextColorOverride = false; 

		public string Text { get { return m_Text; } set { m_Text = value; } }

		public FontType FontType { get { return m_FontType; } set { m_FontType = value; } }

		public bool FixedSize { get { return m_FixedSize; } set { m_FixedSize = value; } }

		public Color4 TextColor { get { return m_TextColor; } set { m_TextColor = value; } }

		public bool TextColorOverride { get { return m_TextColorOverride; } set { m_TextColorOverride = value; } }
		
		public override bool BeginPick(SlimDX.Vector2 mousePos, out UiControl control)
		{
			control = null; 
			return false; 
		}

		public override void EndPick(SlimDX.Vector2 mousePos, PickType pickType, UiControl control) { }

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
			SizeF stringSize = TextRenderHelper.MessureString(m_Text, m_FontType, view, 1f);

			if (m_FixedSize == false)
			{
				this.Size = new System.Drawing.Size((int)stringSize.Width, (int)stringSize.Height);
			}

			base.WriteVisibleElements(view, ClientBounds, ref RemainingBounds,
										LineVerts, ref LineVertsCount,
										LinesIndices, ref LinesIndicesCount,
										TriangleVerts, ref TriangleVertsCount,
										TriangleIndices, ref TriangleIndicesCount);

			//m_Bounds = UiStyleHelper.LayoutControlBounds(RemainingBounds, Location, Size, Anchor, Docking, out RemainingBounds);

			float x, y;
			float px = view.PixelSize.X;
			float py = view.PixelSize.Y;

			PointF textLocation = UiStyleHelper.LayoutTextBounds(Bounds, stringSize, ContentAlignment.TopLeft, new Padding(0));

			UiStyleHelper.CovertToVertCoords_Relitive(textLocation, view.PixelSize, out x, out y);	

			Color4 textColor; 

			if (TextColorOverride == true)
			{
				textColor = m_TextColor;
			}
			else
			{
				textColor = UiStyleHelper.GetControlTextColor(this, DisplayMode.Auto);
			}

			TextRenderHelper.WriteString(view, Bounds,
											m_Text, m_FontType, new Vector3(x, y, ZIndexForLines_Float), 1f, textColor,
											TriangleVerts, ref TriangleVertsCount,
											TriangleIndices, ref TriangleIndicesCount);
		}
	}
}
