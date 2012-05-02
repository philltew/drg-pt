using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Experiments.Effects;
using Experiments.Objects;
using Experiments.Panels;
using RugTech1.Framework;
using RugTech1.Framework.Effects;
using RugTech1.Framework.Objects;
using RugTech1.Framework.Objects.Simple;
using RugTech1.Framework.Objects.Util;
using Rvg.Win32.Avi;
using SlimDX;
using SlimDX.Direct3D10;
using Format = SlimDX.DXGI.Format; 

namespace Experiments
{
	class ExperimentApp : GameBase
	{
		#region Private Members
		
		private SplashScreen m_Splash;
		private View3D m_View;
		private ScreenCapture m_ScreenCapture;

		private ExperimentScene m_Scene;
		private Imposter m_Imposter;		
		private BrightPassView m_BrightPassView;
		private BloomChain m_Bloom;

		#endregion

		#region On Initialize

		public ExperimentApp()
		{
			//SharedEffects.Effects.Add("Diffuse", new Diffuse());
			//SharedEffects.Effects.Add("SkyBox", new SkyBoxEffect());

			AVIWriter.Initiate();
		}

		protected override void OnInitialize()
		{
			base.OnInitialize();

			this.BackgroundColor = new Color4(0, 0, 0, 0);

			m_View = new View3D(WindowWidth, WindowHeight, (float)Math.PI / 4, 1f);			

			m_Splash = new SplashScreen();			

			m_Splash.Initialize();

			m_Scene = new ExperimentScene();
			m_Imposter = new Imposter(WindowWidth, WindowHeight, Format.R16G16B16A16_Float, new Color4(0f, 0f, 0f, 0f), ImposterOverlayType.None);
			m_BrightPassView = new BrightPassView(WindowWidth / 2, WindowHeight / 2, Format.R16G16B16A16_Float, new Color4(0f, 0f, 0f, 0f), ImposterOverlayType.None_BrightPass);
			m_Bloom = new BloomChain(WindowWidth / 2, WindowHeight / 2, Format.R16G16B16A16_Float);

			m_Splash.IsVisible = true;

			m_ScreenCapture = new ScreenCapture();

			if (m_Splash.IsVisible == false)
			{ 
				Cursor.Hide();
			}			
		}

		protected override void OnDispose()
		{
			Cursor.Show(); 
	
			base.OnDispose();

			AVIWriter.Shutdown(); 
		}

		#endregion

		#region Load Configuration

		protected override void LoadConfiguration()
		{
			base.LoadConfiguration();
		}

		#endregion

		#region OnResize
		
		protected override void OnResize()
		{
			base.OnResize();

			m_Imposter.Resize(WindowWidth, WindowHeight);
			m_Bloom.Resize(WindowWidth / 2, WindowHeight / 2);
			m_BrightPassView.Resize(WindowWidth / 2, WindowHeight / 2);
			m_View.Resize(WindowWidth, WindowHeight);

			m_Splash.Invalidate(); 		
		}

		#endregion

		#region On Resource Load

		protected override void OnResourceLoad()
		{
			Cursor.Show(); 

			base.OnResourceLoad();
			
			m_ScreenCapture.Description = new Texture2DDescription()
			{
				ArraySize = 1,
				BindFlags = BindFlags.None, 
				CpuAccessFlags= CpuAccessFlags.Read,
				Format = GameEnvironment.RenderView.Description.Format,
				Height = GameConfiguration.WindowHeight, 
				Width = GameConfiguration.WindowWidth, 
				MipLevels = 1, 
				OptionFlags = ResourceOptionFlags.None, 
				SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0),
				Usage = ResourceUsage.Staging			 
			};
			m_ScreenCapture.PixelSizeInBytes = 3; 

			m_ScreenCapture.LoadResources();			

			m_Splash.LoadResources();
			m_Splash.Invalidate(); 

			m_Bloom.LoadResources();
			m_BrightPassView.LoadResources();

			m_Imposter.LoadResources();
			m_Imposter.UseDepth = true;
			m_Imposter.DepthState = GameEnvironment.DepthState;
			m_Imposter.DepthView = GameEnvironment.DepthView;

			if (m_Splash.IsVisible == false)
			{
				Cursor.Hide();
			}
		}

		#endregion

		#region On Resource Unload

		protected override void OnResourceUnload()
		{
			base.OnResourceUnload();

			m_ScreenCapture.UnloadResources(); 

			m_Splash.UnloadResources();
			
			m_Imposter.UnloadResources();
			m_Bloom.UnloadResources();
			m_BrightPassView.UnloadResources();		
		}

		#endregion

		#region On Update

		protected override void OnUpdate()
		{
			base.OnUpdate();
		}

		#endregion

		#region On Render

		protected override void OnRender()
		{
			base.OnRender();

			m_Imposter.RenderToImposter(m_Scene, m_View);						
			m_Imposter.Render();

			m_Splash.Update(); 

			if (m_Splash.IsVisible == true)
			{
				GameEnvironment.Device.OutputMerger.SetTargets(GameEnvironment.DepthView, GameEnvironment.RenderView);
				GameEnvironment.Device.ClearDepthStencilView(GameEnvironment.DepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
				GameEnvironment.Device.OutputMerger.DepthStencilState = GameEnvironment.DepthState;

				m_Splash.Render(m_View);
			}
		}

		protected override void OnRenderEnd()
		{
			base.OnRenderEnd();
		}

		#endregion

		#region Handle Keys

		protected override void HandleKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			base.HandleKeyDown(sender, e);

			if (e.KeyCode == Keys.Escape)
			{
				this.Quit();
				return; 
			}

			if (e.KeyCode == System.Windows.Forms.Keys.Tab)
			{
				m_Splash.IsVisible = !m_Splash.IsVisible;

				if (m_Splash.IsVisible == true)
				{
					Cursor.Show();
				}
				else
				{
					Cursor.Hide();
				}
			}

			bool shouldUpdate = false;

			if (m_Splash.IsVisible == false)
			{
				//m_CameraControl.OnKeyDown(e);
			}
			else 
			{				
				m_Splash.OnKeyDown(e, out shouldUpdate);

				if (shouldUpdate == true)
				{
					m_Splash.Invalidate();
				}
			}
		}

		protected override void HandleKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			base.HandleKeyPress(sender, e);

			if (m_Splash.IsVisible == true)
			{
				bool shouldUpdate;

				m_Splash.OnKeyPress(e.KeyChar, out shouldUpdate);

				if (shouldUpdate == true)
				{
					m_Splash.Invalidate();
				}
			}
		}

		protected override void HandleKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			base.HandleKeyUp(sender, e);

			if (m_Splash.IsVisible == false)
			{
				//m_CameraControl.OnKeyUp(e);
			}
			else
			{
				bool shouldUpdate;

				m_Splash.OnKeyUp(e, out shouldUpdate);

				if (shouldUpdate == true)
				{
					m_Splash.Invalidate();
				}
			}
		}

		#endregion

		#region Handle Mouse

		protected override void HandleMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			base.HandleMouseClick(sender, e);

			if (m_Splash.IsVisible == true)
			{
				bool shouldUpdate = false;
				Vector2 mousePos = new Vector2((float)e.X, (float)e.Y);

				m_Splash.OnMouseClick(mousePos, e.Button, out shouldUpdate);

				if (shouldUpdate == true)
				{
					m_Splash.Invalidate();
				}
			}
		}

		protected override void HandleMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			base.HandleMouseDown(sender, e);

			if (m_Splash.IsVisible == true)
			{
				bool shouldUpdate = false;
				Vector2 mousePos = new Vector2((float)e.X, (float)e.Y);

				m_Splash.OnMouseDown(mousePos, e.Button, out shouldUpdate);

				if (shouldUpdate == true)
				{
					m_Splash.Invalidate();
				}
			}
			else
			{
				//m_CameraControl.OnMouseDown(e);
			}
		}

		private bool m_DissableMouseMove = false; 

		protected override void HandleMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			base.HandleMouseMove(sender, e);

			if (m_Splash.IsVisible == true)
			{
				bool shouldUpdate = false;
				Vector2 mousePos = new Vector2((float)e.X, (float)e.Y);

				m_Splash.OnMouseMoved(mousePos, out shouldUpdate);

				if (shouldUpdate == true)
				{
					m_Splash.Invalidate();
				}
			}
			else
			{
				/*Point center = new Point(GameEnvironment.Form.Bounds.X + (GameEnvironment.Form.Bounds.Width / 2), GameEnvironment.Form.Bounds.Y + (GameEnvironment.Form.Bounds.Height / 2));

				if (Cursor.Position != center && GameEnvironment.Form.PointToScreen(e.Location) != center)
				{
					m_DissableMouseMove = true; 
					m_CameraControl.OnMouseMove(e, GameEnvironment.Form.PointToClient(center));
					Cursor.Position = center;
				}*/ 					
			}
		}

		protected override void HandleMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			base.HandleMouseUp(sender, e);

			if (m_Splash.IsVisible == true)
			{
				bool shouldUpdate = false;
				Vector2 mousePos = new Vector2((float)e.X, (float)e.Y);

				m_Splash.OnMouseUp(mousePos, e.Button, out shouldUpdate);

				if (shouldUpdate == true)
				{
					m_Splash.Invalidate();
				}
			}
			else
			{
				//m_CameraControl.OnMouseUp(e);
			}
		}

		#endregion
	}
}
