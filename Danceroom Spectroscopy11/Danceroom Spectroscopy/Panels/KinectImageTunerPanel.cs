using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.UI;
using RugTech1.Framework.Objects;
using DS.Kinect;
using DS.Scenes;
using SlimDX.Direct3D11;
using System.Drawing;

namespace DS.Panels
{
	enum KinectImageMode { Color, RawDepth, DepthBackgroundImage, DepthBackgroundRemoved }

	class KinectImageTunerPanel : UiSubScene, IResourceManager
	{
		private bool m_Disposed = true;
		private List<KinectColorImageTexture> m_KinectColorImages = new List<KinectColorImageTexture>();
		private List<KinectDepthImageTexture> m_KinectDepthImages = new List<KinectDepthImageTexture>();

		private KinectImageMode m_KinectImageMode = KinectImageMode.Color;

		public KinectImageMode KinectImageMode
		{
			get { return m_KinectImageMode; }
			set { m_KinectImageMode = value; }
		}

		private View3D m_View;

		public bool Disposed { get { return m_Disposed; } }

		public KinectImageTunerPanel()
		{
			ControlStyle = DisplayMode.Normal;

			InteractionType = ControlInteractionType.None;

			m_View = new View3D(new System.Drawing.Rectangle(0, 0, 1, 1), 8, 8, (float)Math.PI / 4, 1f);

			foreach (KinectDevice device in ArtworkStaticObjects.KinectDevices.Devices)
			{
				KinectColorImageTexture color = new KinectColorImageTexture(device);
				color.FlipVertical = true;

				m_KinectColorImages.Add(color);

				KinectDepthImageTexture depth = new KinectDepthImageTexture(device, KinectDepthImageType.DepthBackgroundRemoved);
				depth.FlipVertical = true;

				m_KinectDepthImages.Add(depth);
			}
		}

		public void Update()
		{
		}

		public override void Render(View3D view, Viewport viewport)
		{
			m_View.Resize(new System.Drawing.Rectangle(0, 0, (int)viewport.Width, (int)viewport.Height), (int)viewport.Width, (int)viewport.Height);

			float x, y, w, h;

			float widthPerImage = (float)(viewport.Width / m_KinectColorImages.Count);
			float heightPerImage = (widthPerImage / 4) * 3;

			RectangleF imageBounds = new RectangleF(0, (viewport.Height - heightPerImage) * 0.5f, widthPerImage, heightPerImage);

			switch (KinectImageMode)
			{
				case KinectImageMode.Color:
					foreach (KinectColorImageTexture color in m_KinectColorImages)
					{
						UiStyleHelper.CovertToVertCoords(imageBounds, m_View.WindowSize, m_View.PixelSize, out x, out y, out w, out h);
						color.Rectangle = new RectangleF(x, y, w, h);

						color.Update();
						color.Render(m_View);

						imageBounds.Offset(widthPerImage, 0);
					}
					break;
				case KinectImageMode.RawDepth:
				case KinectImageMode.DepthBackgroundImage:
				case KinectImageMode.DepthBackgroundRemoved:
					foreach (KinectDepthImageTexture depth in m_KinectDepthImages)
					{
						UiStyleHelper.CovertToVertCoords(imageBounds, m_View.WindowSize, m_View.PixelSize, out x, out y, out w, out h);
						depth.Rectangle = new RectangleF(x, y, w, h);

						switch (KinectImageMode)
						{
							case KinectImageMode.RawDepth:
								depth.ImageType = KinectDepthImageType.RawDepth;
								break;
							case KinectImageMode.DepthBackgroundImage:
								depth.ImageType = KinectDepthImageType.DepthBackgroundImage;
								break;
							case KinectImageMode.DepthBackgroundRemoved:
								depth.ImageType = KinectDepthImageType.DepthBackgroundRemoved;
								break;
							default:
								break;
						}

						depth.Update();
						depth.Render(m_View);

						imageBounds.Offset(widthPerImage, 0);
					}
					break;
				default:
					break;
			}
		}

		#region ResourceManager Members

		public void LoadResources()
		{
			if (m_Disposed == true)
			{
				foreach (KinectColorImageTexture color in m_KinectColorImages)
				{
					color.LoadResources();
				}

				foreach (KinectDepthImageTexture depth in m_KinectDepthImages)
				{
					depth.LoadResources();
				}

				m_Disposed = false;
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				foreach (KinectColorImageTexture color in m_KinectColorImages)
				{
					color.UnloadResources();
				}

				foreach (KinectDepthImageTexture depth in m_KinectDepthImages)
				{
					depth.UnloadResources();
				}

				m_Disposed = true;
			}
		}

		public void Dispose()
		{
			UnloadResources();

			foreach (KinectColorImageTexture color in m_KinectColorImages)
			{
				color.Dispose();
			}

			foreach (KinectDepthImageTexture depth in m_KinectDepthImages)
			{
				depth.Dispose();
			}
		}

		#endregion

		public override void UnfocusControls()
		{

		}

		public override void UnhoverControls()
		{

		}

		public override void OnMouseDown(View3D view, SlimDX.Vector2 mousePosition, System.Windows.Forms.MouseButtons mouseButtons, out bool shouldSubUpdate)
		{
			shouldSubUpdate = false;
		}

		public override void OnMouseUp(View3D view, SlimDX.Vector2 mousePosition, System.Windows.Forms.MouseButtons mouseButtons, out bool shouldSubUpdate)
		{
			shouldSubUpdate = false;
		}

		public override void OnMouseMoved(View3D view, SlimDX.Vector2 mousePosition, out bool shouldSubUpdate)
		{
			shouldSubUpdate = false;
		}

		public override void OnKeyPress(char @char, out bool shouldUpdate)
		{
			shouldUpdate = false;
		}

		public override void OnKeyDown(System.Windows.Forms.KeyEventArgs args, out bool shouldUpdate)
		{
			shouldUpdate = false;
		}

		public override void OnKeyUp(System.Windows.Forms.KeyEventArgs args, out bool shouldUpdate)
		{
			shouldUpdate = false;
		}
	}
}
