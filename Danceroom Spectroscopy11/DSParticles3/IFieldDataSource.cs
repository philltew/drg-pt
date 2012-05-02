using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticles3
{
	public interface IFieldDataSource
	{
		int Width { get; }
		int Height { get; }

		void GetSubsetOfPixelsAlongX(double Xposition, double Yposition, int PixelsEitherSide, ref double[] Xpixels, out int count);
		void GetSubsetOfPixelsAlongY(double Xposition, double Yposition, int PixelsEitherSide, ref double[] Ypixels, out int count);
	}
}
