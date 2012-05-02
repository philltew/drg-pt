using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.Text;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.UI.Dynamic;
using RugTech1.Framework.Objects.UI.Menus;
using DS.Scenes;
using DS.Kinect;

namespace DS.Panels
{
	class KinectPanel : PanelBase
	{
		#region Private Members

		private KinectImageTunerPanel m_KinectImageTunerPanel;

		private List<ToggleButton> m_KinectImageModeButtons = new List<ToggleButton>(); 

		private Slider m_ElevationAngleSlider;
		private DynamicLabel m_ElevationAngleValueLabel;

		private Slider m_NearClippingPlaneSlider;
		private DynamicLabel m_NearClippingPlaneLabel;

		private Slider m_FarClippingPlaneSlider;
		private DynamicLabel m_FarClippingPlaneLabel;

		private Slider m_NoiseToleranceSlider;
		private DynamicLabel m_NoiseToleranceValueLabel;		

		private Slider m_BackgroundCalibarationFramesSlider;
		private DynamicLabel m_BackgroundCalibarationFramesValueLabel;

		private ProgressBar m_BackgroundProgressBar; 

		#endregion

		#region Public Properties

		public override SplashSceenPanels ScreenPanel
		{
			get { return SplashSceenPanels.Cameras; }
		} 

		#endregion

		public KinectPanel(VisiblePanelControler controler, int index, KinectImageTunerPanel kinectImageTunerPanel)
			: base(controler, index)
		{
			m_KinectImageTunerPanel = kinectImageTunerPanel; 
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
			label.Text = "Camera Settings";
			label.FontType = FontType.Heading;
			label.IsVisible = true;
			label.Padding = new System.Windows.Forms.Padding(5);
			label.RelitiveZIndex = index++;

			this.Controls.Add(label);

			int verticalOffset = 35;

			// KinectImageMode { Color, RawDepth, DepthBackgroundImage, DepthBackgroundRemoved }
			AddButtonSet(this, "Image Mode",
						 new object[] { KinectImageMode.Color, KinectImageMode.RawDepth, KinectImageMode.DepthBackgroundImage, KinectImageMode.DepthBackgroundRemoved },
						 new string[] { "Color", "Raw", "Back", "Final" }, new EventHandler(kinectImageModeButton_Click), ref index, ref verticalOffset, m_KinectImageModeButtons);

			ToggleButton UseTestImageButton = new ToggleButton();
			UseTestImageButton.Text = "Use Test Image";
			UseTestImageButton.FontType = FontType.Regular;
			UseTestImageButton.Size = new System.Drawing.Size(290, 20);
			UseTestImageButton.Location = new System.Drawing.Point(5, verticalOffset);
			UseTestImageButton.IsVisible = true;
			UseTestImageButton.Click += new EventHandler(UseTestImageButton_Click);
			UseTestImageButton.RelitiveZIndex = index++;

			this.Controls.Add(UseTestImageButton);

			verticalOffset += 25;		

			AddSlider(this, "Elevation Angle",
						-27, 27, (float)ArtworkStaticObjects.Options.Kinect.ElevationAngle,
						new SliderValueChangedEvent(m_ElevationAngleSlider_ValueChanged),
						ref index, ref verticalOffset,
						out m_ElevationAngleSlider, out m_ElevationAngleValueLabel);

			AddSlider(this, "Near Clipping Plane",
						0, 8000, (float)ArtworkStaticObjects.Options.Kinect.NearClippingPlane,
						new SliderValueChangedEvent(m_NearClippingPlaneSlider_ValueChanged),
						ref index, ref verticalOffset,
						out m_NearClippingPlaneSlider, out m_NearClippingPlaneLabel);

			AddSlider(this, "Far Clipping Plane",
						0, 8000, (float)ArtworkStaticObjects.Options.Kinect.FarClippingPlane,
						new SliderValueChangedEvent(m_FarClippingPlaneSlider_ValueChanged),
						ref index, ref verticalOffset,
						out m_FarClippingPlaneSlider, out m_FarClippingPlaneLabel);

			AddSlider(this, "Noise Tolerance",
						0, 100, (float)ArtworkStaticObjects.Options.Kinect.NoiseTolerance,
						new SliderValueChangedEvent(m_NoiseToleranceSlider_ValueChanged),
						ref index, ref verticalOffset,
						out m_NoiseToleranceSlider, out m_NoiseToleranceValueLabel);
			
			AddSlider(this, "Background Calibaration Frames",
						0, 10000, (float)ArtworkStaticObjects.Options.Kinect.BackgroundCalibarationFrames,
						new SliderValueChangedEvent(m_BackgroundCalibarationFramesSlider_ValueChanged),
						ref index, ref verticalOffset,
						out m_BackgroundCalibarationFramesSlider, out m_BackgroundCalibarationFramesValueLabel);
			

			verticalOffset += 10;

			m_BackgroundProgressBar = new ProgressBar();

			m_BackgroundProgressBar.MaxValue = (float)ArtworkStaticObjects.Options.Kinect.BackgroundCalibarationFrames;
			m_BackgroundProgressBar.Size = new System.Drawing.Size(290, 15);
			m_BackgroundProgressBar.Location = new System.Drawing.Point(5, verticalOffset);
			m_BackgroundProgressBar.IsVisible = true;
			m_BackgroundProgressBar.Value = 0;
			m_BackgroundProgressBar.RelitiveZIndex = index++;
			this.Controls.Add(m_BackgroundProgressBar);

			verticalOffset += 25;

			Button SampleBackgroundButton = new Button();
			SampleBackgroundButton.Text = "Calibarate Background";
			SampleBackgroundButton.FontType = FontType.Regular;
			SampleBackgroundButton.Size = new System.Drawing.Size(290, 20);
			SampleBackgroundButton.Location = new System.Drawing.Point(5, verticalOffset);
			SampleBackgroundButton.IsVisible = true;
			SampleBackgroundButton.Click += new EventHandler(SampleBackgroundButton_Click);
			SampleBackgroundButton.RelitiveZIndex = index++;

			verticalOffset += 25;			 

			this.Controls.Add(SampleBackgroundButton);

			this.Size = new System.Drawing.Size(this.Size.Width, verticalOffset + 5);
		}

		public override void AddMenuItems(RugTech1.Framework.Objects.UI.Menus.MenuBar menu)
		{
			MenuBarItem kinectConfig = menu.AddItem("Cameras");
			kinectConfig.IsVisible = true;
			kinectConfig.FontType = FontType.Small;
			kinectConfig.Click += new EventHandler(filterProperties_Click);
		}

		public override void ResetControlValues()
		{
			m_ElevationAngleSlider.Value = (float)ArtworkStaticObjects.Options.Kinect.ElevationAngle;
			m_ElevationAngleValueLabel.Text = ((float)ArtworkStaticObjects.Options.Kinect.ElevationAngle).ToString("N2");

			m_NearClippingPlaneSlider.Value = (float)ArtworkStaticObjects.Options.Kinect.NearClippingPlane;
			m_NearClippingPlaneLabel.Text = ((float)ArtworkStaticObjects.Options.Kinect.NearClippingPlane).ToString("N2");

			m_FarClippingPlaneSlider.Value = (float)ArtworkStaticObjects.Options.Kinect.FarClippingPlane;
			m_FarClippingPlaneLabel.Text = ((float)ArtworkStaticObjects.Options.Kinect.FarClippingPlane).ToString("N2");

			m_NoiseToleranceSlider.Value = (float)ArtworkStaticObjects.Options.Kinect.NoiseTolerance;
			m_NoiseToleranceValueLabel.Text = ((float)ArtworkStaticObjects.Options.Kinect.NoiseTolerance).ToString("N2");
			
			m_BackgroundCalibarationFramesSlider.Value = (float)ArtworkStaticObjects.Options.Kinect.BackgroundCalibarationFrames;
			m_BackgroundCalibarationFramesValueLabel.Text = ((float)ArtworkStaticObjects.Options.Kinect.BackgroundCalibarationFrames).ToString("N2");
			
		}

		public override void UpdateControls()
		{
			m_BackgroundProgressBar.Value = ArtworkStaticObjects.KinectDevices.Devices[0].Filter.GrabberCalls;
		} 

		#endregion

		#region Control Events
		
		void filterProperties_Click(object sender, EventArgs e)
		{
			PanelControler.SetOrToggleCurrentPanel(ScreenPanel);

			m_KinectImageTunerPanel.IsVisible = this.IsVisible;

			ArtworkStaticObjects.KinectDevices.EnableColorCameras = this.IsVisible;
		}

		void kinectImageModeButton_Click(object sender, EventArgs e)
		{
			foreach (ToggleButton button in m_KinectImageModeButtons)
			{
				button.Value = button == sender;
			}

			m_KinectImageTunerPanel.KinectImageMode = (KinectImageMode)(sender as ToggleButton).Tag; 
		}

		void UseTestImageButton_Click(object sender, EventArgs e)
		{
			ToggleButton button = sender as ToggleButton;

			ArtworkStaticObjects.KinectDevices.UseTestImage = button.Value; 
		}		
		
		void SampleBackgroundButton_Click(object sender, EventArgs e)
		{
			ArtworkStaticObjects.KinectDevices.CalibarateBackground();
			
			m_BackgroundProgressBar.MaxValue = (float)ArtworkStaticObjects.Options.Kinect.BackgroundCalibarationFrames;
		}

		void m_ElevationAngleSlider_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.KinectDevices.Devices[0].ElevationAngle = (int)value;
			ArtworkStaticObjects.Options.Kinect.ElevationAngle = ArtworkStaticObjects.KinectDevices.Devices[0].ElevationAngle;
			m_ElevationAngleValueLabel.Text = ((int)value).ToString() + ".00";
		}

		void m_NearClippingPlaneSlider_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.Kinect.NearClippingPlane = (short)value;
			m_NearClippingPlaneLabel.Text = ((int)value).ToString() + ".00";
		}

		void m_FarClippingPlaneSlider_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.Kinect.FarClippingPlane = (short)value;
			m_FarClippingPlaneLabel.Text = ((int)value).ToString() + ".00";
		}

		void m_NoiseToleranceSlider_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.Kinect.NoiseTolerance = (double)value;
			m_NoiseToleranceValueLabel.Text = ((double)value).ToString() + ".00";
		}
		
		void m_BackgroundCalibarationFramesSlider_ValueChanged(Slider sender, float value)
		{
			ArtworkStaticObjects.Options.Kinect.BackgroundCalibarationFrames = (short)value;
			m_BackgroundCalibarationFramesValueLabel.Text = ((int)value).ToString() + ".00";
		}

		#endregion
	}
}
