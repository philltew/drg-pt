using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.UI.Dynamic;
using RugTech1.Framework.Objects.Text;
using RugTech1.Framework.Objects.UI.Menus;

namespace DS.Panels
{
	class FFTPanel : PanelBase
	{
		#region Private Members
		
		private ToggleButton m_FFTEnabled;
		private ToggleButton m_ParticleEventsEnabled;		

		private Slider m_FFTFrequency;
		private DynamicLabel m_FFTFrequencyLabel;
				
		private Slider m_ParticleCorrelationFunctionFreqency;
		private DynamicLabel m_ParticleCorrelationFunctionFreqencyLabel;

		private Slider m_PeakCount;
		private DynamicLabel m_PeakCountLabel;

		private Slider m_SendFFTFrequency;
		private DynamicLabel m_SendFFTFrequencyLabel;

		#endregion


		#region Public Properties

		public override SplashSceenPanels ScreenPanel
		{
			get { return SplashSceenPanels.FFT; }
		} 

		#endregion

		public FFTPanel(VisiblePanelControler controler, int index)
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
			label.Text = "FFT Properties";
			label.FontType = FontType.Heading;
			label.IsVisible = true;
			label.Padding = new System.Windows.Forms.Padding(5); 
			label.RelitiveZIndex = index++;
			this.Controls.Add(label);

			int verticalOffset = 35;

			int buttonSize = (Size.Width - 15) / 2; 

			m_FFTEnabled = new ToggleButton();
			m_FFTEnabled.Text = ArtworkStaticObjects.Options.FFT.FFTEnabled ? "FFT Enabled" : "FFT Disabled";
			m_FFTEnabled.Value = !ArtworkStaticObjects.Options.FFT.FFTEnabled;
			m_FFTEnabled.Size = new System.Drawing.Size(buttonSize, 20);
			m_FFTEnabled.Location = new System.Drawing.Point(5, verticalOffset);
			m_FFTEnabled.FontType = FontType.Small;
			m_FFTEnabled.RelitiveZIndex = index++;
			m_FFTEnabled.IsVisible = true;
			m_FFTEnabled.Click += new EventHandler(m_FFTEnabled_Click);
			this.Controls.Add(m_FFTEnabled);

			m_ParticleEventsEnabled = new ToggleButton();
			m_ParticleEventsEnabled.Text = ArtworkStaticObjects.Options.FFT.ParticleEventsEnabled ? "Particle Events On" : "Particle Events Off";
			m_ParticleEventsEnabled.Value = !ArtworkStaticObjects.Options.FFT.ParticleEventsEnabled;
			m_ParticleEventsEnabled.Size = new System.Drawing.Size(buttonSize, 20);
			m_ParticleEventsEnabled.Location = new System.Drawing.Point(buttonSize + 10, verticalOffset);
			m_ParticleEventsEnabled.FontType = FontType.Small;
			m_ParticleEventsEnabled.RelitiveZIndex = index++;
			m_ParticleEventsEnabled.IsVisible = true;
			m_ParticleEventsEnabled.Click += new EventHandler(m_ParticleEventsEnabled_Click);
			this.Controls.Add(m_ParticleEventsEnabled);

			verticalOffset += 25;

			AddSlider(this, "FFT Frequency",
				1, 60, (float)ArtworkStaticObjects.Options.FFT.FFTFrequency,
				new SliderValueChangedEvent(m_FFTFrequency_ValueChanged),
				ref index, ref verticalOffset,
				out m_FFTFrequency, out m_FFTFrequencyLabel);

			AddSlider(this, "Correlation Function Update Frequency",
				1, 60, (float)ArtworkStaticObjects.Options.FFT.CorrelationFunctionUpdateFrequency,
				new SliderValueChangedEvent(m_ParticleCorrelationFunctionFreqency_ValueChanged),
				ref index, ref verticalOffset,
				out m_ParticleCorrelationFunctionFreqency, out m_ParticleCorrelationFunctionFreqencyLabel);

			AddSlider(this, "Peak Count",
				1, 24, (float)ArtworkStaticObjects.Options.FFT.PeakCount,
				new SliderValueChangedEvent(m_PeakCount_ValueChanged),
				ref index, ref verticalOffset,
				out m_PeakCount, out m_PeakCountLabel);

			AddSlider(this, "Send FFT Frequency",
				1, 60 * 60, (float)ArtworkStaticObjects.Options.FFT.SendFFTFrequency,
				new SliderValueChangedEvent(m_SendFFTFrequency_ValueChanged),
				ref index, ref verticalOffset,
				out m_SendFFTFrequency, out m_SendFFTFrequencyLabel);

			this.Size = new System.Drawing.Size(this.Size.Width, verticalOffset + 5);
		}

		public override void AddMenuItems(RugTech1.Framework.Objects.UI.Menus.MenuBar menu)
		{
			MenuBarItem fftConfig = menu.AddItem("FFT");
			fftConfig.IsVisible = true;
			fftConfig.FontType = FontType.Small;
			fftConfig.Click += new EventHandler(fftProperties_Click);
		}

		public override void ResetControlValues()
		{
			m_FFTEnabled.Text = ArtworkStaticObjects.Options.FFT.FFTEnabled ? "FFT Enabled" : "FFT Disabled";
			m_FFTEnabled.Value = !ArtworkStaticObjects.Options.FFT.FFTEnabled;

			m_ParticleEventsEnabled.Text = ArtworkStaticObjects.Options.FFT.ParticleEventsEnabled ? "Particle Events On" : "Particle Events Off";
			m_ParticleEventsEnabled.Value = !ArtworkStaticObjects.Options.FFT.ParticleEventsEnabled;

			m_FFTFrequency.Value = (float)ArtworkStaticObjects.Options.FFT.FFTFrequency;
			m_FFTFrequencyLabel.Text = ArtworkStaticObjects.Options.FFT.FFTFrequency.ToString() + ".00";

			m_PeakCount.Value = (float)ArtworkStaticObjects.Options.FFT.PeakCount;
			m_PeakCountLabel.Text = ArtworkStaticObjects.Options.FFT.PeakCount.ToString() + ".00";

			m_SendFFTFrequency.Value = (float)ArtworkStaticObjects.Options.FFT.SendFFTFrequency;
			m_SendFFTFrequencyLabel.Text = ArtworkStaticObjects.Options.FFT.SendFFTFrequency.ToString() + ".00";

			m_ParticleCorrelationFunctionFreqency.Value = (float)ArtworkStaticObjects.Options.FFT.CorrelationFunctionUpdateFrequency;
			m_ParticleCorrelationFunctionFreqencyLabel.Text = ArtworkStaticObjects.Options.FFT.CorrelationFunctionUpdateFrequency.ToString() + ".00";
		}

		public override void UpdateControls()
		{

		}

		#endregion

		#region Control Events
		
		void fftProperties_Click(object sender, EventArgs e)
		{
			PanelControler.SetOrToggleCurrentPanel(ScreenPanel); 
		}

		void m_FFTEnabled_Click(object sender, EventArgs e)
		{
			ArtworkStaticObjects.Options.FFT.FFTEnabled = !ArtworkStaticObjects.Options.FFT.FFTEnabled;

			m_FFTEnabled.Text = ArtworkStaticObjects.Options.FFT.FFTEnabled ? "FFT Enabled" : "FFT Disabled";
			m_FFTEnabled.Value = !ArtworkStaticObjects.Options.FFT.FFTEnabled; 
		}

		void m_ParticleEventsEnabled_Click(object sender, EventArgs e)
		{
			ArtworkStaticObjects.Options.FFT.ParticleEventsEnabled = !ArtworkStaticObjects.Options.FFT.ParticleEventsEnabled;

			m_ParticleEventsEnabled.Text = ArtworkStaticObjects.Options.FFT.ParticleEventsEnabled ? "Particle Events On" : "Particle Events Off";
			m_ParticleEventsEnabled.Value = !ArtworkStaticObjects.Options.FFT.ParticleEventsEnabled; 
		}

		void m_FFTFrequency_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.FFT.FFTFrequency = (int)value;
			m_FFTFrequencyLabel.Text = ((float)ArtworkStaticObjects.Options.FFT.FFTFrequency).ToString("N2");
		}

		void m_PeakCount_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.FFT.PeakCount = (int)value;
			m_PeakCountLabel.Text = ((float)ArtworkStaticObjects.Options.FFT.PeakCount).ToString("N2");
		}		

		void m_SendFFTFrequency_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.FFT.SendFFTFrequency = (int)value;
			m_SendFFTFrequencyLabel.Text = ((float)ArtworkStaticObjects.Options.FFT.SendFFTFrequency).ToString("N2");
		}

		void m_ParticleCorrelationFunctionFreqency_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.FFT.CorrelationFunctionUpdateFrequency = (int)value;
			m_ParticleCorrelationFunctionFreqencyLabel.Text = ((float)ArtworkStaticObjects.Options.FFT.CorrelationFunctionUpdateFrequency).ToString("N2");

			if (ArtworkStaticObjects.Options.FFT.FFTFrequency < ArtworkStaticObjects.Options.FFT.CorrelationFunctionUpdateFrequency)
			{
				ArtworkStaticObjects.Options.FFT.FFTFrequency = (int)value;
				m_FFTFrequency.Value = (float)(int)value; 
				m_FFTFrequencyLabel.Text = ((float)ArtworkStaticObjects.Options.FFT.FFTFrequency).ToString("N2");
			}
		}

		#endregion
	}
}
