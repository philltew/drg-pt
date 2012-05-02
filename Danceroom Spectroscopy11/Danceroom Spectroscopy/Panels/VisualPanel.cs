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
	class VisualPanel : PanelBase
	{
		#region Private Members

		private Slider m_FeedbackLevel;
		private DynamicLabel m_FeedbackLevelLabel;

		private Slider m_ParticleFeedbackLevel;
		private DynamicLabel m_ParticleFeedbackLevelLabel;		

		private Slider m_SelfImage;
		private DynamicLabel m_SelfImageLabel;

		private Slider m_SelfColor;
		private DynamicLabel m_SelfColorLabel;

		private Slider m_SelfFeedback;
		private DynamicLabel m_SelfFeedbackLabel;

		private Slider m_SelfFeedbackColor;
		private DynamicLabel m_SelfFeedbackColorLabel;

		private Slider m_WarpVariance;
		private DynamicLabel m_WarpVarianceLabel;

		private Slider m_WarpPersistence;
		private DynamicLabel m_WarpPersistenceLabel;

		private Slider m_WarpPropagation;
		private DynamicLabel m_WarpPropagationLabel;
		
		#endregion

		#region Public Properties

		public override SplashSceenPanels ScreenPanel
		{
			get { return SplashSceenPanels.Visual; }
		} 

		#endregion

		public VisualPanel(VisiblePanelControler controler, int index)
			: base(controler, index)
		{
			this.Size = new System.Drawing.Size(600, 100); 
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
				label.Text = "Visual Properties";
				label.FontType = FontType.Heading;
				label.IsVisible = true;
				label.Padding = new System.Windows.Forms.Padding(5);
				label.RelitiveZIndex = index++;

				this.Controls.Add(label);

				verticalOffset += 30;
			}

			AddSlider(this, "Feedback Level",
						0, 1, ArtworkStaticObjects.Options.Visual.FeedbackLevel,
						new SliderValueChangedEvent(m_FeedbackLevel_ValueChanged),
						ref index, ref verticalOffset,
						out m_FeedbackLevel, out m_FeedbackLevelLabel);

			AddSlider(this, "Particle Feedback Level",
						0, 1, ArtworkStaticObjects.Options.Visual.ParticleFeedbackLevel,
						new SliderValueChangedEvent(m_ParticleFeedbackLevel_ValueChanged),
						ref index, ref verticalOffset,
						out m_ParticleFeedbackLevel, out m_ParticleFeedbackLevelLabel);

			AddSlider(this, "Self Image",
						0, 1f, ArtworkStaticObjects.Options.Visual.SelfImage, // * 10f,
						new SliderValueChangedEvent(m_SelfImage_ValueChanged),
						ref index, ref verticalOffset,
						out m_SelfImage, out m_SelfImageLabel);

			AddSlider(this, "Self Color",
						0, 1f, ArtworkStaticObjects.Options.Visual.SelfColor, // * 10f,
						new SliderValueChangedEvent(m_SelfColor_ValueChanged),
						ref index, ref verticalOffset,
						out m_SelfColor, out m_SelfColorLabel);	

			AddSlider(this, "Self Feedback",
						0, 1f, ArtworkStaticObjects.Options.Visual.SelfFeedback, // * 10f,
						new SliderValueChangedEvent(m_SelfFeedback_ValueChanged),
						ref index, ref verticalOffset,
						out m_SelfFeedback, out m_SelfFeedbackLabel);

			AddSlider(this, "Self Feedback Color",
						0, 1f, ArtworkStaticObjects.Options.Visual.SelfColor, // * 10f,
						new SliderValueChangedEvent(m_SelfFeedbackColor_ValueChanged),
						ref index, ref verticalOffset,
						out m_SelfFeedbackColor, out m_SelfFeedbackColorLabel);	

			AddSlider(this, "Warp Variance",
						-0.2f, 0.2f, ArtworkStaticObjects.Options.Visual.WarpVariance, // * 10f,
						new SliderValueChangedEvent(m_WarpVariance_ValueChanged),
						ref index, ref verticalOffset,
						out m_WarpVariance, out m_WarpVarianceLabel);

			AddSlider(this, "Warp Persistence",
						-0.6f, 0.6f, ArtworkStaticObjects.Options.Visual.WarpPersistence, // * 10f,
						new SliderValueChangedEvent(m_WarpPersistence_ValueChanged),
						ref index, ref verticalOffset,
						out m_WarpPersistence, out m_WarpPersistenceLabel);

			AddSlider(this, "Warp Propagation",
						0, 0.25f, ArtworkStaticObjects.Options.Visual.WarpPropagation, // * 10f,
						new SliderValueChangedEvent(m_WarpPropagation_ValueChanged),
						ref index, ref verticalOffset,
						out m_WarpPropagation, out m_WarpPropagationLabel);

			verticalOffset += 10;

			this.Size = new System.Drawing.Size(this.Size.Width, verticalOffset + 5);
		}		

		public override void AddMenuItems(RugTech1.Framework.Objects.UI.Menus.MenuBar menu)
		{
			MenuBarItem menuItem = menu.AddItem("Visual");
			menuItem.IsVisible = true;
			menuItem.FontType = FontType.Small;
			menuItem.Click += new EventHandler(Properties_Click);
		}

		public override void ResetControlValues()
		{
			m_FeedbackLevel.Value = ArtworkStaticObjects.Options.Visual.FeedbackLevel;
			m_FeedbackLevelLabel.Text = ArtworkStaticObjects.Options.Visual.FeedbackLevel.ToString("N2");

			m_ParticleFeedbackLevel.Value = ArtworkStaticObjects.Options.Visual.ParticleFeedbackLevel;
			m_ParticleFeedbackLevelLabel.Text = ArtworkStaticObjects.Options.Visual.ParticleFeedbackLevel.ToString("N2");

			m_SelfImage.Value = ArtworkStaticObjects.Options.Visual.SelfImage;
			m_SelfImageLabel.Text = ArtworkStaticObjects.Options.Visual.SelfImage.ToString("N2");

			m_SelfColor.Value = ArtworkStaticObjects.Options.Visual.SelfColor;
			m_SelfColorLabel.Text = ArtworkStaticObjects.Options.Visual.SelfColor.ToString("N2");

			m_SelfFeedback.Value = ArtworkStaticObjects.Options.Visual.SelfFeedback;
			m_SelfFeedbackLabel.Text = ArtworkStaticObjects.Options.Visual.SelfFeedback.ToString("N2");

			m_SelfFeedbackColor.Value = ArtworkStaticObjects.Options.Visual.SelfFeedbackColor;
			m_SelfFeedbackColorLabel.Text = ArtworkStaticObjects.Options.Visual.SelfFeedbackColor.ToString("N2");

			m_WarpVariance.Value = ArtworkStaticObjects.Options.Visual.WarpVariance; // * 0.01f;
			m_WarpVarianceLabel.Text = ArtworkStaticObjects.Options.Visual.WarpVariance.ToString("N2");

			m_WarpPersistence.Value = ArtworkStaticObjects.Options.Visual.WarpPersistence; // * 0.01f;
			m_WarpPersistenceLabel.Text = ArtworkStaticObjects.Options.Visual.WarpPersistence.ToString("N2");

			m_WarpPropagation.Value = ArtworkStaticObjects.Options.Visual.WarpPropagation; // * 0.01f;
			m_WarpPropagationLabel.Text = ArtworkStaticObjects.Options.Visual.WarpPropagation.ToString("N2");
		}

		public override void UpdateControls()
		{
		
		} 

		#endregion

		#region Control Events
		
		void Properties_Click(object sender, EventArgs e)
		{
			PanelControler.SetOrToggleCurrentPanel(ScreenPanel);
		}

		void m_FeedbackLevel_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.Visual.FeedbackLevel = value;
			m_FeedbackLevelLabel.Text = value.ToString("N2");
		}

		void m_ParticleFeedbackLevel_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.Visual.ParticleFeedbackLevel = value;
			m_ParticleFeedbackLevelLabel.Text = value.ToString("N2");
		}

		void m_SelfImage_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.Visual.SelfImage = value;
			m_SelfImageLabel.Text = value.ToString("N2");
		}

		void m_SelfColor_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.Visual.SelfColor = value;
			m_SelfColorLabel.Text = value.ToString("N2");
		}

		void m_SelfFeedback_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.Visual.SelfFeedback = value;
			m_SelfFeedbackLabel.Text = value.ToString("N2");
		}

		void m_SelfFeedbackColor_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.Visual.SelfFeedbackColor = value;
			m_SelfFeedbackColorLabel.Text = value.ToString("N2");
		}

		void m_WarpVariance_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.Visual.WarpVariance = value; // * 0.01f;
			m_WarpVarianceLabel.Text = value.ToString("N2");
		}

		void m_WarpPersistence_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.Visual.WarpPersistence = value; // * 0.01f;
			m_WarpPersistenceLabel.Text = value.ToString("N2");
		}

		void m_WarpPropagation_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.Visual.WarpPropagation = value; // * 0.01f;
			m_WarpPropagationLabel.Text = value.ToString("N2");
		}

		#endregion
	}
}
