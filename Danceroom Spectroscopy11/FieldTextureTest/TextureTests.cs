using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework;
using RugTech1.Framework.Objects;
using RugTech1.Framework.Objects.Simple;
using SlimDX;
using System.Drawing;
using System.Windows.Forms;
using SlimDX.Direct3D11;

namespace FieldTextureTest
{
	class TextureTests : GameBase
	{
		private View3D m_View;
		private TestImage2 m_TestImage;
		private Imposter2 m_Imposter;
		private Texture2D m_Texture;
		private SlimDX.DXGI.Surface m_TextureSurface;
		private ShaderResourceView m_TextureResourceView;
		private TestFloatScene m_Scene = new TestFloatScene(); 

		protected override void LoadConfiguration()
		{
			base.LoadConfiguration();
		}

		protected override void OnInitialize()
		{
			base.OnInitialize();

			this.BackgroundColor = new Color4(0, 0, 0, 0);
			
			GameConfiguration.ActiveRegion = new Rectangle(0, 0, WindowWidth, WindowHeight);

			m_View = new View3D(GameConfiguration.ActiveRegion, WindowWidth, WindowHeight, (float)Math.PI / 4, 1f);

			m_TestImage = new TestImage2(null);

			m_Imposter = new Imposter2(640, 480, SlimDX.DXGI.Format.R32_Float, new Color4(0, 0, 0, 0), RugTech1.Framework.Effects.ImposterOverlayType.None);

			m_Scene.m_TestImage = m_TestImage; 
		}

		protected override void OnResize()
		{
			base.OnResize();
			
			GameConfiguration.ActiveRegion = new Rectangle(0, 0, WindowWidth, WindowHeight);

			m_View.Resize(GameConfiguration.ActiveRegion, WindowWidth, WindowHeight);
		}

		protected override void OnDispose()
		{
			base.OnDispose();
		}

		protected override void OnResourceLoad()
		{
			base.OnResourceLoad();

			m_Imposter.LoadResources(); 

			m_Texture = new Texture2D(GameEnvironment.Device, new Texture2DDescription()
			{
				Format = SlimDX.DXGI.Format.R32_Float,
				Width = 640,
				Height = 480,
				MipLevels = 1,
				ArraySize = 1,
				BindFlags = BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.Write,
				OptionFlags = ResourceOptionFlags.None,
				Usage = ResourceUsage.Dynamic,
				SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0)
			});

			m_TextureSurface = m_Texture.AsSurface();

			DataRectangle data = m_TextureSurface.Map(SlimDX.DXGI.MapFlags.Discard | SlimDX.DXGI.MapFlags.Write);

			//int rowSize = (m_Texture.Description.Width * 4);
			int stride = data.Pitch / 4;
			int stripeWidth = 16;
			int stripeValue = 0;
			bool stripeOn = false; 
			for (int i = 0, ie = m_Texture.Description.Height; i < ie; i++)
			{
				stripeValue = 0;
				stripeOn = false; 
				for (int p = 0; p < stride; p++)
				{
					if (stripeValue++ >= stripeWidth)
					{
						stripeOn = !stripeOn;
						stripeValue = 0; 
					}

					if (stripeOn == true)
					{
						data.Data.Write(1f);
					}
					else
					{
						data.Data.Write(0f);
					}
				}
			}

			m_TextureSurface.Unmap();

			m_TextureResourceView = new ShaderResourceView(GameEnvironment.Device, m_Texture, new ShaderResourceViewDescription()
			{
				Dimension = ShaderResourceViewDimension.Texture2D,
				Format = SlimDX.DXGI.Format.R32_Float,
				ArraySize = 1,
				MipLevels = 1,
				MostDetailedMip = 0,
			});

			m_TestImage.TextureView = m_TextureResourceView;

			m_TestImage.LoadResources();
		}

		protected override void OnResourceUnload()
		{
			base.OnResourceUnload();

			m_Imposter.UnloadResources(); 

			m_TextureResourceView.Dispose(); 

			m_TestImage.UnloadResources();
			
			m_Texture.Dispose();
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
		}

		protected override void OnRenderBegin()
		{
			base.OnRenderBegin();

			GameEnvironment.Device.ImmediateContext.Rasterizer.SetViewports(m_View.Viewport);
		}

		protected override void OnRender()
		{
			base.OnRender();

			m_Imposter.RenderToImposter(m_Scene, m_View);

			m_Imposter.Render(); 			
		}

		protected override void OnRenderEnd()
		{
			base.OnRenderEnd();
		}

		#region Handle Keys

		protected override void HandleKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			base.HandleKeyDown(sender, e);

			if (e.KeyCode == Keys.Escape)
			{
				this.Quit();
				return;
			}
		}

		protected override void HandleKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			base.HandleKeyPress(sender, e);
		}

		protected override void HandleKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			base.HandleKeyUp(sender, e);
		}

		#endregion

		#region Handle Mouse

		protected override void HandleMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			base.HandleMouseClick(sender, e);
		}

		protected override void HandleMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			base.HandleMouseDown(sender, e);
		}

		protected override void HandleMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			base.HandleMouseMove(sender, e);
		}

		protected override void HandleMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			base.HandleMouseUp(sender, e);
		}

		#endregion
	}
}
