using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticles3
{
	public struct DPVector : IEquatable<DPVector>
	{
		public double X;
		public double Y;

		public DPVector(double value)
		{
			X = value;
			Y = value;
		}

		public DPVector(double x, double y)
		{
			X = x;
			Y = y;
		}

		public bool IsNanOrInfinate
		{
			get
			{
				return double.IsNaN(X) || double.IsInfinity(X) || double.IsNaN(Y) || double.IsInfinity(Y);
			}
		}

		#region IEquatable<DPVector> Members

		public bool Equals(DPVector other)
		{
			return this == other;
		}

		#endregion	

		#region To String

		public override string ToString()
		{
			return string.Format("{0}, {1}", X, Y);
		}

		#endregion

		#region Operators
		
		public static DPVector operator -(DPVector value)
		{
			return new DPVector(-value.X, -value.Y);
		}

		public static DPVector operator -(DPVector left, DPVector right)
		{
			return new DPVector(left.X - right.X, left.Y - right.Y);
		}

		public static bool operator !=(DPVector left, DPVector right)
		{
			return left.X != right.X || left.Y != right.Y;
		}

		public static DPVector operator *(double scale, DPVector vector)
		{
			return new DPVector(vector.X * scale, vector.Y * scale);
		}

		public static DPVector operator *(DPVector vector, double scale)
		{
			return new DPVector(vector.X * scale, vector.Y * scale);
		}

		public static DPVector operator /(DPVector vector, double scale)
		{
			return new DPVector(vector.X / scale, vector.Y / scale);
		}

		public static DPVector operator +(DPVector left, DPVector right)
		{
			return new DPVector(left.X + right.X, left.Y + right.Y);
		}

		public static bool operator ==(DPVector left, DPVector right)
		{
			return left.X == right.X && left.Y == right.Y;
		}

		public static double SeperationSquared(DPVector left, DPVector right)
		{
			return Math.Pow(left.X - right.X, 2.0) + Math.Pow(left.Y - right.Y, 2.0);
		}

		public static double Seperation(DPVector left, DPVector right)
		{
			return Math.Sqrt(Math.Pow(left.X - right.X, 2.0) + Math.Pow(left.Y - right.Y, 2.0));
		}

		#endregion
	}
}
