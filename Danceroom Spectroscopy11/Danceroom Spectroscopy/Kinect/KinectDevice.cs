using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework;
using RugTech1.Framework.Data;
using RugTech1.Framework.Objects;
using SlimDX;
using SlimDX.Direct3D11;
using Rug.Cmd;
using OpenNI;
using System.Runtime.InteropServices;

namespace DS.Kinect
{
	class KinectDevice : IResourceManager
	{
		#region Private Members
		
		private bool m_Disposed = true;

		private int m_DeviceID = 0;
		private OpenNI.DepthGenerator m_DepthGenerator;
		private DepthMetaData m_DepthMD = new DepthMetaData();

		//private Runtime m_KinectRuntime;
		private KinectBackgroundFilter m_Filter; 

		private ImageResolution m_DepthResolution;
		private ImageResolution m_ColorResolution; 

		private short[] m_DepthBuffer;
		private short[] m_DepthBuffer_Flip;
		private byte[] m_ColorBuffer;		

		private string m_Message = "Not loaded";
		private bool m_LoadedOK = false;
		private bool m_HasNewDepthFrame = false;
		private bool m_HasNewColorFrame = false;
		private bool m_IsTrackingAPlayer = false;
		private bool m_ShouldCalibarateBackground = true;
		private short m_BackgroundCalibarationFrames = 0;
		private bool m_EnableColorCamera = false;
		private bool m_TestModeOnly = false;
		
		
		#endregion

		#region Public Properties

		public bool Disposed { get { return m_Disposed; } protected set { m_Disposed = value; } }

		public int DeviceID { get { return m_DeviceID; } }

		public OpenNI.DepthGenerator DepthGenerator { get { return m_DepthGenerator; } }
		//public Runtime KinectRuntime { get { return m_KinectRuntime; } }
		public KinectBackgroundFilter Filter { get { return m_Filter; } }

		public short[] DepthBuffer { get { return m_DepthBuffer; } }
		public byte[] ColorBuffer { get { return m_ColorBuffer; } }	

		public ImageResolution DepthResolution { get { return m_DepthResolution; } }
		public ImageResolution ColorResolution { get { return m_ColorResolution; } }

		public string Message { get { return m_Message; } }
		public bool LoadedOK { get { return m_LoadedOK; } }
		public bool TestModeOnly { get { return m_TestModeOnly; } }
		public bool HasNewDepthFrame { get { return m_HasNewDepthFrame; } }
		public bool HasNewColorFrame { get { return m_HasNewColorFrame; } }
		public bool ShouldCalibarateBackground { get { return m_ShouldCalibarateBackground; } }

		public bool EnableColorCamera { get { return m_EnableColorCamera; } set { m_EnableColorCamera = value; } }

		public int ElevationAngle
		{ 
			get 
			{
				/*
				if (m_KinectRuntime != null && m_KinectRuntime.NuiCamera != null)
				{
					try
					{
						return m_KinectRuntime.NuiCamera.ElevationAngle;
					}
					catch { return ArtworkStaticObjects.Options.Kinect.ElevationAngle; }
				}
				else
				{
					return ArtworkStaticObjects.Options.Kinect.ElevationAngle; 
				}
				 */

				return ArtworkStaticObjects.Options.Kinect.ElevationAngle; 
			}  
			set
			{
				/*
				if (m_KinectRuntime != null && m_KinectRuntime.NuiCamera != null)
				{
					try
					{
						m_KinectRuntime.NuiCamera.ElevationAngle = value;
					} 
					catch { }
				}
				else
				{
					ArtworkStaticObjects.Options.Kinect.ElevationAngle = value;
				}
				*/ 
			}
		}

		#endregion

		#region Constructor

		public KinectDevice(int deviceID, ImageResolution depthResolution, ImageResolution colorResolution)
		{
			m_DeviceID = deviceID;
			m_DepthGenerator = null;
			m_DepthResolution = depthResolution;
			m_ColorResolution = colorResolution;

			if (m_DepthResolution != ImageResolution.Invalid)
			{
				m_DepthBuffer = new short[KinectHelper.GetSizeForResolution(m_DepthResolution)];
				m_DepthBuffer_Flip = new short[KinectHelper.GetSizeForResolution(m_DepthResolution)];

				m_Filter = new KinectBackgroundFilter(KinectHelper.GetWidthForResolution(m_DepthResolution), KinectHelper.GetHeightForResolution(m_DepthResolution));
			}

			if (m_ColorResolution != ImageResolution.Invalid)
			{
				m_ColorBuffer = new byte[KinectHelper.GetSizeForResolution(m_ColorResolution) * 4];
			}

			StartKinect(); 
		}

		public KinectDevice(int deviceID, OpenNI.DepthGenerator depthGenerator, ImageResolution depthResolution, ImageResolution colorResolution)
		{
			m_DeviceID = deviceID;
			m_DepthGenerator = depthGenerator;
			m_DepthResolution = depthResolution;
			m_ColorResolution = colorResolution;

			if (m_DepthResolution != ImageResolution.Invalid)
			{
				m_DepthBuffer = new short[KinectHelper.GetSizeForResolution(m_DepthResolution)];
				m_DepthBuffer_Flip = new short[KinectHelper.GetSizeForResolution(m_DepthResolution)];

				m_Filter = new KinectBackgroundFilter(KinectHelper.GetWidthForResolution(m_DepthResolution), KinectHelper.GetHeightForResolution(m_DepthResolution));
			}

			if (m_ColorResolution != ImageResolution.Invalid)
			{
				m_ColorBuffer = new byte[KinectHelper.GetSizeForResolution(m_ColorResolution) * 4];
			}

			StartKinect();
		}

		#endregion

		#region Start / Shutdown
		
		private void StartKinect()
		{
			RC.WriteLine(ConsoleThemeColor.TitleText1, "Starting Kinect Device : " + m_DeviceID);
			
			m_Filter.Clear();

			m_LoadedOK = false;
			m_TestModeOnly = true;
			m_HasNewDepthFrame = true; 

			/* 
			m_KinectRuntime = new Runtime(m_DeviceID);

			try
			{				
				RuntimeOptions options = ((m_DepthResolution != ImageResolution.Invalid) ? RuntimeOptions.UseDepth : (RuntimeOptions)0) |
										 ((m_ColorResolution != ImageResolution.Invalid) ? RuntimeOptions.UseColor : (RuntimeOptions)0);

				m_KinectRuntime.Initialize(options);
			}
			catch (InvalidOperationException)
			{
				m_Message = "Runtime initialization failed. Please make sure Kinect device is plugged in.";
				RC.WriteLine(ConsoleThemeColor.ErrorColor1, m_Message);
				return; 
			}

			if (m_DepthResolution != ImageResolution.Invalid)
			{
				try
				{
					RC.WriteLine(ConsoleThemeColor.SubText1, "Opening depth stream : " + m_DepthResolution.ToString());
					m_KinectRuntime.DepthStream.Open(ImageStreamType.Depth, 2, m_DepthResolution, ImageType.Depth);
				}
				catch (InvalidOperationException)
				{
					m_Message = "Failed to open depth stream : " + m_DepthResolution.ToString();
					RC.WriteLine(ConsoleThemeColor.ErrorColor1, m_Message);
					return;
				}
			}

			if (m_ColorResolution != ImageResolution.Invalid)
			{
				try
				{
					RC.WriteLine(ConsoleThemeColor.SubText1, "Opening color stream : " + m_ColorResolution.ToString());
					m_KinectRuntime.VideoStream.Open(ImageStreamType.Video, 2, m_ColorResolution, ImageType.Color);
				}
				catch (InvalidOperationException)
				{
					m_Message = "Failed to open color stream : " + m_ColorResolution.ToString();
					RC.WriteLine(ConsoleThemeColor.ErrorColor1, m_Message);
					return;
				}
			}
			*/ 

			if (m_DepthResolution != ImageResolution.Invalid)
			{				
				m_BackgroundCalibarationFrames = ArtworkStaticObjects.Options.Kinect.BackgroundCalibarationFrames;
				
				RC.WriteLine(ConsoleThemeColor.SubText1, "Opening depth stream : " + m_DepthResolution.ToString());
				RC.WriteLine(ConsoleThemeColor.SubText1, "Listening to depth stream");
				//m_KinectRuntime.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);				
			}

			/* 
			if (m_ColorResolution != ImageResolution.Invalid)
			{
				RC.WriteLine(ConsoleThemeColor.SubText1, "Listening to color stream");
				m_KinectRuntime.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(m_KinectRuntime_VideoFrameReady);
			}
			*/ 

			//m_KinectRuntime.NuiCamera.ElevationAngle = ArtworkStaticObjects.Options.Kinect.ElevationAngle;

			if (m_DepthGenerator != null)
			{
				m_TestModeOnly = false;
			}

			m_LoadedOK = true;
		}

		private void ShutdownKinect()
		{
			if (m_LoadedOK == true)
			{
				//RC.WriteLine(ConsoleThemeColor.TitleText1, "Shutdown Kinect Device : " + m_DeviceID);
				//m_KinectRuntime.Uninitialize();
				m_LoadedOK = false; 
			}
		}

		#endregion

		#region Depth Frame Ready Delegate

		internal unsafe void ProcessDepthFrame()
		{
			if (m_DepthGenerator == null)
			{
				return; 
			}

			m_DepthGenerator.GetMetaData(m_DepthMD);

			/* 
			if (m_DepthMD.XRes != 320 || m_DepthMD.YRes != 240)
			{
				//MapOutputMode requiredMode = new MapOutputMode() { FPS = 30, XRes = 320, YRes = 240 };

				//m_DepthGenerator.MapOutputMode = requiredMode;

				return; 
			}
			*/ 
			short* pDepth = (short*)m_DepthGenerator.DepthMapPtr.ToPointer();
			int i = 0; 

			for (int y = 0; y < 240; y++)
			{
				for (int x = 0; x < 320; x++)
				{
					m_DepthBuffer_Flip[i++] = *pDepth++; 
					pDepth++; 
				}

				pDepth += m_DepthMD.XRes; 
			}

			// Marshal.Copy(m_DepthGenerator.DepthMapPtr, m_DepthBuffer_Flip, 0, m_DepthMD.XRes * m_DepthMD.YRes);

			if (ShouldCalibarateBackground == true)
			{
				m_Filter.Clear();
				m_BackgroundCalibarationFrames = ArtworkStaticObjects.Options.Kinect.BackgroundCalibarationFrames;

				RC.WriteLine("Calibarating background for device " + m_DeviceID);
				m_ShouldCalibarateBackground = false;
			}

			if (m_Filter.GrabberCalls < m_BackgroundCalibarationFrames)
			{
				m_Filter.GrabBackground(m_DepthBuffer_Flip);

				if (m_Filter.GrabberCalls == m_BackgroundCalibarationFrames)
				{
					RC.WriteLine("Completed background calibaration for device " + m_DeviceID);

					RC.WriteLine("Filling in missing data for device " + m_DeviceID);

					m_Filter.FillInMissingDataPoints();
				}
			}

			

			/* 
			
			for (int y = 0; y < depthMD.YRes; ++y)
			{
				byte* pDest = (byte*)data.Scan0.ToPointer() + y * data.Stride;
				for (int x = 0; x < depthMD.XRes; ++x, ++pDepth, pDest += 3)
				{

				}
			}
			 * */ 
		}

		internal void MarkAsHasNewFrame()
		{
			if (m_DepthGenerator == null)
			{
				return;
			}

			short[] temp = m_DepthBuffer;

			m_DepthBuffer = m_DepthBuffer_Flip;

			m_DepthBuffer_Flip = temp; 

			m_HasNewDepthFrame = true; 
		}

		/* 
		void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
		{					PlanarImage Image = e.ImageFrame.Image;

			KinectHelper.CopyKinectBuffer(Image.Bits, m_DepthBuffer, KinectCopyMode.NoIdent, out m_IsTrackingAPlayer);

			if (ShouldCalibarateBackground == true)
			{
				m_Filter.Clear();
				m_BackgroundCalibarationFrames = ArtworkStaticObjects.Options.Kinect.BackgroundCalibarationFrames;

				RC.WriteLine("Calibarating background for device " + m_DeviceID);
				m_ShouldCalibarateBackground = false;
			}

			if (m_Filter.GrabberCalls < m_BackgroundCalibarationFrames)
			{
				m_Filter.GrabBackground(m_DepthBuffer);

				if (m_Filter.GrabberCalls == m_BackgroundCalibarationFrames)
				{
					RC.WriteLine("Completed background calibaration for device " + m_DeviceID);

					RC.WriteLine("Filling in missing data for device " + m_DeviceID);

					m_Filter.FillInMissingDataPoints(); 
				}
			}

			m_HasNewDepthFrame = true; 
		}
		*/ 

		#endregion

		#region Video Frame Ready Delegate 
		
		/* 
		void m_KinectRuntime_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
		{
			if (m_EnableColorCamera == true)
			{
				KinectHelper.CopyColorBuffer(e.ImageFrame, m_ColorBuffer);

				m_HasNewColorFrame = true;
			}
		}
		*/ 

		#endregion

		#region Reset Frame State

		public void ResetFrameState()
		{
			if (m_TestModeOnly == false)
			{
				m_HasNewDepthFrame = false;
			}

			m_HasNewColorFrame = false;			
		}

		#endregion

		#region Calibarate Background

		public void CalibarateBackground()
		{
			m_ShouldCalibarateBackground = true; 
		}
		
		#endregion

		#region Resource Managment

		public void LoadResources()
		{
			if (m_Disposed == true)
			{			
				//StartKinect();

				m_Disposed = false;
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				//ShutdownKinect();

				m_Disposed = true;
			}
		}

		public void Dispose()
		{
			ShutdownKinect();
			UnloadResources(); 
		}

		#endregion
	}
}
