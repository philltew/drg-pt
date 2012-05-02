using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rug.Cmd;
using RugTech1.Framework.Objects;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;

namespace RugTech1.Framework.Contextual
{
	public class FormContext : IResourceManager
	{
		#region Private Members
		
		private bool m_Disposed = true; 

		private RenderForm m_Form;

		private SwapChainDescription m_SwapChainDescription;
		private SwapChain m_SwapChain;

		private FormWindowState m_CurrentFormWindowState;

		private Texture2D m_BackBuffer;		
		private RenderTargetView m_RenderView;

		private Texture2D m_DepthBuffer;

		private DepthStencilView m_DepthView;

		private DepthStencilState m_DepthState;

		private Color4 m_BackgroundColor = new Color4(0, 0, 0, 0);		

		private bool m_CheckForResize = false;
		private bool m_WasMinimized = false;
		private bool m_CloseNow = false;
		private int m_Index;
		
		#endregion

		#region Public Properties

		public int Index { get { return m_Index; } }
		
		public RenderForm Form { get { return m_Form; } }

		public SwapChainDescription SwapChainDescription { get { return m_SwapChainDescription; } }

		public SwapChain SwapChain { get { return m_SwapChain; } set { m_SwapChain = value; } }

		public Texture2D BackBuffer { get { return m_BackBuffer; } }

		public RenderTargetView RenderView { get { return m_RenderView; } }

		public Texture2D DepthBuffer { get { return m_DepthBuffer; } }

		public DepthStencilView DepthView { get { return m_DepthView; } }

		public DepthStencilState DepthState { get { return m_DepthState; } }

		public Color4 BackgroundColor { get { return m_BackgroundColor; } set { m_BackgroundColor = value; } }

		#endregion

		public FormContext(int index, string title, int windowWidth, int windowHeight)
		{
			m_Index = index;

#if DEBUG
			RC.WriteLine(ConsoleThemeColor.SubText3, "RenderContext [" + m_Index + "]");
#endif

			m_Form = new RenderForm(title)
			{
				ClientSize = new Size(windowWidth, windowHeight)
			};

			m_SwapChainDescription = new SwapChainDescription
			{
				BufferCount = 1,
				Flags = SwapChainFlags.AllowModeSwitch,
				IsWindowed = true,
				ModeDescription = new ModeDescription(m_Form.ClientSize.Width, m_Form.ClientSize.Height, new Rational(GameConfiguration.DesiredFrameRate, 1), Format.R8G8B8A8_UNorm),
				OutputHandle = m_Form.Handle,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = SwapEffect.Discard,
				Usage = Usage.RenderTargetOutput
			};
		}

		public System.Drawing.Size GetModeSize(int windowWidth, int windowHeight)
		{
			ModeDescription currentMode = m_SwapChain.Description.ModeDescription;
			ModeDescription resultMode;

			GameEnvironment.Device.Factory.GetAdapter(GameConfiguration.AdapterOrdinal).GetOutput(m_Index).GetClosestMatchingMode(GameEnvironment.Device, new ModeDescription(windowWidth, windowHeight, currentMode.RefreshRate, currentMode.Format), out resultMode);

#if DEBUG
			RC.WriteLine(ConsoleThemeColor.SubText3, "RenderContext [" + m_Index + "]: Desired Mode (" + windowWidth + "x" + windowHeight + "), Nearest Mode (" + resultMode.Width + "x" + resultMode.Height + ")");
#endif

			return new System.Drawing.Size(resultMode.Width, resultMode.Height);
		}

		public void RenderBegin()
		{
			if (m_Disposed == true)
			{
				RC.WriteError(555, "Render Context [" + m_Index + "] Has Been Disposed"); 
			}

			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext;

			Viewport port = new Viewport(0, 0, m_Form.ClientSize.Width, m_Form.ClientSize.Height, 0.0f, 1.0f);

			context.OutputMerger.SetTargets(m_DepthView, m_RenderView);
			context.Rasterizer.SetViewports(port);

			context.ClearRenderTargetView(m_RenderView, m_BackgroundColor);
			ClearDepthBuffer();
		}

		public void ClearDepthBuffer()
		{
			GameEnvironment.Device.ImmediateContext.ClearDepthStencilView(m_DepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
		}

		public void RenderEnd()
		{
			m_SwapChain.Present(GameConfiguration.LockFrameRate ? 1 : 0, PresentFlags.None);
		}

		#region IResourceManager Members

		public bool Disposed
		{
			get { return m_Disposed; }
		}

		public void LoadResources()
		{
#if DEBUG
			RC.WriteLine(ConsoleThemeColor.SubText3, "RenderContext [" + m_Index + "]: Load Resources, Mode (" + m_Form.ClientSize.Width + "x" + m_Form.ClientSize.Height + ")");
#endif
			if (m_Disposed == true)
			{
				m_BackBuffer = Texture2D.FromSwapChain<Texture2D>(m_SwapChain, 0);
				m_RenderView = new RenderTargetView(GameEnvironment.Device, m_BackBuffer);

				#region Create Depth Buffer

				m_DepthBuffer = new Texture2D(GameEnvironment.Device, new Texture2DDescription()
				{
					Width = m_Form.ClientSize.Width,
					Height = m_Form.ClientSize.Height,
					MipLevels = 1,
					ArraySize = 1,
					Format = Format.D32_Float,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default,
					BindFlags = BindFlags.DepthStencil,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None
				});

				m_DepthView = new DepthStencilView(GameEnvironment.Device, m_DepthBuffer, new DepthStencilViewDescription()
				{
					Format = Format.D32_Float,
					Dimension = DepthStencilViewDimension.Texture2D,
					MipSlice = 0
				});

				m_DepthState = DepthStencilState.FromDescription(GameEnvironment.Device, new DepthStencilStateDescription()
				{
					IsDepthEnabled = true,
					DepthWriteMask = DepthWriteMask.All,
					DepthComparison = Comparison.LessEqual, // Comparison.LessEqual,
					IsStencilEnabled = false,
					StencilReadMask = 0xff,
					StencilWriteMask = 0xff,

					BackFace = new DepthStencilOperationDescription()
					{
						Comparison = Comparison.Always,
						DepthFailOperation = StencilOperation.Decrement,
						FailOperation = StencilOperation.Keep,
						PassOperation = StencilOperation.Keep
					},

					FrontFace = new DepthStencilOperationDescription()
					{
						Comparison = Comparison.Always,
						DepthFailOperation = StencilOperation.Increment,
						FailOperation = StencilOperation.Keep,
						PassOperation = StencilOperation.Keep
					}
				});

				#endregion

				m_Disposed = false; 
			}
		}

		public void UnloadResources()
		{
#if DEBUG
			RC.WriteLine(ConsoleThemeColor.SubText3, "RenderContext [" + m_Index + "]: Unload Resources");
#endif

			if (m_Disposed == false)
			{
				m_DepthState.Dispose(); 
				m_DepthView.Dispose(); 
				m_DepthBuffer.Dispose(); 
				m_RenderView.Dispose(); 
				m_BackBuffer.Dispose();				

				m_Disposed = true;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
#if DEBUG
			RC.WriteLine(ConsoleThemeColor.SubText3, "RenderContext [" + m_Index + "]: Dispose");
#endif

			UnloadResources();

			if (m_SwapChain != null && m_SwapChain.Disposed == false)
			{
				m_SwapChain.Dispose();
			}
		}

		#endregion

		public bool NeedsResize(int WindowWidth, int WindowHeight)
		{
			return WindowWidth != m_Form.ClientSize.Width ||
				   WindowHeight != m_Form.ClientSize.Height; 
		}

		public void Resize(int WindowWidth, int WindowHeight)
		{
#if DEBUG
			RC.WriteLine(ConsoleThemeColor.SubText3, "RenderContext [" + m_Index + "]: Resize, Mode (" + WindowWidth + "x" + WindowHeight + ")");
#endif
			m_SwapChain.ResizeTarget(new ModeDescription(WindowWidth, WindowHeight, m_SwapChain.Description.ModeDescription.RefreshRate, m_SwapChain.Description.ModeDescription.Format));
			m_SwapChain.ResizeBuffers(1, WindowWidth, WindowHeight, m_SwapChain.Description.ModeDescription.Format, m_SwapChain.Description.Flags);			
		}

		public void Resize(int WindowWidth, int WindowHeight, bool IsFullScreen)
		{
#if DEBUG
			RC.WriteLine(ConsoleThemeColor.SubText3, "RenderContext [" + m_Index + "]: Resize, Mode (" + WindowWidth + "x" + WindowHeight + ") IsFullScreen=" + IsFullScreen);
#endif
			m_SwapChain.ResizeTarget(new ModeDescription(WindowWidth, WindowHeight, m_SwapChain.Description.ModeDescription.RefreshRate, m_SwapChain.Description.ModeDescription.Format));
			m_SwapChain.ResizeBuffers(1, WindowWidth, WindowHeight, m_SwapChain.Description.ModeDescription.Format, m_SwapChain.Description.Flags);			

			if (IsFullScreen == true)
			{
				m_SwapChain.SetFullScreenState(IsFullScreen, GameEnvironment.Device.Factory.GetAdapter(GameConfiguration.AdapterOrdinal).GetOutput(m_Index));
			}
			else
			{
				m_SwapChain.SetFullScreenState(IsFullScreen, null);
			}
		}
	}
}
