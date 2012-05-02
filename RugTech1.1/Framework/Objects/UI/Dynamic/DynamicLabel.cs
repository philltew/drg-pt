using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.Text;
using SlimDX;

namespace RugTech1.Framework.Objects.UI.Dynamic
{
	public class DynamicLabel : UiControlBase, IDynamicUiControl
	{
		private string m_Text = "";
		private int m_MaxLength = 4; 
		private FontType m_FontType = FontType.Small;
		private ContentAlignment m_TextAlign = ContentAlignment.MiddleCenter;
		private System.Windows.Forms.Padding m_Padding = new System.Windows.Forms.Padding(0);
		private bool m_FixedSize = false;
		private Color4 m_ForeColor = new Color4();

		public Color4 ForeColor { get { return m_ForeColor; } set { m_ForeColor = value; } }

		public string Text { get { return m_Text; } set { m_Text = value; } }

		public int MaxLength { get { return m_MaxLength; } set { m_MaxLength = value; } }

		public FontType FontType { get { return m_FontType; } set { m_FontType = value; } }

		public ContentAlignment TextAlign { get { return m_TextAlign; } set { m_TextAlign = value; } }

		public System.Windows.Forms.Padding Padding { get { return m_Padding; } set { m_Padding = value; } }

		public bool FixedSize { get { return m_FixedSize; } set { m_FixedSize = value; } }

		public override bool BeginPick(SlimDX.Vector2 mousePos, out UiControl control)
		{
			control = null;
			return false;
		}

		public override void EndPick(SlimDX.Vector2 mousePos, PickType pickType, UiControl control) { }

		public override void GetTotalElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices) { }

		public override void WriteVisibleElements(View3D view, RectangleF ClientBounds, ref RectangleF RemainingBounds, SlimDX.DataStream LineVerts, ref int LineVertsCount, SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			if (FixedSize == false)
			{ 
				SizeF stringSize = TextRenderHelper.MessureString(m_MaxLength, m_FontType, view, 1f);

				this.Size = new System.Drawing.Size(m_Padding.Horizontal + (int)stringSize.Width, m_Padding.Vertical + (int)stringSize.Height);
			}

			m_Bounds = UiStyleHelper.LayoutControlBounds(RemainingBounds, Location, Size, Anchor, Docking, out RemainingBounds);
		}

		#region IDynamicUiControl Members

		public void GetTotalDynamicElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			int textIndexCount, textTriangleCount;
			TextRenderHelper.GetTotalElementCounts(m_MaxLength, out textIndexCount, out textTriangleCount);

			TriangleVerts += textTriangleCount;
			TriangleIndices += textIndexCount;
		}

		public void WriteDynamicElements(View3D view, SlimDX.DataStream LineVerts, ref int LineVertsCount, SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			string text = m_Text;
			
			if (text == null)
			{
				text = ""; 
			}
			else if (text.Length > m_MaxLength)
			{
				text = text.Substring(0, m_MaxLength);
			}

			SizeF stringSize = TextRenderHelper.MessureString(text, m_FontType, view, 1f);
			
			float x, y;
			float px = view.PixelSize.X;
			float py = view.PixelSize.Y;

			PointF textLocation = UiStyleHelper.LayoutTextBounds(Bounds, stringSize, TextAlign, Padding);

			UiStyleHelper.CovertToVertCoords_Relitive(textLocation, view.PixelSize, out x, out y);			

			Color4 textColor;

			if (m_ForeColor == new Color4())
			{
				textColor = UiStyleHelper.GetControlTextColor(this, DisplayMode.Auto);
			}
			else
			{
				textColor = m_ForeColor; 
			}

			TextRenderHelper.WriteString(view, Bounds,
											text, m_FontType, new Vector3(x, y, ZIndexForLines_Float), 1f, textColor,
											TriangleVerts, ref TriangleVertsCount,
											TriangleIndices, ref TriangleIndicesCount);
		}

		#endregion

	}
}
