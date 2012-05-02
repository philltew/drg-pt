using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;
using RugTech1.Framework;
using RugTech1.Framework.Objects;
using RugTech1.Framework.Objects.Simple;
using System.Drawing;
using SlimDX.DXGI;

namespace DS.Kinect
{	
	class KinectColorImageTexture : SceneObject
	{
		private KinectDevice m_Device;
		private bool m_IsVisible = true;
		private Texture2D m_Texture;
		private Surface m_TextureSurface;
		private ShaderResourceView m_TextureResourceView;

		private ImageBox m_Image; 

		public bool IsVisible { get { return m_IsVisible; } set { m_IsVisible = value; } }
		public KinectDevice Device { get { return m_Device; } set { m_Device = value; } }
		
		public RectangleF Rectangle { get { return m_Image.Rectangle; } set { m_Image.Rectangle = value; } }

		public bool FlipVertical { get { return m_Image.FlipVertical; } set { m_Image.FlipVertical = value; } }
		public bool FlipHorizontal { get { return m_Image.FlipHorizontal; } set { m_Image.FlipHorizontal = value; } } 

		public KinectColorImageTexture(KinectDevice device)
		{
			m_Device = device;

			m_Image = new ImageBox(null);
			m_Image.FlipHorizontal = true;			
		}

		public void Update()
		{
			if (m_Device != null && m_IsVisible == true && Disposed == false)
			{
				if (m_Device.HasNewColorFrame == true)
				{					
					//DataRectangle data = m_Texture.Map(0, MapMode.WriteDiscard, MapFlags.None); 
					DataRectangle data = m_TextureSurface.Map(SlimDX.DXGI.MapFlags.Discard | SlimDX.DXGI.MapFlags.Write);

					int s = 0; 
					byte[] source = m_Device.ColorBuffer; 					
					int rowSize = (m_Texture.Description.Width * 4);
					int strideOffset = data.Pitch - rowSize;

					if (strideOffset == 0)
					{
						data.Data.Write(source, s, rowSize * m_Texture.Description.Height);
					}
					else
					{
						for (int i = 0, ie = m_Texture.Description.Height; i < ie; i++)
						{
							data.Data.Write(source, s, rowSize);
							data.Data.Position += strideOffset;
							s += rowSize;
						}
					}

					/*
					int rowSize = (m_Texture.Description.Width * 4);
					int stride = data.Pitch;

					for (int i = 0, ie = m_Texture.Description.Height; i < ie; i++)
					{
						for (int p = 0; p < stride; p++)
						{
							data.Data.WriteByte(255);
						}
					}
					*/ 

					//m_Texture.Unmap(0);

					m_TextureSurface.Unmap();
				}
			}
		}

		public override void Render(View3D view)
		{
			if (Disposed == true)
			{
				return; 
			}

			m_Image.Render(); 
		}

		public override void LoadResources()
		{
			if (m_Device != null && m_Device.ColorResolution != ImageResolution.Invalid)
			{
				if (Disposed == true)
				{
					m_Texture = new Texture2D(GameEnvironment.Device, new Texture2DDescription()
					{
						Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm, 
						Width = KinectHelper.GetWidthForResolution(m_Device.ColorResolution),
						Height = KinectHelper.GetHeightForResolution(m_Device.ColorResolution),
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

					int rowSize = (m_Texture.Description.Width * 4);
					int stride = data.Pitch;

					for (int i = 0, ie = m_Texture.Description.Height; i < ie; i++)
					{
						for (int p = 0; p < stride; p++)
						{
							data.Data.WriteByte(0);
						}
					}

					m_TextureSurface.Unmap();

					m_TextureResourceView = new ShaderResourceView(GameEnvironment.Device, m_Texture, new ShaderResourceViewDescription()
					{
						Dimension = ShaderResourceViewDimension.Texture2D,
						Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm, 
						ArraySize = 1,
						MipLevels = 1,
						MostDetailedMip = 0,
					});
					
					m_Image.TextureView = m_TextureResourceView; 
					
					m_Image.LoadResources();				

					Disposed = false;
				}
			}
		}

		public override void UnloadResources()
		{
			if (Disposed == false)
			{
				m_Image.UnloadResources(); 

				m_TextureResourceView.Dispose();

				m_Texture.Dispose();

				m_TextureSurface.Dispose();

				Disposed = true;
			}
		}

		public override void Dispose()
		{
			UnloadResources(); 
		}
	}
}
