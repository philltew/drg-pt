using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SlimDX;

namespace RugTech1.Framework.Objects.UI.Dynamic
{
	public delegate void SliderValueChangedEvent(Slider sender, float value); 

	public class Slider : UiControlBase, IDynamicUiControl
	{
		private float m_MinValue = 0f;
		private float m_MaxValue = 100f;
		private float m_Value = 0f;

		private BarAlignment m_BarAlignment = BarAlignment.Horizontal;
		private bool m_InvertHighlight = false; 

		public float MinValue { get { return m_MinValue; } set { m_MinValue = value; } }
		public float MaxValue { get { return m_MaxValue; } set { m_MaxValue = value; } }

		public float Value
		{
			get { return m_Value; } 
			set 
			{
				if (m_Value != value)
				{
					m_Value = value;
					OnValueChanged(); 
				}
			} 
		}

		public BarAlignment BarAlignment { get { return m_BarAlignment; } set { m_BarAlignment = value; } }

		public bool InvertHighlight { get { return m_InvertHighlight; } set { m_InvertHighlight = value; } }
		

		public event SliderValueChangedEvent ValueChanged;

		public Slider()
		{
			InteractionType = ControlInteractionType.Drag;
			ControlStyle = DisplayMode.Normal; 
		}

		private void OnValueChanged()
		{
			if (ValueChanged != null)
			{
				ValueChanged(this, m_Value);
			}
		}

		public override void DoMouseInteraction(MouseState mouseState, System.Windows.Forms.MouseButtons mouseButtons, Vector2 mousePos, out bool shouldUpdate)
		{
			shouldUpdate = false; 
			switch (mouseState)
			{
				case MouseState.DragStart:
				case MouseState.Dragging:
				case MouseState.DragEnd:
					CalculateValue(mousePos);
					break;
				default:
					base.DoMouseInteraction(mouseState, mouseButtons, mousePos, out shouldUpdate);
					break;
			}			
		}

		private void CalculateValue(Vector2 mousePos)
		{
			float oldValue = m_Value; 

			switch (m_BarAlignment)
			{
				case BarAlignment.Horizontal:
					if (mousePos.X < Bounds.X) m_Value = m_MinValue;
					else if (mousePos.X > Bounds.X + Bounds.Width) m_Value = m_MaxValue;
					else m_Value = m_MinValue + (((m_MaxValue - m_MinValue) / Bounds.Width) * (mousePos.X - Bounds.X)); 
					break;
				case BarAlignment.Vertical:
					if (mousePos.Y < Bounds.Y) m_Value = m_MaxValue;
					else if (mousePos.Y > Bounds.Y + Bounds.Height) m_Value = m_MinValue;
					else m_Value = m_MaxValue - (((m_MaxValue - m_MinValue) / Bounds.Height) * (mousePos.Y - Bounds.Y)); 
					break;
				default:
					break;
			}

			if (oldValue != m_Value)
			{
				OnValueChanged();
			}
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

			float value = m_Value < m_MinValue ? m_MinValue : m_Value > m_MaxValue ? m_MaxValue : m_Value;

			if (m_BarAlignment == Dynamic.BarAlignment.Horizontal)
			{
				percent = (m_Bounds.Width / (m_MaxValue - m_MinValue)) * (value - m_MinValue);

				if (m_InvertHighlight == false)
				{
					barBounds = new RectangleF(m_Bounds.X, m_Bounds.Y, percent, m_Bounds.Height - 1);
				}
				else
				{
					barBounds = new RectangleF(m_Bounds.X + percent, m_Bounds.Y, m_Bounds.Width - percent, m_Bounds.Height - 1);
				}
			}
			else
			{
				percent = (m_Bounds.Height / (m_MaxValue - m_MinValue)) * (value - m_MinValue);

				if (m_InvertHighlight == false)
				{
					barBounds = new RectangleF(m_Bounds.X, m_Bounds.Y + (m_Bounds.Height - percent), m_Bounds.Width, percent);
				}
				else
				{
					barBounds = new RectangleF(m_Bounds.X, m_Bounds.Y, m_Bounds.Width, m_Bounds.Height - percent);
				}
			}

			UiStyleHelper.CovertToVertCoords(barBounds, view.WindowSize, view.PixelSize, out x, out y, out w, out h);
			
			Color4 backColor;

			if (this.IsFocused)
			{
				backColor = UiStyleHelper.GetControlBackColor(this, DisplayMode.Open);
			}
			else
			{
				backColor = UiStyleHelper.GetControlBackColor(this, DisplayMode.Hovering);
			}

			WriteTrianglesForBounds(x, y, w, h, ZIndexForOver_Float, backColor, TriangleVerts, ref TriangleVertsCount, TriangleIndices, ref TriangleIndicesCount);
		}

		#endregion
	}
}
