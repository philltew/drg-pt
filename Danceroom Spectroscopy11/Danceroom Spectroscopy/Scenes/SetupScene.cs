using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects;
using DS.Kinect;
using Rug.Cmd;

namespace DS.Scenes
{	
	class SetupScene : IScene, IResourceManager
	{
		private bool m_Disposed = true;
		/* 
		private KinectColorImageTexture m_KinectColorImage;
		private KinectColorImageTexture m_KinectColorImage2;
		private KinectDepthImageTexture m_KinectDepthImage;
		private KinectDepthImageTexture m_KinectDepthImage2;
		 */ 
		//private KinectBackgroundFilter m_Filter; 
		//private KinectImageMode m_KinectImageMode = KinectImageMode.Color;

		//private KinectFieldImageTexture m_Field;
		//private KinectFieldImageTexture m_Field2; 

		//public KinectImageMode KinectImageMode
		//{
		//	get { return m_KinectImageMode; }
		//	set { m_KinectImageMode = value; }
		//} 

		public SetupScene()
		{
			//m_Filter = new KinectBackgroundFilter(KinectHelper.GetWidthForResolution(ArtworkStaticObjects.KinectDevice.DepthResolution), KinectHelper.GetHeightForResolution(ArtworkStaticObjects.KinectDevice.DepthResolution));
			/*
			m_KinectColorImage = new KinectColorImageTexture(ArtworkStaticObjects.KinectDevices.Devices[0]);
			m_KinectColorImage2 = new KinectColorImageTexture(ArtworkStaticObjects.KinectDevices.Devices[1]);

			m_KinectDepthImage = new KinectDepthImageTexture(ArtworkStaticObjects.KinectDevices.Devices[0], KinectDepthImageType.DepthBackgroundRemoved);
			m_KinectDepthImage2 = new KinectDepthImageTexture(ArtworkStaticObjects.KinectDevices.Devices[1], KinectDepthImageType.DepthBackgroundRemoved);

			m_KinectColorImage.Rectangle = new System.Drawing.RectangleF(-1, -0.5f, 1, 1);
			m_KinectColorImage2.Rectangle = new System.Drawing.RectangleF(0, -0.5f, 1, 1);

			m_KinectDepthImage.Rectangle = new System.Drawing.RectangleF(-1, -0.5f, 1, 1);
			m_KinectDepthImage2.Rectangle = new System.Drawing.RectangleF(0, -0.5f, 1, 1);
			*/ 

		}

		#region IScene Members

		public void Render(View3D view)
		{
			/* 
			switch (KinectImageMode)
			{
				case KinectImageMode.Color:
					m_KinectColorImage.Update();
					m_KinectColorImage2.Update(); 

					m_KinectColorImage.Render(view);								
					m_KinectColorImage2.Render(view);			
					break;
				case KinectImageMode.RawDepth:
				case KinectImageMode.DepthBackgroundImage:
				case KinectImageMode.DepthBackgroundRemoved:				
					switch (KinectImageMode)
					{
						case KinectImageMode.RawDepth:
							m_KinectDepthImage.ImageType = KinectDepthImageType.RawDepth;
							m_KinectDepthImage2.ImageType = KinectDepthImageType.RawDepth;
							break;
						case KinectImageMode.DepthBackgroundImage:
							m_KinectDepthImage.ImageType = KinectDepthImageType.DepthBackgroundImage;
							m_KinectDepthImage2.ImageType = KinectDepthImageType.DepthBackgroundImage;
							break;
						case KinectImageMode.DepthBackgroundRemoved:
							m_KinectDepthImage.ImageType = KinectDepthImageType.DepthBackgroundRemoved;
							m_KinectDepthImage2.ImageType = KinectDepthImageType.DepthBackgroundRemoved;
							break;
						default:
							break;
					}

					m_KinectDepthImage.Update();
					m_KinectDepthImage2.Update();

					m_KinectDepthImage.Render(view);				
					m_KinectDepthImage2.Render(view);
					break;
				default:
					break;
			}
			 * */ 
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
				//m_KinectColorImage.LoadResources();
				//m_KinectColorImage2.LoadResources();
				//m_KinectDepthImage.LoadResources();
				//m_KinectDepthImage2.LoadResources();

				m_Disposed = false; 
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				//m_KinectColorImage.UnloadResources();
				//m_KinectColorImage2.UnloadResources();
				//m_KinectDepthImage.UnloadResources();
				//m_KinectDepthImage2.UnloadResources();

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
