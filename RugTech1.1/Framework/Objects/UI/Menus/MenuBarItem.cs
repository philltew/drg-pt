using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using RugTech1.Framework.Data;
using RugTech1.Framework.Objects.Text;
using SlimDX;

namespace RugTech1.Framework.Objects.UI.Menus
{
	public class MenuBarItem : UiControlBase, IMenuItem, IUiControlContainer
	{
		private string m_Text = "";
		private bool m_IsOpen = false;
		private FontType m_FontType = FontType.Regular; 

		public bool IsOpen
		{
			get { return m_IsOpen; }
			set { m_IsOpen = value; }
		}

		public FontType FontType { get { return m_FontType; } set { m_FontType = value; } }

		public string Text { get { return m_Text; } set { m_Text = value; } }

		public event EventHandler Click;

		public readonly UiControlCollection<MenuBarItem, MenuItem> Items;

		public MenuBarItem()
		{
			Items = new UiControlCollection<MenuBarItem, MenuItem>(this);
			InteractionType = ControlInteractionType.Click; // | ControlInteractionType.Drag; 
		}

		public override void DoMouseInteraction(MouseState mouseState, System.Windows.Forms.MouseButtons mouseButtons, Vector2 mousePos, out bool shouldUpdate)
		{
			shouldUpdate = false;

			switch (mouseState)
			{
				case MouseState.ClickStart:
					if (Click == null)
					{
						this.IsOpen = true;						
					}
					shouldUpdate = true; 					
					break;
				case MouseState.ClickEnd:
					if (Bounds.Contains(mousePos.X, mousePos.Y))
					{
						if (DoClick() == true)
						{ 
							this.IsOpen = false;
						}
					}
					shouldUpdate = true; 
					break;
				default:
					base.DoMouseInteraction(mouseState, mouseButtons, mousePos, out shouldUpdate);
					break;
			}			
		}

		public bool DoClick()
		{
			if (Click != null)
			{
				Click(this, EventArgs.Empty);

				return true; 
			}
			else
			{
				return false; 
			}
		}

		public MenuItem AddItem(string text)
		{
			MenuItem item = new MenuItem();

			item.Text = text;
			item.Size = this.Size;
			item.RelitiveZIndex = Items.Count + 1;
			item.Location = new Point(0, 0);
			item.IsVisible = true;
			item.Docking = System.Windows.Forms.DockStyle.Top;
			item.Parent = this;

			Items.Add(item);

			return item;
		}

		public override bool BeginPick(SlimDX.Vector2 mousePos, out UiControl control)
		{
			if (base.BeginPick(mousePos, out control))
			{
				return true; 
			}

			if (IsOpen)
			{
				foreach (MenuItem item in Items)
				{
					if (item.IsVisible == false)
					{
						continue;
					}

					if (item.BeginPick(mousePos, out control) == true)
					{
						return true;
					}
				}
			}

			return false; 
		}

		public override void EndPick(SlimDX.Vector2 mousePos, PickType pickType, UiControl control)
		{
			base.EndPick(mousePos, pickType, control);

			switch (pickType)
			{
				case PickType.Focus:
					/* if (DoClick() == false)
					{
						this.IsOpen = true;
					}
					else
					{
						this.IsOpen = false;
					}*/ 
					break;
				case PickType.UnFocus:
					if (control == null || control.IsDescendantOf(this) == false)
					{ 
						this.IsOpen = false;
					}
					break;
				default:
					break;
			}
		}

		public override void GetTotalElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			GetTotalSelfElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);

			GetTotalSubElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);
		}

		public void GetTotalSelfElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			base.GetTotalElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);

			LineVerts += 4;
			LinesIndices += 4;

			int textIndexCount, textTriangleCount;
			TextRenderHelper.GetTotalElementCounts(m_Text, out textIndexCount, out textTriangleCount);

			TriangleVerts += textTriangleCount;
			TriangleIndices += textIndexCount; 
		}

		public void GetTotalSubElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			int subLineVerts = 0;
			int subLinesIndices = 0;
			int subTriangleVerts = 0;
			int subTriangleIndices = 0;

			foreach (MenuItem item in Items)
			{
				int lv = 0, li = 0, tv = 0, ti = 0;

				item.GetTotalSelfElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);
				item.GetTotalSubElementCounts(ref lv, ref li, ref tv, ref ti); 
				
				subLineVerts = lv > subLineVerts ? lv : subLineVerts;
				subLinesIndices = li > subLinesIndices ? li : subLinesIndices;
				subTriangleVerts = tv > subTriangleVerts ? tv : subTriangleVerts;
				subTriangleIndices = ti > subTriangleIndices ? ti : subTriangleIndices;
			}

			LineVerts += subLineVerts;
			LinesIndices += subLinesIndices;
			TriangleVerts += subTriangleVerts;
			TriangleIndices += subTriangleIndices;
		}

		public float MessureTextWidth(View3D view)
		{
			return TextRenderHelper.MessureString(m_Text, m_FontType, view, 1f).Width; 
		}

		public override void WriteVisibleElements(View3D view, RectangleF ClientBounds, ref RectangleF RemainingBounds, SlimDX.DataStream LineVerts, ref int LineVertsCount, SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			float x, y, w, h;

			SizeF stringSize = TextRenderHelper.MessureString(m_Text, m_FontType, view, 1f); 

			Size = new System.Drawing.Size((int)stringSize.Width + 20, Size.Height); 

			m_Bounds = UiStyleHelper.LayoutControlBounds(RemainingBounds, Location, Size, Anchor, Docking, out RemainingBounds);

			m_Bounds = new RectangleF(m_Bounds.X, m_Bounds.Y, m_Bounds.Width, m_Bounds.Height - 1f);

			UiStyleHelper.CovertToVertCoords(m_Bounds, view.WindowSize, view.PixelSize, out x, out y, out w, out h);

			Color4 lineColor = UiStyleHelper.GetControlLineColor(this);

			WriteLinesForBounds(x, y, w, h, lineColor, LineVerts, ref LineVertsCount, LinesIndices, ref LinesIndicesCount);

			Color4 backColor = UiStyleHelper.GetControlBackColor(this, IsOpen ? DisplayMode.Open : DisplayMode.Auto);

			WriteTrianglesForBounds(x, y, w, h, backColor, TriangleVerts, ref TriangleVertsCount, TriangleIndices, ref TriangleIndicesCount);

			Color4 textColor = UiStyleHelper.GetControlTextColor(this, IsOpen ? DisplayMode.Open : DisplayMode.Auto);

			TextRenderHelper.WriteString(view, Bounds,
													m_Text, m_FontType, new Vector3(view.PixelSize.X * 10, -view.PixelSize.Y * (Size.Height - stringSize.Height) * 0.5f, ZIndexForLines_Float), 1f, textColor,
													TriangleVerts, ref TriangleVertsCount,
													TriangleIndices, ref TriangleIndicesCount);

			if (Items.Count > 0 && (IsOpen))
			{
				float maxWidth = m_Bounds.Width;
				foreach (MenuItem item in Items)
				{
					float itemWidth = item.MessureTextWidth(view) + 20;

					if (itemWidth > maxWidth)
					{
						maxWidth = itemWidth;
					}
				}

				float topMostLinesZ = Items[Items.Count - 1].ZIndexForLines_Float; 

				RectangleF ItemsBounds = new RectangleF(m_Bounds.X, m_Bounds.Bottom + 1, maxWidth, ClientBounds.Height - (m_Bounds.Bottom - 1));
				RectangleF MenuBounds = ItemsBounds; 
				
				foreach (MenuItem item in Items)
				{
					item.WriteVisibleElements(view, ClientBounds, ref ItemsBounds, LineVerts, ref LineVertsCount, LinesIndices, ref LinesIndicesCount, TriangleVerts, ref TriangleVertsCount, TriangleIndices, ref TriangleIndicesCount);
				}

				MenuBounds = new RectangleF(MenuBounds.X, MenuBounds.Y, MenuBounds.Width, MenuBounds.Height - ItemsBounds.Height);

				UiStyleHelper.CovertToVertCoords(MenuBounds, view.WindowSize, view.PixelSize, out x, out y, out w, out h);
				 				
				int i = LineVertsCount;

				LineVerts.WriteRange(new UIVertex[] { 
					new UIVertex() { Color = lineColor, Position = new Vector3(x, y, topMostLinesZ), TextureCoords = new Vector2(0, 0) },
					new UIVertex() { Color = lineColor, Position = new Vector3(x + w, y, topMostLinesZ), TextureCoords = new Vector2(0, 0) },
					new UIVertex() { Color = lineColor, Position = new Vector3(x, y + h, topMostLinesZ), TextureCoords = new Vector2(0, 0) },
					new UIVertex() { Color = lineColor, Position = new Vector3(x + w, y + h, topMostLinesZ), TextureCoords = new Vector2(0, 0) }
				});

				LinesIndices.WriteRange(new int[] { 
					i + 0, i + 2, 
					i + 1, i + 3, 					
				});

				LineVertsCount += 4;
				LinesIndicesCount += 4;

			}
		}


		#region IUiControlContainer Members

		public void AttachDynamicControls()
		{
			Items.AttachDynamicControls();
		}

		public void DetachDynamicControls()
		{
			Items.DetachDynamicControls();
		}

		#endregion
	}
}
