using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rug.Cmd;
using Rug.Cmd.Colors;
using RugTech1.Framework;
using RugTech1.Framework.Effects;

namespace FieldTextureTest
{
	class Program
	{
		static void Main(string[] args)
		{
			ConsoleColorState state = RC.ColorState;

			try
			{
				SharedEffects.Effects.Add("Imposter2", new ImposterEffect2()); 

				RC.Theme = ConsoleColorTheme.Load(ConsoleColorDefaultThemes.Colorful);
				RC.Verbosity = ConsoleVerbosity.Normal;

				try
				{
					//GameEnvironment.SetupConsole();

					GameConfiguration.WindowTitle = "Texture Tests";
					GameConfiguration.WindowWidth = 800;
					GameConfiguration.WindowHeight = 600;

					using (TextureTests app = new TextureTests())
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

				RC.ReadKey(true); 
			}
			finally
			{
				RC.ColorState = state;
			}
		} 
	}
}
