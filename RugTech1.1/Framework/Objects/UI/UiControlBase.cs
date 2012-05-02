using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using RugTech1.Framework.Data;
using SlimDX;

namespace RugTech1.Framework.Objects.UI
{
	public class UiControlBase : UiControl
	{
		#region Private Members
		
		private bool m_IsVisible = true;
		protected RectangleF m_Bounds;
		private Size m_Size = new Size(100, 100);
		private Point m_Location = new Point(100, 100);

		private System.Windows.Forms.DockStyle m_Docking = System.Windows.Forms.DockStyle.None;
		private System.Windows.Forms.AnchorStyles m_Anchor = System.Windows.Forms.AnchorStyles.None;
		private DisplayMode m_ControlStyle = DisplayMode.Auto;

		private bool m_ShowBackground = true;
		private bool m_ShowBorder = true;

		#endregion

		#region Properties

		public DisplayMode ControlStyle { get { return m_ControlStyle; } protected set { m_ControlStyle = value; } }		
		public System.Windows.Forms.DockStyle Docking { get { return m_Docking; } set { m_Docking = value; } }		
		public System.Windows.Forms.AnchorStyles Anchor { get { return m_Anchor; } set { m_Anchor = value; } }

		public Size Size { get { return m_Size; } set { m_Size = value; } }
		public Point Location { get { return m_Location; } set { m_Location = value; } }

		public override bool IsVisible { get { return m_IsVisible; } set { m_IsVisible = value; } }
		
		public bool IsParentVisible
		{
			get
			{
				UiControl par = Parent;

				while (par != null)
				{
					if (par.IsVisible == false)
					{
						return false;
					}
					par = par.Parent;
				}

				return true;
			}
		}

		public override RectangleF Bounds { get { return m_Bounds; } }

		public bool ShowBackground { get { return m_ShowBackground; } set { m_ShowBackground = value; } }
		public bool ShowBorder { get { return m_ShowBorder; } set { m_ShowBorder = value; } }

		#endregion

		public UiControlBase() { }

		#region Elements
		
		public override void GetTotalElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			if (m_ShowBorder == true)
			{
				LineVerts += 4;
				LinesIndices += 8;
			}

			if (m_ShowBackground == true)
			{
				TriangleVerts += 4;
				TriangleIndices += 6;
			}
		}

		public override void WriteVisibleElements(View3D view,
													RectangleF ClientBounds, ref RectangleF RemainingBounds,
													SlimDX.DataStream LineVerts, ref int LineVertsCount, 
													SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, 
													SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, 
													SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			float x, y, w, h;

			m_Bounds = UiStyleHelper.LayoutControlBounds(RemainingBounds, Location, Size, Anchor, Docking, out RemainingBounds);

			UiStyleHelper.CovertToVertCoords(m_Bounds, view.WindowSize, view.PixelSize, out x, out y, out w, out h);

			if (m_ShowBorder == true)
			{
				Color4 lineColor = UiStyleHelper.GetControlLineColor(this);

				WriteLinesForBounds(x, y, w, h, lineColor, LineVerts, ref LineVertsCount, LinesIndices, ref LinesIndicesCount);
			}

			if (m_ShowBackground == true)
			{
				Color4 backColor = UiStyleHelper.GetControlBackColor(this, m_ControlStyle);

				WriteTrianglesForBounds(x, y, w, h, backColor, TriangleVerts, ref TriangleVertsCount, TriangleIndices, ref TriangleIndicesCount);
			}
		}

		#endregion

		#region Write Helpers

		protected void WriteLinesForBounds(float x, float y, float w, float h, Color4 lineColor, 
											SlimDX.DataStream LineVerts, ref int LineVertsCount, 
											SlimDX.DataStream LinesIndices, ref int LinesIndicesCount)
		{
			int i = LineVertsCount;

			if (Docking == System.Windows.Forms.DockStyle.Top)
			{
				LineVerts.WriteRange(new UIVertex[] { 
					new UIVertex() { Color = lineColor, Position = new Vector3(x, y + h, ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) },
					new UIVertex() { Color = lineColor, Position = new Vector3(x + w, y + h, ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) }
				});

				LinesIndices.WriteRange(new int[] { 
					i + 0, i + 1, 
				});

				LineVertsCount += 2;
				LinesIndicesCount += 2;
			}
			else if (Docking == System.Windows.Forms.DockStyle.Bottom)
			{
				LineVerts.WriteRange(new UIVertex[] { 
					new UIVertex() { Color = lineColor, Position = new Vector3(x, y, ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) },
					new UIVertex() { Color = lineColor, Position = new Vector3(x + w, y, ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) },
				});

				LinesIndices.WriteRange(new int[] { 
					i + 0, i + 1, 
				});

				LineVertsCount += 2;
				LinesIndicesCount += 2;
			}
			else if (Docking == System.Windows.Forms.DockStyle.Left)
			{
				LineVerts.WriteRange(new UIVertex[] { 					
					new UIVertex() { Color = lineColor, Position = new Vector3(x + w, y, ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) },					
					new UIVertex() { Color = lineColor, Position = new Vector3(x + w, y + h, ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) }
				});

				LinesIndices.WriteRange(new int[] { 
					i + 0, i + 1, 
				});

				LineVertsCount += 2;
				LinesIndicesCount += 2;
			}
			else if (Docking == System.Windows.Forms.DockStyle.Right)
			{
				LineVerts.WriteRange(new UIVertex[] { 
					new UIVertex() { Color = lineColor, Position = new Vector3(x, y, ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) },					
					new UIVertex() { Color = lineColor, Position = new Vector3(x, y + h, ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) },					
				});

				LinesIndices.WriteRange(new int[] { 
					i + 0, i + 1, 
				});

				LineVertsCount += 2;
				LinesIndicesCount += 2;
			}
			else if (Docking != System.Windows.Forms.DockStyle.Fill)
			{
				LineVerts.WriteRange(new UIVertex[] { 
					new UIVertex() { Color = lineColor, Position = new Vector3(x, y, ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) },
					new UIVertex() { Color = lineColor, Position = new Vector3(x + w, y, ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) },
					new UIVertex() { Color = lineColor, Position = new Vector3(x, y + h, ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) },
					new UIVertex() { Color = lineColor, Position = new Vector3(x + w, y + h, ZIndexForLines_Float), TextureCoords = new Vector2(0, 0) }
				});

				LinesIndices.WriteRange(new int[] { 
					i + 0, i + 1, 
					i + 1, i + 3, 
					i + 3, i + 2,
 					i + 2, i + 0,
				});

				LineVertsCount += 4;
				LinesIndicesCount += 8;
			}

		}

		protected void WriteTrianglesForBounds(float x, float y, float w, float h, Color4 backColor,
												SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount,
												SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			WriteTrianglesForBounds(x, y, w, h, ZIndex_Float, backColor,
									TriangleVerts, ref TriangleVertsCount,
									TriangleIndices, ref TriangleIndicesCount);
		} 

		protected void WriteTrianglesForBounds(float x, float y, float w, float h, float z, Color4 backColor,
												SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, 
												SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			TriangleVerts.WriteRange(new UIVertex[] { 
				new UIVertex() { Color = backColor, Position = new Vector3(x, y, z), TextureCoords = new Vector2(0, 0) },
				new UIVertex() { Color = backColor, Position = new Vector3(x + w, y, z), TextureCoords = new Vector2(0, 0) },
				new UIVertex() { Color = backColor, Position = new Vector3(x, y + h, z), TextureCoords = new Vector2(0, 0) },
				new UIVertex() { Color = backColor, Position = new Vector3(x + w, y + h, z), TextureCoords = new Vector2(0, 0) }
			});

			int i = TriangleVertsCount;

			TriangleIndices.WriteRange(new int[] { 
				i + 0, i + 1, i + 2,
				i + 1, i + 3, i + 2
			});

			TriangleVertsCount += 4;
			TriangleIndicesCount += 6;
		} 

		#endregion
			
	}
}
