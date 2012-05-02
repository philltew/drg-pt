using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rug.Cmd;
using Rug.Cmd.Colors;
using RugTech1.Framework;
using RugTech1.Framework.Effects;

namespace DS
{
	class Program
	{
		static void Main(string[] args)
		{
			ConsoleColorState state = RC.ColorState;

			try
			{
				RC.Theme = ConsoleColorTheme.Load(ConsoleColorDefaultThemes.Colorful);
				RC.Verbosity = ConsoleVerbosity.Normal;

				try
				{
					GameConfiguration.WindowCount = 2;
					GameEnvironment.SetupConsole();
					GameEnvironment.ConsoleBuffer.BufferWidth = 93; 

					GameConfiguration.WindowTitle = "Danceroom Spectroscopy";
					GameConfiguration.WindowWidth = 1024;
					GameConfiguration.WindowHeight = 768;

					using (MainWindow app = new MainWindow())
					{
						app.Run();
					}
				}
				finally
				{
					GameEnvironment.ShutdownConsole();
				}
			}
			catch (Exception ex)
			{
				RC.WriteException(01, "Unhandled Exception", ex);

				if (RC.ShouldWrite(ConsoleVerbosity.Debug) == false)
				{
					RC.WriteStackTrace(ex.StackTrace);
				}

				Exception e = ex.InnerException;
				while (e != null)
				{
					RC.WriteException(01, e);

					if (RC.ShouldWrite(ConsoleVerbosity.Debug) == false)
					{
						RC.WriteStackTrace(e.StackTrace);
					}

					e = e.InnerException; 
				}

				RC.PromptForKey("Press any key to exit", true, true);
			}
			finally
			{
				RC.ColorState = state;
			}
		}
	}
}
