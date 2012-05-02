using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using RugTech1.Framework.Data;
using RugTech1.Framework.Objects.Text;
using SlimDX;

namespace RugTech1.Framework.Objects.UI.Controls
{
	public class Panel : UiControlBase, IUiControlContainer
	{
		public readonly UiControlCollection<Panel, UiControl> Controls;

		public Panel()
		{
			Controls = new UiControlCollection<Panel, UiControl>(this);
		}

		public override bool BeginPick(Vector2 mousePos, out UiControl control)
		{
			foreach (UiControl ctrl in Controls)
			{
				if (ctrl.BeginPick(mousePos, out control))
				{
					return true; 
				}
			}

			control = null; 
			
			return false; 
		}

		public override void EndPick(Vector2 mousePos, PickType pickType, UiControl control) { }

		public override void GetTotalElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			base.GetTotalElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);

			foreach (UiControl ctrl in Controls)
			{
				ctrl.GetTotalElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);
			}
		}

		public override void WriteVisibleElements(View3D view,
													RectangleF ClientBounds, ref RectangleF RemainingBounds,
													SlimDX.DataStream LineVerts, ref int LineVertsCount, 
													SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, 
													SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, 
													SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			base.WriteVisibleElements(view, ClientBounds, ref RemainingBounds,
									LineVerts, ref LineVertsCount, 
									LinesIndices, ref LinesIndicesCount, 
									TriangleVerts, ref TriangleVertsCount, 
									TriangleIndices, ref TriangleIndicesCount);

			RectangleF innerBounds = Bounds; 

			foreach (UiControl ctrl in Controls)
			{
				if (ctrl.IsVisible == false)
				{
					continue; 
				}

				ctrl.WriteVisibleElements(view, ClientBounds, ref innerBounds,
											LineVerts, ref LineVertsCount,
											LinesIndices, ref LinesIndicesCount,
											TriangleVerts, ref TriangleVertsCount,
											TriangleIndices, ref TriangleIndicesCount);
			}
		}

		#region IUiControlContainer Members

		public void AttachDynamicControls()
		{
			Controls.AttachDynamicControls();
		}

		public void DetachDynamicControls()
		{
			Controls.DetachDynamicControls();
		}

		#endregion
	}
}