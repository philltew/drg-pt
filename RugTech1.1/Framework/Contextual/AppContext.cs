using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rug.Cmd;
using RugTech1.Framework.Objects;
using SlimDX.Windows;

namespace RugTech1.Framework.Contextual
{
	public class AppContext : ApplicationContext, IEnumerable<FormContext>, IResourceManager
	{
		private bool m_Disposed = true;
		private int m_ActiveFormCount = 0; 

		private List<FormContext> m_Forms = new List<FormContext>();		

		public FormContext this[int index] { get { return m_Forms[index]; } }

		public int Count { get { return m_Forms.Count; } }

		public AppContext(int windowWidth, int windowHeight)
		{
#if DEBUG
			RC.WriteLine(ConsoleThemeColor.SubText3, "App Context");
#endif
			m_ActiveFormCount = 0;

			// Handle the ApplicationExit event to know when the application is exiting.
			Application.ApplicationExit += new EventHandler(this.OnApplicationExit);

			FormContext newContext = new FormContext(0, GameConfiguration.WindowTitle + " " + 1.ToString(), windowWidth, windowHeight);

			m_Forms.Add(newContext);

			newContext.Form.Closed += new EventHandler(OnFormClosed);
			newContext.Form.Closing += new CancelEventHandler(OnFormClosing);

			m_ActiveFormCount++;

#if DEBUG
			RC.WriteLine(ConsoleThemeColor.SubText3, "App Context: Show Forms");
#endif
			newContext.Form.Show(); 
			
			MainForm = m_Forms[0].Form; 
		}

		public void CreateAdditionalForms(int count)
		{
			for (int i = 0; i < count; i++)
			{
				int index = i + 1;

				FormContext newContext = new FormContext(index, GameConfiguration.WindowTitle + " " + (index + 1).ToString(), m_Forms[0].Form.ClientSize.Width, m_Forms[0].Form.ClientSize.Height);

				m_Forms.Add(newContext);

				newContext.Form.Closed += new EventHandler(OnFormClosed);
				newContext.Form.Closing += new CancelEventHandler(OnFormClosing);

				m_ActiveFormCount++;

				newContext.Form.Show(); 
			}
		}

		private void OnApplicationExit(object sender, EventArgs e)
		{

		}

		private void OnFormClosing(object sender, CancelEventArgs e)
		{

		}

		private void OnFormClosed(object sender, EventArgs e)
		{
			// When a form is closed, decrement the count of open forms.

			// When the count gets to 0, exit the app by calling
			// ExitThread().
			m_ActiveFormCount--;

			if (m_ActiveFormCount == 0)
			{
#if DEBUG
				RC.WriteLine(ConsoleThemeColor.SubText3, "App Context: Thread Exit");
#endif
				ExitThread();
			}
		}

		#region IEnumerable<RenderContext> Members

		public IEnumerator<FormContext> GetEnumerator()
		{
			return m_Forms.GetEnumerator(); 
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return (m_Forms as System.Collections.IEnumerable).GetEnumerator(); 
		}

		#endregion

		#region IResourceManager Members

		public bool Disposed
		{
			get { return m_Disposed; }
		}

		public void LoadResources()
		{
#if DEBUG
			RC.WriteLine(ConsoleThemeColor.SubText3, "App Context: Load Resources");
#endif
			if (m_Disposed == true)
			{
				foreach (FormContext context in m_Forms)
				{
					context.LoadResources();
				}

				m_Disposed = false; 
			}
		}

		public void UnloadResources()
		{
#if DEBUG
			RC.WriteLine(ConsoleThemeColor.SubText3, "App Context: Unload Resources");
#endif
			if (m_Disposed == false)
			{
				foreach (FormContext context in m_Forms)
				{
					context.UnloadResources();
				}

				m_Disposed = true;
			}
		}

		
		#endregion

		#region IDisposable Members

		void IDisposable.Dispose()
		{
#if DEBUG
			RC.WriteLine(ConsoleThemeColor.SubText3, "App Context: Dispose");
#endif
			UnloadResources();

			foreach (FormContext context in m_Forms)
			{
				context.Dispose();
			}
		}

		#endregion

		public bool NeedsResize(int WindowWidth, int WindowHeight)
		{
			bool needsResize = false; 

			foreach (FormContext context in m_Forms)
			{
				needsResize |= context.NeedsResize(WindowWidth, WindowHeight);
			}

			return needsResize; 
		}
	}
}
