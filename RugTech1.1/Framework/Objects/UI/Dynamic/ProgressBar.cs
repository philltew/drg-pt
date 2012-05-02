using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.Text;
using SlimDX;

namespace RugTech1.Framework.Objects.UI.Dynamic
{
	public enum BarAlignment { Horizontal, Vertical }

	public class ProgressBar : UiControlBase, IDynamicUiControl
	{
		private float m_MaxValue = 100f;
		private float m_Value = 0f;
		private BarAlignment m_BarAlignment = BarAlignment.Horizontal; 

		public float MaxValue { get { return m_MaxValue; } set { m_MaxValue = value; } }

		public float Value { get { return m_Value; } set { m_Value = value; } }

		public BarAlignment BarAlignment { get { return m_BarAlignment; } set { m_BarAlignment = value; } }

		public override bool BeginPick(SlimDX.Vector2 mousePos, out UiControl control)
		{
			control = null; 
			
			return false;
		}

		public override void EndPick(SlimDX.Vector2 mousePos, PickType pickType, UiControl control) { }

		public override void GetTotalElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			base.GetTotalElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);
		}

		public override void WriteVisibleElements(View3D view, RectangleF ClientBounds, ref RectangleF RemainingBounds, SlimDX.DataStream LineVerts, ref int LineVertsCount, SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			base.WriteVisibleElements(view, ClientBounds, ref RemainingBounds,
										LineVerts, ref LineVertsCount,
										LinesIndices, ref LinesIndicesCount,
										TriangleVerts, ref TriangleVertsCount,
										TriangleIndices, ref TriangleIndicesCount);			
		}

		#region IDynamicUiControl Members

		public void GetTotalDynamicElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			TriangleVerts += 4;
			TriangleIndices += 6;
		}

		public void WriteDynamicElements(View3D view, SlimDX.DataStream LineVerts, ref int LineVertsCount, SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			float x, y, w, h;

			float percent;
			RectangleF barBounds;

			if (m_BarAlignment == Dynamic.BarAlignment.Horizontal)
			{
				percent = (m_Bounds.Width / m_MaxValue) * m_Value;

				barBounds = new RectangleF(m_Bounds.X, m_Bounds.Y, percent, m_Bounds.Height);
			}
			else 
			{
				percent = (m_Bounds.Height / m_MaxValue) * m_Value;

				barBounds = new RectangleF(m_Bounds.X, m_Bounds.Y + (m_Bounds.Height - percent), m_Bounds.Width, percent);
			}
						
			UiStyleHelper.CovertToVertCoords(barBounds, view.WindowSize, view.PixelSize, out x, out y, out w, out h);

			Color4 backColor = UiStyleHelper.GetControlBackColor(this, DisplayMode.Hovering);

			WriteTrianglesForBounds(x, y, w, h, ZIndexForOver_Float, backColor, TriangleVerts, ref TriangleVertsCount, TriangleIndices, ref TriangleIndicesCount);
		}

		#endregion
	}
}
