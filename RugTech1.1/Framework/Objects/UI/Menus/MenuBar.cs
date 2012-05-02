using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RugTech1.Framework.Objects.UI.Menus
{
	public class MenuBar : UiControlBase, IUiControlContainer
	{
		public readonly UiControlCollection<MenuBar, MenuBarItem> Items;

		public MenuBar()
		{
			Items = new UiControlCollection<MenuBar, MenuBarItem>(this);
		}

		public MenuBarItem AddItem(string text)
		{
			MenuBarItem item = new MenuBarItem();

			item.Text = text;
			item.Size = this.Size;
			item.RelitiveZIndex = Items.Count + 1;
			item.Location = new Point(0, 0);
			item.IsVisible = true;
			item.Docking = System.Windows.Forms.DockStyle.Left;
			item.Parent = this;

			Items.Add(item);

			return item;
		}

		public override bool BeginPick(SlimDX.Vector2 mousePos, out UiControl control)
		{
			control = null;

			foreach (MenuBarItem item in Items)
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

			return false; 
		}

		public override void EndPick(SlimDX.Vector2 mousePos, PickType pickType, UiControl control) { }

		public override void GetTotalElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			base.GetTotalElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);

			int subLineVerts = 0;
			int subLinesIndices = 0;
			int subTriangleVerts = 0;
			int subTriangleIndices = 0;

			foreach (MenuBarItem item in Items)
			{
				item.GetTotalElementCounts(ref subLineVerts, ref subLinesIndices, ref subTriangleVerts, ref subTriangleIndices);
			}

			LineVerts += subLineVerts;
			LinesIndices += subLinesIndices;
			TriangleVerts += subTriangleVerts;
			TriangleIndices += subTriangleIndices;
		}

		public override void WriteVisibleElements(View3D view, RectangleF ClientBounds, ref RectangleF RemainingBounds, SlimDX.DataStream LineVerts, ref int LineVertsCount, SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			base.WriteVisibleElements(view, ClientBounds,ref RemainingBounds, LineVerts, ref LineVertsCount, LinesIndices, ref LinesIndicesCount, TriangleVerts, ref TriangleVertsCount, TriangleIndices, ref TriangleIndicesCount);

			RectangleF inner = Bounds; 
			foreach (MenuBarItem item in Items)
			{
				item.WriteVisibleElements(view, ClientBounds, ref inner, LineVerts, ref LineVertsCount, LinesIndices, ref LinesIndicesCount, TriangleVerts, ref TriangleVertsCount, TriangleIndices, ref TriangleIndicesCount);
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
