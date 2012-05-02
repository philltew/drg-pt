using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.Text;
using SlimDX;

namespace RugTech1.Framework.Objects.UI.Dynamic
{
	public class TextBox : UiControlBase, IDynamicUiControl
	{
		private int m_MaxLength = 30;
		private int m_Carat = 0; 
		private string m_Text = "";

		private FontType m_FontType = FontType.Small;
		private System.Windows.Forms.Padding m_Padding = new System.Windows.Forms.Padding(4);
		private ContentAlignment m_TextAlign = ContentAlignment.MiddleCenter;		

		public int MaxLength { get { return m_MaxLength; } set { m_MaxLength = value; } }

		public int Carat { get { return m_Carat; } set { m_Carat = value; } }
		
		public string Text { get { return m_Text; } set { m_Text = value; } }

		public FontType FontType { get { return m_FontType; } set { m_FontType = value; } }

		public ContentAlignment TextAlign { get { return m_TextAlign; } set { m_TextAlign = value; } }

		public System.Windows.Forms.Padding Padding { get { return m_Padding; } set { m_Padding = value; } }

		public event EventHandler TextChanged; 

		public TextBox()
		{
			InteractionType = ControlInteractionType.Drag;
			ControlStyle = DisplayMode.Normal; 
		}

		private void OnTextChanged()
		{
			if (TextChanged != null)
			{
				TextChanged(this, EventArgs.Empty); 
			}
		}

		public override void DoMouseInteraction(MouseState mouseState, System.Windows.Forms.MouseButtons mouseButtons, Vector2 mousePos, out bool shouldUpdate)
		{
			shouldUpdate = false;
			base.DoMouseInteraction(mouseState, mouseButtons, mousePos, out shouldUpdate);
			/* 
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
			*/
		}

		public override void DoKeyInteraction(KeyInteractionType keyInteractionType, char @char, System.Windows.Forms.KeyEventArgs eventArgs, out bool shouldUpdate)
		{
			shouldUpdate = false; 
			switch (keyInteractionType)
			{
				case KeyInteractionType.KeyPress:
					if (@char == '\b')
					{
						if (m_Text.Length > 0)
						{
							m_Text = m_Text.Substring(0, m_Text.Length - 1);
							OnTextChanged();
						}
					}
					else
					{
						m_Text += @char;
						OnTextChanged();
					}
					break;
				case KeyInteractionType.KeyUp:					
					break;
				case KeyInteractionType.KeyDown:
					if (eventArgs.KeyCode == System.Windows.Forms.Keys.Delete)
					{

					}
					break;
				default:
					base.DoKeyInteraction(keyInteractionType, @char, eventArgs, out shouldUpdate);
					break;
			}			
		}

		/*
		private void SelectTextValue(Vector2 mousePos)
		{
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
		}
		*/ 

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
			SizeF stringSize = TextRenderHelper.MessureString(m_Text, m_FontType, view, 1f);

			float x, y;
			float px = view.PixelSize.X;
			float py = view.PixelSize.Y;

			PointF textLocation = UiStyleHelper.LayoutTextBounds(Bounds, stringSize, m_TextAlign, Padding);

			UiStyleHelper.CovertToVertCoords_Relitive(textLocation, view.PixelSize, out x, out y);

			Color4 textColor = UiStyleHelper.GetControlTextColor(this, DisplayMode.Normal);

			TextRenderHelper.WriteString(view, Bounds,
											m_Text, m_FontType, new Vector3(x, y, ZIndexForLines_Float), 1f, textColor,
											TriangleVerts, ref TriangleVertsCount,
											TriangleIndices, ref TriangleIndicesCount);
		}

		#endregion
	}
}
