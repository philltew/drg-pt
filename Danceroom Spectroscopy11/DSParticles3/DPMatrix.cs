using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSParticles3
{
	public class GenericMatrix<T>
	{
		private T[,] m_Data;

		public T this[int i, int j]
		{
			get { return m_Data[i, j]; }
			set { m_Data[i, j] = value; }
		}

		public GenericMatrix(T value, int iSize, int jSize)
		{
			m_Data = new T[iSize, jSize];

			for (int i = 0; i < iSize; i++)
			{
				for (int j = 0; j < jSize; j++)
				{
					m_Data[i, j] = value;
				}
			}
		}

		public void SetValueRange(T value, int iSize, int jSize)
		{
			for (int i = 0; i < iSize; i++)
			{
				for (int j = 0; j < jSize; j++)
				{
					m_Data[i, j] = value;
				}
			}
		}
	}

	public class DPMatrix : GenericMatrix<double> 
	{
		public DPMatrix(double value, int iSize, int jSize) 
			: base(value, iSize, jSize)
		{

		}
	}
}
