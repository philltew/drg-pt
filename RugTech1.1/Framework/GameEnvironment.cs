using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rug.Cmd;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;

namespace RugTech1.Framework
{
	public static class GameEnvironment
	{
		public static float FrameDelta;

		public static bool FramesClick;

		public static float FramesPerSecond;

		public static Device Device;

		public static RenderForm Form;

		public static DepthStencilView DepthView;
		public static DepthStencilState DepthState;
		public static RenderTargetView RenderView;

		public static ConsoleBuffer ConsoleBuffer; 

		public static void SetupConsole()
		{
			if (ConsoleBuffer == null)
			{
				ConsoleBuffer = new ConsoleBuffer();

				RC.App = ConsoleBuffer;
			}
		}

		public static void ShutdownConsole()
		{
			if (ConsoleBuffer != null)
			{
				ConsoleBuffer = null; 

				RC.App = RC.Sys; 
			}
		}
	}
}
