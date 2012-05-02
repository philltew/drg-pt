using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticles
{
	public class BoolMatrix : GenericMatrix<bool>	
	{
		public BoolMatrix(bool value, int iSize, int jSize)
			: base(value, iSize, jSize)
		{

		}
	}
}
