using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticles3
{
	public static class MathHelper
	{
		public static double Pow2(double value)
		{
			return value * value; 
		}

		public static double Pow7(double value)
		{
			return
				value * // 1 
				value * // 2
				value * // 3
				value * // 4
				value * // 5
				value * // 6
				value;  // 7
		}

		public static double Pow13(double value)
		{
			return
				value * // 1 
				value * // 2
				value * // 3
				value * // 4
				value * // 5
				value * // 6
				value * // 7
				value * // 8
				value * // 9
				value * // 10
				value * // 11
				value * // 12
				value;  // 13
		}
	}
}
