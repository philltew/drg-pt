using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Rug.Cmd;
using RugTech1.Framework.Objects.Text;
using SlimDX;

namespace RugTech1.Framework.Objects.UI.Dynamic
{
	public class ConsoleControl : UiControlBase, IDynamicUiControl 
	{
		private ConsoleBuffer m_ConsoleBuffer;
		private int m_NumberOfLines = 20;
		private int m_NumberOfChars = 80;
		private long m_TopVisibleLine = 0;
		private long m_LastBottomLine = 0;
		private string m_EmptyLine;
		private System.Windows.Forms.Padding m_Padding = new System.Windows.Forms.Padding(5, 2, 5, 2);
		private Color4[] m_ColorLookup;

		public System.Windows.Forms.Padding Padding { get { return m_Padding; } set { m_Padding = value; } }
		
		public ConsoleBuffer ConsoleBuffer 
		{ 
			get 
			{ 
				return m_ConsoleBuffer; 
			} 
			set 
			{
				if (m_ConsoleBuffer != null)
				{
					m_ConsoleBuffer.ContentsChanged -= new EventHandler(m_ConsoleBuffer_ContentsChanged);
				}

				m_ConsoleBuffer = value;

				if (m_ConsoleBuffer != null)
				{
					m_ConsoleBuffer.ContentsChanged +=new EventHandler(m_ConsoleBuffer_ContentsChanged);

					m_ConsoleBuffer.BufferWidth = m_NumberOfChars;

					m_EmptyLine = new string(' ', m_NumberOfChars);
				}
			} 
		}

		public int NumberOfLines { get { return m_NumberOfLines; } set { m_NumberOfLines = value; } }
		public int NumberOfChars 
		{ 
			get { return m_NumberOfChars; } 
			set 
			{ 
				m_NumberOfChars = value;

				if (m_ConsoleBuffer != null)
				{
					m_ConsoleBuffer.BufferWidth = m_NumberOfChars; 
				}
			} 
		} 

		public ConsoleControl() 
		{
			this.ShowBackground = true;
			this.ShowBorder = true;
			this.InteractionType = ControlInteractionType.None;

			m_ColorLookup = new Color4[(int)ConsoleColorExt.Inhreit + 1];
			m_ColorLookup[(int)ConsoleColorExt.Black] = new Color4(Color.Black);
			m_ColorLookup[(int)ConsoleColorExt.DarkBlue] = new Color4(Color.DarkBlue);
			m_ColorLookup[(int)ConsoleColorExt.DarkGreen] = new Color4(Color.DarkGreen);
			m_ColorLookup[(int)ConsoleColorExt.DarkCyan] = new Color4(Color.DarkCyan);
			m_ColorLookup[(int)ConsoleColorExt.DarkRed] = new Color4(Color.DarkRed);
			m_ColorLookup[(int)ConsoleColorExt.DarkMagenta] = new Color4(Color.DarkMagenta);
			m_ColorLookup[(int)ConsoleColorExt.DarkYellow] = new Color4(Color.Olive);
			m_ColorLookup[(int)ConsoleColorExt.Gray] = new Color4(Color.Gray);
			m_ColorLookup[(int)ConsoleColorExt.DarkGray] = new Color4(Color.DarkGray);
			m_ColorLookup[(int)ConsoleColorExt.Blue] = new Color4(Color.Blue);
			m_ColorLookup[(int)ConsoleColorExt.Green] = new Color4(Color.Green);
			m_ColorLookup[(int)ConsoleColorExt.Cyan] = new Color4(Color.Cyan);
			m_ColorLookup[(int)ConsoleColorExt.Red] = new Color4(Color.Red);
			m_ColorLookup[(int)ConsoleColorExt.Magenta] = new Color4(Color.Magenta);
			m_ColorLookup[(int)ConsoleColorExt.Yellow] = new Color4(Color.Yellow);
			m_ColorLookup[(int)ConsoleColorExt.White] = new Color4(Color.White);
			m_ColorLookup[(int)ConsoleColorExt.Inhreit] = new Color4(Color.White);
		}

		public override void DoKeyInteraction(KeyInteractionType keyInteractionType, char @char, System.Windows.Forms.KeyEventArgs eventArgs, out bool shouldUpdate)
		{
			base.DoKeyInteraction(keyInteractionType, @char, eventArgs, out shouldUpdate);

			if (keyInteractionType == KeyInteractionType.KeyDown)
			{
				if (eventArgs.KeyData == System.Windows.Forms.Keys.PageUp)
				{
					m_TopVisibleLine -= (m_NumberOfLines / 2);
					
					if (m_TopVisibleLine < m_ConsoleBuffer.TopLine)
					{
						m_TopVisibleLine = m_ConsoleBuffer.TopLine;
					}
				} 
				else if (eventArgs.KeyData == System.Windows.Forms.Keys.PageDown)
				{
					m_TopVisibleLine += (m_NumberOfLines / 2);

					if (m_TopVisibleLine > m_ConsoleBuffer.BottomLine - 1)
					{
						m_TopVisibleLine = m_ConsoleBuffer.BottomLine - 1;
					}
				}
				else if (eventArgs.KeyData == System.Windows.Forms.Keys.Up)
				{
					m_TopVisibleLine -= 1;

					if (m_TopVisibleLine < m_ConsoleBuffer.TopLine)
					{
						m_TopVisibleLine = m_ConsoleBuffer.TopLine;
					}
				}
				else if (eventArgs.KeyData == System.Windows.Forms.Keys.Down)
				{
					m_TopVisibleLine += 1;

					if (m_TopVisibleLine > m_ConsoleBuffer.BottomLine - 1)
					{
						m_TopVisibleLine = m_ConsoleBuffer.BottomLine - 1;
					}
				}
			}
		}

		public override void WriteVisibleElements(View3D view, RectangleF ClientBounds, ref RectangleF RemainingBounds, SlimDX.DataStream LineVerts, ref int LineVertsCount, SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			float x, y, w, h;

			SizeF lineSize = TextRenderHelper.MessureString(m_EmptyLine, FontType.Monospaced, view, 1f);

			Size = new System.Drawing.Size((int)lineSize.Width + m_Padding.Horizontal, (int)(lineSize.Height * m_NumberOfLines) + m_Padding.Vertical);

			m_Bounds = UiStyleHelper.LayoutControlBounds(RemainingBounds, Location, Size, Anchor, Docking, out RemainingBounds);

			m_Bounds = new RectangleF(m_Bounds.X, m_Bounds.Y, m_Bounds.Width, m_Bounds.Height - 1f);

			UiStyleHelper.CovertToVertCoords(m_Bounds, view.WindowSize, view.PixelSize, out x, out y, out w, out h);

			Color4 lineColor = UiStyleHelper.GetControlLineColor(this);

			WriteLinesForBounds(x, y, w, h, lineColor, LineVerts, ref LineVertsCount, LinesIndices, ref LinesIndicesCount);

			Color4 backColor = UiStyleHelper.GetControlBackColor(this, DisplayMode.Normal);

			WriteTrianglesForBounds(x, y, w, h, backColor, TriangleVerts, ref TriangleVertsCount, TriangleIndices, ref TriangleIndicesCount);
		}

		#region IDynamicUiControl Members

		public void GetTotalDynamicElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			int textIndexCount, textTriangleCount;
			
			int totalChars = m_NumberOfChars * m_NumberOfLines;

			TextRenderHelper.GetTotalElementCounts(totalChars, out textIndexCount, out textTriangleCount);

			TriangleVerts += textTriangleCount;
			TriangleIndices += textIndexCount;
		}

		public void WriteDynamicElements(View3D view, SlimDX.DataStream LineVerts, ref int LineVertsCount, SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			SizeF lineSize = TextRenderHelper.MessureString(m_EmptyLine, FontType.Monospaced, view, 1f);

			float x, y;
			float px = view.PixelSize.X;
			float py = view.PixelSize.Y;

			PointF textLocation = UiStyleHelper.LayoutTextBounds(Bounds, lineSize, ContentAlignment.TopLeft, m_Padding);

			long top = m_TopVisibleLine;

			if (top < m_ConsoleBuffer.TopLine)
			{
				top = m_ConsoleBuffer.TopLine; 
			}

			List<string> lines = m_ConsoleBuffer.GetLines(top, m_NumberOfLines);

			foreach (string line in lines)
			{
				UiStyleHelper.CovertToVertCoords_Relitive(textLocation, view.PixelSize, out x, out y);

				Color4 textColor = UiStyleHelper.GetControlTextColor(this, DisplayMode.Normal);

				TextRenderHelper.WriteInterpreted(view,
													Bounds, lineSize, m_ConsoleBuffer.BufferWidth,
													line, FontType.Monospaced, new Vector3(x, y, ZIndexForLines_Float), 1f,
													ConsoleColorExt.White, m_ColorLookup,
													TriangleVerts, ref TriangleVertsCount,
													TriangleIndices, ref TriangleIndicesCount);

				textLocation.Y = textLocation.Y + lineSize.Height; 
			} 
		}

		#endregion

		void m_ConsoleBuffer_ContentsChanged(object sender, EventArgs e)
		{
			long newTop = m_ConsoleBuffer.BottomLine - (m_NumberOfLines - 1);

			if (newTop < m_ConsoleBuffer.TopLine)
			{
				newTop = m_ConsoleBuffer.TopLine;
			}

			if (m_TopVisibleLine == m_LastBottomLine - (m_NumberOfLines - 1))
			{
				m_TopVisibleLine = newTop; 
			}

			if (m_TopVisibleLine < m_ConsoleBuffer.TopLine)
			{
				m_TopVisibleLine = m_ConsoleBuffer.TopLine;
			}

			m_LastBottomLine = m_ConsoleBuffer.BottomLine; 
		}
	}
}
