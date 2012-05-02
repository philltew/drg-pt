using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RugTech1.Framework.Objects.UI
{
	public interface IDynamicUiControl
	{
		bool IsVisible { get; set; }

		bool IsParentVisible { get; }

		void GetTotalDynamicElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices);

		void WriteDynamicElements(View3D view, 
									SlimDX.DataStream LineVerts, ref int LineVertsCount, 
									SlimDX.DataStream LinesIndices, ref int LinesIndicesCount, 
									SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, 
									SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount);
	}
}
