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
	public class MaterialEffect : BasicEffectBase
	{
		#region Private Members

		//private string m_ShaderLocation = @"~/Shaders/Material2.fx";
		//private string m_ShaderLocation = @"~/Shaders/EnvMapObj.fx";
		private string m_ShaderLocation = @"~/Shaders/EnvMapObj2.fx"; 

		private bool m_Disposed = true;

		private Effect m_Effect;
		private EffectTechnique m_Technique;
		private InputLayout m_Layout;
		private EffectPass m_Pass0;
		
		private EffectResourceVariable m_DiffuseVariable;
		private EffectMatrixVariable m_WorldVariable;
		private EffectMatrixVariable m_ViewVariable;
		private EffectMatrixVariable m_ProjectionVariable;
		private EffectResourceVariable m_SpecularMapVariable;
		private EffectMatrixVariable m_InvViewVariable;
		private EffectVectorVariable m_EyeVariable;
		private EffectResourceVariable m_DiffuseMapVariable;
		private EffectResourceVariable m_NormalMapVariable;

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
				m_Effect = m_Effect = new Effect(GameEnvironment.Device, Bytecode); // Effect.FromFile(GameEnvironment.Device, Helper.ResolvePath(m_ShaderLocation), "fx_4_0", ShaderFlags.Debug | ShaderFlags.EnableStrictness, EffectFlags.None, null, null);
				m_Technique = m_Effect.GetTechniqueByName("Render");
				m_Pass0 = m_Technique.GetPassByName("P0");

				/*m_Layout = new InputLayout(GameEnvironment.Device, m_Pass0.Description.Signature, new[] {
					new InputElement( "POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
					new InputElement( "NORMAL", 0, Format.R32G32B32_Float, 0, 12, InputClassification.PerVertexData, 0),
					new InputElement( "TEXCOORD", 0, Format.R32G32_Float, 0, 24, InputClassification.PerVertexData, 0),
				});*/

				m_Layout = new InputLayout(GameEnvironment.Device, m_Pass0.Description.Signature, new[] {
					new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
					new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
				});

				//texColorMap
				//texNormalMap
				//texDiffuseMap
				//texSpecularMap

				//m_DiffuseVariable = m_Effect.GetVariableByName("g_txDiffuse").AsResource();
				m_DiffuseVariable = m_Effect.GetVariableByName("texColorMap").AsResource();
				m_NormalMapVariable = m_Effect.GetVariableByName("texNormalMap").AsResource();
				m_WorldVariable = m_Effect.GetVariableByName("World").AsMatrix();
				m_ViewVariable = m_Effect.GetVariableByName("View").AsMatrix();
				m_InvViewVariable = m_Effect.GetVariableByName("InvView").AsMatrix();
				m_ProjectionVariable = m_Effect.GetVariableByName("Projection").AsMatrix();
				//m_SpecularMapVariable = m_Effect.GetVariableByName("g_txEnvMap").AsResource();
				m_SpecularMapVariable = m_Effect.GetVariableByName("texSpecularMap").AsResource();
				m_DiffuseMapVariable = m_Effect.GetVariableByName("texDiffuseMap").AsResource();
				m_EyeVariable = m_Effect.GetVariableByName("Eye").AsVector();

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

		public void Render(Objects.View3D view, 
						   SlimDX.Matrix model, 
			               SlimDX.Direct3D11.ShaderResourceView diffuse, 
						   SlimDX.Direct3D11.ShaderResourceView diffuseLightMap,
						   SlimDX.Direct3D11.ShaderResourceView specularLightMap, 							
						   SlimDX.Direct3D11.Buffer vertices, 
						   SlimDX.Direct3D11.Buffer indices, int indexCount, 
						   SlimDX.Direct3D11.VertexBufferBinding bindings)
		{		
			m_ProjectionVariable.SetMatrix(view.Projection);
			m_ViewVariable.SetMatrix(view.View);
			m_WorldVariable.SetMatrix(model);
			//Matrix invView = model;
			//invView.Invert();
			//m_InvViewVariable.SetMatrix(invView);
			m_DiffuseVariable.SetResource(diffuse);
			m_SpecularMapVariable.SetResource(specularLightMap);
			m_DiffuseMapVariable.SetResource(diffuseLightMap);
			
			Vector3 eye = view.Camera.Center; 
			//eye = Vector3.TransformCoordinate(eye, model);
			//eye = Vector3.TransformCoordinate(eye, view.View);
			//eye = Vector3.TransformCoordinate(eye, view.Projection);

			m_EyeVariable.Set(eye);

			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext;

			context.InputAssembler.InputLayout = m_Layout;
			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
			context.InputAssembler.SetVertexBuffers(0, bindings);
			context.InputAssembler.SetIndexBuffer(indices, Format.R32_UInt, 0);
			m_Pass0.Apply(context);

			context.DrawIndexed(indexCount, 0, 0);
		}
 
		#endregion		
	}
}
