/*
* Copyright (c) 2007-2012 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

struct ParticleStruct
{
	float2 Position;
	float2 Force;
	float4 Properties;
};

// Indexing in constant buffers works on 16-byte boundaries --
// even though the input data is a float[16], declaring the
// cbuffer as such will misalign the data. 16 individual floats
// would work properly, but such a structure cannot be indexed
// at runtime. But an array of four float4 objects allows runtime
// indexing and satisfies the alignment issues.
cbuffer Constants {
	float4 ParticleProperties[16];
}

Texture2D<float> FieldImage; 
StructuredBuffer<float> Gaussian; 
RWStructuredBuffer<ParticleStruct> Particles;

#define NumPoints 3

float LinearRegression(float4 yvalues)
{
	int i;
	float xsum = 0, ysum = 0;
	float XminusXavg = 0.0, YminusYavg = 0.0;
	float xavg = 0.0, yavg = 0.0, npoints = 0.0;
	float slope = 0.0, numerator = 0.0, denominator = 0.0;

	[unroll]
	for (i = 0; i < NumPoints; ++i)
	{
		//    cout << i << " " << yvalues[i] << endl;
		xsum += i;
		ysum += yvalues[i];
	}

	xavg = xsum / NumPoints;
	yavg = ysum / NumPoints;

	[unroll]
	for (i = 0; i < NumPoints; ++i)
	{
		XminusXavg = float(i) - xavg;
		YminusYavg = yvalues[i] - yavg;
		numerator += XminusXavg * YminusYavg;
		denominator += XminusXavg * XminusXavg;
	}

	slope = numerator / denominator;

	return slope;
}

float GaussianSmoothedSlope(float PositionX) // , float[] pixelValues, int number)
{
	int i, j, ctr;
	double gradient = 0.0, sum = 0.0;
	float4 gaussianSlopeTemp = float4(0, 0, 0, 0); 

	// int pos = (int)Position;

	[unroll]
	for (i = 0; i < NumPoints; ++i)
	{
		sum = 0.0;

		//for (j = 0; j < nGaussPoints; ++j)
		//{
		//	sum += gaussian[j] * pixelValues[i + j];
		//}

		//if (float.IsNaN(sum))
		//{
		//	sum = 0.0;
		//}

		gaussianSlopeTemp[i] = sum;
	}

	gradient = LinearRegression(gaussianSlopeTemp);

	return gradient;
}

// Utilize 16 threads per group. The thread ID will be used
// to index into the constant buffer.
[numthreads(16, 1, 1)]
void main(uint3 threadId : SV_DispatchThreadID)
{
	// For each particle 
	float2 position = Particles[threadId.x].Position; 	
	float particleType = Particles[threadId.x].Properties[0]; 
	float gradiantScaleFactor = ParticleProperties[(int)particleType][0];
	
	float2 force = float2(0,0); 
	
	// for a subset along x
	// 

	force.x = 42; 

	// for a subset along y
	// 

	force.x = 24;

	// The complex indexing is required due to the inability to
	// simply declare gInput as a float[16].
	//gOutput[threadId.x] = gInput[threadId.x / 4][threadId.x % 4]; // 2 * gInput[threadId.x / 4][threadId.x % 4];
	//gOutput[threadId.x] = 2 * gInput[threadId.x / 4][threadId.x % 4];

	Particles[threadId.x].Force = force;
}

/*

		#region Gaussian Smoothing

		/// <summary>
		/// function for doing Gaussian smoothing and then simple 3 point linear regression to find the slope of the 
		/// middle point in a data vector
		/// </summary>
		/// <param name="Position"></param>
		/// <param name="pixelValues"></param>
		/// <param name="number"></param>
		/// <returns></returns>
		private double GaussianSmoothedSlope(double Position, double[] pixelValues, int number)
		{
			int i, j, ctr;
			double gradient = 0.0, sum = 0.0;

			int pos = (int)Position;

			for (i = 0; i < 3; ++i)
			{
				sum = 0.0;

				for (j = 0; j < nGaussPoints; ++j)
				{
					sum += gaussian[j] * pixelValues[i + j];
				}

				if (double.IsNaN(sum))
				{
					sum = 0.0;
				}

				m_GaussianSmoothedSlope_Temp[i] = sum;
			}

			gradient = LinearRegression(m_GaussianSmoothedSlope_Temp, 3);

			return gradient;
		}

		/// <summary>
		/// the function below uses a simple linear regression formula to get the slope of a vector of n points
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		private double LinearRegression(double[] yvalues, int count)
		{
			int i, xsum = 0, ysum = 0;
			double XminusXavg = 0.0, YminusYavg = 0.0;
			double xavg = 0.0, yavg = 0.0, npoints = 0.0;
			double slope = 0.0, numerator = 0.0, denominator = 0.0;

			npoints = (double)count;

			for (i = 0; i < count; ++i)
			{
				//    cout << i << " " << yvalues[i] << endl;
				xsum += i;
				ysum += (int)yvalues[i];
			}

			xavg = (double)xsum / npoints;
			yavg = (double)ysum / npoints;

			for (i = 0; i < npoints; ++i)
			{
				XminusXavg = (double)i - xavg;
				YminusYavg = (double)yvalues[i] - yavg;
				numerator += XminusXavg * YminusYavg;
#if !UsePOW
				denominator += Math.Pow(XminusXavg, 2.0);
#else
				denominator += MathHelper.Pow2(XminusXavg);
#endif
			}

			slope = numerator / denominator;

			return slope;
		}

		#endregion
*/
