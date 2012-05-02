using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using RugTech1.Framework.Data;
using RugTech1.Framework.Objects;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D11.Buffer;
using Device = SlimDX.Direct3D11.Device;

namespace RugTech1.Framework.Effects
{
	public class VolumeEffect : BasicEffectBase
	{
		private string m_ShaderLocation = @"~/Shaders/Volume.fx";
		private bool m_Disposed = true; 

		public Effect m_Effect;
		public EffectTechnique RayStartTechnique;
		public EffectPass RayStartOutsidePass;
		public EffectPass RayStartInsidePass;
		public InputLayout VolumeLayout;

		public EffectTechnique RayDirectionTechnique;
		public EffectPass RayDirectionPass0; 
		//public EffectPass RayDirectionPass1;
	
		public Buffer BillboardVertices;
		public VertexBufferBinding BillboardVerticesBindings;

		public EffectTechnique BillboardTechnique;
		public EffectPass BillboardPass0;

		public EffectTechnique ImposterTechnique;
		public EffectPass ImposterPass0;

		public InputLayout BillboardLayout;

		public EffectVectorVariable LocationColor;
		public EffectResourceVariable rayStart_texture;
		public EffectResourceVariable rayDir_texture;
		public EffectResourceVariable volume_texture;
		public EffectResourceVariable imposter;

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
				#region Load Volume Effect

				m_Effect = m_Effect = new Effect(GameEnvironment.Device, Bytecode); // Effect.FromFile(GameEnvironment.Device, Helper.ResolvePath(m_ShaderLocation), "fx_4_0", ShaderFlags.None, EffectFlags.None, null, null);

				RayStartTechnique = m_Effect.GetTechniqueByName("RayStart");
				RayStartOutsidePass = RayStartTechnique.GetPassByName("Outside");
				RayStartInsidePass = RayStartTechnique.GetPassByName("Inside");

				VolumeLayout = new InputLayout(GameEnvironment.Device, RayStartOutsidePass.Description.Signature, new[] {
				   new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
				   new InputElement("COLOR", 0, Format.R32G32B32_Float, 12, 0)			
				});


				RayDirectionTechnique = m_Effect.GetTechniqueByName("RayDirection");
				RayDirectionPass0 = RayDirectionTechnique.GetPassByIndex(0);
				//RayDirectionPass1 = RayDirectionTechnique.GetPassByIndex(1);

				BillboardTechnique = m_Effect.GetTechniqueByName("Final");
				BillboardPass0 = BillboardTechnique.GetPassByIndex(0);

				ImposterTechnique = m_Effect.GetTechniqueByName("Imposter");
				ImposterPass0 = ImposterTechnique.GetPassByIndex(0);

				BillboardLayout = new InputLayout(GameEnvironment.Device, BillboardPass0.Description.Signature, new[] {
					new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 8, 0)		
				});

				rayStart_texture = m_Effect.GetVariableByName("rayStart_texture").AsResource();
				rayDir_texture = m_Effect.GetVariableByName("rayDir_texture").AsResource();
				imposter = m_Effect.GetVariableByName("imposter").AsResource();
				volume_texture = m_Effect.GetVariableByName("volume_texture").AsResource();
				LocationColor = m_Effect.GetVariableByName("locationColor").AsVector();

				#endregion	

				#region Billboard Verts

				float minX = -1f;
				float miny = -1f;
				float maxX = 1f;
				float maxY = 1f; 

				using (DataStream stream = new DataStream(4 * Marshal.SizeOf(typeof(Vertex2D)), true, true))
				{
					stream.WriteRange(new Vertex2D[] {					
						new Vertex2D() { Position = new Vector2(maxX, miny), TextureCoords =  new Vector2(1.0f, 1.0f) }, 
						new Vertex2D() { Position = new Vector2(minX, miny), TextureCoords =  new Vector2(0.0f, 1.0f) }, 
						new Vertex2D() { Position = new Vector2(maxX, maxY), TextureCoords = new Vector2(1.0f, 0.0f) },  
						new Vertex2D() { Position = new Vector2(minX, maxY), TextureCoords =  new Vector2(0.0f, 0.0f) } 
					});
					stream.Position = 0;

					BillboardVertices = new SlimDX.Direct3D11.Buffer(GameEnvironment.Device, stream, new BufferDescription()
					{
						BindFlags = BindFlags.VertexBuffer,
						CpuAccessFlags = CpuAccessFlags.None,
						OptionFlags = ResourceOptionFlags.None,
						SizeInBytes = 4 * Marshal.SizeOf(typeof(Vertex2D)),
						Usage = ResourceUsage.Default
					});
				}

				BillboardVerticesBindings = new VertexBufferBinding(BillboardVertices, Marshal.SizeOf(typeof(UIVertex)), 0);

				#endregion

				m_Disposed = false; 
			}	
		}

		public override void UnloadResources()
		{
			if (m_Disposed == false)
			{ 
				m_Effect.Dispose();
				VolumeLayout.Dispose();

				BillboardVertices.Dispose();
				BillboardLayout.Dispose();

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

		public void RenderVolumeTexture(Viewport VolumeViewport, Vector3 CameraPosition, 
										BoundingBox VolumeBounds,  VertexBufferBinding VolumeBoundsVertsBindings, Buffer VolumeBoundsIndices,
										ShaderResourceView VolumeTexture, 
										RenderTargetView RayStartView, ShaderResourceView RayStartResourceView, 
										RenderTargetView RayDirectionView, ShaderResourceView RayDirectionResourceView, 
										RenderTargetView ImposterView, 
										Buffer InsideVertices, VertexBufferBinding InsideVerticesBindings)
		{
			
			/*device.Rasterizer.State = SlimDX.Direct3D11.RasterizerState.FromDescription(device, new RasterizerStateDescription()
			{
				CullMode = CullMode.Back,
				DepthBias = 0,
				DepthBiasClamp = 0,
				FillMode = FillMode.Solid,
				IsAntialiasedLineEnabled = false,
				IsDepthClipEnabled = false,
				IsFrontCounterclockwise = true,
				IsMultisampleEnabled = false,
				IsScissorEnabled = false,
				SlopeScaledDepthBias = 0
			}); */

			//public ShaderResourceView ImposterResourceView;
			//public ShaderResourceView RayDirectionResourceView;
			//public ShaderResourceView RayStartResourceView;			

			rayStart_texture.SetResource(RayStartResourceView);
			rayDir_texture.SetResource(RayDirectionResourceView);			
			volume_texture.SetResource(VolumeTexture);
			//LocationColor.SetResource();

			Device device = GameEnvironment.Device;
			SlimDX.Direct3D11.DeviceContext context = device.ImmediateContext; 

			RenderTargetView[] backupTargets = context.OutputMerger.GetRenderTargets(1);
			Viewport[] backupViewPorts = context.Rasterizer.GetViewports();

			context.Rasterizer.SetViewports(VolumeViewport);
			context.InputAssembler.InputLayout = VolumeLayout;

			context.OutputMerger.SetTargets(RayStartView);
			context.InputAssembler.SetVertexBuffers(0, VolumeBoundsVertsBindings);
			context.InputAssembler.SetIndexBuffer(VolumeBoundsIndices, Format.R32_UInt, 0);

			if (BoundingBox.Contains(VolumeBounds, CameraPosition) == ContainmentType.Contains)
			{
				float x, y, z;
				//float size = 1f / ((float)ExplicitMapObjects.Volume.Size * 16); 
				//float depth = 1f / ((float)ExplicitMapObjects.Volume.Depth * 16 * 8);

				//Vector3 vec = new Vector3(size * (Camera.Center.X / Camera.MapScale), size * (Camera.Center.Y / Camera.MapScale), depth * (Camera.Center.Z / Camera.MapScale)); 

				x = (1f / VolumeBounds.Maximum.X) * CameraPosition.X;
				y = (1f / VolumeBounds.Maximum.Y) * CameraPosition.Y;
				z = (1f / VolumeBounds.Maximum.Z) * CameraPosition.Z; 

				//device.ClearRenderTargetView(RayStartView, new Color4(1, vec.X, vec.Y, vec.Z));*/

				DataStream stream = context.MapSubresource(InsideVertices, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None).Data;

				stream.WriteRange(new VolumeVertex[] {					
					new VolumeVertex() { Position = new Vector3( 1f, -1f, 0), Color = new Vector3(x, y, z) }, 
					new VolumeVertex() { Position = new Vector3(-1f, -1f, 0), Color = new Vector3(x, y, z) }, 
					new VolumeVertex() { Position = new Vector3( 1f,  1f, 0), Color = new Vector3(x, y, z) },  
					new VolumeVertex() { Position = new Vector3(-1f,  1f, 0), Color = new Vector3(x, y, z) } 
				});

				context.UnmapSubresource(InsideVertices, 0);

				context.InputAssembler.SetVertexBuffers(0, InsideVerticesBindings);
				context.ClearRenderTargetView(RayStartView, Color.Black);
				context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

				RayStartInsidePass.Apply(context);
				context.Draw(4, 0);

				context.InputAssembler.SetVertexBuffers(0, VolumeBoundsVertsBindings);
				context.InputAssembler.SetIndexBuffer(VolumeBoundsIndices, Format.R32_UInt, 0);
				context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
			}
			else
			{
				context.InputAssembler.SetVertexBuffers(0, VolumeBoundsVertsBindings);
				context.InputAssembler.SetIndexBuffer(VolumeBoundsIndices, Format.R32_UInt, 0);
				context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

				context.ClearRenderTargetView(RayStartView, Color.Black);

				RayStartOutsidePass.Apply(context);
				context.DrawIndexed(6 * 6, 0, 0);
			}

			context.OutputMerger.SetTargets(RayDirectionView);
			context.ClearRenderTargetView(RayDirectionView, Color.Black);

			RayDirectionPass0.Apply(context);
			context.DrawIndexed(6 * 6, 0, 0);

			context.OutputMerger.SetTargets(ImposterView);
			context.ClearRenderTargetView(ImposterView, Color.Black);

			context.InputAssembler.InputLayout = BillboardLayout;
			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
			context.InputAssembler.SetVertexBuffers(0, BillboardVerticesBindings);
			context.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);

			BillboardPass0.Apply(context);
			context.Draw(4, 0);

			context.OutputMerger.SetTargets(backupTargets);
			context.Rasterizer.SetViewports(backupViewPorts);
		}

		public void RenderImposter(ShaderResourceView ImposterResourceView)
		{
			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext; 
			imposter.SetResource(ImposterResourceView);

			context.InputAssembler.InputLayout = BillboardLayout;
			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
			context.InputAssembler.SetVertexBuffers(0, BillboardVerticesBindings);
			context.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);

			ImposterPass0.Apply(context);
			context.Draw(4, 0);
		}



		// Render 
		// if the camera is outside the volume 
		//  - Render ray starting position to texture RayStart 
		// if the camera is inside the volume 
		//  - render a single colour to to indicate the pixel location of the camera to RayStart
		// Render ray endding to texture RayEnd 
		// get the ray direction as Dir[p] = (RayEnd[p] - RayStart[p])
		// shader based for loop to propergate the ray
	}
}
