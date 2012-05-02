using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework;
using RugTech1.Framework.Objects;
using SlimDX;
using System.Drawing;
using System.Windows.Forms;
using DS.Panels;
using DS.Scenes;
using Rug.Cmd;
using System.Threading;
using RugTech1.Framework.Effects;

namespace DS
{
	class MainWindow : GameBase
	{
		#region Private Members
		
		private List<IResourceManager> m_Resources = new List<IResourceManager>(); 		
		private View3D m_View;

		private SplashScreen m_SplashScreen;
		private SimulationScene m_SimulationScene;
		private SetupScene m_SetupScene;

		#endregion 

		#region Load Configuration
		
		protected override void LoadConfiguration()
		{
			ArtworkStaticObjects.Initialize();
			ArtworkStaticObjects.LoadConfig(ArtworkStaticObjects.DefaultConfigFilePath);

			base.LoadConfiguration();
		}

		#endregion

		#region On Initialize
		
		protected override void OnInitialize()
		{			
			Thread.CurrentThread.Priority = ThreadPriority.Highest;

			SharedEffects.Effects.Add("ImposterFloat", new ImposterFloatEffect());
			SharedEffects.Effects.Add("ImposterFloatPalette", new ImposterFloatPaletteEffect()); 
			SharedEffects.Effects.Add("WarpGridEffect", new WarpGridEffect()); 
			SharedEffects.Effects.Add("Particle2", new ParticleEffect2()); 
			
			base.OnInitialize();
			
			m_Resources.Add(ArtworkStaticObjects.KinectDevices); 

			this.BackgroundColor = new Color4(0, 0, 0, 0);
			
			GameConfiguration.ActiveRegion = new Rectangle(0, 0, WindowWidth, WindowHeight);
						
			m_View = new View3D(GameConfiguration.ActiveRegion, WindowWidth, WindowHeight, (float)Math.PI / 4, 1f);

			ArtworkStaticObjects.View = m_View;
			ArtworkStaticObjects.View.Projection = Matrix.OrthoLH(2, 2, 0, 10);
			ArtworkStaticObjects.View.View = Matrix.Translation(-1, 0, 0); 

			m_SimulationScene = new SimulationScene();
			m_Resources.Add(m_SimulationScene);

			m_SetupScene = new SetupScene();
			m_Resources.Add(m_SetupScene);

			m_SplashScreen = new SplashScreen(m_SetupScene);
			m_SplashScreen.Initialize();
			m_Resources.Add(m_SplashScreen); 		
		}

		#endregion

		#region On Resize

		protected override void OnResize()
		{
			base.OnResize();

			GameConfiguration.ActiveRegion = new Rectangle(0, 0, WindowWidth, WindowHeight);
			m_View.Resize(GameConfiguration.ActiveRegion, WindowWidth, WindowHeight);

			m_SplashScreen.Resize(); 
			m_SplashScreen.Invalidate();


			RC.WriteLine(ConsoleVerbosity.Normal, ConsoleThemeColor.WarningColor1, "Window resized: " + WindowWidth + "x" + WindowHeight);
		}

		#endregion

		#region Resource Management
		
		protected override void OnDispose()
		{
			base.OnDispose();

			foreach (IResourceManager manager in m_Resources)
			{
				manager.Dispose();
			}

			ArtworkStaticObjects.Dispose();
			ArtworkStaticObjects.SaveConfig(ArtworkStaticObjects.DefaultConfigFilePath);
		}

		protected override void OnResourceLoad()
		{
			base.OnResourceLoad();

			foreach (IResourceManager manager in m_Resources)
			{
				manager.LoadResources();
			}
		}

		protected override void OnResourceUnload()
		{
			base.OnResourceUnload();

			foreach (IResourceManager manager in m_Resources)
			{
				manager.UnloadResources();
			}
		}

		#endregion

		#region Update / Render

		protected override void OnUpdate()
		{
			base.OnUpdate();

			m_SplashScreen.Update();			
		}

		protected override void OnRenderBegin()
		{
			ArtworkStaticObjects.KinectDevices.TryProcessFrame();

			base.OnRenderBegin();

			GameEnvironment.Device.ImmediateContext.Rasterizer.SetViewports(m_View.Viewport);
		}

		protected override void OnRender()
		{
			base.OnRender();

			if (AppContext.Count == 1)
			{				
				if (m_SplashScreen.IsVisible == true && (
					m_SplashScreen.CurrentPanel == SplashSceenPanels.Cameras ||
					m_SplashScreen.CurrentPanel == SplashSceenPanels.FieldData))
				{
					m_SetupScene.Render(m_View);
				}
				else
				{
					m_SimulationScene.Render(AppContext[0], m_View);
				}

				if (m_SplashScreen.IsVisible == true)
				{
					m_SplashScreen.Render(m_View);
				}
			}
			else
			{
				if (m_SplashScreen.IsVisible == true)
				{
					m_SplashScreen.Render(m_View);
				}				

				/* 
				if (m_SplashScreen.IsVisible == true && (
					m_SplashScreen.CurrentPanel == SplashSceenPanels.Cameras ||
					m_SplashScreen.CurrentPanel == SplashSceenPanels.FieldData))
				{
					m_SetupScene.Render(m_View);
				}
				*/ 

				m_SimulationScene.Render(AppContext[1], m_View);
			}
		}

		protected override void OnRenderEnd()
		{
			base.OnRenderEnd();
			
			ArtworkStaticObjects.KinectDevices.ResetFrameState(); 
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
			else if (e.KeyCode == Keys.Tab)
			{
				m_SplashScreen.IsVisible = !m_SplashScreen.IsVisible; 
			}

			if (m_SplashScreen.IsVisible == false)
			{
				return;
			}
			
			bool shouldUpdate = false;

			m_SplashScreen.OnKeyDown(e, out shouldUpdate);

			if (shouldUpdate == true)
			{
				m_SplashScreen.Invalidate();
			}
		}

		protected override void HandleKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			base.HandleKeyPress(sender, e);

			if (m_SplashScreen.IsVisible == false)
			{
				return;
			}

			bool shouldUpdate;

			m_SplashScreen.OnKeyPress(e.KeyChar, out shouldUpdate);

			if (shouldUpdate == true)
			{
				m_SplashScreen.Invalidate();
			}
		}

		protected override void HandleKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			base.HandleKeyUp(sender, e);

			if (m_SplashScreen.IsVisible == false)
			{
				return;
			}

			bool shouldUpdate;
			m_SplashScreen.OnKeyUp(e, out shouldUpdate);

			if (shouldUpdate == true)
			{
				m_SplashScreen.Invalidate();
			}
		}

		#endregion

		#region Handle Mouse

		protected override void HandleMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			base.HandleMouseClick(sender, e);

			if (m_SplashScreen.IsVisible == false)
			{
				return;
			}

			if (sender != AppContext[0].Form)
			{
				return;
			}

			bool shouldUpdate = false;
			//Vector2 mousePos = new Vector2((float)e.X, (float)e.Y);

			Vector2 mousePos = m_View.TransformMouseCoords(sender as Form, e); 

			m_SplashScreen.OnMouseClick(m_View, mousePos, e.Button, out shouldUpdate);

			if (shouldUpdate == true)
			{
				m_SplashScreen.Invalidate();
			}
		}

		protected override void HandleMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			base.HandleMouseDown(sender, e);

			if (m_SplashScreen.IsVisible == false)
			{
				return;
			}

			if (sender != AppContext[0].Form)
			{
				return;
			}

			bool shouldUpdate = false;
			//Vector2 mousePos = new Vector2((float)e.X, (float)e.Y);
			
			Vector2 mousePos = m_View.TransformMouseCoords(sender as Form, e); 

			m_SplashScreen.OnMouseDown(m_View, mousePos, e.Button, out shouldUpdate);

			if (shouldUpdate == true)
			{
				m_SplashScreen.Invalidate();
			}
		}

		protected override void HandleMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			base.HandleMouseMove(sender, e);

			if (m_SplashScreen.IsVisible == false)
			{
				return;
			}

			if (sender != AppContext[0].Form)
			{
				return;
			}

			bool shouldUpdate = false;
			//Vector2 mousePos = new Vector2((float)e.X, (float)e.Y);
			Vector2 mousePos = m_View.TransformMouseCoords(sender as Form, e); 

			m_SplashScreen.OnMouseMoved(m_View, mousePos, out shouldUpdate);

			if (shouldUpdate == true)
			{
				m_SplashScreen.Invalidate();
			}
		}

		protected override void HandleMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			base.HandleMouseUp(sender, e);

			if (m_SplashScreen.IsVisible == false)
			{
				return;
			}

			if (sender != AppContext[0].Form)
			{
				return;
			}

			bool shouldUpdate = false;
			//Vector2 mousePos = new Vector2((float)e.X, (float)e.Y);
			Vector2 mousePos = m_View.TransformMouseCoords(sender as Form, e); 

			m_SplashScreen.OnMouseUp(m_View, mousePos, e.Button, out shouldUpdate);

			if (shouldUpdate == true)
			{
				m_SplashScreen.Invalidate();
			}
		}

		#endregion
	}
}
