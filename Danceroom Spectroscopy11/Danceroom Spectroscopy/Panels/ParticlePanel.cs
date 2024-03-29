﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.Text;
using DSParticles3;
using RugTech1.Framework.Objects.UI.Menus;

namespace DS.Panels
{
	class ParticlePanel : PanelBase
	{
		#region Private Members

		private ToggleButton m_AttractiveOrRepulsive;
		private ToggleButton m_IsSoundOn;
		private ToggleButton m_IsEnabled; 

		private int m_ActiveIndex = 0; 
		
		#endregion

		#region Public Properties

		public override SplashSceenPanels ScreenPanel
		{
			get { return SplashSceenPanels.Particle; }
		} 

		#endregion

		public ParticlePanel(VisiblePanelControler controler, int index, int activeIndex)
			: base(controler, index)
		{
			m_ActiveIndex = activeIndex; 
		}

		#region Panel Method Overrides

		public override void Initiate()
		{			
			int index = 1;

			int verticalOffset = 5;

			List<object> objects = new List<object>();
			List<string> strings = new List<string>(); 

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
			MenuBarItem particleConfig = menu.AddItem("Particle " + m_ActiveIndex);
			particleConfig.IsVisible = true;
			particleConfig.FontType = FontType.Small;
			particleConfig.Click += new EventHandler(particleProperties_Click);
		}

		public override void ResetControlValues()
		{
			m_AttractiveOrRepulsive.Value = ArtworkStaticObjects.Options.Simulation.ParticleTypes[m_ActiveIndex].AttractiveOrRepulsive;
			ParticleStaticObjects.AtomPropertiesDefinition.SetAttractiveOrRepulsive(m_ActiveIndex, m_AttractiveOrRepulsive.Value);

			m_IsEnabled.Value = !ArtworkStaticObjects.Options.Simulation.ParticleTypes[m_ActiveIndex].IsEnabled;
			m_IsSoundOn.Value = !ArtworkStaticObjects.Options.Simulation.ParticleTypes[m_ActiveIndex].IsSoundOn;

			ParticleStaticObjects.AtomPropertiesDefinition.SetEnabled(m_ActiveIndex, !m_IsEnabled.Value);
			ParticleStaticObjects.AtomPropertiesDefinition.SetSound(m_ActiveIndex, !m_IsSoundOn.Value);
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

		void IsEnabled_Click(object sender, EventArgs e)
		{
			ToggleButton button = sender as ToggleButton;

			ArtworkStaticObjects.Options.Simulation.ParticleTypes[m_ActiveIndex].IsEnabled = !button.Value;
			ParticleStaticObjects.AtomPropertiesDefinition.SetEnabled(m_ActiveIndex, !button.Value); 
		}	
		

		void AttractiveOrRepulsive_Click(object sender, EventArgs e)
		{
			ToggleButton button = sender as ToggleButton;

			ArtworkStaticObjects.Options.Simulation.ParticleTypes[m_ActiveIndex].AttractiveOrRepulsive = button.Value;
			ParticleStaticObjects.AtomPropertiesDefinition.SetAttractiveOrRepulsive(m_ActiveIndex, button.Value);			
		}	
	
		void IsSoundOn_Click(object sender, EventArgs e)
		{
			ToggleButton button = sender as ToggleButton;

			ArtworkStaticObjects.Options.Simulation.ParticleTypes[m_ActiveIndex].IsSoundOn = !button.Value;
			ParticleStaticObjects.AtomPropertiesDefinition.SetSound(m_ActiveIndex, !button.Value); 
		}	

		#endregion
	}
}
