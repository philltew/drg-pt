using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Effects;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace RugTech1.Framework.Objects.Simple
{
	public class BloomImposter : IResourceManager
	{
		protected static BloomEffect Effect;

		private Texture2D PartialBlur;

		public RenderTargetView PartialBlurRTV;
		public ShaderResourceView PartialBlurSRV;
		public BloomQuad BlurQuadCoordsH;
		public BloomQuad BlurQuadCoordsV;
		
		public float[] GaussWeights;

		private int m_BufferWidth;
		private int m_BufferHeight;
		private Format m_BufferFormat;

		private bool m_Disposed = true; 

		public BloomImposter(int width, int height, Format bufferFormat)
		{
			if (Effect == null)
			{
				Effect = SharedEffects.Effects["Bloom"] as BloomEffect;
			}

			GaussWeights = new float[BloomEffect.GAUSSIAN_MAX_SAMPLES];

			m_BufferWidth = width;
			m_BufferHeight = height;
			m_BufferFormat = bufferFormat;

			// Create the fullscreen quad data
			CalculateOffsetsAndQuadCoords();
		}

		#region Init

		//-----------------------------------------------------------------------------
		// Name: CalculateOffsetsAndQuadCoords
		// Desc: Everytime we resize the screen we will recalculate an array of
		// vertices and texture coordinates to be use to render the full screen 
		// primitive for the different postprocessing effects. Note that different 
		// effects use a different set of values! 
		//-----------------------------------------------------------------------------
		void CalculateOffsetsAndQuadCoords()
		{
			Vector2[] sampleOffsets_blurV = new Vector2[BloomEffect.GAUSSIAN_MAX_SAMPLES];
			Vector2[] sampleOffsets_blurH = new Vector2[BloomEffect.GAUSSIAN_MAX_SAMPLES];

			float[] coordOffsets = new float[BloomEffect.GAUSSIAN_MAX_SAMPLES];

			CoordSubRect tempCoords = new CoordSubRect();

			// Blur quad coords.
			tempCoords.Left   = 0.0f;
			tempCoords.Top    = 0.0f;
			tempCoords.Right  = 1.0f;
			tempCoords.Bottom = 1.0f;

			CalculateOffsets_GaussianBilinear((float)m_BufferWidth, coordOffsets, GaussWeights, BloomEffect.GAUSSIAN_MAX_SAMPLES);

			for (int i = 0; i < BloomEffect.GAUSSIAN_MAX_SAMPLES; i++)
			{
				sampleOffsets_blurH[i].X = coordOffsets[i];
				sampleOffsets_blurH[i].Y = 0;

				sampleOffsets_blurV[i].X = 0;
				sampleOffsets_blurV[i].Y = coordOffsets[i] * m_BufferWidth / m_BufferHeight;
			}

			BlurQuadCoordsV = new BloomQuad(m_BufferWidth, m_BufferHeight, tempCoords, sampleOffsets_blurV, BloomEffect.GAUSSIAN_MAX_SAMPLES);
			BlurQuadCoordsH = new BloomQuad(m_BufferWidth, m_BufferHeight, tempCoords, sampleOffsets_blurH, BloomEffect.GAUSSIAN_MAX_SAMPLES);	
		}
		
		//-----------------------------------------------------------------------------
		// Name: CalculateOffsets_GaussianBilinear
		//
		//  We want the general convolution:
		//    a*f(i) + b*f(i+1)
		//  Linear texture filtering gives us:
		//    f(x) = (1-alpha)*f(i) + alpha*f(i+1);
		//  It turns out by using the correct weight and offset we can use a linear lookup to achieve this:
		//    (a+b) * f(i + b/(a+b))
		//  as long as 0 <= b/(a+b) <= 1.
		//
		//  Given a standard deviation, we can calculate the size of the kernel and viceversa.
		//
		//-----------------------------------------------------------------------------
		void CalculateOffsets_GaussianBilinear(float texSize, float[] coordOffset, float[] gaussWeight, int maxSamples )
		{
			int i=0;
			float du = 1.0f / texSize;

			//  store all the intermediate offsets & weights, then compute the bilinear
			//  taps in a second pass
			float[] tmpWeightArray = generateGaussianWeights( maxSamples );

			// Bilinear filtering taps 
			// Ordering is left to right.
			float sScale;
			float sFrac;

			for( i = 0; i < maxSamples; i++ )
			{
				sScale = tmpWeightArray[i*2 + 0] + tmpWeightArray[i*2 + 1];
				sFrac  = tmpWeightArray[i*2 + 1] / sScale;

				coordOffset[i] = ((2.0f*i - maxSamples) + sFrac) * du;
				gaussWeight[i] = sScale;
			}

			//delete []tmpWeightArray;
		}


		//-----------------------------------------------------------------------------
		// Calculate Gaussian weights based on kernel size
		//-----------------------------------------------------------------------------
		// generate array of weights for Gaussian blur
		float[] generateGaussianWeights(int kernelRadius)
		{
			int size = kernelRadius * 2 + 1;

			float x;
			//float s        = floor(kernelRadius / 4.0f);  
			float[] weights = new float[size];

			float sum = 0.0f;
			for (int i = 0; i < size; i++)
			{
				x = (float)(i - kernelRadius);

				// True Gaussian
				// weights[i] = expf(-x*x/(2.0f*s*s)) / (s*sqrtf(2.0f*D3DX_PI));

				// This sum of exps is not really a separable kernel but produces a very interesting star-shaped effect
				weights[i] = (float)(Math.Exp(-0.0625f * x * x) + 2 * Math.Exp(-0.25f * x * x) + 4 * Math.Exp(-x * x) + 8 * Math.Exp(-4.0f * x * x) + 16 * Math.Exp(-16.0f * x * x));

				sum += weights[i];
			}

			for (int i = 0; i < size; i++)
			{
				weights[i] /= sum;
			}

			return weights;
		}
		
		#endregion

		#region Render

		public void Render(RenderTargetView destination, ShaderResourceView source)
		{
			Effect.BlurTexture(destination, source, this); 
		}

		#endregion

		#region IResourceManager Members

		public bool Disposed
		{
			get { return m_Disposed; }
		}

		public void LoadResources()
		{
			if (m_Disposed == true)
			{
				BlurQuadCoordsV.LoadResources();
				BlurQuadCoordsH.LoadResources(); 

				PartialBlur = new Texture2D(GameEnvironment.Device, new Texture2DDescription()
				{
					Width = m_BufferWidth,
					Height = m_BufferHeight,
					MipLevels = 1,
					ArraySize = 1,
					Format = m_BufferFormat,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default,
					BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
					CpuAccessFlags = SlimDX.Direct3D11.CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None
				});

				PartialBlurSRV = new ShaderResourceView(GameEnvironment.Device, PartialBlur, new ShaderResourceViewDescription()
				{
					Format = m_BufferFormat,
					Dimension = ShaderResourceViewDimension.Texture2D,
					MipLevels = 1,
					MostDetailedMip = 0
				});

				PartialBlurRTV = new RenderTargetView(GameEnvironment.Device, PartialBlur, new RenderTargetViewDescription()
				{
					Format = m_BufferFormat,
					Dimension = RenderTargetViewDimension.Texture2D,
					MipSlice = 0
				});

				m_Disposed = false;
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				BlurQuadCoordsV.UnloadResources();
				BlurQuadCoordsH.UnloadResources();

				PartialBlur.Dispose();
				PartialBlurSRV.Dispose();
				PartialBlurRTV.Dispose();

				m_Disposed = true;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			UnloadResources(); 
		}

		#endregion
	}
}
