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
	public class ImposterFloatEffect : BasicEffectBase
	{
		#region Private Members

		private string m_ShaderLocation = @"~/Shaders/ImposterFloat.fx";
		private bool m_Disposed = true;

		private Effect m_Effect;
		private EffectTechnique m_Technique;	
		private EffectTechnique m_Technique_BrightPass;
		private InputLayout m_Layout;
		
		private EffectPass m_Pass_NoBlend;
		private EffectPass m_Pass_OverlayAdd;
		private EffectPass m_Pass_OverlaySubtract;
		private EffectPass m_Pass_OverlayInvert; 
		private EffectPass m_Pass_OverlayAlpha;

		private EffectPass m_Pass_NoBlend_BrightPass;
		private EffectPass m_Pass_OverlayAdd_BrightPass;
		private EffectPass m_Pass_OverlaySubtract_BrightPass;
		private EffectPass m_Pass_OverlayAlpha_BrightPass;

		private Buffer m_Vertices;
		private VertexBufferBinding m_VerticesBindings;

		private EffectResourceVariable m_ImposterTextureResource;
		private EffectScalarVariable m_BrightPassThreshold; 

		#endregion

		public override string ShaderLocation { get { return m_ShaderLocation; } }

		#region IResourceManager Members

		public override bool Disposed { get { return m_Disposed; } }

		public override void LoadResources()
		{
			if (m_Disposed == true)
			{
				m_Effect = m_Effect = new Effect(GameEnvironment.Device, Bytecode); // Effect.FromFile(GameEnvironment.Device, Helper.ResolvePath(m_ShaderLocation), "fx_4_0", ShaderFlags.None, EffectFlags.None, null, null);
				m_Technique = m_Effect.GetTechniqueByName("Imposter");

				m_Pass_NoBlend = m_Technique.GetPassByName("NoBlend");
				m_Pass_OverlayAdd = m_Technique.GetPassByName("OverlayAdd");
				m_Pass_OverlaySubtract = m_Technique.GetPassByName("OverlaySubtract");
				m_Pass_OverlayInvert = m_Technique.GetPassByName("OverlayInvert");
				m_Pass_OverlayAlpha = m_Technique.GetPassByName("OverlayAlpha");

				m_Technique_BrightPass = m_Effect.GetTechniqueByName("Imposter_BrightPass");

				m_Pass_NoBlend_BrightPass = m_Technique_BrightPass.GetPassByName("NoBlend");
				m_Pass_OverlayAdd_BrightPass = m_Technique_BrightPass.GetPassByName("OverlayAdd");
				m_Pass_OverlaySubtract_BrightPass = m_Technique_BrightPass.GetPassByName("OverlaySubtract");
				m_Pass_OverlayAlpha_BrightPass = m_Technique_BrightPass.GetPassByName("OverlayAlpha");

				m_ImposterTextureResource = m_Effect.GetVariableByName("imposter").AsResource();

				m_BrightPassThreshold = m_Effect.GetVariableByName("brightPassThreshold").AsScalar();

				m_Layout = new InputLayout(GameEnvironment.Device, m_Pass_NoBlend.Description.Signature, new[] {
					new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 8, 0)		
				});

				float minX = -1f, miny = -1f, maxX = 1f, maxY = 1f;

				using (DataStream stream = new DataStream(4 * Marshal.SizeOf(typeof(Vertex2D)), true, true))
				{
					stream.WriteRange(new Vertex2D[] {					
						new Vertex2D() { Position = new Vector2(maxX, miny), TextureCoords =  new Vector2(1.0f, 1.0f) }, 
						new Vertex2D() { Position = new Vector2(minX, miny), TextureCoords =  new Vector2(0.0f, 1.0f) }, 
						new Vertex2D() { Position = new Vector2(maxX, maxY), TextureCoords = new Vector2(1.0f, 0.0f) },  
						new Vertex2D() { Position = new Vector2(minX, maxY), TextureCoords =  new Vector2(0.0f, 0.0f) } 
					});
					stream.Position = 0;

					m_Vertices = new SlimDX.Direct3D11.Buffer(GameEnvironment.Device, stream, new BufferDescription()
					{
						BindFlags = BindFlags.VertexBuffer,
						CpuAccessFlags = CpuAccessFlags.None,
						OptionFlags = ResourceOptionFlags.None,
						SizeInBytes = 4 * Marshal.SizeOf(typeof(Vertex2D)),
						Usage = ResourceUsage.Default
					});
				}

				m_VerticesBindings = new VertexBufferBinding(m_Vertices, Marshal.SizeOf(typeof(Vertex2D)), 0);

				m_Disposed = false;
			}
		}

		public override void UnloadResources()
		{
			if (m_Disposed == false)
			{
				m_Disposed = true; 
				m_Vertices.Dispose();
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

		#region Render Methods

		public void BeginRender(RenderTargetView imposterTarget, DepthStencilView depthView, DepthStencilState depthState, Color4 background, Viewport viewport, RenderTargetBackupState state)
		{
			if (m_Disposed == true)
			{
				return;
			}

			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext;

			state.RenderTargets = context.OutputMerger.GetRenderTargets(1);
			state.Viewports = context.Rasterizer.GetViewports();

//			GameEnvironment.Device.OutputMerger.SetTargets(GameEnvironment.DepthView, GameEnvironment.RenderView);
//			GameEnvironment.Device.ClearDepthStencilView(GameEnvironment.DepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
//			GameEnvironment.Device.OutputMerger.DepthStencilState = GameEnvironment.DepthState;


			context.OutputMerger.SetTargets(depthView, imposterTarget);
			context.Rasterizer.SetViewports(viewport);
			context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
			context.ClearRenderTargetView(imposterTarget, background);
			context.OutputMerger.DepthStencilState = depthState;
		}

		public void BeginRender(RenderTargetView imposterTarget, Color4 background, Viewport viewport, RenderTargetBackupState state)
		{
			if (m_Disposed == true)
			{
				return; 
			}
			
			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext;

			state.RenderTargets = context.OutputMerger.GetRenderTargets(1);
			state.Viewports = context.Rasterizer.GetViewports();

			context.OutputMerger.SetTargets(imposterTarget);
			context.Rasterizer.SetViewports(viewport);

			context.ClearRenderTargetView(imposterTarget, background); 
		}

		public void EndRender(RenderTargetBackupState state)
		{
			if (m_Disposed == true)
			{
				return;
			}

			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext;

			context.OutputMerger.SetTargets(state.RenderTargets);
			context.Rasterizer.SetViewports(state.Viewports);
		}

		public void RenderImposter(ImposterOverlayType overlay, ShaderResourceView imposterTexture)
		{
			RenderImposter(overlay, imposterTexture, 1f); 
		}

		public void RenderImposter(ImposterOverlayType overlay, VertexBufferBinding bindings, ShaderResourceView imposterTexture)
		{
			RenderImposter(overlay, bindings, imposterTexture, 1f);
		}

		public void RenderImposter(ImposterOverlayType overlay, ShaderResourceView imposterTexture, float brightPassThreshold)
		{
			if (m_Disposed == true)
			{
				return;
			}

			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext;

			m_ImposterTextureResource.SetResource(imposterTexture);

			context.InputAssembler.InputLayout = m_Layout;
			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
			context.InputAssembler.SetVertexBuffers(0, m_VerticesBindings);
			context.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);
			
			m_BrightPassThreshold.Set(brightPassThreshold);

			if (overlay == ImposterOverlayType.None)
			{
				m_Pass_NoBlend.Apply(context);
			}
			else if (overlay == ImposterOverlayType.Add)
			{
				m_Pass_OverlayAdd.Apply(context);
			}
			else if (overlay == ImposterOverlayType.Subtract)
			{
				m_Pass_OverlaySubtract.Apply(context);
			}
			else if (overlay == ImposterOverlayType.Invert)
			{
				m_Pass_OverlayInvert.Apply(context);
			}
			else if (overlay == ImposterOverlayType.Alpha)
			{
				m_Pass_OverlayAlpha.Apply(context);
			}
			else if (overlay == ImposterOverlayType.None_BrightPass)
			{
				m_Pass_NoBlend_BrightPass.Apply(context);
			}
			else if (overlay == ImposterOverlayType.Add_BrightPass)
			{
				m_Pass_OverlayAdd_BrightPass.Apply(context);
			}
			else if (overlay == ImposterOverlayType.Subtract_BrightPass)
			{
				m_Pass_OverlaySubtract_BrightPass.Apply(context);
			}
			else if (overlay == ImposterOverlayType.Alpha_BrightPass)
			{
				m_Pass_OverlayAlpha_BrightPass.Apply(context);
			}
			else
			{
				throw new Exception("Unknown imposter overlay mode '" + overlay.ToString() + "'");
			}

			context.Draw(4, 0);
		}

		public void RenderImposter(ImposterOverlayType overlay, VertexBufferBinding bindings, ShaderResourceView imposterTexture, float brightPassThreshold)
		{
			if (m_Disposed == true)
			{
				return;
			}

			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext;

			m_ImposterTextureResource.SetResource(imposterTexture);

			context.InputAssembler.InputLayout = m_Layout;
			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
			context.InputAssembler.SetVertexBuffers(0, bindings);
			context.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);

			m_BrightPassThreshold.Set(brightPassThreshold);

			if (overlay == ImposterOverlayType.None)
			{
				m_Pass_NoBlend.Apply(context);
			}
			else if (overlay == ImposterOverlayType.Add)
			{
				m_Pass_OverlayAdd.Apply(context);
			}
			else if (overlay == ImposterOverlayType.Subtract)
			{
				m_Pass_OverlaySubtract.Apply(context);
			}
			else if (overlay == ImposterOverlayType.Invert)
			{
				m_Pass_OverlayInvert.Apply(context);
			}
			else if (overlay == ImposterOverlayType.Alpha)
			{
				m_Pass_OverlayAlpha.Apply(context);
			}
			else if (overlay == ImposterOverlayType.None_BrightPass)
			{
				m_Pass_NoBlend_BrightPass.Apply(context);
			}
			else if (overlay == ImposterOverlayType.Add_BrightPass)
			{
				m_Pass_OverlayAdd_BrightPass.Apply(context);
			}
			else if (overlay == ImposterOverlayType.Subtract_BrightPass)
			{
				m_Pass_OverlaySubtract_BrightPass.Apply(context);
			}
			else if (overlay == ImposterOverlayType.Alpha_BrightPass)
			{
				m_Pass_OverlayAlpha_BrightPass.Apply(context);
			}
			else
			{
				throw new Exception("Unknown imposter overlay mode '" + overlay.ToString() + "'");
			}

			context.Draw(4, 0);
		}

		#endregion
	}
}
