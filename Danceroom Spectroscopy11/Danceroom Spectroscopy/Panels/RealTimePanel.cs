using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.UI.Dynamic;
using RugTech1.Framework;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.Text;
using RugTech1.Framework.Objects.UI.Menus;
using DSParticles3;

namespace DS.Panels
{
	class RealTimePanel : PanelBase
	{
		#region Private Members

		private List<ToggleButton> m_SceneButtons = new List<ToggleButton>(); 

		private List<PanelBase> m_SubPanels = new List<PanelBase>(); 
		
		#endregion

		#region Public Properties

		public override SplashSceenPanels ScreenPanel
		{
			get { return SplashSceenPanels.Realtime; }
		} 

		#endregion

		public RealTimePanel(VisiblePanelControler controler, int index)
			: base(controler, index)
		{
			Size = new System.Drawing.Size(GameConfiguration.WindowWidth - 30, 100); 
		}

		#region Panel Method Overrides

		public override void Initiate()
		{			
			int index = 1;

			int verticalOffset = 5;

			List<object> objects = new List<object>();
			List<string> strings = new List<string>();

			for (int i = 0; i < ArtworkStaticObjects.Options.Scenes.Scenes.Length; i++)
			{
				objects.Add(i);
				strings.Add(i.ToString());
			}

			AddButtonSet(this, "Scenes",
						 objects.ToArray(),
						 strings.ToArray(), new EventHandler(SceneButton_Click), ref index, ref verticalOffset, m_SceneButtons);

			SimulationPanel simulationPanel = new SimulationPanel(null, index);
			simulationPanel.Location = new System.Drawing.Point(5, verticalOffset);
			simulationPanel.Size = new System.Drawing.Size(Size.Width - 10, simulationPanel.Size.Height); 
			m_SubPanels.Add(simulationPanel);
			Controls.Add(simulationPanel);
			simulationPanel.Initiate();
			verticalOffset += simulationPanel.Size.Height + 10; 

			index += 10;

			VisualPanel visualPanel = new VisualPanel(null, index);
			visualPanel.Location = new System.Drawing.Point(5, verticalOffset);
			visualPanel.Size = new System.Drawing.Size(Size.Width - 10, visualPanel.Size.Height);
			m_SubPanels.Add(visualPanel);
			Controls.Add(visualPanel);
			visualPanel.Initiate();

			verticalOffset += visualPanel.Size.Height + 10;

			/* 
			Label label = new Label();
			label.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			label.FixedSize = true;
			label.Size = new System.Drawing.Size(290, 30);
			label.Location = new System.Drawing.Point(0, verticalOffset);
			label.Text = "Particle Settings";
			label.FontType = FontType.Heading;
			label.IsVisible = true;
			label.Padding = new System.Windows.Forms.Padding(5);
			label.RelitiveZIndex = index++;

			this.Controls.Add(label);

			verticalOffset += 35;
			*/ 

			int particleBoxSize = (Size.Width - 5) / ParticleStaticObjects.AtomPropertiesDefinition.Count;
			int particleBoxOffset = 5;

			int particlePanelSize = 0; 

			for (int i = 0; i < ParticleStaticObjects.AtomPropertiesDefinition.Count; i++)
			{
				ParticlePanel particlePanel = new ParticlePanel(null, index, i);

				particlePanel.Location = new System.Drawing.Point(particleBoxOffset, verticalOffset);
				particlePanel.Size = new System.Drawing.Size(particleBoxSize - 5, particlePanel.Size.Height);
				m_SubPanels.Add(particlePanel);
				Controls.Add(particlePanel);
				particlePanel.Initiate();

				particleBoxOffset += particleBoxSize;

				particlePanelSize = particlePanel.Size.Height; 
			}

			verticalOffset += particlePanelSize + 10;

			index += 10; 

			//verticalOffset += 10;

			this.Size = new System.Drawing.Size(this.Size.Width, verticalOffset + 5);
		}		

		public override void AddMenuItems(RugTech1.Framework.Objects.UI.Menus.MenuBar menu)
		{
			MenuBarItem realtimeConfig = menu.AddItem("Realtime");
			realtimeConfig.IsVisible = true;
			realtimeConfig.FontType = FontType.Small;
			realtimeConfig.Click += new EventHandler(filterProperties_Click);
		}

		public override void ResetControlValues()
		{
			foreach (PanelBase panel in m_SubPanels)
			{
				panel.ResetControlValues(); 
			}
		}

		public override void UpdateControls()
		{
			foreach (PanelBase panel in m_SubPanels)
			{
				panel.UpdateControls();
			}		
		} 

		#endregion

		#region Control Events

		void SceneButton_Click(object sender, EventArgs e)
		{
			foreach (ToggleButton button in m_SceneButtons)
			{
				button.Value = button == sender;
			}

			int scene = (int)(sender as ToggleButton).Tag;

			if (e is System.Windows.Forms.MouseEventArgs)
			{
				System.Windows.Forms.MouseEventArgs args = e as System.Windows.Forms.MouseEventArgs;

				if (args.Button == System.Windows.Forms.MouseButtons.Right)
				{
					ArtworkStaticObjects.Options.Scenes.Store(scene);
				}
				else
				{
					ArtworkStaticObjects.Options.Scenes.Recall(scene);

					(this.Scene as SplashScreen).ResetControlValues(); 
				}
			}
			else
			{
				ArtworkStaticObjects.Options.Scenes.Recall(scene);

				(this.Scene as SplashScreen).ResetControlValues(); 
			}

		}

		void filterProperties_Click(object sender, EventArgs e)
		{
			PanelControler.SetOrToggleCurrentPanel(ScreenPanel);
		}

		#endregion
	}
}
