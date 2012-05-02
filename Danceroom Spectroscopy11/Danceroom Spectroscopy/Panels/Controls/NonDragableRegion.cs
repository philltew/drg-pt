using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using RugTech1.Framework.Data;
using RugTech1.Framework.Objects.Text;
using SlimDX;
using RugTech1.Framework.Objects.UI;
using RugTech1.Framework.Objects;

namespace DS.Panels
{
	public class NonDragableRegion : UiControlBase
	{
		public event EventHandler MoveEnded;
		public event EventHandler Moving;

		public NonDragableRegion()
		{
			InteractionType = ControlInteractionType.NonSelectable;

			this.ShowBackground = false;
			this.ShowBorder = true; 
		}

		public override void DoMouseInteraction(MouseState mouseState, System.Windows.Forms.MouseButtons mouseButtons, Vector2 mousePos, out bool shouldUpdate)
		{
			shouldUpdate = false;		
		}

		public override void EndPick(SlimDX.Vector2 mousePos, PickType pickType, UiControl control)
		{

		}

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
	}
}
