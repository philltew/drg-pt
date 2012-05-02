using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticles3
{
	public struct GridSector
	{
		public int X;
		public int Y; 

		public void GetSector(DPVector position)
		{
			int xpos = (int)position.X;
			int ypos = (int)position.Y;

			X = xpos >> 6; // divide by 64
			Y = ypos >> 6; // divide by 64
		}

		public bool IsInJoiningSectors(GridSector other)
		{
			if (((X + 1 >= other.X) &&
				 (X - 1 <= other.X)) &&
				((Y + 1 >= other.Y) &&
				 (Y - 1 <= other.Y)))
			{
				return true; 
			}
			else
			{
				return false; 
			}
		}
	}
}
