using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework;
using RugTech1.Framework.Objects.Text;
using RugTech1.Framework.Objects.UI;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.UI.Dynamic;
using RugTech1.Framework.Objects.UI.Menus;

namespace Experiments.Panels
{
	class SplashScreen : UiScene
	{
		#region Private Members

		private bool m_IsVisible = true;

		private DynamicLabel m_FPSLabel;
		private MultiGraph m_SpeedGraph;
		private SubGraph m_FPSGraph;

		private VisiblePanelControler m_PanelControler = new VisiblePanelControler(); 
		private List<PanelBase> m_Panels = new List<PanelBase>(); 

		#endregion

		#region Properties
		
		public bool IsVisible { get { return m_IsVisible; } set { m_IsVisible = value; } }

		#endregion

		#region Initialize Controls

		protected override void InitializeControls()
		{
			base.InitializeControls();

			m_Panels.Add(new SpeedTestPanel(m_PanelControler, 10));
			m_Panels.Add(new ConsistencyTestPanel(m_PanelControler, 20));

			CreateMenu();

			CreateStatusBar();

			foreach (PanelBase panel in m_Panels)
			{
				this.Controls.Add(panel);

				panel.Initiate(); 
			}
		}

		#endregion

		#region Update Dynamic Controls

		public override void Update()
		{
			base.Update();

			if (GameEnvironment.FramesClick)
			{ 
				m_FPSLabel.Text = "FPS: " + GameEnvironment.FramesPerSecond.ToString("N2");
				m_FPSGraph.AddValue(GameEnvironment.FramesPerSecond);
			}

			if (IsVisible == true)
			{ 
				foreach (PanelBase panel in m_Panels)
				{
					panel.UpdateControls(); 
				}
			}
		}

		#endregion

		#region Create Status Bar

		private void CreateStatusBar()
		{
			#region Status Bar

			int index = 20; 

			Panel statusPanel = new Panel();

			statusPanel.ShowBackground = true;
			statusPanel.ShowBorder = false;
			statusPanel.Docking = System.Windows.Forms.DockStyle.Bottom;
			statusPanel.IsVisible = true;
			statusPanel.RelitiveZIndex = index++; 
			statusPanel.Size = new System.Drawing.Size(152, 24);

			Controls.Add(statusPanel);

			index = 1; 

			m_SpeedGraph = new MultiGraph();

			m_SpeedGraph.Location = new System.Drawing.Point(2, 2);
			m_SpeedGraph.Size = new System.Drawing.Size(150, 20);
			m_SpeedGraph.IsVisible = true;
			m_SpeedGraph.RelitiveZIndex = index++; 

			statusPanel.Controls.Add(m_SpeedGraph);

			m_FPSGraph = new SubGraph(150);
			m_FPSGraph.IsVisible = true;
			m_FPSGraph.LineColor = new SlimDX.Color4(0.75f, 0.3f, 1f, 0.3f);			
			m_SpeedGraph.Graphs.Add(m_FPSGraph);

			m_FPSLabel = new DynamicLabel();
			m_FPSLabel.ForeColor = new SlimDX.Color4(0.75f, 0.3f, 1f, 0.3f);
			m_FPSLabel.MaxLength = 16;
			m_FPSLabel.Location = new System.Drawing.Point(154, 0);
			m_FPSLabel.FixedSize = false;
			m_FPSLabel.FontType = FontType.Small;
			m_FPSLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			m_FPSLabel.IsVisible = true;
			m_FPSLabel.RelitiveZIndex = index++; 
			statusPanel.Controls.Add(m_FPSLabel);			

			#endregion
		}

		#endregion

		#region Create Menu

		private void CreateMenu()
		{
			#region Menu Bar

			int index = 100; 

			MenuBar menu = new MenuBar();

			menu.IsVisible = true;
			menu.Docking = System.Windows.Forms.DockStyle.Top;
			menu.Size = new System.Drawing.Size(20, 20);
			menu.RelitiveZIndex = index; 
			Controls.Add(menu);

			foreach (PanelBase panel in m_Panels)
			{
				panel.AddMenuItems(menu);
			}

			#endregion
		}
		
		#endregion

		#region Reset Control Values
		
		public void ResetControlValues()
		{
			foreach (PanelBase panel in m_Panels)
			{
				panel.ResetControlValues(); 
			}

			this.Invalidate();
		}

		#endregion
	}
}

