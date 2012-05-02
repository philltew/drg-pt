using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.UI.Menus;
using RugTech1.Framework.Objects.Text;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.UI.Dynamic;
using System.Threading;
using RugTech1;
using System.IO;

namespace Experiments.Panels
{
	public class SpeedTestPanel : PanelBase 
	{
		private ProgressBar m_ProgressBar;
		private bool m_TestsInProgress = false;
		
		private MultiGraph m_SpeedGraph;
		private SubGraph m_OrginalGraph;
		private SubGraph m_OptoGraph;
		private SubGraph m_OptoGraph2;
		private DynamicLabel m_GraphMax;
		private DynamicLabel m_GraphMin;

		private bool m_DoGraph_OrginalGraph = true;
		private bool m_DoGraph_OptoGraph = true;
		private bool m_DoGraph_OptoGraph2 = true; 

		public override SplashSceenPanels ScreenPanel
		{
			get { return SplashSceenPanels.SpeedTests; }
		}

		public SpeedTestPanel(VisiblePanelControler controler, int index)
			: base(controler, index)
		{

		}

		public override void Initiate()
		{
			int index = 1;

			Label label = new Label();
			label.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			label.FixedSize = true;
			label.Size = new System.Drawing.Size(this.Size.Width - 10, 30);
			label.Location = new System.Drawing.Point(0, 5);
			label.Text = "Ensemble Tests";
			label.FontType = FontType.Heading;
			label.IsVisible = true;
			label.Padding = new System.Windows.Forms.Padding(5);
			label.RelitiveZIndex = index++;

			this.Controls.Add(label);

			int verticalOffset = 35;

			Button button = new Button();
			button.Size = new System.Drawing.Size(this.Size.Width - 10, 20);
			button.Location = new System.Drawing.Point(5, verticalOffset);
			button.Text = "Start Tests";
			button.FontType = FontType.Small;
			button.IsVisible = true;
			button.RelitiveZIndex = index++;
			button.Click += new EventHandler(button_Click);
			this.Controls.Add(button);

			verticalOffset += 25;

			m_ProgressBar = new ProgressBar();
			m_ProgressBar.Size = new System.Drawing.Size(this.Size.Width - 10, 20);
			m_ProgressBar.Location = new System.Drawing.Point(5, verticalOffset);
			m_ProgressBar.IsVisible = true;
			m_ProgressBar.RelitiveZIndex = index++;
			m_ProgressBar.Value = 0;
			m_ProgressBar.MaxValue = 100;
			this.Controls.Add(m_ProgressBar);

			verticalOffset += 25;

			m_SpeedGraph = new MultiGraph();

			m_SpeedGraph.Location = new System.Drawing.Point(5, verticalOffset);
			m_SpeedGraph.Size = new System.Drawing.Size(this.Size.Width - 40, 200);
			m_SpeedGraph.IsVisible = true;
			m_SpeedGraph.RelitiveZIndex = index++;

			this.Controls.Add(m_SpeedGraph);

			m_OrginalGraph = new SubGraph(100);
			m_OrginalGraph.IsVisible = true;
			m_OrginalGraph.LineColor = new SlimDX.Color4(1f, 1f, 0.3f, 0.3f);
			m_OrginalGraph.MaxValue = 0.1f;
			m_OrginalGraph.Scrolling = false;
			m_SpeedGraph.Graphs.Add(m_OrginalGraph);

			m_OptoGraph = new SubGraph(100);
			m_OptoGraph.IsVisible = true;
			m_OptoGraph.LineColor = new SlimDX.Color4(1f, 0.3f, 1f, 0.3f);
			m_OptoGraph.MaxValue = 0.1f;
			m_OptoGraph.Scrolling = false;
			m_SpeedGraph.Graphs.Add(m_OptoGraph);


			m_OptoGraph2 = new SubGraph(100);
			m_OptoGraph2.IsVisible = true;
			m_OptoGraph2.LineColor = new SlimDX.Color4(1f, 1f, 1f, 0.3f);
			m_OptoGraph2.MaxValue = 0.1f;
			m_OptoGraph2.Scrolling = false;
			m_SpeedGraph.Graphs.Add(m_OptoGraph2);


			m_GraphMax = new DynamicLabel();
			m_GraphMax.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			m_GraphMax.FixedSize = true;
			m_GraphMax.Size = new System.Drawing.Size(30, 15);
			m_GraphMax.Location = new System.Drawing.Point(this.Size.Width - 35, verticalOffset);
			m_GraphMax.MaxLength = 10;
			m_GraphMax.Text = "0";
			m_GraphMax.FontType = FontType.Small;
			m_GraphMax.IsVisible = true;
			m_GraphMax.Padding = new System.Windows.Forms.Padding(5);
			m_GraphMax.RelitiveZIndex = index++;
			this.Controls.Add(m_GraphMax);


			m_GraphMin = new DynamicLabel();
			m_GraphMin.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			m_GraphMin.FixedSize = true;
			m_GraphMin.Size = new System.Drawing.Size(30, 15);
			m_GraphMin.Location = new System.Drawing.Point(this.Size.Width - 35, verticalOffset + 185);
			m_GraphMin.MaxLength = 10;
			m_GraphMin.Text = "0";
			m_GraphMin.FontType = FontType.Small;
			m_GraphMin.IsVisible = true;
			m_GraphMin.Padding = new System.Windows.Forms.Padding(5);
			m_GraphMin.RelitiveZIndex = index++;
			this.Controls.Add(m_GraphMin);
			

			verticalOffset += 200;

			this.Size = new System.Drawing.Size(this.Size.Width, verticalOffset + 5);
		}

		public override void AddMenuItems(RugTech1.Framework.Objects.UI.Menus.MenuBar menu)
		{
			MenuBarItem controlsConfig = menu.AddItem("Speed Tests");
			controlsConfig.IsVisible = true;
			controlsConfig.FontType = FontType.Small;

			MenuItem controlProperties = controlsConfig.AddItem("Ensemble");
			controlProperties.IsVisible = true;
			controlProperties.FontType = FontType.Small;
			controlProperties.Click += new EventHandler(Ensemble_Click);
		}

		public override void ResetControlValues()
		{
			
		}

		public override void UpdateControls()
		{
			if (m_TestsInProgress == true)
			{
				if (m_ProgressBar.Value == 100)
				{
					m_TestsInProgress = false;

					if (m_DoGraph_OrginalGraph == true)
					{
						WriteGraphData(Helper.ResolvePath("~/test-data/OrginalGraph.data"), m_OrginalGraph.Values);
					}

					if (m_DoGraph_OptoGraph == true)
					{
						WriteGraphData(Helper.ResolvePath("~/test-data/DSParticles2.data"), m_OptoGraph.Values);
					}

					if (m_DoGraph_OptoGraph2 == true)
					{
						WriteGraphData(Helper.ResolvePath("~/test-data/DSParticles3.data"), m_OptoGraph2.Values);
					}

					//Helper.ResolvePath("~/test-data/OptoGraph.data");
					//Helper.ResolvePath("~/test-data/OptoGraph2.data");

					return;
				}

				bool first = m_ProgressBar.Value == 0;

				m_ProgressBar.Value += 1;

				int frames = 50;
				double ticksToSeconds = 1000000.0; // 10000000.0; 
				float time1 = 0, time2 = 0, time3 = 0;

				if (m_DoGraph_OrginalGraph == true)
				{
					DSParticles.Tester test = new DSParticles.Tester(1024, 1024, new double[1024 * 1024]);

					long timeInTicks = test.RunTest((int)m_ProgressBar.Value * 10, frames, 5);

					double timeInSeconds = ((double)timeInTicks) / (ticksToSeconds * (double)frames);

					time1 = (float)timeInSeconds;

					GC.WaitForFullGCComplete();
				}

				if (m_DoGraph_OptoGraph == true)
				{
					DSParticles2.Tester test = new DSParticles2.Tester(1024, 1024, new double[1024 * 1024]);

					long timeInTicks = test.RunTest((int)m_ProgressBar.Value * 10, frames, 5);

					double timeInSeconds = ((double)timeInTicks) / (ticksToSeconds * (double)frames);

					time2 = (float)timeInSeconds;

					GC.WaitForFullGCComplete();
				}	
				/* 
				if (m_DoGraph_OptoGraph == true)
				{
					DSParticlesOpt.Tester test = new DSParticlesOpt.Tester(1024, 1024, new double[1024 * 1024]);

					long timeInTicks = test.RunTest((int)m_ProgressBar.Value * 10, frames, 5);

					double timeInSeconds = ((double)timeInTicks) / (ticksToSeconds * (double)frames);

					time2 = (float)timeInSeconds;

					GC.WaitForFullGCComplete();
				}				
				*/ 
				if (m_DoGraph_OptoGraph2 == true)
				{
					DSParticles3.Tester test = new DSParticles3.Tester(1024, 1024, new double[1024 * 1024]);

					long timeInTicks = test.RunTest((int)m_ProgressBar.Value * 10, frames, 5);

					double timeInSeconds = ((double)timeInTicks) / (ticksToSeconds * (double)frames);

					time3 = (float)timeInSeconds;

					GC.WaitForFullGCComplete();
				}								

				if (first == true)
				{
					float min = time1;

					if (min > time2)
					{
						min = time2;
					}

					if (min > time3)
					{
						min = time3;
					}

					m_OrginalGraph.MinValue = min;
					m_OptoGraph.MinValue = min;
					m_OptoGraph2.MinValue = min;

					m_OrginalGraph.Clear();
					m_OptoGraph.Clear();
					m_OptoGraph2.Clear();

					if (m_DoGraph_OrginalGraph == false)
					{
						foreach (float data in ReadGraphData(Helper.ResolvePath("~/test-data/OrginalGraph.data")))
						{
							m_OrginalGraph.AddValue(data);
						}
					}

					if (m_DoGraph_OptoGraph == false)
					{
						foreach (float data in ReadGraphData(Helper.ResolvePath("~/test-data/DSParticles2.data")))
						{
							m_OptoGraph.AddValue(data);
						}
					}

					if (m_DoGraph_OptoGraph2 == false)
					{
						foreach (float data in ReadGraphData(Helper.ResolvePath("~/test-data/DSParticles3.data")))
						{
							m_OptoGraph2.AddValue(data);
						}
					}
				}

				if (m_DoGraph_OrginalGraph == true)
				{
					m_OrginalGraph.AddValue(time1);
				}

				if (m_DoGraph_OptoGraph == true)
				{
					m_OptoGraph.AddValue(time2);
				}

				if (m_DoGraph_OptoGraph2 == true)
				{
					m_OptoGraph2.AddValue(time3);
				}
				
				{
					float max = m_OrginalGraph.MaxValue;

					if (m_OptoGraph.MaxValue > max)
					{
						max = m_OptoGraph.MaxValue;
					}

					if (m_OptoGraph2.MaxValue > max)
					{
						max = m_OptoGraph2.MaxValue;
					}

					m_OptoGraph.MaxValue = max;
					m_OptoGraph2.MaxValue = max;
					m_OrginalGraph.MaxValue = max;

					m_GraphMax.Text = max.ToString("N2");
				}

				{
					float min = m_OrginalGraph.MinValue;
					
					if (min > m_OptoGraph.MinValue)
					{
						min = m_OptoGraph.MinValue;
					}

					if (min > m_OptoGraph2.MinValue)
					{
						min = m_OptoGraph2.MinValue;
					}

					m_OrginalGraph.MinValue = min;
					m_OptoGraph.MinValue = min;
					m_OptoGraph2.MinValue = min;

					m_GraphMin.Text = min.ToString("N2");
				}

				GC.WaitForFullGCComplete();
			}
		}


		void Ensemble_Click(object sender, EventArgs e)
		{
			PanelControler.SetOrToggleCurrentPanel(ScreenPanel);
		}
		
		void button_Click(object sender, EventArgs e)
		{
			if (m_TestsInProgress == false)
			{
				Thread.CurrentThread.Priority = ThreadPriority.Highest; 

				m_TestsInProgress = true;

				m_OrginalGraph.Clear();
				m_OptoGraph.Clear();
				m_OptoGraph2.Clear(); 

				m_ProgressBar.Value = 0;
			}
		}

		private void WriteGraphData(string filepath, float[] data)
		{
			FileInfo file = new FileInfo(filepath);

			if (file.Directory.Exists == false)
			{
				file.Directory.Create();
			}

			using (FileStream fileStream = new FileStream(file.FullName, FileMode.Create))
			{
				using (BinaryWriter writer = new BinaryWriter(fileStream))
				{
					writer.Write(data.Length);

					foreach (float d in data)
					{
						writer.Write(d);
					}
				}
			}
		}

		private float[] ReadGraphData(string filepath)
		{
			FileInfo file = new FileInfo(filepath);

			if (file.Exists == false)
			{
				return new float[0];
			}

			using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open))
			{
				using (BinaryReader reader = new BinaryReader(fileStream))
				{
					int length = reader.ReadInt32();
					float[] data = new float[length];

					for (int i = 0; i < data.Length; i++)
					{
						data[i] = reader.ReadSingle();
					}

					return data;
				}
			}
		}
	}
}
