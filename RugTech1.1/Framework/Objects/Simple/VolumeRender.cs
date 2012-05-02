using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using RugTech1.Framework.Data;
using RugTech1.Framework.Effects;
using RugTech1.Framework.Objects;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D11.Buffer;
using Device = SlimDX.Direct3D11.Device;

namespace RugTech1.Framework.Objects.Simple
{
	public class VolumeRender : IResourceManager
	{
		private bool m_Disposed = true;
		private static VolumeEffect m_Effect; 

		public Texture3D VolumeTexture;
		public ShaderResourceView VolumeTextureResourceView;

		public Texture2D RayStart;
		public RenderTargetView RayStartView;
		public ShaderResourceView RayStartResourceView;

		public Texture2D RayDirection;
		public RenderTargetView RayDirectionView;
		public ShaderResourceView RayDirectionResourceView;


		public Buffer VolumeBoundsVerts;
		public VertexBufferBinding VolumeBoundsVertsBindings; 
		public Buffer VolumeBoundsIndices;

		public Buffer InsideVertices;
		public VertexBufferBinding InsideVerticesBindings; 

		public Texture2D Imposter;
		public RenderTargetView ImposterView;
		public ShaderResourceView ImposterResourceView;

		public BoundingBox VolumeBounds;
		public Viewport VolumeViewport;


		public readonly int ViewWidth; 
		public readonly int ViewHeight;
		
		public readonly int VolumeWidth;
		public readonly int VolumeHeight;
		public readonly int VolumeDepth;
		public readonly Vector3 Scale; 
		public readonly Format Format; 

		public VolumeRender(int width, int height, int VolumeWidth, int VolumeHeight, int VolumeDepth, Vector3 Scale, Format format)
		{
			if (m_Effect == null)
			{
				m_Effect = SharedEffects.Effects["Volume"] as VolumeEffect; 
			}

			this.ViewWidth = width;
			this.ViewHeight = height;

			this.Scale = Scale; 
			this.VolumeWidth = VolumeWidth;
			this.VolumeHeight = VolumeHeight;
			this.VolumeDepth = VolumeDepth;

			this.VolumeViewport = new Viewport(0, 0, ViewWidth, ViewHeight);
		}

		public virtual Half4[] GetVolumeImage()
		{
			throw new NotImplementedException();
		}

		public void RenderVolumeToImposter(IScene scene, View3D view)
		{
			m_Effect.RenderVolumeTexture(VolumeViewport, view.Camera.Center,
										 VolumeBounds, VolumeBoundsVertsBindings, VolumeBoundsIndices, VolumeTextureResourceView,
										 RayStartView, RayStartResourceView,
										 RayDirectionView, RayDirectionResourceView,
										 ImposterView, InsideVertices, InsideVerticesBindings); 
		}

		public void RenderImposter(IScene scene, View3D view)
		{
			m_Effect.RenderImposter(ImposterResourceView);

		}

		#region IResourceManager Members

		public bool Disposed
		{
			get { return m_Disposed; }
		}

		public void LoadResources()
		{
			if (m_Disposed == true)
			{
				/* 
				Texture3D VolumeTexture;
				ShaderResourceView VolumeTextureResourceView;

				Texture2D RayStart;
				RenderTargetView RayStartView;
				ShaderResourceView RayStartResourceView;

				Texture2D RayDirection;
				RenderTargetView RayDirectionView;
				ShaderResourceView RayDirectionResourceView;


				Buffer VolumeBoundsVerts;
				VertexBufferBinding VolumeBoundsVertsBindings;
				Buffer VolumeBoundsIndices;

				Buffer InsideVertices;
				VertexBufferBinding InsideVerticesBindings;

				Texture2D Imposter;
				RenderTargetView ImposterView;
				ShaderResourceView ImposterResourceView;

				BoundingBox VolumeBounds;
				Viewport VolumeViewport;
				 * */

				using (DataStream stream = new DataStream(GetVolumeImage(), false, false))
				{
					VolumeTexture = new Texture3D(GameEnvironment.Device, new Texture3DDescription()
					{
						Format = SlimDX.DXGI.Format.R16G16B16A16_Float,
						Width = (int)VolumeWidth,
						Height = (int)VolumeHeight,
						Depth = (int)VolumeDepth,
						MipLevels = 1,
						BindFlags = BindFlags.ShaderResource,
						CpuAccessFlags = CpuAccessFlags.None,
						OptionFlags = ResourceOptionFlags.None,
						Usage = ResourceUsage.Immutable
					},
					new DataBox(((int)VolumeWidth) * Marshal.SizeOf(typeof(Half4)),
								((int)VolumeWidth * VolumeHeight) * Marshal.SizeOf(typeof(Half4)),
								stream));
				}

				VolumeTextureResourceView = new ShaderResourceView(GameEnvironment.Device, VolumeTexture);


				#region Create Ray Start Texture

				RayStart = new Texture2D(GameEnvironment.Device, new Texture2DDescription()
				{
					Format = Format,
					Width = ViewWidth,
					Height = ViewHeight,
					OptionFlags = ResourceOptionFlags.None,
					MipLevels = 1,
					ArraySize = 1,
					BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.None,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default
				});

				RayStartView = new RenderTargetView(GameEnvironment.Device, RayStart);
				RayStartResourceView = new ShaderResourceView(GameEnvironment.Device, RayStart);

				#endregion

				#region Create Ray End Texture

				RayDirection = new Texture2D(GameEnvironment.Device, new Texture2DDescription()
				{
					Format = Format,
					Width = ViewWidth,
					Height = ViewHeight,
					OptionFlags = ResourceOptionFlags.None,
					MipLevels = 1,
					ArraySize = 1,
					BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.None,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default
				});

				RayDirectionView = new RenderTargetView(GameEnvironment.Device, RayDirection);
				RayDirectionResourceView = new ShaderResourceView(GameEnvironment.Device, RayDirection);

				#endregion

				#region Create Imposter Texture

				Imposter = new Texture2D(GameEnvironment.Device, new Texture2DDescription()
				{
					Format = Format,
					Width = ViewWidth,
					Height = ViewHeight,
					OptionFlags = ResourceOptionFlags.None,
					MipLevels = 1,
					ArraySize = 1,
					BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.None,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default
				});

				ImposterView = new RenderTargetView(GameEnvironment.Device, Imposter);
				ImposterResourceView = new ShaderResourceView(GameEnvironment.Device, Imposter);

				#endregion


				#region Create Volume Bounding Box

				float volumeWidth = (float)VolumeWidth * Scale.X;
				float volumeHeight = (float)VolumeHeight * Scale.Y;
				float volumeDepth = (float)VolumeDepth * Scale.Z;

				using (DataStream stream = new DataStream(8 * Marshal.SizeOf(typeof(VolumeVertex)), false, true))
				{
					stream.WriteRange(new VolumeVertex[] {					
					new VolumeVertex() { Position = new Vector3(0,			 0,				0), Color = new Vector3(0f, 0f, 0f) }, 
					new VolumeVertex() { Position = new Vector3(volumeWidth, 0,				0), Color = new Vector3(1f, 0f, 0f)  }, 
					new VolumeVertex() { Position = new Vector3(0,			 volumeHeight,	0), Color = new Vector3(0f, 1f, 0f)  },  
					new VolumeVertex() { Position = new Vector3(volumeWidth, volumeHeight,	0), Color = new Vector3(1f, 1f, 0f)  }, 
					
					new VolumeVertex() { Position = new Vector3(0,			 0,				volumeDepth), Color = new Vector3(0f, 0f, 1f) }, 
					new VolumeVertex() { Position = new Vector3(volumeWidth, 0,				volumeDepth), Color = new Vector3(1f, 0f, 1f)  }, 
					new VolumeVertex() { Position = new Vector3(0,			 volumeHeight,	volumeDepth), Color = new Vector3(0f, 1f, 1f)  },  
					new VolumeVertex() { Position = new Vector3(volumeWidth, volumeHeight,	volumeDepth), Color = new Vector3(1f, 1f, 1f)  }

				});


					stream.Position = 0;

					VolumeBoundsVerts = new SlimDX.Direct3D11.Buffer(GameEnvironment.Device, stream, new BufferDescription()
					{
						BindFlags = BindFlags.VertexBuffer,
						CpuAccessFlags = CpuAccessFlags.None,
						OptionFlags = ResourceOptionFlags.None,
						SizeInBytes = 8 * Marshal.SizeOf(typeof(VolumeVertex)),
						Usage = ResourceUsage.Default
					});
				}

				VolumeBoundsVertsBindings = new VertexBufferBinding(VolumeBoundsVerts, Marshal.SizeOf(typeof(VolumeVertex)), 0);

				using (DataStream stream = new DataStream(4 * 6 * 6, true, true))
				{
					stream.WriteRange(new int[] { 
					0, 1, 2, 
					1, 3, 2, 

					0, 6, 4, 
					0, 2, 6, 

					2, 7, 6, 
					3, 7, 2, 

					1, 5, 7, 
					7, 3, 1, 

					0, 4, 5, 
					5, 1, 0, 
 
					4, 6, 5, 
					6, 7, 5
				});
					stream.Position = 0;

					VolumeBoundsIndices = new SlimDX.Direct3D11.Buffer(GameEnvironment.Device, stream, new BufferDescription()
					{
						BindFlags = BindFlags.IndexBuffer,
						CpuAccessFlags = CpuAccessFlags.None,
						OptionFlags = ResourceOptionFlags.None,
						SizeInBytes = 4 * 6 * 6,
						Usage = ResourceUsage.Default
					});
				}

				#endregion
				
				m_Disposed = false;
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				VolumeTexture.Dispose();
				VolumeTextureResourceView.Dispose();

				RayStart.Dispose();
				RayStartView.Dispose();
				RayStartResourceView.Dispose();

				RayDirection.Dispose();
				RayDirectionView.Dispose();
				RayDirectionResourceView.Dispose();


				VolumeBoundsVerts.Dispose();
				VolumeBoundsIndices.Dispose();

				InsideVertices.Dispose();

				Imposter.Dispose();
				ImposterView.Dispose();
				ImposterResourceView.Dispose();

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

		/* 
		public VolumeRender(Device device, Camera camera, int width, int height, Format format, float mapScale)
		{
			Half4[] pal = new Half4[256];

			using (Bitmap bmp = (Bitmap)Bitmap.FromFile("gal-pal-02.png"))
			{
				BitmapData data = bmp.LockBits(new Rectangle(0, 0, 256, 1), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb); 
				
				byte[] byteData = new byte[4 * 256]; 

				Marshal.Copy(data.Scan0, byteData, 0, byteData.Length); 

				bmp.UnlockBits(data); 

				int off = 0; 

				for (int i = 0; i < 256; i++)
				{	
					float r, g, b, a; 
					
					r = ((float)byteData[off++]) / 255f; 
					g = ((float)byteData[off++]) / 255f; 
					b = ((float)byteData[off++]) / 255f;
					a = ((float)byteData[off++]) / 255f;

					pal[i] = new Half4(new Half(a), new Half(r), new Half(g), new Half(b)); 
				}
			}

			//Camera = camera;

			//VolumeBounds = new BoundingBox(new Vector3(0, 0, 0), new Vector3(ExplicitMapObjects.Volume.Size * 16 * mapScale, ExplicitMapObjects.Volume.Size * 16 * mapScale, ExplicitMapObjects.Volume.Depth * 8 * 16 * mapScale));

			//VolumeViewport = new Viewport(0, 0, width, height);
		*/ 
			/*
			#region Load Volume Data

			int stacking = 16; 

			using (DataStream stream = new DataStream(ExplicitMapObjects.GetVolumeImage(stacking, pal), false, false))
			{
				VolumeTexture = new Texture3D(device, new Texture3DDescription()
				{
					Format = SlimDX.DXGI.Format.R16G16B16A16_Float,
					Width = (int)ExplicitMapObjects.Volume.Size,
					Height = (int)ExplicitMapObjects.Volume.Size,
					Depth = (int)ExplicitMapObjects.Volume.Depth * stacking,
					MipLevels = 1,
					BindFlags = BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,
					Usage = ResourceUsage.Immutable
				},
				new DataBox(((int)ExplicitMapObjects.Volume.Size) * Marshal.SizeOf(typeof(Half4)),
							((int)ExplicitMapObjects.Volume.LayerSize) * Marshal.SizeOf(typeof(Half4)), 
							stream));
			}

			VolumeTextureResourceView = new ShaderResourceView(device, VolumeTexture);

			#endregion

			#region Create Ray Start Texture
			
			RayStart = new Texture2D(device, new Texture2DDescription()
			{
				Format = format,
				Width = width,
				Height = height,
				OptionFlags = ResourceOptionFlags.None,
				MipLevels = 1,
				ArraySize = 1,
				BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default
			});

			RayStartView = new RenderTargetView(device, RayStart);
			RayStartResourceView = new ShaderResourceView(device, RayStart);
			
			#endregion

			#region Create Ray End Texture

			RayDirection = new Texture2D(device, new Texture2DDescription()
			{
				Format = format,
				Width = width,
				Height = height,
				OptionFlags = ResourceOptionFlags.None,
				MipLevels = 1,
				ArraySize = 1,
				BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default
			});

			RayDirectionView = new RenderTargetView(device, RayDirection);
			RayDirectionResourceView = new ShaderResourceView(device, RayDirection);

			#endregion

			#region Create Imposter Texture

			Imposter = new Texture2D(device, new Texture2DDescription()
			{
				Format = format,
				Width = width,
				Height = height,
				OptionFlags = ResourceOptionFlags.None,
				MipLevels = 1,
				ArraySize = 1,
				BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default
			});

			ImposterView = new RenderTargetView(device, Imposter);
			ImposterResourceView = new ShaderResourceView(device, Imposter);

			#endregion

			#region Create Volume Bounding Box
			
			float volumeSize = (float)ExplicitMapObjects.Volume.Size * 16 * mapScale;
			float volumeDepth = (float)ExplicitMapObjects.Volume.Depth * 8 * 16 * mapScale; 

			using (DataStream stream = new DataStream(8 * Marshal.SizeOf(typeof(VolumeVertex)), false, true))
			{
				stream.WriteRange(new VolumeVertex[] {					
					new VolumeVertex() { Position = new Vector3(0,			0,			0), Color = new Vector3(0f, 0f, 0f) }, 
					new VolumeVertex() { Position = new Vector3(volumeSize, 0,			0), Color = new Vector3(1f, 0f, 0f)  }, 
					new VolumeVertex() { Position = new Vector3(0,			volumeSize, 0), Color = new Vector3(0f, 1f, 0f)  },  
					new VolumeVertex() { Position = new Vector3(volumeSize, volumeSize, 0), Color = new Vector3(1f, 1f, 0f)  }, 
					
					new VolumeVertex() { Position = new Vector3(0,			0,			volumeDepth), Color = new Vector3(0f, 0f, 1f) }, 
					new VolumeVertex() { Position = new Vector3(volumeSize, 0,			volumeDepth), Color = new Vector3(1f, 0f, 1f)  }, 
					new VolumeVertex() { Position = new Vector3(0,			volumeSize, volumeDepth), Color = new Vector3(0f, 1f, 1f)  },  
					new VolumeVertex() { Position = new Vector3(volumeSize, volumeSize, volumeDepth), Color = new Vector3(1f, 1f, 1f)  }

				});


				stream.Position = 0;

				VolumeBoundsVerts = new SlimDX.Direct3D11.Buffer(device, stream, new BufferDescription()
				{
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = 8 * Marshal.SizeOf(typeof(VolumeVertex)),
					Usage = ResourceUsage.Default
				});
			}

			VolumeBoundsVertsBindings = new VertexBufferBinding(VolumeBoundsVerts, Marshal.SizeOf(typeof(VolumeVertex)), 0);

			using (DataStream stream = new DataStream(4 * 6 * 6, true, true))
			{
				stream.WriteRange(new int[] { 
					0, 1, 2, 
					1, 3, 2, 

					0, 6, 4, 
					0, 2, 6, 

					2, 7, 6, 
					3, 7, 2, 

					1, 5, 7, 
					7, 3, 1, 

					0, 4, 5, 
					5, 1, 0, 
 
					4, 6, 5, 
					6, 7, 5
				});
				stream.Position = 0;

				VolumeBoundsIndices = new SlimDX.Direct3D11.Buffer(device, stream, new BufferDescription()
				{
					BindFlags = BindFlags.IndexBuffer,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = 4 * 6 * 6,
					Usage = ResourceUsage.Default
				});
			}

			#endregion

			#region Inside Volume Billboard Verts
			
			float minX = -1f;
			float miny = -1f;
			float maxX = 1f; // (float)backBuffer.Description.Width;
			float maxY = 1f; // (float)backBuffer.Description.Height; 

			using (DataStream stream = new DataStream(4 * Marshal.SizeOf(typeof(VolumeVertex)), true, true))
			{
				stream.WriteRange(new VolumeVertex[] {					
					new VolumeVertex() { Position = new Vector3(maxX, miny, 0), Color = new Vector3(0f, 0f, 1f) }, 
					new VolumeVertex() { Position = new Vector3(minX, miny, 0), Color = new Vector3(0f, 0f, 1f) }, 
					new VolumeVertex() { Position = new Vector3(maxX, maxY, 0), Color = new Vector3(0f, 0f, 1f) },  
					new VolumeVertex() { Position = new Vector3(minX, maxY, 0), Color = new Vector3(0f, 0f, 1f) } 
				});
				stream.Position = 0;

				InsideVertices = new SlimDX.Direct3D11.Buffer(device, stream, new BufferDescription()
				{
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = 4 * Marshal.SizeOf(typeof(VolumeVertex)),
					Usage = ResourceUsage.Dynamic
				});
			}

			InsideVerticesBindings = new VertexBufferBinding(InsideVertices, Marshal.SizeOf(typeof(VolumeVertex)), 0);

			#endregion

			#region Billboard Verts

			using (DataStream stream = new DataStream(4 * Marshal.SizeOf(typeof(UIVertex)), true, true))
			{
				stream.WriteRange(new UIVertex[] {					
					new UIVertex() { Position = new Vector2(maxX, miny), TextureCoords =  new Vector2(1.0f, 1.0f) }, 
					new UIVertex() { Position = new Vector2(minX, miny), TextureCoords =  new Vector2(0.0f, 1.0f) }, 
					new UIVertex() { Position = new Vector2(maxX, maxY), TextureCoords = new Vector2(1.0f, 0.0f) },  
					new UIVertex() { Position = new Vector2(minX, maxY), TextureCoords =  new Vector2(0.0f, 0.0f) } 
				});
				stream.Position = 0;

				BillboardVertices = new SlimDX.Direct3D11.Buffer(device, stream, new BufferDescription()
				{
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = 4 * Marshal.SizeOf(typeof(UIVertex)),
					Usage = ResourceUsage.Default
				});
			}

			BillboardVerticesBindings = new VertexBufferBinding(BillboardVertices, Marshal.SizeOf(typeof(UIVertex)), 0);

			#endregion

			#region Load Volume Effect 
			
			VolumeEffect = Effect.FromFile(device, @"Shaders\Volume.fx", "fx_4_0", ShaderFlags.None, EffectFlags.None, null, null);

			RayStartTechnique = VolumeEffect.GetTechniqueByName("RayStart");
			RayStartOutsidePass = RayStartTechnique.GetPassByName("Outside");
			RayStartInsidePass = RayStartTechnique.GetPassByName("Inside");

			VolumeLayout = new InputLayout(device, RayStartOutsidePass.Description.Signature, new[] {
               new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
               new InputElement("COLOR", 0, Format.R32G32B32_Float, 12, 0)			
            });


			RayDirectionTechnique = VolumeEffect.GetTechniqueByName("RayDirection");
			RayDirectionPass0 = RayDirectionTechnique.GetPassByIndex(0);
			//RayDirectionPass1 = RayDirectionTechnique.GetPassByIndex(1);

			BillboardTechnique = VolumeEffect.GetTechniqueByName("Final"); 
			BillboardPass0 = BillboardTechnique.GetPassByIndex(0);

			ImposterTechnique = VolumeEffect.GetTechniqueByName("Imposter");
			ImposterPass0 = ImposterTechnique.GetPassByIndex(0);
			
			BillboardLayout = new InputLayout(device, BillboardPass0.Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 8, 0)		
            });

			VolumeEffect.GetVariableByName("rayStart_texture").AsResource().SetResource(RayStartResourceView);
			VolumeEffect.GetVariableByName("rayDir_texture").AsResource().SetResource(RayDirectionResourceView);
			VolumeEffect.GetVariableByName("imposter").AsResource().SetResource(ImposterResourceView);
			VolumeEffect.GetVariableByName("volume_texture").AsResource().SetResource(VolumeTextureResourceView);

			LocationColor = VolumeEffect.GetVariableByName("locationColor").AsVector(); 

			#endregion
			*/
		//}
		/*
		private void CreateDepthBuffer(Device device, int width, int height)
		{
			var depthBufferDesc = new Texture2DDescription
			{
				ArraySize = 1,
				BindFlags = BindFlags.DepthStencil,
				CpuAccessFlags = CpuAccessFlags.None,
				Format = Format.D32_Float,				
				Width = width,
				Height = height,
				MipLevels = 1,
				OptionFlags = ResourceOptionFlags.None,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default
			};

			using (var depthBuffer = new Texture2D(Context10.Device, depthBufferDesc))
			{
				depthStencilView = new DepthStencilView(Context10.Device, depthBuffer);
			}
		}
		 * */ 

		/* 
		public void RenderVolumeTexture(Device device)
		{
			
			/ *device.Rasterizer.State = SlimDX.Direct3D11.RasterizerState.FromDescription(device, new RasterizerStateDescription()
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
			}); * /

			RenderTargetView[] backupTargets = device.OutputMerger.GetRenderTargets(1);
			Viewport[] backupViewPorts = device.Rasterizer.GetViewports(); 

			device.Rasterizer.SetViewports(VolumeViewport);
			device.InputAssembler.SetInputLayout(VolumeLayout);			

			device.OutputMerger.SetTargets(RayStartView);
			device.InputAssembler.SetVertexBuffers(0, VolumeBoundsVertsBindings);
			device.InputAssembler.SetIndexBuffer(VolumeBoundsIndices, Format.R32_UInt, 0);

			if (BoundingBox.Contains(VolumeBounds, Camera.Position) == ContainmentType.Contains)
			{
				float x, y, z;
				//float size = 1f / ((float)ExplicitMapObjects.Volume.Size * 16); 
				//float depth = 1f / ((float)ExplicitMapObjects.Volume.Depth * 16 * 8);

				//Vector3 vec = new Vector3(size * (Camera.Center.X / Camera.MapScale), size * (Camera.Center.Y / Camera.MapScale), depth * (Camera.Center.Z / Camera.MapScale)); 

				x = (1f / ((float)ExplicitMapObjects.Volume.Size * 16f)) * (Camera.Position.X / (Camera.MapScale));
				y = (1f / ((float)ExplicitMapObjects.Volume.Size * 16f)) * (Camera.Position.Y / (Camera.MapScale));
				z = (1f / ((float)ExplicitMapObjects.Volume.Depth * 8f * 16f)) * (Camera.Position.Z / (Camera.MapScale)); 

				//device.ClearRenderTargetView(RayStartView, new Color4(1, vec.X, vec.Y, vec.Z));* /

				DataStream stream = InsideVertices.Map(MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None);

				stream.WriteRange(new VolumeVertex[] {					
					new VolumeVertex() { Position = new Vector3( 1f, -1f, 0), Color = new Vector3(x, y, z) }, 
					new VolumeVertex() { Position = new Vector3(-1f, -1f, 0), Color = new Vector3(x, y, z) }, 
					new VolumeVertex() { Position = new Vector3( 1f,  1f, 0), Color = new Vector3(x, y, z) },  
					new VolumeVertex() { Position = new Vector3(-1f,  1f, 0), Color = new Vector3(x, y, z) } 
				});

				InsideVertices.Unmap(); 

				device.InputAssembler.SetVertexBuffers(0, InsideVerticesBindings);
				device.ClearRenderTargetView(RayStartView, Color.Black);
				device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);

				RayStartInsidePass.Apply();
				device.Draw(4, 0);

				device.InputAssembler.SetVertexBuffers(0, VolumeBoundsVertsBindings);
				device.InputAssembler.SetIndexBuffer(VolumeBoundsIndices, Format.R32_UInt, 0);
				device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
			}
			else
			{
				device.InputAssembler.SetVertexBuffers(0, VolumeBoundsVertsBindings);
				device.InputAssembler.SetIndexBuffer(VolumeBoundsIndices, Format.R32_UInt, 0);
				device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

				device.ClearRenderTargetView(RayStartView, Color.Black);

				RayStartOutsidePass.Apply();
				device.DrawIndexed(6 * 6, 0, 0);
			}

			device.OutputMerger.SetTargets(RayDirectionView);
			device.ClearRenderTargetView(RayDirectionView, Color.Black);

			RayDirectionPass0.Apply();
			device.DrawIndexed(6 * 6, 0, 0);
			//RayDirectionPass1.Apply();
			//device.DrawIndexed(6 * 6, 0, 0);

			device.OutputMerger.SetTargets(ImposterView);
			device.ClearRenderTargetView(ImposterView, Color.Black);

			device.InputAssembler.SetInputLayout(BillboardLayout);
			device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);
			device.InputAssembler.SetVertexBuffers(0, BillboardVerticesBindings);
			device.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);

			BillboardPass0.Apply();
			device.Draw(4, 0);

			device.OutputMerger.SetTargets(backupTargets);
			device.Rasterizer.SetViewports(backupViewPorts);
		}

		public void RenderImposter(Device device)
		{
			device.InputAssembler.SetInputLayout(BillboardLayout);
			device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);
			device.InputAssembler.SetVertexBuffers(0, BillboardVerticesBindings);
			device.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);

			ImposterPass0.Apply();
			device.Draw(4, 0);
		}

		*/ 

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
