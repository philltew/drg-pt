using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DS.Panels
{
	public enum SplashSceenPanels { None, Cameras, FieldData, Simulation, Visual, Osc, FFT, Particles, Realtime, Particle }

	public delegate void PanelEvent(SplashSceenPanels CurrentPanel); 

	public class VisiblePanelControler
	{
		#region Private Members

		private SplashSceenPanels m_CurrentPanel = SplashSceenPanels.None;

		#endregion

		#region Public Properties and Events

		public SplashSceenPanels CurrentPanel { get { return m_CurrentPanel; } set { m_CurrentPanel = value; OnPanelChanged(); } }

		public event PanelEvent PanelChanged;  

		#endregion

		private void OnPanelChanged()
		{
			if (PanelChanged != null)
			{
				PanelChanged(m_CurrentPanel); 
			}
		}

		public void SetOrToggleCurrentPanel(SplashSceenPanels ScreenPanel)
		{
			if (CurrentPanel == ScreenPanel)
			{
				CurrentPanel = SplashSceenPanels.None;
			}
			else
			{
				CurrentPanel = ScreenPanel; 
			}
		}
	}
}
