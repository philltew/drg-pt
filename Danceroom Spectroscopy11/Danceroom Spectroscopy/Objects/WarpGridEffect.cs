using System;
using System.Runtime.InteropServices;
using RugTech1.Framework.Data;
using RugTech1.Framework.Objects;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D11.Buffer; 

namespace RugTech1.Framework.Effects
{
	public class WarpGridEffect : BasicEffectBase
	{
		#region Private Members

		private string m_ShaderLocation = @"~/Shaders/WarpGrid.fx";
		private bool m_Disposed = true;

		private Effect m_Effect;
		private EffectTechnique m_Technique;	
		private InputLayout m_Layout;
		
		private EffectPass m_Pass;

		private EffectResourceVariable m_ImposterTextureResource;
		private EffectScalarVariable m_GlobalAlpha; 

		#endregion

		public override string ShaderLocation { get { return m_ShaderLocation; } }

		#region IResourceManager Members

		public override bool Disposed { get { return m_Disposed; } }

		public override void LoadResources()
		{
			if (m_Disposed == true)
			{
				m_Effect = m_Effect = new Effect(GameEnvironment.Device, Bytecode);
				m_Technique = m_Effect.GetTechniqueByName("Imposter");

				m_Pass = m_Technique.GetPassByName("Pass0");

				m_ImposterTextureResource = m_Effect.GetVariableByName("imposter").AsResource();

				m_GlobalAlpha = m_Effect.GetVariableByName("globalAlpha").AsScalar();

				m_Layout = new InputLayout(GameEnvironment.Device, m_Pass.Description.Signature, new[] {
					new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 8, 0)		
				});

				m_Disposed = false;
			}
		}

		public override void UnloadResources()
		{
			if (m_Disposed == false)
			{
				m_Disposed = true; 
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

		public void RenderGrid(ShaderResourceView texture, float feedbackLevel, VertexBufferBinding bindings, Buffer indicesBuffer, int indexCount)
		{
			if (m_Disposed == true)
			{
				return;
			}

			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext;

			m_ImposterTextureResource.SetResource(texture);
			m_GlobalAlpha.Set(feedbackLevel);

			context.InputAssembler.InputLayout = m_Layout;
			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
			context.InputAssembler.SetVertexBuffers(0, bindings);
			context.InputAssembler.SetIndexBuffer(indicesBuffer, Format.R32_UInt, 0);

			m_Pass.Apply(context);

			context.DrawIndexed(indexCount, 0, 0);
		}
	}
}
