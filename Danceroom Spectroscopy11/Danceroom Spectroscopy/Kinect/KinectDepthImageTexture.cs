using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects;
using SlimDX.Direct3D11;
using RugTech1.Framework.Objects.Simple;
using SlimDX;
using RugTech1.Framework;
using System.Drawing;

namespace DS.Kinect
{
	public enum KinectDepthImageType { RawDepth, DepthBackgroundImage, DepthBackgroundRemoved,  }

	class KinectDepthImageTexture : SceneObject
	{
		private bool m_IsVisible = true;
		private KinectDevice m_Device; 
		private KinectDepthImageType m_ImageType = KinectDepthImageType.RawDepth;

		private Texture2D m_Texture;
		private ShaderResourceView m_TextureResourceView;
		private ImageBox m_Image;
		private byte[] m_ColorBuffer;
		private SlimDX.DXGI.Surface m_TextureSurface; 

		public bool IsVisible { get { return m_IsVisible; } set { m_IsVisible = value; } }
		public KinectDevice Device { get { return m_Device; } set { m_Device = value; } }
		public KinectDepthImageType ImageType { get { return m_ImageType; } set { m_ImageType = value; } }

		public RectangleF Rectangle { get { return m_Image.Rectangle; } set { m_Image.Rectangle = value; } }

		public bool FlipVertical { get { return m_Image.FlipVertical; } set { m_Image.FlipVertical = value; } }
		public bool FlipHorizontal { get { return m_Image.FlipHorizontal; } set { m_Image.FlipHorizontal = value; } } 

		public KinectDepthImageTexture(KinectDevice device, KinectDepthImageType type)
		{
			m_Device = device;
			m_ImageType = type;

			m_Image = new ImageBox(null);	
		}

		public void Update()
		{
			if (m_Device != null && m_IsVisible == true && Disposed == false)
			{
				if (m_Device.HasNewDepthFrame == true)
				{
					switch (m_ImageType)
					{
						case KinectDepthImageType.RawDepth:
							{
								//DataRectangle data = m_Texture.Map(0, MapMode.WriteDiscard, MapFlags.None);
								DataRectangle data = m_TextureSurface.Map(SlimDX.DXGI.MapFlags.Discard | SlimDX.DXGI.MapFlags.Write);

								int s = 0;
								short[] source = m_Device.DepthBuffer;
								KinectHelper.ScaleBufferForDisplay32(source, m_ColorBuffer, ArtworkStaticObjects.Options.Kinect.NearClippingPlane, ArtworkStaticObjects.Options.Kinect.FarClippingPlane);

								int rowSize = (m_Texture.Description.Width * 4);
								int strideOffset = data.Pitch - rowSize;

								if (strideOffset == 0)
								{
									data.Data.Write(m_ColorBuffer, s, rowSize * m_Texture.Description.Height);
								}
								else
								{
									for (int i = 0, ie = m_Texture.Description.Height; i < ie; i++)
									{
										data.Data.Write(m_ColorBuffer, s, rowSize);
										data.Data.Position += strideOffset;
										s += rowSize;
									}
								}

								m_TextureSurface.Unmap();
							}
							break;
						case KinectDepthImageType.DepthBackgroundImage:
							{
								//DataRectangle data = m_Texture.Map(0, MapMode.WriteDiscard, MapFlags.None);
								DataRectangle data = m_TextureSurface.Map(SlimDX.DXGI.MapFlags.Discard | SlimDX.DXGI.MapFlags.Write);

								int s = 0;
								short[] source = m_Device.DepthBuffer;
								m_Device.Filter.GetBackgroundImage(m_ColorBuffer, ArtworkStaticObjects.Options.Kinect.NearClippingPlane, ArtworkStaticObjects.Options.Kinect.FarClippingPlane); 
								
								int rowSize = (m_Texture.Description.Width * 4);
								int strideOffset = data.Pitch - rowSize;

								if (strideOffset == 0)
								{
									data.Data.Write(m_ColorBuffer, s, rowSize * m_Texture.Description.Height);
								}
								else
								{
									for (int i = 0, ie = m_Texture.Description.Height; i < ie; i++)
									{
										data.Data.Write(m_ColorBuffer, s, rowSize);
										data.Data.Position += strideOffset;
										s += rowSize;
									}
								}

								m_TextureSurface.Unmap();
							}
							break;
						case KinectDepthImageType.DepthBackgroundRemoved:
							{
								//DataRectangle data = m_Texture.Map(0, MapMode.WriteDiscard, MapFlags.None);
								DataRectangle data = m_TextureSurface.Map(SlimDX.DXGI.MapFlags.Discard | SlimDX.DXGI.MapFlags.Write);

								int s = 0;
								short[] source = m_Device.DepthBuffer;
								m_Device.Filter.GetBackgroundCanceledImage(source, m_ColorBuffer, ArtworkStaticObjects.Options.Kinect.NearClippingPlane, ArtworkStaticObjects.Options.Kinect.FarClippingPlane, ArtworkStaticObjects.Options.Kinect.NoiseTolerance, false);

								int rowSize = (m_Texture.Description.Width * 4);
								int strideOffset = data.Pitch - rowSize;

								if (strideOffset == 0)
								{
									data.Data.Write(m_ColorBuffer, s, rowSize * m_Texture.Description.Height);
								}
								else
								{
									for (int i = 0, ie = m_Texture.Description.Height; i < ie; i++)
									{
										data.Data.Write(m_ColorBuffer, s, rowSize);
										data.Data.Position += strideOffset;
										s += rowSize;
									}
								}

								m_TextureSurface.Unmap();
							}
							break;
						default:
							break;
					}					
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
			if (m_Device != null && m_Device.DepthResolution != ImageResolution.Invalid)
			{
				if (Disposed == true)
				{
					m_ColorBuffer = new byte[KinectHelper.GetSizeForResolution(m_Device.DepthResolution) * 4]; 

					m_Texture = new Texture2D(GameEnvironment.Device, new Texture2DDescription()
					{
						Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm,
						Width = KinectHelper.GetWidthForResolution(m_Device.DepthResolution),
						Height = KinectHelper.GetHeightForResolution(m_Device.DepthResolution),
						MipLevels = 1,
						ArraySize = 1,
						BindFlags = BindFlags.ShaderResource,
						CpuAccessFlags = CpuAccessFlags.Write,
						OptionFlags = ResourceOptionFlags.None,
						Usage = ResourceUsage.Dynamic,
						SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0)
					});

					//DataRectangle data = m_Texture.Map(0, MapMode.WriteDiscard, MapFlags.None);
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
