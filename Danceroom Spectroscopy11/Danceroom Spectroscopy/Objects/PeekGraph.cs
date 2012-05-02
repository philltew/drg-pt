using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.UI.Dynamic;
using RugTech1.Framework.Objects.UI;
using RugTech1.Framework.Objects;
using SlimDX;
using RugTech1.Framework.Data;

namespace DS.Objects
{
	class PeakGraph : Graph
	{
		private int[] m_PeakLocations;
		private int m_PeakCount = 0;
		private int m_MaxPeakCount = 0;
		private bool m_ShowPeaks = false;
		private Color4 m_PeakLineColor;

		public int MaxPeakCount { get { return m_MaxPeakCount; } }

		public int PeakCount
		{
			get { return m_PeakCount; }
			set
			{
				m_PeakCount = value;

				if (m_PeakCount > m_MaxPeakCount)
				{
					m_PeakCount = m_MaxPeakCount;
				}
			}
		}

		public bool ShowPeaks { get { return m_ShowPeaks; } set { m_ShowPeaks = value; } }

		public Color4 PeakLineColor { get { return m_PeakLineColor; } set { m_PeakLineColor = value; } }

		public int[] PeakLocations { get { return m_PeakLocations; } } 

		public PeakGraph(int peekCount, int numberOfDataPoints)
			: base(numberOfDataPoints)
		{
			m_MaxPeakCount = peekCount;
			m_PeakCount = 0; 
			m_PeakLocations = new int[m_MaxPeakCount]; 
		}

		public override void GetTotalDynamicElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			base.GetTotalDynamicElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);

			LineVerts += m_MaxPeakCount * 2;
			LinesIndices += m_MaxPeakCount * 2;
		}

		public override void WriteDynamicElements(View3D view, SlimDX.DataStream LineVerts, ref int LineVertsCount, SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			base.WriteDynamicElements(view, LineVerts, ref LineVertsCount, LinesIndices, ref LinesIndicesCount, TriangleVerts, ref TriangleVertsCount, TriangleIndices, ref TriangleIndicesCount); 

			float x, y, w, h;

			if (IsNested == true)
			{
				UiStyleHelper.CovertToVertCoords(Parent.Bounds, view.WindowSize, view.PixelSize, out x, out y, out w, out h);
			}
			else
			{
				UiStyleHelper.CovertToVertCoords(Bounds, view.WindowSize, view.PixelSize, out x, out y, out w, out h);
			}

			int i = LineVertsCount;
			float xInc = w / Values.Length;
			float xOffset;

			//foreach (int index in m_PeakLocations)

			for (int j = 0; j < m_PeakCount; j++)
			{
				int index = m_PeakLocations[j]; 

				if (index <= 0 || index >= Values.Length)
				{
					continue;
				}

				xOffset = x + (index * xInc);

				LineVerts.WriteRange(new UIVertex[] { 
							new UIVertex() { Color = m_PeakLineColor, Position = new Vector3(xOffset, y, ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) },
							new UIVertex() { Color = m_PeakLineColor, Position = new Vector3(xOffset, y + h, ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) }
						});

				LinesIndices.WriteRange(new int[] { 
							i + 0, i + 1, 
						});

				LineVertsCount += 2;
				LinesIndicesCount += 2;

				i += 2;
			}
		}
	}
}
