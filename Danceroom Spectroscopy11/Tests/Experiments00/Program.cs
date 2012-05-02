using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RugTech1.Framework;
using System.Text;

namespace Experiments
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			double number = 30;

			System.Diagnostics.Debug.WriteLine("powRes:" + Math.Pow(number, 2));
			System.Diagnostics.Debug.WriteLine("manRes:" + DSParticles3.MathHelper.Pow2(number));

			number = -30;

			System.Diagnostics.Debug.WriteLine("powRes:" + Math.Pow(number, 2));
			System.Diagnostics.Debug.WriteLine("manRes:" + DSParticles3.MathHelper.Pow2(number));

			number = 1.0e8;

			System.Diagnostics.Debug.WriteLine("powRes:" + Math.Pow(number, 2));
			System.Diagnostics.Debug.WriteLine("manRes:" + DSParticles3.MathHelper.Pow2(number));

			number = 0.00000000000024;

			System.Diagnostics.Debug.WriteLine("powRes:" + Math.Pow(number, 2));
			System.Diagnostics.Debug.WriteLine("manRes:" + DSParticles3.MathHelper.Pow2(number));


			try
			{
				GameConfiguration.WindowTitle = "Experiments";
				GameConfiguration.WindowWidth = 800;
				GameConfiguration.WindowHeight = 600;

				using (ExperimentApp app = new ExperimentApp())
				{
					app.Run();
				}
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder();

				Exception e = ex;
				while (e != null)
				{
					sb.AppendLine(e.Message);
					sb.AppendLine(e.StackTrace);
					sb.AppendLine();

					e = e.InnerException;
				}

				MessageBox.Show(sb.ToString(), "Unhandled Exception");
			}
		}
	}
}
