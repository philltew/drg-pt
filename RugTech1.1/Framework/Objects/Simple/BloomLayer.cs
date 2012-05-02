using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D11.Buffer;
using Device = SlimDX.Direct3D11.Device;


namespace RugTech1.Framework.Objects.Simple
{
	public class BloomLayer : IResourceManager
	{
		private bool m_Disposed = true; 

		public Texture2D Buffer;
		public RenderTargetView View;
		public ShaderResourceView ResourceView;
		public Viewport Viewport; 

		int m_Width;
		int m_Height;
		Format m_Format;
		float m_Intensity = 1f;

		public float Intensity { get { return m_Intensity; } set { m_Intensity = value; } }

		public BloomLayer(int width, int height, Format format)
		{
			m_Width = width;
			m_Height = height; 
			m_Format = format; 
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
				Buffer = new Texture2D(GameEnvironment.Device, new Texture2DDescription()
				{
					Format = m_Format,
					Width = m_Width,
					Height = m_Height,
					OptionFlags = ResourceOptionFlags.None,
					MipLevels = 1,
					ArraySize = 1,
					BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.None,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default
				});

				View = new RenderTargetView(GameEnvironment.Device, Buffer);
				ResourceView = new ShaderResourceView(GameEnvironment.Device, Buffer);
				Viewport = new Viewport(0, 0, Buffer.Description.Width, Buffer.Description.Height);

				m_Disposed = false;
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				Buffer.Dispose();
				View.Dispose();
				ResourceView.Dispose();

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
	}
}
