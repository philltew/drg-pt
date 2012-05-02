using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.UI.Dynamic;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.Text;
using RugTech1.Framework.Objects.UI.Menus;

namespace DS.Panels
{
	class SimulationPanel : PanelBase
	{
		#region Private Members

		private Slider m_NumberOfParticles;
		private DynamicLabel m_NumberOfParticlesLabel;

		private Slider m_ParticleScale;
		private DynamicLabel m_ParticleScaleLabel;

		private Slider m_BerendsenThermostatCoupling;
		private DynamicLabel m_BerendsenThermostatCouplingLabel;

		private Slider m_EquilibriumTemperature;
		private DynamicLabel m_EquilibriumTemperatureLabel;

		private Slider m_GradientScaleFactor;
		private DynamicLabel m_GradientScaleFactorLabel;
		
		#endregion

		#region Public Properties

		public override SplashSceenPanels ScreenPanel
		{
			get { return SplashSceenPanels.Simulation; }
		} 

		#endregion

		public SimulationPanel(VisiblePanelControler controler, int index)
			: base(controler, index)
		{
			Size = new System.Drawing.Size(800, 100); 
		}

		#region Panel Method Overrides

		public override void Initiate()
		{			
			int index = 1;

			int verticalOffset = 5;

			if (this.PanelControler != null)
			{
				Label label = new Label();
				label.TextAlign = System.Drawing.ContentAlignment.TopLeft;
				label.FixedSize = true;
				label.Size = new System.Drawing.Size(290, 30);
				label.Location = new System.Drawing.Point(0, 5);
				label.Text = "Simulation Properties";
				label.FontType = FontType.Heading;
				label.IsVisible = true;
				label.Padding = new System.Windows.Forms.Padding(5);
				label.RelitiveZIndex = index++;

				this.Controls.Add(label);

				verticalOffset += 30;
			}

			AddSlider(this, "Number Of Particles",
						0, ArtworkStaticObjects.Ensemble.MaxNumberOfParticles, (float)ArtworkStaticObjects.Ensemble.NumberOfParticles,
						new SliderValueChangedEvent(m_NumberOfParticles_ValueChanged),
						ref index, ref verticalOffset,
						out m_NumberOfParticles, out m_NumberOfParticlesLabel);
			
			AddSlider(this, "Particle Scale",
						0.01f, 5f, (float)ArtworkStaticObjects.Ensemble.ParticleScale,
						new SliderValueChangedEvent(m_ParticleScale_ValueChanged),
						ref index, ref verticalOffset,
						out m_ParticleScale, out m_ParticleScaleLabel);

			AddSlider(this, "Equilibrium Temperature",
						0, 100000, (float)ArtworkStaticObjects.Ensemble.EquilibriumTemperature,
						new SliderValueChangedEvent(m_EquilibriumTemperature_ValueChanged),
						ref index, ref verticalOffset,
						out m_EquilibriumTemperature, out m_EquilibriumTemperatureLabel);

			AddSlider(this, "Gradient Scale Factor",
						-100000, 100000, (float)ArtworkStaticObjects.Ensemble.GradientScaleFactor,
						new SliderValueChangedEvent(m_GradientScaleFactor_ValueChanged),
						ref index, ref verticalOffset,
						out m_GradientScaleFactor, out m_GradientScaleFactorLabel);

			AddSlider(this, "Berendsen Thermostat Coupling",
						0.1f, 1, (float)ArtworkStaticObjects.Ensemble.BerendsenThermostatCoupling,
						new SliderValueChangedEvent(m_BerendsenThermostatCoupling_ValueChanged),
						ref index, ref verticalOffset,
						out m_BerendsenThermostatCoupling, out m_BerendsenThermostatCouplingLabel);

			verticalOffset += 10;

			this.Size = new System.Drawing.Size(this.Size.Width, verticalOffset + 5);
		}		

		public override void AddMenuItems(RugTech1.Framework.Objects.UI.Menus.MenuBar menu)
		{
			MenuBarItem kinectConfig = menu.AddItem("Simulation");
			kinectConfig.IsVisible = true;
			kinectConfig.FontType = FontType.Small;
			kinectConfig.Click += new EventHandler(filterProperties_Click);
		}

		public override void ResetControlValues()
		{			
			ArtworkStaticObjects.Ensemble.ParticleScale = ArtworkStaticObjects.Options.Simulation.ParticleScale;
			m_ParticleScale.Value = (float)ArtworkStaticObjects.Options.Simulation.ParticleScale;
			m_ParticleScaleLabel.Text = ArtworkStaticObjects.Options.Simulation.ParticleScale.ToString("N2");

			ArtworkStaticObjects.Ensemble.EquilibriumTemperature = ArtworkStaticObjects.Options.Simulation.EquilibriumTemperature;
			m_EquilibriumTemperature.Value = (float)ArtworkStaticObjects.Ensemble.EquilibriumTemperature;
			m_EquilibriumTemperatureLabel.Text = ArtworkStaticObjects.Ensemble.EquilibriumTemperature.ToString("N2");

			ArtworkStaticObjects.Ensemble.GradientScaleFactor = ArtworkStaticObjects.Options.Simulation.GradientScaleFactor;
			m_GradientScaleFactor.Value = (float)ArtworkStaticObjects.Options.Simulation.GradientScaleFactor;
			m_GradientScaleFactorLabel.Text = ArtworkStaticObjects.Options.Simulation.GradientScaleFactor.ToString("N2");

			ArtworkStaticObjects.Ensemble.BerendsenThermostatCoupling = ArtworkStaticObjects.Options.Simulation.BerendsenThermostatCoupling;
			m_BerendsenThermostatCoupling.Value = (float)ArtworkStaticObjects.Options.Simulation.BerendsenThermostatCoupling;
			m_BerendsenThermostatCouplingLabel.Text = ArtworkStaticObjects.Options.Simulation.BerendsenThermostatCoupling.ToString("N2");

			m_NumberOfParticles.Value = ArtworkStaticObjects.Options.Simulation.NumberOfParticles;
			m_NumberOfParticlesLabel.Text = ArtworkStaticObjects.Options.Simulation.NumberOfParticles.ToString() + ".00"; 
		}

		public override void UpdateControls()
		{
		
		} 

		#endregion

		#region Control Events
		
		void filterProperties_Click(object sender, EventArgs e)
		{
			PanelControler.SetOrToggleCurrentPanel(ScreenPanel);
		}

		void m_NumberOfParticles_ValueChanged(Slider sender, float value)
		{			
			int newNumber = (int)value; 
			while (ArtworkStaticObjects.Ensemble.NumberOfParticles < newNumber)
			{
				ArtworkStaticObjects.Ensemble.InitializeOneNewParticle(); 
			}

			while (ArtworkStaticObjects.Ensemble.NumberOfParticles > newNumber)
			{
				ArtworkStaticObjects.Ensemble.Particles.Pop(); 
			}

			m_NumberOfParticlesLabel.Text = ((int)value).ToString() + ".00";

			ArtworkStaticObjects.Options.Simulation.NumberOfParticles = newNumber;
		}

		void m_ParticleScale_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Ensemble.ParticleScale = (double)value;
			ArtworkStaticObjects.Options.Simulation.ParticleScale = ArtworkStaticObjects.Ensemble.ParticleScale;
			m_ParticleScaleLabel.Text = value.ToString("N2");
		}

		void m_EquilibriumTemperature_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Ensemble.EquilibriumTemperature = (double)value;
			ArtworkStaticObjects.Options.Simulation.EquilibriumTemperature = ArtworkStaticObjects.Ensemble.EquilibriumTemperature;
			m_EquilibriumTemperatureLabel.Text = value.ToString("N2");
		}

		void m_GradientScaleFactor_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Ensemble.GradientScaleFactor = (double)value;
			ArtworkStaticObjects.Options.Simulation.GradientScaleFactor = ArtworkStaticObjects.Ensemble.GradientScaleFactor;
			m_GradientScaleFactorLabel.Text = value.ToString("N2");
		}

		void m_BerendsenThermostatCoupling_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Ensemble.BerendsenThermostatCoupling = (double)value;
			ArtworkStaticObjects.Options.Simulation.BerendsenThermostatCoupling = ArtworkStaticObjects.Ensemble.BerendsenThermostatCoupling;
			m_BerendsenThermostatCouplingLabel.Text = value.ToString("N2");
		}

		#endregion
	}
}
