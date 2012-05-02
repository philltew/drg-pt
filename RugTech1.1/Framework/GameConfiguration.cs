using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RugTech1.Framework
{
	public static class GameConfiguration
	{
		public static string WindowTitle = "Base Game";

		public static int WindowCount = 2;
		public static int WindowWidth = 640;
		public static int WindowHeight = 480;

		public static Rectangle ActiveRegion = new Rectangle(0, 0, WindowWidth, WindowHeight);

		public static bool IsFullScreen = false;
		public static int AdapterOrdinal = 0;
		public static bool LockFrameRate = true;
		public static int DesiredFrameRate = 30; 

		public static SlimDX.Direct3D11.DeviceCreationFlags CreationFlags = SlimDX.Direct3D11.DeviceCreationFlags.None;		
	}
}
