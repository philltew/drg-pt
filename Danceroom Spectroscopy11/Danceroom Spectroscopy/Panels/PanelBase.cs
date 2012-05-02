using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using DS.OSC;
//using DS.ShadowKinect;
using RugTech1.Framework.Objects.Text;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.UI.Dynamic;
using RugTech1.Framework.Objects.UI.Menus;
using SlimDX;
//using Rug.UI;

namespace DS.Panels
{
	abstract class PanelBase : Panel
	{
		private static System.Drawing.Point ColourDialogLocation = System.Drawing.Point.Empty;  
		//private static ColorPickerDialog.ColorPickerDialogMode ColourDialogMode = ColorPickerDialog.ColorPickerDialogMode.Mix;

		#region Private Members

		private VisiblePanelControler m_PanelControler;		

		#endregion

		#region Public Properties

		public ArtworkOptions Options { get { return ArtworkStaticObjects.Options; } }
		//public KinectDevice KinectDevice { get { return ArtworkStaticObjects.KinectDevice; } }
		//public SpatialSoundControler OscControler { get { return ArtworkStaticObjects.OscControler; } }

		public abstract SplashSceenPanels ScreenPanel { get; }

		public VisiblePanelControler PanelControler { get { return m_PanelControler; } }

		#endregion

		public PanelBase(VisiblePanelControler controler, int index)
		{
			m_PanelControler = controler;
			if (m_PanelControler != null)
			{
				m_PanelControler.PanelChanged += new PanelEvent(m_PanelControler_PanelChanged);
			}

			this.Size = new System.Drawing.Size(300, 300);
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
			label.Size = new System.Drawing.Size(200, 15);
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
			valueLabel.Location = new System.Drawing.Point(Size.Width - 100, verticalOffset);
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
			slider.Size = new System.Drawing.Size(Size.Width - 10, 15);
			slider.Location = new System.Drawing.Point(5, verticalOffset);
			slider.IsVisible = true;
			slider.Value = value;
			slider.ValueChanged += changed;
			slider.RelitiveZIndex = index++;
			panel.Controls.Add(slider);

			verticalOffset += 15;
		}

		protected void AddSliderAndButton(Panel panel, string text,
											float min, float max, float value, SliderValueChangedEvent changed,
											EventHandler buttonClicked,
											ref int index, ref int verticalOffset,
											out Slider slider, out DynamicLabel valueLabel)
		{
			Label label = new Label();
			label.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			label.FixedSize = true;
			label.Size = new System.Drawing.Size(200, 15);
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
			valueLabel.Location = new System.Drawing.Point(Size.Width - 100, verticalOffset);
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
			slider.Size = new System.Drawing.Size(Size.Width - 40, 15);
			slider.Location = new System.Drawing.Point(5, verticalOffset);
			slider.IsVisible = true;
			slider.Value = value;
			slider.ValueChanged += changed;
			slider.RelitiveZIndex = index++;
			panel.Controls.Add(slider);

			Button openLocationButton = new Button();
			openLocationButton.Size = new System.Drawing.Size(25, 15);
			openLocationButton.Location = new System.Drawing.Point(Size.Width - 30, verticalOffset);
			openLocationButton.Text = "..";
			openLocationButton.FontType = FontType.Small;
			openLocationButton.IsVisible = true;
			openLocationButton.RelitiveZIndex = index++;
			openLocationButton.Click += buttonClicked;
			panel.Controls.Add(openLocationButton);
		
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
			label.Size = new System.Drawing.Size(200, 15);
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
			valueLabel.Size = new System.Drawing.Size(Size.Width - 70, 15);
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
			openFileDialogButton.Location = new System.Drawing.Point(Size.Width - 65, verticalOffset);
			openFileDialogButton.Text = "...";
			openFileDialogButton.FontType = FontType.Small;
			openFileDialogButton.IsVisible = true;
			openFileDialogButton.RelitiveZIndex = index++;
			openFileDialogButton.Click += openFileDialog; 
			panel.Controls.Add(openFileDialogButton);

			Button openLocationButton = new Button();
			openLocationButton.Size = new System.Drawing.Size(30, 20);
			openLocationButton.Location = new System.Drawing.Point(Size.Width - 35, verticalOffset);
			openLocationButton.Text = "Open";
			openLocationButton.FontType = FontType.Small;
			openLocationButton.IsVisible = true;
			openLocationButton.RelitiveZIndex = index++;
			openLocationButton.Click += openLocation;
			panel.Controls.Add(openLocationButton);

			verticalOffset += 25;
		}

		#endregion

		#region Control Builder Helpers

		protected void AddButtonSet(Panel panel, string labelText, object[] obj, string[] text, EventHandler click, ref int index, ref int verticalOffset, List<ToggleButton> buttons)
		{
			Label label = new Label();
			label.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			label.FixedSize = true;
			label.Size = new System.Drawing.Size(200, 15);
			label.Location = new System.Drawing.Point(0, verticalOffset);
			label.Text = labelText;
			label.FontType = FontType.Small;
			label.IsVisible = true;
			label.Padding = new System.Windows.Forms.Padding(5);
			label.RelitiveZIndex = index++;
			panel.Controls.Add(label);

			verticalOffset += 20;

			int totalSpace = Size.Width - 5;

			int buttonSize = ((totalSpace - (5 * obj.Length)) / obj.Length);
			int buttonSpace = buttonSize + 5;
			int currentButtonOffset = 5;

			for (int i = 0; i < obj.Length; i++)
			{
				buttons.Add(AddToggleButton(panel, obj[i], text[i], click, buttonSize, buttonSpace, ref currentButtonOffset, ref index, ref verticalOffset));
			}

			verticalOffset += 30;
		}

		protected ToggleButton AddToggleButton(Panel panel, object obj, string text, EventHandler click, int buttonSize, int buttonSpace, ref int currentButtonOffset, ref int index, ref int verticalOffset)
		{
			ToggleButton button = new ToggleButton();
			button.Size = new System.Drawing.Size(buttonSize, 20);
			button.Location = new System.Drawing.Point(currentButtonOffset, verticalOffset);
			button.Text = text;
			button.Tag = obj;
			button.FontType = FontType.Small;
			button.IsVisible = true;
			button.RelitiveZIndex = index++;
			button.Click += click;
			panel.Controls.Add(button);

			currentButtonOffset += buttonSpace;

			return button;
		}

		#endregion

		#region Color Picker

		/* 
		protected bool OpenColorPicker(Color3 color, out Color3 result, ColorEvent colorChanged)
		{
			System.Drawing.Color col = System.Drawing.Color.FromArgb((int)(color.Red * 255f), (int)(color.Green * 255f), (int)(color.Blue * 255f)); 
			System.Drawing.Color res;

			if (OpenColorPicker(col, out res, colorChanged))
			{
				result = new Color3((float)res.R / 255f, (float)res.G / 255f, (float)res.B / 255f); 

				return true; 
			}
			else
			{
				result = color;
				return false; 
			}

		}

		protected bool OpenColorPicker(Color4 color, out Color4 result, ColorEvent colorChanged)
		{
			System.Drawing.Color col = System.Drawing.Color.FromArgb((int)(color.Red * 255f), (int)(color.Green * 255f), (int)(color.Blue * 255f)); 
			System.Drawing.Color res;

			if (OpenColorPicker(col, out res, colorChanged))
			{
				result = new Color4(color.Alpha, (float)res.R / 255f, (float)res.G / 255f, (float)res.B / 255f); 

				return true; 
			}
			else
			{
				result = color;
				return false; 
			}

		}

		private bool OpenColorPicker(System.Drawing.Color color, out System.Drawing.Color result, ColorEvent colorChanged)
		{
			bool hasPickedValue = false;

			using (ColorPickerDialog dialog = new ColorPickerDialog())
			{

				if (ColourDialogLocation != System.Drawing.Point.Empty)
				{
					dialog.Location = ColourDialogLocation;
					dialog.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
				}
				else
				{
					dialog.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
				}

				dialog.ColorSelected += colorChanged;

				dialog.PickerMode = ColourDialogMode;

				dialog.Color = color;

				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					result = dialog.Color;
					hasPickedValue = true;
				}
				else
				{
					result = color;
					hasPickedValue = false;
				}

				ColourDialogLocation = dialog.Location;
				ColourDialogMode = dialog.PickerMode;

			}

			return hasPickedValue;
		}
		*/

		#endregion
	}
}
