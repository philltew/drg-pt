using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using RugTech1.Framework.Data;
using SlimDX;

namespace RugTech1.Framework.Objects.UI.Dynamic
{
	public class Graph : UiControlBase, IDynamicUiControl
	{
		#region Private Members

		private float m_MaxValue = 1f;
		private float m_MinValue = 0f;
		private float[] m_Values;
		private int m_NumberOfDataPoints;
		private int m_CurrentIndex = 0;
		private Color4 m_LineColor = new Color4(1f, 1f, 0f, 0f);
		private Color4 m_ZeroLineColor = new Color4(0.5f, 1f, 1f, 1f);
		private Color4 m_MinorTickLineColor = new Color4(0.1f, 1f, 1f, 1f);
		private Color4 m_MajorTickLineColor = new Color4(0.5f, 1f, 1f, 1f);		
		private bool m_FixedRange = false;
		private bool m_Scrolling = true;
		private bool m_ScaleEveryFrame;
		private bool m_IsNested = true;
		private bool m_ShowTicks = false;
		private int m_TickSpace = 10;
		private int m_MajorTicksEvery = 10;
		
		#endregion

		#region Public Properties

		public bool FixedRange { get { return m_FixedRange; } set { m_FixedRange = value; } }
		public float MinValue { get { return m_MinValue; } set { m_MinValue = value; } }
		public float MaxValue { get { return m_MaxValue; } set { m_MaxValue = value; } }

		public float[] Values { get { return m_Values; } }

		public Color4 LineColor { get { return m_LineColor; } set { m_LineColor = value; } }
		public Color4 ZeroLineColor { get { return m_ZeroLineColor; } set { m_ZeroLineColor = value; } }
		public Color4 MinorTickLineColor { get { return m_MinorTickLineColor; } set { m_MinorTickLineColor = value; } }
		public Color4 MajorTickLineColor { get { return m_MajorTickLineColor; } set { m_MajorTickLineColor = value; } }
		//public Color4 TickLineColor { get { return m_TickLineColor; } set { m_TickLineColor = value; } }

		public bool ScaleEveryFrame { get { return m_ScaleEveryFrame; } set { m_ScaleEveryFrame = value; } }

		public bool Scrolling { get { return m_Scrolling; } set { m_Scrolling = value; } }

		public bool IsNested { get { return m_IsNested; } }

		public bool ShowTicks { get { return m_ShowTicks; } set { m_ShowTicks = value; } }

		public int TickSpace { get { return m_TickSpace; } set { m_TickSpace = value; } }

		public int MajorTicksEvery { get { return m_MajorTicksEvery; } set { m_MajorTicksEvery = value; } } 		

		#endregion

		public Graph(int numberOfDataPoints)
		{
			m_NumberOfDataPoints = numberOfDataPoints;
			m_Values = new float[numberOfDataPoints];
		}

		#region Add Value

		public void AddValue(float value)
		{
			if (m_FixedRange == false)
			{
				if (value > m_MaxValue)
				{
					m_MaxValue = value;
				}

				if (value < m_MinValue)
				{
					m_MinValue = value;
				}
			}

			m_Values[m_CurrentIndex] = value;

			m_CurrentIndex += 1;

			if (m_CurrentIndex >= m_NumberOfDataPoints)
			{
				m_CurrentIndex = 0;
			}
		}

		#endregion

		#region Clear

		public void Clear()
		{
			for (int i = 0; i < m_NumberOfDataPoints; i++)
			{
				m_Values[i] = m_MinValue;
			}

			m_CurrentIndex = 0;
		}

		#endregion

		#region Control Picking

		public override bool BeginPick(SlimDX.Vector2 mousePos, out UiControl control)
		{
			control = null;

			return false;
		}

		public override void EndPick(SlimDX.Vector2 mousePos, PickType pickType, UiControl control) { }

		#endregion

		#region Write Elements

		public override void GetTotalElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			m_IsNested = Parent != null && Parent is MultiGraph; 

			if (m_IsNested == false)
			{
				base.GetTotalElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);
			}

			if (m_ShowTicks == true)
			{
				int ticks = (m_Values.Length / m_TickSpace) - 1;

				if (ticks > 0)
				{
					LineVerts += ticks * 2;
					LinesIndices += ticks * 2;
				}
			}
		}

		public override void WriteVisibleElements(View3D view, RectangleF ClientBounds, ref RectangleF RemainingBounds, SlimDX.DataStream LineVerts, ref int LineVertsCount, SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			if (m_IsNested == false)
			{
				base.WriteVisibleElements(view, ClientBounds, ref RemainingBounds,
											LineVerts, ref LineVertsCount,
											LinesIndices, ref LinesIndicesCount,
											TriangleVerts, ref TriangleVertsCount,
											TriangleIndices, ref TriangleIndicesCount);
			}

			if (m_ShowTicks == true)
			{
				int ticks = (m_Values.Length / m_TickSpace) - 1;

				if (ticks > 0)
				{
					float x, y, w, h;

					if (m_IsNested == true)
					{
						UiStyleHelper.CovertToVertCoords(Parent.Bounds, view.WindowSize, view.PixelSize, out x, out y, out w, out h);
					}
					else
					{
						UiStyleHelper.CovertToVertCoords(Bounds, view.WindowSize, view.PixelSize, out x, out y, out w, out h);						
					}

					int i = LineVertsCount;
					float xInc = w / (ticks + 1);
					float xOffset = x + xInc;

					for (int j = 0; j < ticks; j++)
					{						
						Color4 lineColor;
 
						if ((j + 1) % m_MajorTicksEvery == 0)
						{							
							lineColor = m_MajorTickLineColor; 
						}
						else
						{
							lineColor = m_MinorTickLineColor; 
						}

						LineVerts.WriteRange(new UIVertex[] { 
							new UIVertex() { Color = lineColor, Position = new Vector3(xOffset, y, ZIndexForOver_Float), TextureCoords = new Vector2(0, 0) },
							new UIVertex() { Color = lineColor, Position = new Vector3(xOffset, y + h, ZIndexForOver_Float), TextureCoords = new Vector2(0, 0) }
						});

						LinesIndices.WriteRange(new int[] { 
							i + 0, i + 1, 
						});

						LineVertsCount += 2;
						LinesIndicesCount += 2;

						xOffset += xInc;

						i += 2; 
					}
				}
			}
		}

		public virtual void GetTotalDynamicElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			LineVerts += m_NumberOfDataPoints + 1 + 2;
			LinesIndices += (2 * m_NumberOfDataPoints) + 2;
		}

		public virtual void WriteDynamicElements(View3D view, SlimDX.DataStream LineVerts, ref int LineVertsCount, SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			float x, y, w, h;

			if (m_IsNested == true)
			{
				UiStyleHelper.CovertToVertCoords(Parent.Bounds, view.WindowSize, view.PixelSize, out x, out y, out w, out h); 
			}
			else
			{
				UiStyleHelper.CovertToVertCoords(Bounds, view.WindowSize, view.PixelSize, out x, out y, out w, out h);
			}

			if (m_ScaleEveryFrame == true)
			{
				// this rescales the graph to check what should be the max & min
				m_MaxValue = m_Values[0];
				m_MinValue = m_Values[0];

				for (int i = 1; i < m_Values.Count(); ++i)
				{
					if (m_Values[i] > m_MaxValue)
					{
						m_MaxValue = m_Values[i];
					}

					if (m_Values[i] < m_MinValue)
					{
						m_MinValue = m_Values[i];
					}
				}
			}

			if ((m_MaxValue - m_MinValue) == 0.0f)
			{
				m_MaxValue = m_MinValue + 0.1f;
			}

			float xDelta = w / m_NumberOfDataPoints;
			float yDelta = h / (m_MaxValue - m_MinValue);
			float yOff = -m_MinValue;
			float value = 0;

			if (m_MinValue < 0 && m_MaxValue > 0)
			{
				value = 0;

				value = value < m_MinValue ? m_MinValue : value > m_MaxValue ? m_MaxValue : value;

				value += yOff;

				LineVerts.Write(new UIVertex() { Color = m_ZeroLineColor, Position = new Vector3(x, (y + h) - (yDelta * value), ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) });
				LineVerts.Write(new UIVertex() { Color = m_ZeroLineColor, Position = new Vector3(x + w, (y + h) - (yDelta * value), ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) });
				LinesIndices.Write(LineVertsCount++);
				LinesIndices.Write(LineVertsCount++);

				LinesIndicesCount += 2;
			}

			int lv = LineVertsCount;
			int li = LinesIndicesCount;

			if (m_Scrolling == true)
			{
				for (int i = m_CurrentIndex; i < m_NumberOfDataPoints; i++)
				{
					value = m_Values[i];

					value = value < m_MinValue ? m_MinValue : value > m_MaxValue ? m_MaxValue : value;

					value += yOff;
					LineVerts.Write(new UIVertex() { Color = m_LineColor, Position = new Vector3(x, (y + h) - (yDelta * value), ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) });
					LinesIndices.Write(lv++);
					LinesIndices.Write(lv);

					x += xDelta;
				}

				for (int i = 0; i < m_CurrentIndex; i++)
				{
					value = m_Values[i];

					value = value < m_MinValue ? m_MinValue : value > m_MaxValue ? m_MaxValue : value;

					value += yOff;

					LineVerts.Write(new UIVertex() { Color = m_LineColor, Position = new Vector3(x, (y + h) - (yDelta * value), ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) });
					LinesIndices.Write(lv++);
					LinesIndices.Write(lv);

					x += xDelta;
				}
			}
			else
			{
				for (int i = 0; i < m_NumberOfDataPoints; i++)
				{
					value = m_Values[i];

					value = value < m_MinValue ? m_MinValue : value > m_MaxValue ? m_MaxValue : value;

					value += yOff;
					LineVerts.Write(new UIVertex() { Color = m_LineColor, Position = new Vector3(x, (y + h) - (yDelta * value), ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) });
					LinesIndices.Write(lv++);
					LinesIndices.Write(lv);

					x += xDelta;
				}
			}

			LineVerts.Write(new UIVertex() { Color = m_LineColor, Position = new Vector3(x, (y + h) - (yDelta * value), ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) });

			LineVertsCount += m_NumberOfDataPoints + 1;
			LinesIndicesCount += 2 * m_NumberOfDataPoints;
		}

		#endregion
	}
}
