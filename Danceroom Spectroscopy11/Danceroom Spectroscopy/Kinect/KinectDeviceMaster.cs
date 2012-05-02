using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects;
using OpenNI;
using Rug.Cmd;
using System.Threading;
//using Microsoft.Research.Kinect.Nui;

namespace DS.Kinect
{
	public enum ImageResolution { Resolution320x240, Resolution640x480, Invalid, Resolution1280x1024, Resolution80x60 }

	class KinectDeviceMaster : IResourceManager
	{
		private bool m_Disposed = true;
		private bool m_EnableColorCameras = false;
		private bool m_UseTestImage = false;		

		public readonly List<KinectDevice> Devices = new List<KinectDevice>();

		public bool Disposed { get { return m_Disposed; } }

		public ImageResolution DepthResolution = ImageResolution.Resolution320x240;
		public ImageResolution ColorResolution = ImageResolution.Resolution640x480;
		private Context m_MasterContext;
		private bool m_ShouldRun;
		private bool m_ReadyToProcessFrame = false;
		private Thread m_ReaderThread;
		
		public bool EnableColorCameras 
		{ 
			get { return m_EnableColorCameras; } 
			set
			{ 
				m_EnableColorCameras = value;
				
				foreach (KinectDevice device in Devices)
				{
					device.EnableColorCamera = value;
				}
			} 
		}

		public bool UseTestImage
		{
			get
			{
				return m_UseTestImage;
			}

			set
			{
				m_UseTestImage = value;

				foreach (KinectDevice device in Devices)
				{
					device.Filter.UseTestImage = value;
				}
			}
		}

		public KinectDeviceMaster()
		{

		}

		#region Initialize
		
		public void Initialize()
		{
			//Dispose();

			try
			{
				m_MasterContext = new Context();

				NodeInfoList deviceList = m_MasterContext.EnumerateProductionTrees(NodeType.Device, null);

				int deviceCount = 0;
				foreach (NodeInfo nodeInfo in deviceList)
				{
					deviceCount++;
					/* 
					RC.WriteLine("Device: " + nodeInfo.InstanceName);
					RC.WriteLine("Node: " + nodeInfo.CreationInfo);
					RC.WriteLine("Name: " + nodeInfo.Description.Name.ToString());
					RC.WriteLine("Vendor: " + nodeInfo.Description.Vendor.ToString());
					RC.WriteLine("Version: " + nodeInfo.Description.Version.ToString());
					RC.WriteLine("");
					*/
				}

				RC.WriteLine(ConsoleThemeColor.TitleText1, deviceCount + " Kinect Devices Detected");

				NodeInfoList depthList = m_MasterContext.EnumerateProductionTrees(NodeType.Depth, null);

				int depthSourceCount = 0;
				foreach (NodeInfo nodeInfo in depthList)
				{
					depthSourceCount++;
					/* 
					RC.WriteLine("Depth Image Source: " + nodeInfo.InstanceName);
					RC.WriteLine("Node: " + nodeInfo.CreationInfo);
					RC.WriteLine("Name: " + nodeInfo.Description.Name.ToString());
					RC.WriteLine("Vendor: " + nodeInfo.Description.Vendor.ToString());
					RC.WriteLine("Version: " + nodeInfo.Description.Version.ToString());
					RC.WriteLine("");
					*/
				}

				RC.WriteLine(ConsoleThemeColor.TitleText1, depthSourceCount + " Depth Image Sources Detected");


				int deviceIndex = 0;

				foreach (NodeInfo nodeInfo in depthList)
				{
					DepthGenerator depth = m_MasterContext.CreateProductionTree(nodeInfo) as DepthGenerator;

					if (depth == null)
					{
						throw new Exception("Viewer must have a depth node!");
					}

					//MapOutputMode requiredMode = new MapOutputMode() { FPS = 30, XRes = 320, YRes = 240 };

					//depth.MapOutputMode = requiredMode;
					RC.WriteLine(ConsoleThemeColor.TitleText1, "Device: " + deviceIndex);
					/* 
					MapOutputMode selectedMode = depth.MapOutputMode; 

					foreach (MapOutputMode mode in depth.GetSupportedMapOutputModes())
					{
						RC.WriteLine(ConsoleThemeColor.Text1, "Mode:" + mode.XRes + "x" + mode.YRes + ", " + mode.FPS + " FPS");

						if (mode.XRes == 320 && mode.YRes == 240 && mode.FPS == 60)
						{
							selectedMode = mode;
						}
					}
				
					depth.MapOutputMode = selectedMode; 

					MapOutputMode mapMode = depth.MapOutputMode;
					*/

					ImageResolution ColorResolution = ImageResolution.Invalid;
					ImageResolution DepthResolution = ImageResolution.Resolution320x240;

					/* if (mapMode.XRes == 320 && mapMode.YRes == 240)
					{
						DepthResolution = ImageResolution.Resolution320x240;
					}
					else if (mapMode.XRes == 640 && mapMode.YRes == 480)
					{
						DepthResolution = ImageResolution.Resolution640x480;
					}
					else
					{
						DepthResolution = ImageResolution.Invalid;
					}
					 * */

					Devices.Add(new KinectDevice(deviceIndex++, depth, DepthResolution, ColorResolution));
				}

			}
			catch (Exception ex)
			{
				RC.WriteException(666, "Failed to start OpenNI", ex); 
			}

			if (Devices.Count > 0)
			{
				this.m_ShouldRun = true;

				this.m_ReaderThread = new Thread(ReaderThread);
				this.m_ReaderThread.Start();
			}
			else
			{
				Devices.Add(new KinectDevice(0, DepthResolution, ColorResolution));	
			}

		}

		#endregion

		#region Load / Unload Resources

		public void LoadResources()
		{
			if (m_Disposed == true)
			{
				foreach (KinectDevice device in Devices)
				{
					device.LoadResources();
				}

				m_Disposed = false; 
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				foreach (KinectDevice device in Devices)
				{
					device.UnloadResources();
				}

				m_Disposed = true;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			UnloadResources();

			this.m_ShouldRun = false;

			if (m_ReaderThread != null && m_ReaderThread.IsAlive == true)
			{
				this.m_ReaderThread.Join();
			}			

			foreach (KinectDevice device in Devices)
			{
				device.Dispose(); 
			}

			Devices.Clear();
		}

		#endregion

		#region Calibarate Background
		
		public void CalibarateBackground()
		{
			foreach (KinectDevice device in Devices)
			{
				device.CalibarateBackground();
			}
		}

		#endregion

		#region Reset Frame State

		public void ResetFrameState()
		{
			foreach (KinectDevice device in Devices)
			{
				device.ResetFrameState();
			}
		}

		#endregion

		internal void TryProcessFrame()
		{
			if (m_ReadyToProcessFrame == true)
			{
				foreach (KinectDevice device in Devices)
				{
					if (device.Disposed == true)
					{
						continue;
					}

					device.ProcessDepthFrame();
					device.MarkAsHasNewFrame();					
				}

				m_ReadyToProcessFrame = false; 
			}
		}

		private unsafe void ReaderThread()
		{
			m_MasterContext.StartGeneratingAll();

			while (this.m_ShouldRun)
			{
				try
				{
					this.m_MasterContext.WaitAndUpdateAll(); //  WaitAnyUpdateAll();
				}
				catch (Exception)
				{
				}

				m_ReadyToProcessFrame = true;
			}
		}
	}
}
