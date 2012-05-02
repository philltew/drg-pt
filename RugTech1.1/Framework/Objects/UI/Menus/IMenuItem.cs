using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RugTech1.Framework.Objects.UI.Menus
{
	public interface IMenuItem
	{
		bool IsOpen { get; set; }
		event EventHandler Click;
		bool DoClick(); 

		void GetTotalSelfElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices);
		void GetTotalSubElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices);

		float MessureTextWidth(View3D view); 
	}
}
