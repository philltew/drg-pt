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
using DS.Simulation;
using RugTech1.Framework.Effects;

namespace DS.Kinect
{
	class KinectFieldImageTextureFloat : SceneObject
	{
		private bool m_IsVisible = true;
		private DS.Simulation.CompositeFieldImage.KinectFieldImage m_Source; 
		private KinectFieldImageType m_ImageType = KinectFieldImageType.BlendMap;

		private Texture2D m_Texture;
		private SlimDX.DXGI.Surface m_TextureSurface;
		private ShaderResourceView m_TextureResourceView;
		private ImageBoxFloat m_Image;
		private float[] m_Buffer;
		private Color3 m_Color;

		public bool IsVisible { get { return m_IsVisible; } set { m_IsVisible = value; } }
	
		public DS.Simulation.CompositeFieldImage.KinectFieldImage Source { get { return m_Source; } set { m_Source = value; } }

		public KinectFieldImageType ImageType { get { return m_ImageType; } set { m_ImageType = value; } }

		public ImposterOverlayType OverlayType { get { return m_Image.OverlayType; } set { m_Image.OverlayType = value; } } 

		public RectangleF Rectangle { get { return m_Image.Rectangle; } set { m_Image.Rectangle = value; } }

		public Color3 Color { get { return m_Color; } set { m_Color = value; } }

		public bool FlipVertical { get { return m_Image.FlipVertical; } set { m_Image.FlipVertical = value; } }

		public bool FlipHorizontal { get { return m_Image.FlipHorizontal; } set { m_Image.FlipHorizontal = value; } }

		public KinectFieldImageTextureFloat(DS.Simulation.CompositeFieldImage.KinectFieldImage source, Color3 color, KinectFieldImageType type)
		{
			m_Source = source;
			m_Color = color; 
			m_ImageType = type;

			m_Image = new ImageBoxFloat(null);

			m_Image.OverlayType = RugTech1.Framework.Effects.ImposterOverlayType.Add; 
		}

		public void Update()
		{
			if (m_Source != null && m_IsVisible == true && Disposed == false)
			{
				if (m_Source.HasNewDepthFrame == true)
				{
					switch (m_ImageType)
					{
						case KinectFieldImageType.BlendMap:
							{
								//DataRectangle data = m_Texture.Map(0, MapMode.WriteDiscard, MapFlags.None);
								DataRectangle data = m_TextureSurface.Map(SlimDX.DXGI.MapFlags.Discard | SlimDX.DXGI.MapFlags.Write);

								int s = 0;

								m_Source.CopyBlendMapFloat(m_Buffer);

								int rowSize = m_Texture.Description.Width; //  (m_Texture.Description.Width * 4);
								int strideOffset = (data.Pitch / 4) - rowSize;

								if (strideOffset == 0)
								{
									data.Data.WriteRange(m_Buffer, s, rowSize * m_Texture.Description.Height);
								}
								else
								{
									for (int i = 0, ie = m_Texture.Description.Height; i < ie; i++)
									{
										data.Data.WriteRange(m_Buffer, s, rowSize);
										data.Data.Position += strideOffset * 4;
										s += rowSize;
									}
								}

								m_TextureSurface.Unmap();
							}
							break;
						case KinectFieldImageType.ClipMap:
							{
								//DataRectangle data = m_Texture.Map(0, MapMode.WriteDiscard, MapFlags.None);
								DataRectangle data = m_TextureSurface.Map(SlimDX.DXGI.MapFlags.Discard | SlimDX.DXGI.MapFlags.Write);
								
								int s = 0;

								m_Source.CopyClipMapFloat(m_Buffer);

								int rowSize = m_Texture.Description.Width; //  (m_Texture.Description.Width * 4);
								int strideOffset = (data.Pitch / 4) - rowSize;

								if (strideOffset == 0)
								{
									data.Data.WriteRange(m_Buffer, s, rowSize * m_Texture.Description.Height);
								}
								else
								{
									for (int i = 0, ie = m_Texture.Description.Height; i < ie; i++)
									{
										data.Data.WriteRange(m_Buffer, s, rowSize);
										data.Data.Position += strideOffset * 4;
										s += rowSize;
									}
								}

								m_TextureSurface.Unmap();
							}
							break;
						case KinectFieldImageType.Identify:
							{
								//DataRectangle data = m_Texture.Map(0, MapMode.WriteDiscard, MapFlags.None);
								DataRectangle data = m_TextureSurface.Map(SlimDX.DXGI.MapFlags.Discard | SlimDX.DXGI.MapFlags.Write);

								int s = 0;

								m_Source.CopyFinalFloat(m_Buffer);

								int rowSize = m_Texture.Description.Width; //  (m_Texture.Description.Width * 4);
								int strideOffset = (data.Pitch / 4) - rowSize;

								if (strideOffset == 0)
								{
									data.Data.WriteRange(m_Buffer, s, rowSize * m_Texture.Description.Height);
								}
								else
								{
									for (int i = 0, ie = m_Texture.Description.Height; i < ie; i++)
									{
										data.Data.WriteRange(m_Buffer, s, rowSize);
										data.Data.Position += strideOffset * 4;
										s += rowSize;
									}
								}

								m_TextureSurface.Unmap();
							}
							break;
						case KinectFieldImageType.Final:
							{
								//DataRectangle data = m_Texture.Map(0, MapMode.WriteDiscard, MapFlags.None);
								DataRectangle data = m_TextureSurface.Map(SlimDX.DXGI.MapFlags.Discard | SlimDX.DXGI.MapFlags.Write);

								int s = 0;

								m_Source.CopyFinalFloat(m_Buffer);

								int rowSize = m_Texture.Description.Width; //  (m_Texture.Description.Width * 4);
								int strideOffset = (data.Pitch / 4) - rowSize;

								if (strideOffset == 0)
								{
									data.Data.WriteRange(m_Buffer, s, rowSize * m_Texture.Description.Height);
								}
								else
								{
									for (int i = 0, ie = m_Texture.Description.Height; i < ie; i++)
									{
										data.Data.WriteRange(m_Buffer, s, rowSize);
										data.Data.Position += strideOffset * 4;
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
			if (m_Source != null && m_Source.Device.DepthResolution != ImageResolution.Invalid)
			{
				if (Disposed == true)
				{
					m_Buffer = new float[KinectHelper.GetSizeForResolution(m_Source.Device.DepthResolution) * 4]; 

					m_Texture = new Texture2D(GameEnvironment.Device, new Texture2DDescription()
					{
						Format = SlimDX.DXGI.Format.R32_Float,
						Width = KinectHelper.GetWidthForResolution(m_Source.Device.DepthResolution),
						Height = KinectHelper.GetHeightForResolution(m_Source.Device.DepthResolution),
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

					int stride = data.Pitch / 4;

					for (int i = 0, ie = m_Texture.Description.Height; i < ie; i++)
					{
						for (int p = 0; p < stride; p++)
						{
							data.Data.Write(0f);
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
