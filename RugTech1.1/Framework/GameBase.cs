using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Rug.Cmd;
using RugTech1.Framework.Contextual;
using RugTech1.Framework.Effects;
using RugTech1.Framework.Objects;
using RugTech1.Framework.Objects.Simple;
using RugTech1.Framework.Objects.UI;
using RugTech1.Framework.Test;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;

namespace RugTech1.Framework
{
	public class GameBase : IDisposable
	{
		#region Private Members

		private AppContext m_AppContext;
		private GameDeviceContext m_DeviceContext;
		private FormWindowState m_CurrentFormWindowState;

		private readonly Clock m_Clock = new Clock();		
		private float m_FrameAccumulator;
		private int m_FrameCount;
		private bool m_DeviceLost = false;		

		private Color4 m_BackgroundColor = new Color4(0, 0, 0, 0);

		private bool m_CheckForResize = false;
		private bool m_WasMinimized = false;
		private bool m_CloseNow = false;
		private bool m_IsResizing = false; 
		
		#endregion

		public Color4 BackgroundColor { get { return m_BackgroundColor; } set { m_BackgroundColor = value; } }

		#region Config Properties

		public int WindowWidth
		{
			get { return GameConfiguration.WindowWidth; }
			set { GameConfiguration.WindowWidth = value; }
		}

		public int WindowHeight
		{
			get { return GameConfiguration.WindowHeight; }
			set { GameConfiguration.WindowHeight = value; }
		}

		public bool IsFullScreen
		{
			get { return GameConfiguration.IsFullScreen; }
			set { GameConfiguration.IsFullScreen = value; }
		}

		#endregion

		#region Runtime Properties

		public float FrameDelta 
		{ 
			get { return GameEnvironment.FrameDelta; } 
			set { GameEnvironment.FrameDelta = value; } 
		}

		public float FramesPerSecond
		{
			get { return GameEnvironment.FramesPerSecond; }
			set { GameEnvironment.FramesPerSecond = value; }
		}


		public bool FramesClick
		{
			get { return GameEnvironment.FramesClick; }
			set { GameEnvironment.FramesClick = value; }
		}

		public GameDeviceContext DeviceContext
		{
			get { return m_DeviceContext; }
			set { m_DeviceContext = value; }
		}

		public AppContext AppContext { get { return m_AppContext; } }

		#endregion

		#region Create

		private void InitializeDevice()
		{
#if DEBUG
			RC.WriteLine(ConsoleThemeColor.SubText3, "Initialize Device");
#endif
			m_DeviceContext = new Framework.Contextual.GameDeviceContext(m_AppContext); 

			GameEnvironment.Device = m_DeviceContext.Device; 
		}

		#endregion

		#region Dispose

		public void Quit()
		{
			m_CloseNow = true;
		}

		~GameBase()
		{
			Dispose(false);
		}

		/// <summary>
		/// Disposes of object resources.
		/// </summary>
		public void Dispose()
		{
			OnDispose(); 

			Dispose(true);

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposes of object resources.
		/// </summary>
		/// <param name="disposeManagedResources">If true, managed resources should be
		/// disposed of in addition to unmanaged resources.</param>
		protected virtual void Dispose(bool disposeManagedResources)
		{
			if (disposeManagedResources)
			{
				SharedEffects.Dispose();

				if (DeviceContext != null)
				{
					DeviceContext.Dispose();
				}
				
				if (m_AppContext != null) 
				{
					m_AppContext.Dispose(); 
				} 
	
				GameEnvironment.Device = null; 

				// m_Forms.Dispose();
			}
		}

		#endregion

		#region Run

		/// <summary>
		/// Runs the sample.
		/// </summary>
		public void Run()
		{
			LoadConfiguration();

			m_AppContext = new Framework.Contextual.AppContext(320, 240); 

			GameEnvironment.Form = m_AppContext[0].Form;

			m_CurrentFormWindowState = GameEnvironment.Form.WindowState;

			bool IsFormClosed = false;
			bool FormIsResizing = false;

			InitializeDevice();

			foreach (FormContext context in m_AppContext)
			{
				RenderForm form = context.Form;

				form.MouseDown += new MouseEventHandler(HandleMouseDown);
				form.MouseUp += new MouseEventHandler(HandleMouseUp);
				form.MouseClick += new MouseEventHandler(HandleMouseClick);
				form.MouseMove += new MouseEventHandler(HandleMouseMove);

				form.KeyDown += new KeyEventHandler(HandleKeyDown);
				form.KeyUp += new KeyEventHandler(HandleKeyUp);
				form.KeyPress += new KeyPressEventHandler(HandleKeyPress);

				form.Resize += (o, args) =>
				{
					if (m_IsResizing == false)
					{
						if (form.WindowState != m_CurrentFormWindowState)
						{
							HandleResize(o, args);
						}

						m_CurrentFormWindowState = form.WindowState;
					}
				};

				form.ResizeBegin += (o, args) => { if (m_IsResizing == false) FormIsResizing = true; };
				form.ResizeEnd += (o, args) =>
				{
					if (m_IsResizing == false)
					{
						FormIsResizing = false;
						HandleResize(o, args);
					}
				};

				form.Closed += (o, args) => { IsFormClosed = true; };
			}

			SharedEffects.AddEffects();

			OnInitialize();

			LoadResources();

			m_Clock.Start();

			m_CheckForResize = true; 

			MessagePump.Run(m_AppContext.MainForm, () =>
			{
				if (m_CloseNow == true)
				{
					foreach (FormContext context in m_AppContext)
					{
						context.Form.Close(); 
					}

					return;
				}

				if (IsFormClosed)
				{
					return;
				}

				Update();

				if (!FormIsResizing)
				{
					Render();

					if (m_CheckForResize == true)
					{
						m_CheckForResize = false;

						foreach (FormContext context in m_AppContext)
						{
							if (WindowWidth != context.Form.ClientSize.Width ||
								WindowHeight != context.Form.ClientSize.Height)
							{
								HandleResize(null, EventArgs.Empty);
							}
						}
					}
				}
			});

			try
			{
				UnloadResources();
			}
			catch
			{

			}
		}

		private void Update()
		{
			FrameDelta = m_Clock.Update();

			OnUpdate();
		}

		private void Render()
		{
			if (m_DeviceLost)
			{
				Thread.Sleep(100);
			}

			m_FrameAccumulator += FrameDelta;

			++m_FrameCount;

			if (m_FrameAccumulator >= 1.0f)
			{
				FramesPerSecond = m_FrameCount / m_FrameAccumulator;
				FramesClick = true;
				m_FrameAccumulator = 0.0f;
				m_FrameCount = 0;

			}
			else
			{
				FramesClick = false; 
			}

			OnRenderBegin();

			OnRender();

			OnRenderEnd();
		}

		#endregion

		#region Form Events

		#region Mouse Events

		protected virtual void HandleMouseDown(object sender, MouseEventArgs e)
		{

		}

		protected virtual void HandleMouseUp(object sender, MouseEventArgs e)
		{

		}

		protected virtual void HandleMouseMove(object sender, MouseEventArgs e)
		{

		}

		protected virtual void HandleMouseClick(object sender, MouseEventArgs e)
		{

		}

		#endregion

		#region Key Events

		protected virtual void HandleKeyDown(object sender, KeyEventArgs e) 
		{

		}

		protected virtual void HandleKeyUp(object sender, KeyEventArgs e)
		{			
			if (e.Alt && e.KeyCode == Keys.Enter)
			{
				OnResolutionChanged(!IsFullScreen);
			}
		}

		protected virtual void HandleKeyPress(object sender, KeyPressEventArgs e)
		{

		}

		#endregion

		#region Resize Events

		private void HandleResize(object sender, EventArgs e)
		{			
			if (AppContext[0].Form.WindowState == FormWindowState.Minimized)
			{
				m_WasMinimized = true;
				return;
			}

			if (m_WasMinimized == false &&
				//SharedEffects.Disposed == false && 
				!AppContext.NeedsResize(WindowWidth, WindowHeight))
			{
				return;
			}

			try
			{
				m_IsResizing = true;
				m_WasMinimized = false;

				UnloadResources();

				Form form = sender as Form;

				if (form != null)
				{
					WindowWidth = form.ClientSize.Width;
					WindowHeight = form.ClientSize.Height;
				}

				int maxWidth = WindowWidth;
				int maxHeight = WindowHeight; 

				foreach (FormContext context in AppContext)
				{
					Size newSize = context.GetModeSize(WindowWidth, WindowHeight);

					if (newSize.Width < maxWidth || newSize.Height < maxHeight)
					{
						maxWidth = newSize.Width;
						maxHeight = newSize.Height; 
					}
				}
				
				WindowWidth = maxWidth; // newSize.Width;
				WindowHeight = maxHeight; // newSize.Height;

				foreach (FormContext context in AppContext)
				{
					context.Resize(WindowWidth, WindowHeight); 
				}

				OnResize();

				m_CheckForResize = true;
				
				LoadResources();

				m_IsResizing = false;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Exception: " + ex.Message);
			} 
		}

		#endregion

		#endregion

		#region Events To Override

		#region Configuration

		protected virtual void LoadConfiguration() { }

		protected virtual void OnConfigurationChanged() { }

		#endregion

		#region On Initialize

		protected virtual void OnInitialize() { }

		#endregion

		#region On Dispose

		protected virtual void OnDispose() { }

		#endregion

		#region Resize

		private void OnResolutionChanged(bool fullscreen)
		{
			UnloadResources();

			IsFullScreen = fullscreen;			
			 
			if (IsFullScreen)
			{
				//Size newSize = DeviceContext.GetModeSize(WindowWidth, WindowHeight);
				Size newSize = AppContext[0].GetModeSize(WindowWidth, WindowHeight); 

				WindowWidth = newSize.Width;
				WindowHeight = newSize.Height;
			}
			else
			{
				WindowWidth = AppContext[0].Form.ClientSize.Width;
				WindowHeight = AppContext[0].Form.ClientSize.Height;
			}

			m_IsResizing = true; 

			try
			{
				foreach (FormContext context in AppContext)
				{
					context.Resize(WindowWidth, WindowHeight, IsFullScreen);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			

			OnResize();

			m_CheckForResize = true;

			LoadResources();

			m_IsResizing = false; 
		}

		protected virtual void OnResize() { }

		#endregion

		#region Resources

		private void LoadResources()
		{
			SharedEffects.LoadResources();

			m_AppContext.LoadResources();
			
			OnResourceLoad(); 
		}

		protected virtual void OnResourceLoad() { }

		private void UnloadResources()
		{
			OnResourceUnload(); 

			SharedEffects.UnloadResources();

			m_AppContext.UnloadResources();
		}

		protected virtual void OnResourceUnload() { }

		#endregion

		#region Update

		protected virtual void OnUpdate() { }

		#endregion

		#region Render

		protected virtual void OnRenderBegin()
		{
			AppContext[0].RenderBegin(); 
		}

		protected virtual void OnRender() { }

		protected virtual void OnRenderEnd()
		{
			foreach (FormContext context in AppContext)
			{
				context.RenderEnd(); 
			}
		}

		#endregion

		#endregion
	}
}
