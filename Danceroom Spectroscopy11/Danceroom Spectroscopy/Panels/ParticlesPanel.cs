using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.UI.Dynamic;
using DSParticles3;
using RugTech1.Framework.Objects.Text;
using RugTech1.Framework.Objects.UI.Menus;

namespace DS.Panels
{
	class ParticlesPanel : PanelBase
	{
		#region Private Members

		private List<ToggleButton> m_ParticleTypeButtons = new List<ToggleButton>(); 

		private ToggleButton m_AttractiveOrRepulsive;
		private ToggleButton m_IsSoundOn;
		private ToggleButton m_IsEnabled; 

		private int m_ActiveIndex = 0; 
		
		#endregion

		#region Public Properties

		public override SplashSceenPanels ScreenPanel
		{
			get { return SplashSceenPanels.Particles; }
		} 

		#endregion

		public ParticlesPanel(VisiblePanelControler controler, int index)
			: base(controler, index)
		{
			
		}

		#region Panel Method Overrides

		public override void Initiate()
		{			
			int index = 1;

			Label label = new Label();
			label.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			label.FixedSize = true;
			label.Size = new System.Drawing.Size(290, 30);
			label.Location = new System.Drawing.Point(0, 5);
			label.Text = "Particle Settings";
			label.FontType = FontType.Heading;
			label.IsVisible = true;
			label.Padding = new System.Windows.Forms.Padding(5);
			label.RelitiveZIndex = index++;

			this.Controls.Add(label);

			int verticalOffset = 35;

			List<object> objects = new List<object>();
			List<string> strings = new List<string>(); 

			for (int i = 0; i < ParticleStaticObjects.AtomPropertiesDefinition.Count; i++)
			{
				objects.Add(i);
				strings.Add(ParticleStaticObjects.AtomPropertiesDefinition.Lookup[i].Name); 
			}

			AddButtonSet(this, "Particle Type",
						 objects.ToArray(),
						 strings.ToArray(), new EventHandler(particleTypeButton_Click), ref index, ref verticalOffset, m_ParticleTypeButtons);

			m_IsEnabled = new ToggleButton();
			m_IsEnabled.Text = "Is Enabled";
			m_IsEnabled.FontType = FontType.Regular;
			m_IsEnabled.Size = new System.Drawing.Size(Size.Width - 10, 20);
			m_IsEnabled.Location = new System.Drawing.Point(5, verticalOffset);
			m_IsEnabled.IsVisible = true;
			m_IsEnabled.Click += new EventHandler(IsEnabled_Click);
			m_IsEnabled.RelitiveZIndex = index++;

			this.Controls.Add(m_IsEnabled);

			verticalOffset += 25;

			m_AttractiveOrRepulsive = new ToggleButton();
			m_AttractiveOrRepulsive.Text = "Attractive";
			m_AttractiveOrRepulsive.FontType = FontType.Regular;
			m_AttractiveOrRepulsive.Size = new System.Drawing.Size(Size.Width - 10, 20);
			m_AttractiveOrRepulsive.Location = new System.Drawing.Point(5, verticalOffset);
			m_AttractiveOrRepulsive.IsVisible = true;
			m_AttractiveOrRepulsive.Click += new EventHandler(AttractiveOrRepulsive_Click);
			m_AttractiveOrRepulsive.RelitiveZIndex = index++;

			this.Controls.Add(m_AttractiveOrRepulsive);

			verticalOffset += 25;

			m_IsSoundOn = new ToggleButton();
			m_IsSoundOn.Text = "Sound On";
			m_IsSoundOn.FontType = FontType.Regular;
			m_IsSoundOn.Size = new System.Drawing.Size(Size.Width - 10, 20);
			m_IsSoundOn.Location = new System.Drawing.Point(5, verticalOffset);
			m_IsSoundOn.IsVisible = true;
			m_IsSoundOn.Click += new EventHandler(IsSoundOn_Click);
			m_IsSoundOn.RelitiveZIndex = index++;

			this.Controls.Add(m_IsSoundOn);

			verticalOffset += 25;

			m_AttractiveOrRepulsive.Value = ParticleStaticObjects.AtomPropertiesDefinition.Lookup[m_ActiveIndex].AttractiveOrRepulsive > 0;
			m_IsEnabled.Value = !ParticleStaticObjects.AtomPropertiesDefinition.Lookup[m_ActiveIndex].Enabled;
			m_IsSoundOn.Value = !ParticleStaticObjects.AtomPropertiesDefinition.Lookup[m_ActiveIndex].IsSoundOn;

			this.Size = new System.Drawing.Size(this.Size.Width, verticalOffset + 5);
		}

		public override void AddMenuItems(RugTech1.Framework.Objects.UI.Menus.MenuBar menu)
		{
			MenuBarItem particleConfig = menu.AddItem("Particles");
			particleConfig.IsVisible = true;
			particleConfig.FontType = FontType.Small;
			particleConfig.Click += new EventHandler(particleProperties_Click);
		}

		public override void ResetControlValues()
		{

		}

		public override void UpdateControls()
		{

		} 

		#endregion

		#region Control Events
		
		void particleProperties_Click(object sender, EventArgs e)
		{
			PanelControler.SetOrToggleCurrentPanel(ScreenPanel);			
		}

		void particleTypeButton_Click(object sender, EventArgs e)
		{
			foreach (ToggleButton button in m_ParticleTypeButtons)
			{
				button.Value = button == sender;
			}

			m_ActiveIndex = (int)(sender as ToggleButton).Tag;

			m_AttractiveOrRepulsive.Value = ParticleStaticObjects.AtomPropertiesDefinition.Lookup[m_ActiveIndex].AttractiveOrRepulsive > 0;
			m_IsEnabled.Value = !ParticleStaticObjects.AtomPropertiesDefinition.Lookup[m_ActiveIndex].Enabled;
			m_IsSoundOn.Value = !ParticleStaticObjects.AtomPropertiesDefinition.Lookup[m_ActiveIndex].IsSoundOn;

		}

		void IsEnabled_Click(object sender, EventArgs e)
		{
			ToggleButton button = sender as ToggleButton;

			ParticleStaticObjects.AtomPropertiesDefinition.SetEnabled(m_ActiveIndex, !button.Value); 
		}	
		

		void AttractiveOrRepulsive_Click(object sender, EventArgs e)
		{
			ToggleButton button = sender as ToggleButton;

			ParticleStaticObjects.AtomPropertiesDefinition.ToggleAttractiveOrRepulsive(m_ActiveIndex); 
		}	
	
		void IsSoundOn_Click(object sender, EventArgs e)
		{
			ToggleButton button = sender as ToggleButton;

			ParticleStaticObjects.AtomPropertiesDefinition.SetSound(m_ActiveIndex, !button.Value); 
		}	

		#endregion
	}
}
