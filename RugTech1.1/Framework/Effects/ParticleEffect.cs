using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace RugTech1.Framework.Effects
{
	public class ParticleEffect : BasicEffectBase
	{
		#region Private Members

		private string m_ShaderLocation = @"~/Shaders/Particles.fx";

		private bool m_Disposed = true; 

		private Effect m_Effect;
		private EffectTechnique m_Technique;		
		private InputLayout m_Layout;
		private EffectPass m_ParticlePass_Add;

		private EffectMatrixVariable m_WorldViewProj;
		private EffectResourceVariable m_ParticleTexture;
		private EffectScalarVariable m_AmpScale;
		private EffectScalarVariable m_PartScale; 
		private EffectScalarVariable m_MaxDistance;
		private EffectScalarVariable m_MinDistance;
		private EffectScalarVariable m_ScaleDistance;

		#endregion

		public override string ShaderLocation { get { return m_ShaderLocation; } }

		#region IResourceManager Members

		public override bool Disposed
		{
			get { return m_Disposed; }
		}

		public override void LoadResources()
		{
			if (m_Disposed == true)
			{
				m_Effect = m_Effect = new Effect(GameEnvironment.Device, Bytecode); // Effect.FromFile(GameEnvironment.Device, Helper.ResolvePath(m_ShaderLocation), "fx_4_0", ShaderFlags.None, EffectFlags.None, null, null);
				m_Technique = m_Effect.GetTechniqueByName("RenderParticles");
				m_ParticlePass_Add = m_Technique.GetPassByName("Add");

				m_Layout = new InputLayout(GameEnvironment.Device, m_ParticlePass_Add.Description.Signature, new[] {
					new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0),				
					new InputElement("INST_POSITION", 0, Format.R32G32B32_Float, 0, 1, InputClassification.PerInstanceData, 1),
					new InputElement("INST_COLOR", 0, Format.R32G32B32A32_Float, 12, 1, InputClassification.PerInstanceData, 1), 					
				});

				m_WorldViewProj = m_Effect.GetVariableByName("worldViewProj").AsMatrix();
				m_ParticleTexture = m_Effect.GetVariableByName("particle_texture").AsResource();
				m_AmpScale = m_Effect.GetVariableByName("ampScale").AsScalar();
				m_PartScale = m_Effect.GetVariableByName("partScale").AsScalar(); 
				m_MaxDistance = m_Effect.GetVariableByName("maxDistance").AsScalar();
				m_MinDistance = m_Effect.GetVariableByName("minDistance").AsScalar();
				m_ScaleDistance = m_Effect.GetVariableByName("scaleDistance").AsScalar();

				m_Disposed = false;
			}
		}

		public override void UnloadResources()
		{
			if (m_Disposed == false)
			{
				m_Effect.Dispose();
				m_Layout.Dispose(); 

				m_Disposed = true;
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

		#region Render Method

		public void Render(VertexBufferBinding[] bindings, int count, Matrix worldViewProj, ShaderResourceView texture,
							float ampScale, float partScale,
							float minDistance, float maxDistance, float scaleDistance)
		{
			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext; 

			m_WorldViewProj.SetMatrix(worldViewProj);
			m_ParticleTexture.SetResource(texture);
			m_AmpScale.Set(ampScale);
			m_PartScale.Set(partScale);
			m_MaxDistance.Set(maxDistance);
			m_MinDistance.Set(minDistance);
			m_ScaleDistance.Set(scaleDistance);

			context.InputAssembler.InputLayout = m_Layout;
			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
			context.InputAssembler.SetVertexBuffers(0, bindings);

			m_ParticlePass_Add.Apply(context);
			context.DrawInstanced(4, count, 0, 0);			
		}

		#endregion
	}
}
