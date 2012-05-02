using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Effects;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace RugTech1.Framework.Objects
{
	public class Imposter : IResourceManager 
	{
		protected static ImposterEffect Effect;

		private bool m_Disposed = true;

		private bool m_UseDepth = false;
		private Format m_Format; 
		private View2D m_ImposterView;
		private Texture2D m_ImposterTexture;
		private ShaderResourceView m_ImposterTextureView;
		private RenderTargetView m_ImposterTargetView;
		private RenderTargetBackupState m_BackupState;
		private DepthStencilView m_DepthView;
		private DepthStencilState m_DepthState;

		private Color4 m_Background = new Color4(0f, 0f, 0f, 0f);
		
		private ImposterOverlayType m_OverlayType = ImposterOverlayType.None;

		public int Width { get { return (int)m_ImposterView.Viewport.Width; } }

		public int Height { get { return (int)m_ImposterView.Viewport.Height; } }

		public Color4 Background { get { return m_Background; } set { m_Background = value; } }

		public ImposterOverlayType OverlayType { get { return m_OverlayType; } set { m_OverlayType = value; } }

		public ShaderResourceView TextureView { get { return m_ImposterTextureView; } }

		public bool UseDepth { get { return m_UseDepth; } set { m_UseDepth = value; } }		

		public DepthStencilView DepthView
		{
			get { return m_DepthView; }
			set { m_DepthView = value; }
		}

		public DepthStencilState DepthState
		{
			get { return m_DepthState; }
			set { m_DepthState = value; }
		}

		public Imposter(int width, int height, Format format, Color4 background, ImposterOverlayType overlayType)
		{
			if (Effect == null)
			{
				Effect = SharedEffects.Effects["Imposter"] as ImposterEffect; 
			}

			m_Disposed = true;
			m_BackupState = new RenderTargetBackupState();
			m_ImposterView = new View2D(new System.Drawing.Rectangle(0, 0, width, height), width, height);
			m_Format = format;
			m_Background = background;
			m_OverlayType = overlayType; 
		}

		public void Resize(int width, int height)
		{
			UnloadResources();

			m_ImposterView.Resize(new System.Drawing.Rectangle(0, 0, width, height), width, height);

			LoadResources(); 
		}

		public void RenderToImposter(IScene scene, View3D view)
		{
			try
			{
				if (UseDepth == true)
				{					
					Effect.BeginRender(m_ImposterTargetView, m_DepthView, m_DepthState, m_Background, m_ImposterView.Viewport, m_BackupState);
				}
				else 
				{
					Effect.BeginRender(m_ImposterTargetView, m_Background, m_ImposterView.Viewport, m_BackupState);
				}

				scene.Render(view); 
			}
			finally
			{
				Effect.EndRender(m_BackupState); 
			}
		}

		public void Render()
		{
			Effect.RenderImposter(m_OverlayType, m_ImposterTextureView); 
		}

		#region IResourceManager Members
		
		public bool Disposed { get { return m_Disposed; } }

		public void LoadResources()
		{
			if (m_Disposed == true)
			{
				m_ImposterTexture = new Texture2D(GameEnvironment.Device, new Texture2DDescription()
				{
					Width = (int)m_ImposterView.Viewport.Width,
					Height = (int)m_ImposterView.Viewport.Height,
					Format = m_Format,
					ArraySize = 1,
					BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.None,
					MipLevels = 1,
					OptionFlags = ResourceOptionFlags.None,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default
				});

				m_ImposterTextureView = new ShaderResourceView(GameEnvironment.Device, m_ImposterTexture);
				m_ImposterTargetView = new RenderTargetView(GameEnvironment.Device, m_ImposterTexture);
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				m_Disposed = true;
				m_ImposterTargetView.Dispose();
				m_ImposterTextureView.Dispose();
				m_ImposterTexture.Dispose();
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			UnloadResources();
		}

		#endregion
	}
}
