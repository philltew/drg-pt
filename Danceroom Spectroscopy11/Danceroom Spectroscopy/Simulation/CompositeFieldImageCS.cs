using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects;
using DS.Kinect;
using DS.Panels;
using RugTech1.Framework.Objects.UI;
using System.Drawing;
using SlimDX;
using RugTech1.Framework;
using RugTech1.Framework.Objects.Simple;
using SlimDX.Direct3D11;

namespace DS.Simulation
{
	class CompositeFieldImageCS : SceneObject
	{		
		// Imposter
		// Write Sub Images to Imposter 
		// 
		private ImposterFloat m_CompositeFieldImage;

		private FieldTextureScene m_Inner;
		//private FieldGradiantCS m_FieldGrad; 
		private ImageBoxFloatPalette m_Box;

		public RectangleF Rectangle = new RectangleF(-1, -1, 2, 2);

		public ShaderResourceView TextureView { get { return m_CompositeFieldImage.TextureView; } }
		
		public ImposterFloat CompositeFieldImage { get { return m_CompositeFieldImage; } }

		public CompositeFieldImageCS()
		{
			m_CompositeFieldImage = new ImposterFloat(ArtworkStaticObjects.CompositeFieldImage.Width, 
												 ArtworkStaticObjects.CompositeFieldImage.Height, 
												 SlimDX.DXGI.Format.R32_Float, 
												 new Color4(0, 0, 0, 0), 
												 RugTech1.Framework.Effects.ImposterOverlayType.Add);

			m_Inner = new FieldTextureScene();
			//m_FieldGrad = new FieldGradiantCS(); 
			m_Box = new ImageBoxFloatPalette(null);
			m_Box.OverlayType = RugTech1.Framework.Effects.ImposterOverlayType.Add;
		}

		public void Resize(int width, int height)
		{
			m_CompositeFieldImage.Resize(width, height);
		}

		public void Update()
		{
			bool shouldRender = false;

			if (ArtworkStaticObjects.CompositeFieldImage.Bounds.Width != m_CompositeFieldImage.Width ||
				ArtworkStaticObjects.CompositeFieldImage.Bounds.Height != m_CompositeFieldImage.Height)
			{
				Resize(ArtworkStaticObjects.CompositeFieldImage.Bounds.Width, ArtworkStaticObjects.CompositeFieldImage.Bounds.Height);

				shouldRender = true; 
			}

			shouldRender |= m_Inner.ShouldRender(); 

			if (shouldRender == true)
			{
				m_CompositeFieldImage.RenderToImposter(m_Inner, ArtworkStaticObjects.View); 
			}
		}

		public override void Render(View3D view)
		{			
			//m_CompositeFieldImage.Render();
			m_Box.Rectangle = Rectangle;			
			m_Box.Render(); 
		}


		public void Render(View3D view, float alpha, float color)
		{
			//m_CompositeFieldImage.Render();
			m_Box.Rectangle = Rectangle;
			m_Box.Alpha = alpha;
			m_Box.Color = color; 
			m_Box.Render();
		}


		public override void LoadResources()
		{
			//m_FieldGrad.LoadResources(); 
			m_CompositeFieldImage.LoadResources();
			m_Inner.LoadResources();
			m_Box.LoadResources();

			m_Box.TextureView = m_CompositeFieldImage.TextureView; 
		}

		public override void UnloadResources()
		{
			//m_FieldGrad.UnloadResources(); 
			m_CompositeFieldImage.UnloadResources();
			m_Inner.UnloadResources();
			m_Box.UnloadResources(); 
		}

		public override void Dispose()
		{
			//m_FieldGrad.Dispose(); 
			m_CompositeFieldImage.Dispose();
			m_Inner.Dispose();
			m_Box.Dispose(); 
		}

		public class FieldTextureScene : IScene, IResourceManager
		{
			private bool m_Disposed = true;
			private List<KinectFieldImageTextureFloat> m_FieldImages = new List<KinectFieldImageTextureFloat>();

			public FieldTextureScene()
			{
				int i = 0;
				foreach (DS.Simulation.CompositeFieldImage.KinectFieldImage image in ArtworkStaticObjects.CompositeFieldImage.Images)
				{
					//KinectFieldImageTextureFloat texture = new KinectFieldImageTextureFloat(image, CompositeFieldImageEditor.DefaultColors[i++], KinectFieldImageType.Final);
					KinectFieldImageTextureFloat texture = new KinectFieldImageTextureFloat(image, new Color3(1, 1, 1), KinectFieldImageType.Final);

					texture.FlipVertical = true;

					m_FieldImages.Add(texture);
				}
			}

			public bool ShouldRender()
			{
				return ArtworkStaticObjects.CompositeFieldImage.Update();				
			}

			#region IScene Members

			public void Render(View3D view)
			{
				float x, y, w, h;
				float imageOffsetX = ArtworkStaticObjects.CompositeFieldImage.Bounds.X, imageOffsetY = ArtworkStaticObjects.CompositeFieldImage.Bounds.Y; 

				foreach (KinectFieldImageTextureFloat image in m_FieldImages)
				{
					//image.ImageType = KinectFieldImageType.Final;

					//UiStyleHelper.CovertToVertCoords(new RectangleF(((image.Source.X * scale) - imageOffsetX), ((image.Source.Y - imageOffsetY) * scale) + offsetY, image.Source.Stride * scale, image.Source.Height * scale), view.WindowSize, view.PixelSize, out x, out y, out w, out h);
					UiStyleHelper.CovertToVertCoords(new RectangleF(image.Source.X - imageOffsetX, image.Source.Y - imageOffsetY, image.Source.Stride, image.Source.Height), view.WindowSize, view.PixelSize, out x, out y, out w, out h);

					image.Rectangle = new RectangleF(x, y, w, h);

					image.Update();

					image.Render(view);
				}
			}

			#endregion

			#region IResourceManager Members

			public bool Disposed
			{
				get { return m_Disposed; }
			}

			public void LoadResources()
			{
				if (m_Disposed == true)
				{
					foreach (KinectFieldImageTextureFloat image in m_FieldImages)
					{
						image.LoadResources();
					}

					m_Disposed = false;
				}
			}

			public void UnloadResources()
			{
				if (m_Disposed == false)
				{
					foreach (KinectFieldImageTextureFloat image in m_FieldImages)
					{
						image.UnloadResources();
					}

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
}
