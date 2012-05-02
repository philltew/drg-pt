using System;
using System.Runtime.InteropServices;
using RugTech1.Framework.Data;
using RugTech1.Framework.Objects;
using RugTech1.Framework.Objects.Simple;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D11.Buffer; 

namespace RugTech1.Framework.Effects
{
	public enum BloomType { Gaussian }

	public class BloomEffect : BasicEffectBase
	{
		#region Private / Internal Members
		
		internal const int GAUSSIAN_MAX_SAMPLES = 16;

		private string m_ShaderLocation = @"~/Shaders/Bloom.fx";		

		private bool m_Disposed = true;
		private Effect m_Effect;
		private EffectTechnique m_Technique;
		private InputLayout m_Layout;
		private EffectPass m_Pass_Gaussian;
		private EffectResourceVariable m_SourceTex;
		private EffectScalarVariable m_GWeights;

		private int m_DataStride;
		
		private ShaderResourceView[] pSRV = new ShaderResourceView[4] { null, null, null, null };

		#endregion						

		public override string ShaderLocation { get { return m_ShaderLocation; } }

		#region IResourceManager Members

		public override bool Disposed { get { return m_Disposed; } }

		public override void LoadResources()
		{
			if (m_Disposed == true)
			{
				m_Effect = new Effect(GameEnvironment.Device, Bytecode); // Helper.ResolvePath(m_ShaderLocation), "fx_4_0", ShaderFlags.None, EffectFlags.None, null, null);
				m_Technique = m_Effect.GetTechniqueByName("BlurBilinear");

				m_Pass_Gaussian = m_Technique.GetPassByName("Gaussian");

				m_SourceTex = m_Effect.GetVariableByName("g_SourceTex").AsResource();
				m_GWeights = m_Effect.GetVariableByName("g_GWeights").AsScalar();

				//m_ElementCount = 1 + GAUSSIAN_MAX_SAMPLES;
				m_DataStride = Marshal.SizeOf(typeof(Vector2)) * (1 + GAUSSIAN_MAX_SAMPLES);

				InputElement[] IADesc = new InputElement[1 + (GAUSSIAN_MAX_SAMPLES / 2)];

				IADesc[0] = new InputElement()
				{
					SemanticName = "POSITION",
					SemanticIndex = 0,
					AlignedByteOffset = 0,
					Slot = 0,
					Classification = InputClassification.PerVertexData,
					Format = Format.R32G32_Float
				};


				for (int i = 1; i < 1 + (GAUSSIAN_MAX_SAMPLES / 2); i++)
				{
					IADesc[i] = new InputElement()
					{
						SemanticName = "TEXCOORD",
						SemanticIndex = i - 1,
						AlignedByteOffset = 8 + (i - 1) * 16,
						Slot = 0,
						Classification = InputClassification.PerVertexData,
						Format = Format.R32G32B32A32_Float
					};
				}

				// Real number of "sematinc based" elements
				//m_ElementCount = 1 + GAUSSIAN_MAX_SAMPLES / 2;

				EffectPassDescription PassDesc = m_Pass_Gaussian.Description;
				m_Layout = new InputLayout(GameEnvironment.Device, PassDesc.Signature, IADesc);

				m_Disposed = false;
			}
		}

		public override void UnloadResources()
		{
			if (m_Disposed == false)
			{
				m_Disposed = true;
				//m_Vertices.Dispose();
				m_Layout.Dispose();
				m_Effect.Dispose();
			}
		}

		#endregion

		#region IDisposable Members

		public override void Dispose()
		{			
			UnloadResources();

			base.Dispose(); 
		}

		#endregion

		#region Blur / Render Methods
				
		//-----------------------------------------------------------------------------
		// Name: glowPass
		//-----------------------------------------------------------------------------
		public void BlurTexture(RenderTargetView destination, ShaderResourceView source, BloomImposter imposter)
		{
			// Blur horizontally and then vertically the first RT and then the second one
			PartialBlur(imposter.PartialBlurRTV, imposter.BlurQuadCoordsH, source, imposter.GaussWeights);
			PartialBlur(destination, imposter.BlurQuadCoordsV, imposter.PartialBlurSRV, imposter.GaussWeights);
		}

		//-----------------------------------------------------------------------------
		// Name: BlurTarget()
		//-----------------------------------------------------------------------------
		void PartialBlur(RenderTargetView dstRTV, BloomQuad offsets, ShaderResourceView srcSRV, float[] GaussWeights)
		{
			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext; 

			m_SourceTex.SetResource(srcSRV);
			m_GWeights.Set(GaussWeights);

			//device.OMSetRenderTargets(1, &dstRTV, NULL);
			context.OutputMerger.SetTargets(dstRTV);

			// Set vertex buffer
			int stride = (int)offsets.stride;
			int offset = (int)offsets.offset;
			context.InputAssembler.InputLayout = m_Layout;
			context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(offsets.VBdata, stride, offset));
			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
			context.Rasterizer.SetViewports(offsets.Viewport);

			EffectTechniqueDescription techDesc = m_Technique.Description;

			m_Pass_Gaussian.Apply(context);
			context.Draw(3, 0);

			// Unbound all PS shader resources			
			context.PixelShader.SetShaderResources(pSRV, 0, 4);

		}

		#endregion
	}
}
