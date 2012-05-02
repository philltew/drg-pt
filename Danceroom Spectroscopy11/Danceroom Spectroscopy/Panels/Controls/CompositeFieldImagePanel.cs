using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.Text;
using DS.Kinect;
using RugTech1.Framework.Objects.UI.Menus;
using RugTech1.Framework.Objects.UI.Dynamic;
using System.Drawing;

namespace DS.Panels
{
	class CompositeFieldImagePanel : PanelBase
	{
		List<ToggleButton> m_FieldImageModeButtons = new List<ToggleButton>();

		Slider m_LeftEdge;
		Slider m_RightEdge;

		Slider m_TopEdge;
		Slider m_BottomEdge;

		CompositeFieldImageEditor m_CompositeFieldImageEditor; 

		public override SplashSceenPanels ScreenPanel
		{
			get { return SplashSceenPanels.FieldData; }
		}

		public CompositeFieldImagePanel(VisiblePanelControler controler, int index)
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
			label.Text = "Compose Field Data";
			label.FontType = FontType.Heading;
			label.IsVisible = true;
			label.Padding = new System.Windows.Forms.Padding(5);
			label.RelitiveZIndex = index++;

			this.Controls.Add(label);

			int verticalOffset = 35;

			//AddSlider(this, "Feedback Level",
			//	0, 1, ArtworkStaticObjects.Options.Visual.FeedbackLevel,
			//	new SliderValueChangedEvent(m_FeedbackLevel_ValueChanged),
			//	ref index, ref verticalOffset,
			//	out m_FeedbackLevel, out m_FeedbackLevelLabel);
			 

			// KinectFieldImageType { BlendMap, ClipMap, Final }
			AddButtonSet(this, "Image Mode",
						 new object[] { KinectFieldImageType.BlendMap, KinectFieldImageType.ClipMap, KinectFieldImageType.Identify, KinectFieldImageType.Final },
						 new string[] { "Blend", "Clip", "Identify", "Final" }, new EventHandler(fieldImageModeButton_Click), ref index, ref verticalOffset, m_FieldImageModeButtons);

			//float widthPerImage = (float)(this.Size.Width / m_KinectColorImages.Count);
			//float heightPerImage = (widthPerImage / 4) * 3;

			m_CompositeFieldImageEditor = new CompositeFieldImageEditor();
			m_CompositeFieldImageEditor.Location = new System.Drawing.Point(5, verticalOffset);
			m_CompositeFieldImageEditor.Size = new System.Drawing.Size((this.Size.Width - 25), 400);
			m_CompositeFieldImageEditor.ShowBorder = true;
			m_CompositeFieldImageEditor.RelitiveZIndex = index++;
			m_CompositeFieldImageEditor.RebuildImages = true; 

			this.Controls.Add(m_CompositeFieldImageEditor);

			m_TopEdge = new Slider();
			m_TopEdge.BarAlignment = BarAlignment.Vertical;
			m_TopEdge.InvertHighlight = true; 
			m_TopEdge.MinValue = 0;
			m_TopEdge.MaxValue = 198;
			m_TopEdge.Size = new System.Drawing.Size(15, 198);
			m_TopEdge.Location = new System.Drawing.Point(this.Size.Width - 20, verticalOffset);
			m_TopEdge.IsVisible = true;
			m_TopEdge.Value = m_TopEdge.MaxValue - ArtworkStaticObjects.CompositeFieldImage.Bounds.Top;
			m_TopEdge.ValueChanged += new SliderValueChangedEvent(m_TopEdge_ValueChanged);
			m_TopEdge.RelitiveZIndex = index++;
			this.Controls.Add(m_TopEdge);

			m_BottomEdge = new Slider();
			m_BottomEdge.BarAlignment = BarAlignment.Vertical; 
			m_BottomEdge.MinValue = 202;
			m_BottomEdge.MaxValue = 400;
			m_BottomEdge.Size = new System.Drawing.Size(15, 198);
			m_BottomEdge.Location = new System.Drawing.Point(this.Size.Width - 20, verticalOffset + 200);
			m_BottomEdge.IsVisible = true;
			m_BottomEdge.Value = m_BottomEdge.MaxValue - (ArtworkStaticObjects.CompositeFieldImage.Bounds.Bottom - m_BottomEdge.MinValue);
			m_BottomEdge.ValueChanged += new SliderValueChangedEvent(m_BottomEdge_ValueChanged);
			m_BottomEdge.RelitiveZIndex = index++;
			this.Controls.Add(m_BottomEdge);

			verticalOffset += 400;

			int widthOverTwo = (this.Size.Width - 29) / 2; 

			m_LeftEdge = new Slider();
			m_LeftEdge.MinValue = 0;
			m_LeftEdge.MaxValue = widthOverTwo - 2;
			m_LeftEdge.Size = new System.Drawing.Size(widthOverTwo, 15);
			m_LeftEdge.Location = new System.Drawing.Point(5, verticalOffset);
			m_LeftEdge.IsVisible = true;
			m_LeftEdge.Value = ArtworkStaticObjects.CompositeFieldImage.Bounds.Left;
			m_LeftEdge.ValueChanged += new SliderValueChangedEvent(m_LeftEdge_ValueChanged);
			m_LeftEdge.RelitiveZIndex = index++;
			this.Controls.Add(m_LeftEdge);

			m_RightEdge = new Slider();
			m_RightEdge.MinValue = widthOverTwo + 2;
			m_RightEdge.MaxValue = widthOverTwo * 2;
			m_RightEdge.InvertHighlight = true; 
			m_RightEdge.Size = new System.Drawing.Size(widthOverTwo, 15);
			m_RightEdge.Location = new System.Drawing.Point(m_LeftEdge.Location.X + m_LeftEdge.Size.Width + 4, verticalOffset);
			m_RightEdge.IsVisible = true;
			m_RightEdge.Value = ArtworkStaticObjects.CompositeFieldImage.Bounds.Right;
			m_RightEdge.ValueChanged += new SliderValueChangedEvent(m_RightEdge_ValueChanged);
			m_RightEdge.RelitiveZIndex = index++;
			this.Controls.Add(m_RightEdge);

			verticalOffset += 25;

			this.Size = new System.Drawing.Size(this.Size.Width, verticalOffset + 5);

			m_CompositeFieldImageEditor.UpdateRegionBounds(); 
		}

		void m_BottomEdge_ValueChanged(Slider sender, float value)
		{
			Rectangle rect = ArtworkStaticObjects.CompositeFieldImage.Bounds;

			float v = sender.MinValue + (sender.MaxValue - value);

			ArtworkStaticObjects.CompositeFieldImage.Bounds = new Rectangle(rect.Location, new Size(rect.Width, (int)v - rect.Y));
			ArtworkStaticObjects.Ensemble.Resize(rect.Width, rect.Height);

			m_CompositeFieldImageEditor.UpdateRegionBounds(); 
		}

		void m_TopEdge_ValueChanged(Slider sender, float value)
		{
			Rectangle rect = ArtworkStaticObjects.CompositeFieldImage.Bounds;

			float v = sender.MaxValue - value;

			ArtworkStaticObjects.CompositeFieldImage.Bounds = new Rectangle(rect.X, (int)v, rect.Width, rect.Bottom - (int)v);
			ArtworkStaticObjects.Ensemble.Resize(rect.Width, rect.Height);

			m_CompositeFieldImageEditor.UpdateRegionBounds(); 
		}

		void m_RightEdge_ValueChanged(Slider sender, float value)
		{
			Rectangle rect = ArtworkStaticObjects.CompositeFieldImage.Bounds;
			ArtworkStaticObjects.CompositeFieldImage.Bounds = new Rectangle(rect.Location, new Size((int)value - rect.X, rect.Height));
			ArtworkStaticObjects.Ensemble.Resize(rect.Width, rect.Height);

			m_CompositeFieldImageEditor.UpdateRegionBounds(); 
		}

		void m_LeftEdge_ValueChanged(Slider sender, float value)
		{
			Rectangle rect = ArtworkStaticObjects.CompositeFieldImage.Bounds;
			ArtworkStaticObjects.CompositeFieldImage.Bounds = new Rectangle((int)value, rect.Y, rect.Right - (int)value, rect.Height);
			ArtworkStaticObjects.Ensemble.Resize(rect.Width, rect.Height);

			m_CompositeFieldImageEditor.UpdateRegionBounds(); 
		}		

		public override void AddMenuItems(RugTech1.Framework.Objects.UI.Menus.MenuBar menu)
		{
			MenuBarItem kinectConfig = menu.AddItem("Field Composition");
			kinectConfig.IsVisible = true;
			kinectConfig.FontType = FontType.Small;
			kinectConfig.Click += new EventHandler(fieldCompositionProperties_Click);
		}

		public override void ResetControlValues()
		{
			/*m_ElevationAngleSlider.Value = (float)ArtworkStaticObjects.Options.Kinect.ElevationAngle;
			m_ElevationAngleValueLabel.Text = ((float)ArtworkStaticObjects.Options.Kinect.ElevationAngle).ToString("N2");

			m_NearClippingPlaneSlider.Value = (float)ArtworkStaticObjects.Options.Kinect.NearClippingPlane;
			m_NearClippingPlaneLabel.Text = ((float)ArtworkStaticObjects.Options.Kinect.NearClippingPlane).ToString("N2");

			m_FarClippingPlaneSlider.Value = (float)ArtworkStaticObjects.Options.Kinect.FarClippingPlane;
			m_FarClippingPlaneLabel.Text = ((float)ArtworkStaticObjects.Options.Kinect.FarClippingPlane).ToString("N2");

			m_NoiseToleranceSlider.Value = (float)ArtworkStaticObjects.Options.Kinect.NoiseTolerance;
			m_NoiseToleranceValueLabel.Text = ((float)ArtworkStaticObjects.Options.Kinect.NoiseTolerance).ToString("N2");

			m_BackgroundCalibarationFramesSlider.Value = (float)ArtworkStaticObjects.Options.Kinect.BackgroundCalibarationFrames;
			m_BackgroundCalibarationFramesValueLabel.Text = ((float)ArtworkStaticObjects.Options.Kinect.BackgroundCalibarationFrames).ToString("N2");
			*/ 
		}

		public override void UpdateControls()
		{
			m_CompositeFieldImageEditor.Update(); 
		}

		#endregion

		void fieldCompositionProperties_Click(object sender, EventArgs e)
		{
			PanelControler.SetOrToggleCurrentPanel(ScreenPanel);			
		}

		void fieldImageModeButton_Click(object sender, EventArgs e)
		{
			foreach (ToggleButton button in m_FieldImageModeButtons)
			{
				button.Value = button == sender;
			}

			m_CompositeFieldImageEditor.FieldImageMode = (KinectFieldImageType)(sender as ToggleButton).Tag;
		}
	}
}
