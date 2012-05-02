using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.Text;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.UI.Dynamic;
using RugTech1.Framework.Objects.UI.Menus;

namespace Experiments.Panels
{
	public abstract class PanelBase : Panel
	{
		private VisiblePanelControler m_PanelControler;

		public VisiblePanelControler PanelControler { get { return m_PanelControler; } }

		public abstract SplashSceenPanels ScreenPanel { get; }
	
		public PanelBase(VisiblePanelControler controler, int index)
		{
			m_PanelControler = controler;
			m_PanelControler.PanelChanged += new PanelEvent(m_PanelControler_PanelChanged);
			
			this.Size = new System.Drawing.Size(700, 300);
			this.Location = new System.Drawing.Point(15, 15);
			this.ShowBackground = true;
			this.ShowBorder = true;
			this.RelitiveZIndex = index;
		}

		void m_PanelControler_PanelChanged(SplashSceenPanels CurrentPanel)
		{
			this.IsVisible = ScreenPanel == CurrentPanel; 
		}

		#region Abstract Methods
		
		public abstract void Initiate();

		public abstract void AddMenuItems(MenuBar menu);

		public abstract void ResetControlValues();

		public abstract void UpdateControls();

		#endregion

		#region Panel Build Helpers

		protected void AddSlider(Panel panel, string text,
								float min, float max, float value, SliderValueChangedEvent changed,
								ref int index, ref int verticalOffset,
								out Slider slider, out DynamicLabel valueLabel)
		{
			Label label = new Label();
			label.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			label.FixedSize = true;
			label.Size = new System.Drawing.Size(this.Size.Width - 100, 15);
			label.Location = new System.Drawing.Point(0, verticalOffset);
			label.Text = text;
			label.FontType = FontType.Small;
			label.IsVisible = true;
			label.Padding = new System.Windows.Forms.Padding(5);
			label.RelitiveZIndex = index++;
			panel.Controls.Add(label);

			valueLabel = new DynamicLabel();
			valueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			valueLabel.FixedSize = true;
			valueLabel.Size = new System.Drawing.Size(95, 15);
			valueLabel.Location = new System.Drawing.Point(this.Size.Width - 100, verticalOffset);
			valueLabel.MaxLength = 8;
			valueLabel.Text = value.ToString("N2");
			valueLabel.FontType = FontType.Small;
			valueLabel.IsVisible = true;
			valueLabel.Padding = new System.Windows.Forms.Padding(5);
			valueLabel.RelitiveZIndex = index++;
			panel.Controls.Add(valueLabel);

			verticalOffset += 20;

			slider = new Slider();
			slider.MinValue = min;
			slider.MaxValue = max;
			slider.Size = new System.Drawing.Size(this.Size.Width - 10, 15);
			slider.Location = new System.Drawing.Point(5, verticalOffset);
			slider.IsVisible = true;
			slider.Value = value;
			slider.ValueChanged += changed;
			slider.RelitiveZIndex = index++;
			panel.Controls.Add(slider);

			verticalOffset += 15;
		}

		protected void AddPathControl(Panel panel, string text,
								string value, EventHandler openFileDialog, EventHandler openLocation,
								ref int index, ref int verticalOffset,
								out DynamicLabel valueLabel)
		{
			Label label = new Label();
			label.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			label.FixedSize = true;
			label.Size = new System.Drawing.Size(this.Size.Width - 100, 15);
			label.Location = new System.Drawing.Point(0, verticalOffset);
			label.Text = text;
			label.FontType = FontType.Small;
			label.IsVisible = true;
			label.Padding = new System.Windows.Forms.Padding(5);
			label.RelitiveZIndex = index++;
			panel.Controls.Add(label);

			verticalOffset += 20;

			valueLabel = new DynamicLabel();
			valueLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			valueLabel.FixedSize = true;
			valueLabel.Size = new System.Drawing.Size(this.Size.Width - 70, 15);
			valueLabel.Location = new System.Drawing.Point(5, verticalOffset);
			valueLabel.MaxLength = 100;
			valueLabel.Text = value;
			valueLabel.FontType = FontType.Small;
			valueLabel.IsVisible = true;
			valueLabel.Padding = new System.Windows.Forms.Padding(5);
			valueLabel.RelitiveZIndex = index++;
			panel.Controls.Add(valueLabel);

			Button openFileDialogButton = new Button();
			openFileDialogButton.Size = new System.Drawing.Size(25, 20);
			openFileDialogButton.Location = new System.Drawing.Point(this.Size.Width - 65, verticalOffset);
			openFileDialogButton.Text = "...";
			openFileDialogButton.FontType = FontType.Small;
			openFileDialogButton.IsVisible = true;
			openFileDialogButton.RelitiveZIndex = index++;
			openFileDialogButton.Click += openFileDialog; 
			panel.Controls.Add(openFileDialogButton);

			Button openLocationButton = new Button();
			openLocationButton.Size = new System.Drawing.Size(30, 20);
			openLocationButton.Location = new System.Drawing.Point(this.Size.Width - 35, verticalOffset);
			openLocationButton.Text = "Open";
			openLocationButton.FontType = FontType.Small;
			openLocationButton.IsVisible = true;
			openLocationButton.RelitiveZIndex = index++;
			openLocationButton.Click += openLocation;
			panel.Controls.Add(openLocationButton);

			verticalOffset += 25;
		}

		#endregion
	}
}
