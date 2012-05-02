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
	public class RenderContext : IResourceManager
	{
		#region Private Members
		
		private bool m_Disposed = true; 

		private Texture2D m_BackBuffer;		
		private RenderTargetView m_RenderView;
		private ShaderResourceView m_TextureView;		

		private Texture2D m_DepthBuffer;
		private DepthStencilView m_DepthView;
		private DepthStencilState m_DepthState;

		private Color4 m_BackgroundColor = new Color4(0, 0, 0, 0);
		private int m_Width;
		private int m_Height;		
		
		#endregion

		#region Public Properties

		public int Width { get { return m_Width; } }
		public int Height { get { return m_Height; } }

		public Texture2D BackBuffer { get { return m_BackBuffer; } }

		public RenderTargetView RenderView { get { return m_RenderView; } }

		public ShaderResourceView TextureView { get { return m_TextureView; } }

		public Texture2D DepthBuffer { get { return m_DepthBuffer; } }

		public DepthStencilView DepthView { get { return m_DepthView; } }

		public DepthStencilState DepthState { get { return m_DepthState; } }

		public Color4 BackgroundColor { get { return m_BackgroundColor; } set { m_BackgroundColor = value; } }

		#endregion

		public RenderContext(int width, int height)
		{
			m_Width = width;
			m_Height = height; 
		}

		public void RenderBegin()
		{
			if (m_Disposed == true)
			{
				RC.WriteError(555, "Render Context Has Been Disposed"); 
			}

			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext;

			Viewport port = new Viewport(0, 0, Width, Height, 0.0f, 1.0f);

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
			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext;

			context.OutputMerger.SetTargets(null as DepthStencilView, null as RenderTargetView);
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
				m_BackBuffer = new Texture2D(GameEnvironment.Device, new Texture2DDescription()
				{
					Width = (int)Width,
					Height = (int)Height,
					Format = Format.R8G8B8A8_UNorm,
					ArraySize = 1,
					BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.None,
					MipLevels = 1,
					OptionFlags = ResourceOptionFlags.None,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default
				});

				m_RenderView = new RenderTargetView(GameEnvironment.Device, m_BackBuffer);
				m_TextureView = new ShaderResourceView(GameEnvironment.Device, m_BackBuffer);

				#region Create Depth Buffer

				m_DepthBuffer = new Texture2D(GameEnvironment.Device, new Texture2DDescription()
				{
					Width = Width,
					Height = Height,
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
					DepthComparison = Comparison.LessEqual,
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
			if (m_Disposed == false)
			{
				m_DepthState.Dispose(); 
				m_DepthView.Dispose(); 
				m_DepthBuffer.Dispose(); 
				m_RenderView.Dispose();
				m_TextureView.Dispose(); 
				m_BackBuffer.Dispose();				

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

		public bool NeedsResize(int WindowWidth, int WindowHeight)
		{
			return WindowWidth != Width ||
				   WindowHeight != Height; 
		}

		public void Resize(int WindowWidth, int WindowHeight)
		{
			bool recreate = m_Disposed == false;

			UnloadResources();

			m_Width = WindowWidth;
			m_Height = WindowHeight;

			if (recreate == true)
			{
				LoadResources(); 
			}
		}
	}
}
