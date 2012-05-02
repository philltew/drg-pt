using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Experiments.Panels
{
	public enum SplashSceenPanels { None, SpeedTests, Consistency }

	public delegate void PanelEvent(SplashSceenPanels CurrentPanel); 

	public class VisiblePanelControler
	{
		private SplashSceenPanels m_CurrentPanel = SplashSceenPanels.None;

		public SplashSceenPanels CurrentPanel { get { return m_CurrentPanel; } set { m_CurrentPanel = value; OnPanelChanged(); } }

		public event PanelEvent PanelChanged; 

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
