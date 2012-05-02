using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.Text;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.UI.Dynamic;
using RugTech1.Framework.Objects.UI.Menus;

namespace DS.Panels
{
	class OscPanel : PanelBase
	{
		#region Private Members

		private DynamicLabel m_OscMessage;
		private TextBox m_OscAddress;
		private TextBox m_OscPort;
		private Button m_OscConnectButton;
		private Slider m_SpeedThresholdSlider;
		private DynamicLabel m_SpeedThresholdValueLabel;
		private Slider m_DistanceThresholdSlider;
		private DynamicLabel m_DistanceThresholdValueLabel;
		
		private Slider m_PortalIDSlider;
		private DynamicLabel m_PortalIDValueLabel; 
		
		#endregion

		#region Public Properties

		public override SplashSceenPanels ScreenPanel
		{
			get { return SplashSceenPanels.Osc; }
		} 

		#endregion

		public OscPanel(VisiblePanelControler controler, int index)
			: base(controler, index)
		{

		}

		#region Panel Method Overrides

		public override void Initiate()
		{
			ArtworkStaticObjects.OscControler.ConnectionChanged += new EventHandler(OscControler_ConnectionChanged);

			int index = 1;

			Label label = new Label();
			label.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			label.FixedSize = true;
			label.Size = new System.Drawing.Size(290, 30);
			label.Location = new System.Drawing.Point(0, 5);
			label.Text = "Osc Properties";
			label.FontType = FontType.Heading;
			label.IsVisible = true;
			label.Padding = new System.Windows.Forms.Padding(5);
			label.RelitiveZIndex = index++;
			this.Controls.Add(label);

			int verticalOffset = 35;

			m_OscMessage = new DynamicLabel();
			m_OscMessage.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			m_OscMessage.FixedSize = true;
			m_OscMessage.Size = new System.Drawing.Size(Size.Width - 10, 15);
			m_OscMessage.Location = new System.Drawing.Point(0, verticalOffset);
			m_OscMessage.Text = ArtworkStaticObjects.OscControler.Message;
			m_OscMessage.MaxLength = 200;
			m_OscMessage.FontType = FontType.Small;
			m_OscMessage.IsVisible = true;
			m_OscMessage.Padding = new System.Windows.Forms.Padding(5);
			m_OscMessage.RelitiveZIndex = index++;

			this.Controls.Add(m_OscMessage);

			verticalOffset += 20;

			m_OscAddress = new TextBox();
			m_OscAddress.Text = ArtworkStaticObjects.OscControler.Address;
			m_OscAddress.MaxLength = 80;
			m_OscAddress.Size = new System.Drawing.Size(Size.Width - 140, 20);
			m_OscAddress.Location = new System.Drawing.Point(5, verticalOffset);
			m_OscAddress.FontType = FontType.Small;
			m_OscAddress.RelitiveZIndex = index++;
			m_OscAddress.IsVisible = true;
			m_OscAddress.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			m_OscAddress.TextChanged += new EventHandler(m_OscAddress_TextChanged);
			this.Controls.Add(m_OscAddress);

			m_OscPort = new TextBox();
			m_OscPort.Text = ArtworkStaticObjects.OscControler.Port.ToString();
			m_OscPort.MaxLength = 6;
			m_OscPort.Size = new System.Drawing.Size(50, 20);
			m_OscPort.Location = new System.Drawing.Point(Size.Width - 130, verticalOffset);
			m_OscPort.FontType = FontType.Small;
			m_OscPort.RelitiveZIndex = index++;
			m_OscPort.IsVisible = true;
			m_OscPort.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			m_OscPort.TextChanged += new EventHandler(m_OscPort_TextChanged);
			this.Controls.Add(m_OscPort);

			m_OscConnectButton = new Button();
			m_OscConnectButton.Text = ArtworkStaticObjects.OscControler.Connected ? "Dissconnect" : "Connect";
			m_OscConnectButton.Size = new System.Drawing.Size(70, 20);
			m_OscConnectButton.Location = new System.Drawing.Point(Size.Width - 75, verticalOffset);
			m_OscConnectButton.FontType = FontType.Small;
			m_OscConnectButton.RelitiveZIndex = index++;
			m_OscConnectButton.IsVisible = true;
			m_OscConnectButton.Click += new EventHandler(m_OscConnectButton_Click);
			this.Controls.Add(m_OscConnectButton);

			verticalOffset += 25;

			AddSlider(this, "Movement Threshold",
				0, 2, (float)Options.Osc.SpeedThreshold,
				new SliderValueChangedEvent(m_SpeedThresholdSlider_ValueChanged),
				ref index, ref verticalOffset,
				out m_SpeedThresholdSlider, out m_SpeedThresholdValueLabel);

			AddSlider(this, "Distance Threshold",
				0, 10, (float)Options.Osc.DistanceThreshold,
				new SliderValueChangedEvent(m_DistanceThresholdSlider_ValueChanged),
				ref index, ref verticalOffset,
				out m_DistanceThresholdSlider, out m_DistanceThresholdValueLabel);


			AddSlider(this, "Portal ID",
				0, 10, (float)Options.Osc.PortalID,
				new SliderValueChangedEvent(m_PortalIDSlider_ValueChanged),
				ref index, ref verticalOffset,
				out m_PortalIDSlider, out m_PortalIDValueLabel);

			this.Size = new System.Drawing.Size(this.Size.Width, verticalOffset + 5);
		}

		public override void AddMenuItems(RugTech1.Framework.Objects.UI.Menus.MenuBar menu)
		{
			MenuBarItem oscConfig = menu.AddItem("OSC");
			oscConfig.IsVisible = true;
			oscConfig.FontType = FontType.Small;
			oscConfig.Click += new EventHandler(oscProperties_Click);
		}

		public override void ResetControlValues()
		{
			m_SpeedThresholdSlider.Value = ArtworkStaticObjects.Options.Osc.SpeedThreshold;
			m_SpeedThresholdValueLabel.Text = ArtworkStaticObjects.Options.Osc.SpeedThreshold.ToString("N2");
			m_DistanceThresholdSlider.Value = ArtworkStaticObjects.Options.Osc.DistanceThreshold;
			m_DistanceThresholdValueLabel.Text = ArtworkStaticObjects.Options.Osc.DistanceThreshold.ToString("N2");
			// m_
			// m_DistanceThresholdSlider_ValueChanged
		}

		public override void UpdateControls()
		{
			m_OscMessage.Text = ArtworkStaticObjects.OscControler.Message;
		}

		#endregion

		#region Control Events
		
		void oscProperties_Click(object sender, EventArgs e)
		{
			PanelControler.SetOrToggleCurrentPanel(ScreenPanel); 
		}

		void m_OscPort_TextChanged(object sender, EventArgs e)
		{
			int port;

			if (int.TryParse(m_OscPort.Text, out port) == false)
			{
				m_OscPort.Text = ArtworkStaticObjects.OscControler.Port.ToString();
			}
			else
			{
				ArtworkStaticObjects.OscControler.Port = port;
			}
		}

		void m_OscAddress_TextChanged(object sender, EventArgs e)
		{
			ArtworkStaticObjects.OscControler.Address = m_OscAddress.Text;
		}

		void OscControler_ConnectionChanged(object sender, EventArgs e)
		{
			m_OscConnectButton.Text = ArtworkStaticObjects.OscControler.Connected ? "Dissconnect" : "Connect";
			this.Scene.Invalidate();
		}

		void m_OscConnectButton_Click(object sender, EventArgs e)
		{
			if (ArtworkStaticObjects.OscControler.Connected)
			{
				ArtworkStaticObjects.OscControler.Disconnect();
			}
			else
			{
				ArtworkStaticObjects.OscControler.Connect();
			}
		}

		void m_SpeedThresholdSlider_ValueChanged(Slider sender, float value)
		{
			Options.Osc.SpeedThreshold = value;
			m_SpeedThresholdValueLabel.Text = value.ToString("N2");
		}

		void m_DistanceThresholdSlider_ValueChanged(Slider sender, float value)
		{
			Options.Osc.DistanceThreshold = value;
			m_DistanceThresholdValueLabel.Text = value.ToString("N2");
		}

		void m_PortalIDSlider_ValueChanged(Slider sender, float value)
		{
			Options.Osc.PortalID = (int)value;
			m_PortalIDValueLabel.Text = ((int)value).ToString();
		}

		#endregion
	}
}
