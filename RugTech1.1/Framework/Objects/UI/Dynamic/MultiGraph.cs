using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using RugTech1.Framework.Data;
using SlimDX;

namespace RugTech1.Framework.Objects.UI.Dynamic
{
	public class MultiGraph : UiControlBase, IUiControlContainer
	{
		public readonly UiControlCollection<MultiGraph, Graph> Graphs;

		public MultiGraph() 
		{
			Graphs = new UiControlCollection<MultiGraph, Graph>(this); 
		}

		public override bool BeginPick(SlimDX.Vector2 mousePos, out UiControl control)
		{
			control = null; 
			
			return false;
		}

		public override void EndPick(SlimDX.Vector2 mousePos, PickType pickType, UiControl control) { }

		public override void GetTotalElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices)
		{
			base.GetTotalElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);

			foreach (Graph graph in Graphs)
			{
				graph.GetTotalElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);
			}
		}

		public override void WriteVisibleElements(View3D view, RectangleF ClientBounds, ref RectangleF RemainingBounds, SlimDX.DataStream LineVerts, ref int LineVertsCount, SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			base.WriteVisibleElements(view, ClientBounds, ref RemainingBounds,
										LineVerts, ref LineVertsCount,
										LinesIndices, ref LinesIndicesCount,
										TriangleVerts, ref TriangleVertsCount,
										TriangleIndices, ref TriangleIndicesCount);

			RectangleF innerBounds = Bounds;

			foreach (Graph graph in Graphs)
			{
				if (graph.IsVisible == false)
				{
					continue;
				}

				graph.WriteVisibleElements(view, ClientBounds, ref innerBounds,
											LineVerts, ref LineVertsCount,
											LinesIndices, ref LinesIndicesCount,
											TriangleVerts, ref TriangleVertsCount,
											TriangleIndices, ref TriangleIndicesCount);
			}
		}

		#region IUiControlContainer Members

		public void AttachDynamicControls()
		{
			Graphs.AttachDynamicControls();
		}

		public void DetachDynamicControls()
		{
			Graphs.DetachDynamicControls();
		}

		#endregion
	}
}
