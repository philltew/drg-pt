using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using DS.OSC;
//using DS.ShadowKinect;
//using Microsoft.Research.Kinect.Nui;
using RugTech1.Framework;
using RugTech1.Framework.Objects.Text;
using RugTech1.Framework.Objects.UI;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.UI.Dynamic;
using RugTech1.Framework.Objects.UI.Menus;
using DS.Scenes;
using DS.Objects;
using SlimDX;

namespace DS.Panels
{
	class SplashScreen : UiScene
	{
		#region Private Members

		private bool m_IsVisible = true;

		private ArtworkOptions KinectOptions { get { return ArtworkStaticObjects.Options; } }
		//private KinectDevice KinectDevice { get { return ArtworkStaticObjects.KinectDevice; } }
		//private SpatialSoundControler OscControler { get { return ArtworkStaticObjects.OscControler; } }

		private Panel m_StatusPanel; 
		private DynamicLabel m_FPSLabel;
		private DynamicLabel m_PacketLabel;
		private DynamicLabel m_DroppedFramesLabel;
		private DynamicLabel m_RecordingStatusLabel;
		private MultiGraph m_SpeedGraph;
		private Graph m_FPSGraph;
		private Graph m_PacketGraph;
		private Graph m_DroppedFramesGraph;

		private MultiGraph m_FFTGraph;
        private MultiGraph m_FFTGraph2;
		private Graph m_CorrelationFunctionGraph;
		private PeakGraph m_FFTFreqsGraph;
		private ConsoleControl m_Console; 

		private VisiblePanelControler m_PanelControler = new VisiblePanelControler(); 
		private List<PanelBase> m_Panels = new List<PanelBase>();
		private KinectImageTunerPanel m_KinectImageTunerPanel; 

		private SetupScene m_SetupScene; 

		#endregion

		#region Properties
		
		public bool IsVisible { get { return m_IsVisible; } set { m_IsVisible = value; } }

		public SplashSceenPanels CurrentPanel { get { return m_PanelControler.CurrentPanel; } } 

		public event PanelEvent PanelChanged;  

		#endregion

		public SplashScreen(SetupScene setupScene)
		{
			m_SetupScene = setupScene;
			m_PanelControler.PanelChanged += new PanelEvent(m_PanelControler_PanelChanged);
		}

		void m_PanelControler_PanelChanged(SplashSceenPanels CurrentPanel)
		{
			if (PanelChanged != null)
			{
				PanelChanged(CurrentPanel); 
			}

			m_Console.IsVisible = CurrentPanel != SplashSceenPanels.Realtime; 
		}

		#region Initialize Controls

		protected override void InitializeControls()
		{
			base.InitializeControls();

			m_KinectImageTunerPanel = new KinectImageTunerPanel();

			m_KinectImageTunerPanel.Size = new System.Drawing.Size(GameConfiguration.ActiveRegion.Width - 335, 200);
			m_KinectImageTunerPanel.Location = new System.Drawing.Point(320, 15);
			m_KinectImageTunerPanel.ShowBackground = true;
			m_KinectImageTunerPanel.ShowBorder = true;
			m_KinectImageTunerPanel.RelitiveZIndex = 300;

			//m_Panels.Add(new ControlPanel(m_PanelControler, 10));

			m_Panels.Add(new KinectPanel(m_PanelControler, 20, m_KinectImageTunerPanel));
			m_Panels.Add(new CompositeFieldImagePanel(m_PanelControler, 30));
			m_Panels.Add(new FFTPanel(m_PanelControler, 35));
			m_Panels.Add(new OscPanel(m_PanelControler, 40));
			m_Panels.Add(new RealTimePanel(m_PanelControler, 50));
			//m_Panels.Add(new SimulationPanel(m_PanelControler, 40));
			//m_Panels.Add(new ParticlesPanel(m_PanelControler, 50));
			//m_Panels.Add(new VisualPanel(m_PanelControler, 60));
			
			
			m_Panels[1].Size = new System.Drawing.Size(GameConfiguration.ActiveRegion.Width - 30, 3); 

			CreateMenu();

			CreateStatusBar();
			
			int index = 300; 

			m_Console = new ConsoleControl();
			m_Console.NumberOfChars = 98;
			m_Console.NumberOfLines = 10;
			m_Console.ConsoleBuffer = GameEnvironment.ConsoleBuffer; 
			m_Console.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			m_Console.Location = new System.Drawing.Point(2, GameConfiguration.ActiveRegion.Bottom - (m_StatusPanel.Size.Height + m_Console.Padding.Vertical + (((int)TextRenderHelper.MessureString(" ", FontType.Monospaced, 1f).Height + 1) * (m_Console.NumberOfLines + 1))));
			m_Console.IsVisible = true;
			m_Console.RelitiveZIndex = index++;

			this.Controls.Add(m_Console);

			foreach (PanelBase panel in m_Panels)
			{
				this.Controls.Add(panel);

				panel.Initiate();

				panel.IsVisible = false; 
			}

			m_KinectImageTunerPanel.Size = new System.Drawing.Size(m_KinectImageTunerPanel.Size.Width, m_Panels[0].Size.Height);
			m_KinectImageTunerPanel.IsVisible = false; 

			this.Controls.Add(m_KinectImageTunerPanel);
		}

		#endregion

		public void Resize()
		{
			m_Console.Location = new System.Drawing.Point(2, GameConfiguration.ActiveRegion.Bottom - (m_StatusPanel.Size.Height + m_Console.Padding.Vertical + (((int)TextRenderHelper.MessureString(" ", FontType.Monospaced, 1f).Height + 1) * (m_Console.NumberOfLines + 1))));
		}

		#region Update Dynamic Controls

		public override void Update()
		{
			base.Update();

			if (GameEnvironment.FramesClick)
			{ 
				m_FPSLabel.Text = "FPS: " + GameEnvironment.FramesPerSecond.ToString("N2");
				m_FPSGraph.AddValue(GameEnvironment.FramesPerSecond);

				m_PacketLabel.Text = "Packets: " + ArtworkStaticObjects.OscControler.PacketCount.ToString();
				m_PacketGraph.AddValue(ArtworkStaticObjects.OscControler.PacketCount);
				ArtworkStaticObjects.OscControler.ResetPacketCount();

				//m_DroppedFramesLabel.Text = "Dropped: " + ArtworkStaticObjects.RecordingControler.DroppedFrames.ToString();
				//m_DroppedFramesGraph.AddValue((float)ArtworkStaticObjects.RecordingControler.DroppedFrames);
				//ArtworkStaticObjects.RecordingControler.ResetDroppedFramesCount();

				//m_RecordingStatusLabel.Text = ArtworkStaticObjects.RecordingControler.IsOpen ?
				//	(ArtworkStaticObjects.RecordingControler.IsRecording ? "Recording" : "Paused") :
				//	"Not Recording";

                //for (int i = 0; i < ArtworkStaticObjects.FFTData.Length; i++)
                //{
                //    m_RawFFTGraph.AddValue(ArtworkStaticObjects.FFTData[i]); 
			    //}

            }

            float maxValue = 0; 

            for (int i = 0; i < ArtworkStaticObjects.Ensemble.xtest.Length; i++)
            {
                float value = (float)ArtworkStaticObjects.Ensemble.xtest[i]; 

                if (value < 0 && maxValue < value * -1)
                {
                    maxValue = value * -1; 
                }
                else if (value > 0 && maxValue < value)
                {
                    maxValue = value; 
                }

                m_CorrelationFunctionGraph.AddValue((float)ArtworkStaticObjects.Ensemble.xtest[i]);    
            }
			
			int newTickSize = 60;

			if (ArtworkStaticObjects.Options.FFT.CorrelationFunctionUpdateFrequency < 6)
			{
				newTickSize = (ArtworkStaticObjects.Options.FFT.CorrelationFunctionUpdateFrequency * 600) / 60;
				m_FFTFreqsGraph.MinorTickLineColor = new Color4(0.4f, 1f, 0, 0);
				m_FFTFreqsGraph.MajorTickLineColor = new Color4(0.6f, 1f, 1f, 1f);
			}
			else if (ArtworkStaticObjects.Options.FFT.CorrelationFunctionUpdateFrequency < 60)
			{
				newTickSize = ArtworkStaticObjects.Options.FFT.CorrelationFunctionUpdateFrequency;
				m_FFTFreqsGraph.MinorTickLineColor = new Color4(0.2f, 1f, 1f, 1f);
				m_FFTFreqsGraph.MajorTickLineColor = new Color4(0.4f, 1f, 0f, 0f);
			}
			else
			{
				newTickSize = (ArtworkStaticObjects.Options.FFT.CorrelationFunctionUpdateFrequency) / 6;
				m_FFTFreqsGraph.MinorTickLineColor = new Color4(0.1f, 1f, 1f, 1f);			
				m_FFTFreqsGraph.MajorTickLineColor = new Color4(0.2f, 1f, 1f, 1f);									
			}


			if (newTickSize != m_FFTFreqsGraph.TickSpace)
			{
				m_FFTFreqsGraph.TickSpace = newTickSize; 
				Invalidate(); 
			}

			if (ArtworkStaticObjects.FFTScanner.ShouldScan == true)
			{

                float lfoValue = ((1f / maxValue) * (float)ArtworkStaticObjects.Ensemble.xtest[ArtworkStaticObjects.Ensemble.xtest.Length / 4]);
                ArtworkStaticObjects.OscControler.SendLFOValue(lfoValue); 

				//ArtworkStaticObjects.FFTScanner.ScanForPeakFrequencyAndIntensity(ArtworkStaticObjects.Ensemble.AveragedFFTamplitudes, 60f / ((float)ArtworkStaticObjects.Options.FFT.CorrelationFunctionUpdateFrequency * 2f));
				ArtworkStaticObjects.FFTScanner.ScanForPeakFrequencyAndIntensity(ArtworkStaticObjects.Ensemble.AveragedFFTamplitudes, ArtworkStaticObjects.Ensemble.FFTfreqs, GameEnvironment.FramesPerSecond, (float)ArtworkStaticObjects.Options.FFT.CorrelationFunctionUpdateFrequency);

				m_FFTFreqsGraph.PeakCount = ArtworkStaticObjects.Options.FFT.PeakCount;
				for (int i = 0, e = ArtworkStaticObjects.Options.FFT.PeakCount; i < e; i++)
				{
					m_FFTFreqsGraph.PeakLocations[i] = ArtworkStaticObjects.FFTScanner.PeakLocations[i];
				}

				for (int i = 0; i < ArtworkStaticObjects.Ensemble.AveragedFFTamplitudes.Length; i++)
				{
					m_FFTFreqsGraph.AddValue((float)ArtworkStaticObjects.Ensemble.AveragedFFTamplitudes[i]);
				}

				ArtworkStaticObjects.OscControler.SendFFTData(ArtworkStaticObjects.FFTScanner.PeaksFrequencyAndIntensity); 
				
				ArtworkStaticObjects.FFTScanner.ShouldScan = false; 
			}
                
			if (IsVisible)
			{
				m_KinectImageTunerPanel.IsVisible = m_Panels[0].IsVisible; 

				foreach (PanelBase panel in m_Panels)
				{
					panel.UpdateControls();
				}
			}
		}

		#endregion

		#region Create Status Bar

		private void CreateStatusBar()
		{
			#region Status Bar

			int index = 20; 

			m_StatusPanel = new Panel();

			m_StatusPanel.ShowBackground = true;
			m_StatusPanel.ShowBorder = false;
			m_StatusPanel.Docking = System.Windows.Forms.DockStyle.Bottom;
			m_StatusPanel.IsVisible = true;
			m_StatusPanel.RelitiveZIndex = index++; 
			m_StatusPanel.Size = new System.Drawing.Size(152, 50);

			Controls.Add(m_StatusPanel);

			index = 1; 

			m_SpeedGraph = new MultiGraph();

			m_SpeedGraph.Location = new System.Drawing.Point(2, 2);
			m_SpeedGraph.Size = new System.Drawing.Size(150, 48);
			m_SpeedGraph.IsVisible = true;
			m_SpeedGraph.RelitiveZIndex = index++; 

			m_StatusPanel.Controls.Add(m_SpeedGraph);

			m_FPSGraph = new Graph(150);
			m_FPSGraph.IsVisible = true;
			m_FPSGraph.LineColor = new SlimDX.Color4(0.75f, 0.3f, 1f, 0.3f);			
			m_SpeedGraph.Graphs.Add(m_FPSGraph);

			m_FPSLabel = new DynamicLabel();
			m_FPSLabel.ForeColor = new SlimDX.Color4(0.75f, 0.3f, 1f, 0.3f);
			m_FPSLabel.MaxLength = 16;
			m_FPSLabel.Location = new System.Drawing.Point(154, 0);
			m_FPSLabel.FixedSize = false;
			m_FPSLabel.FontType = FontType.Small;
			m_FPSLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			m_FPSLabel.IsVisible = true;
			m_FPSLabel.RelitiveZIndex = index++; 
			m_StatusPanel.Controls.Add(m_FPSLabel);


			m_PacketGraph = new Graph(150);
			m_PacketGraph.IsVisible = true;
			m_PacketGraph.LineColor = new SlimDX.Color4(0.75f, 1f, 0.3f, 0.3f);
			m_SpeedGraph.Graphs.Add(m_PacketGraph);

			m_PacketLabel = new DynamicLabel();
			m_PacketLabel.ForeColor = new SlimDX.Color4(0.75f, 1f, 0.3f, 0.3f);
			m_PacketLabel.MaxLength = 20;
			m_PacketLabel.Location = new System.Drawing.Point(154, 10);
			m_PacketLabel.FixedSize = false;
			m_PacketLabel.FontType = FontType.Small;
			m_PacketLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			m_PacketLabel.IsVisible = true;
			m_PacketLabel.RelitiveZIndex = index++; 
			m_StatusPanel.Controls.Add(m_PacketLabel);


			m_DroppedFramesGraph = new Graph(150);
			m_DroppedFramesGraph.IsVisible = true;
			m_DroppedFramesGraph.LineColor = new SlimDX.Color4(0.75f, 1f, 0.3f, 1f);
			m_SpeedGraph.Graphs.Add(m_DroppedFramesGraph);


			m_DroppedFramesLabel = new DynamicLabel();
			m_DroppedFramesLabel.ForeColor = new SlimDX.Color4(0.75f, 1f, 0.3f, 1f);
			m_DroppedFramesLabel.MaxLength = 20;
			m_DroppedFramesLabel.Location = new System.Drawing.Point(240, 0);
			m_DroppedFramesLabel.FixedSize = false;
			m_DroppedFramesLabel.FontType = FontType.Small;
			m_DroppedFramesLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			m_DroppedFramesLabel.IsVisible = true;
			m_DroppedFramesLabel.RelitiveZIndex = index++;
			m_StatusPanel.Controls.Add(m_DroppedFramesLabel);

			m_RecordingStatusLabel = new DynamicLabel();
			m_RecordingStatusLabel.ForeColor = new SlimDX.Color4(0.75f, 1f, 0.3f, 1f);
			m_RecordingStatusLabel.MaxLength = 20;
			m_RecordingStatusLabel.Location = new System.Drawing.Point(240, 10);
			m_RecordingStatusLabel.FixedSize = false;
			m_RecordingStatusLabel.FontType = FontType.Small;
			m_RecordingStatusLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			m_RecordingStatusLabel.IsVisible = true;
			m_RecordingStatusLabel.RelitiveZIndex = index++;
			m_StatusPanel.Controls.Add(m_RecordingStatusLabel);


			m_FFTGraph = new MultiGraph();
			m_FFTGraph.Location = new System.Drawing.Point(340, 2);
            m_FFTGraph.Size = new System.Drawing.Size(DSParticles3.ParticleEnsemble.VelocityAutoCorrelationLength / 2, 22);
			//m_FFTGraph.Size = new System.Drawing.Size(512, 22);

			m_FFTGraph.IsVisible = true;
			m_FFTGraph.RelitiveZIndex = index++;
			m_StatusPanel.Controls.Add(m_FFTGraph);

			m_CorrelationFunctionGraph = new Graph(ArtworkStaticObjects.Ensemble.xtest.Length);// the number in brackets is how many data points in the graph
            m_CorrelationFunctionGraph.Scrolling = false; // this may need to be true (test) 
			m_CorrelationFunctionGraph.IsVisible = true;
			m_CorrelationFunctionGraph.LineColor = new SlimDX.Color4(0.75f, 1f, 1f, 1f);
            m_CorrelationFunctionGraph.ScaleEveryFrame = true;
			m_FFTGraph.Graphs.Add(m_CorrelationFunctionGraph);


            m_FFTGraph2 = new MultiGraph();
            m_FFTGraph2.Location = new System.Drawing.Point(340, 26);
            m_FFTGraph2.Size = new System.Drawing.Size(DSParticles3.ParticleEnsemble.VelocityAutoCorrelationLength / 2, 22);
			//m_FFTGraph2.Size = new System.Drawing.Size(512, 22);
            m_FFTGraph2.IsVisible = true;
            m_FFTGraph2.RelitiveZIndex = index++;
            m_StatusPanel.Controls.Add(m_FFTGraph2);

            m_FFTFreqsGraph = new PeakGraph(24, ArtworkStaticObjects.Ensemble.AveragedFFTamplitudes.Length);// the number in brackets is how many data points in the graph
            m_FFTFreqsGraph.Scrolling = false; // this may need to be true (test) 
            m_FFTFreqsGraph.IsVisible = true;
            m_FFTFreqsGraph.LineColor = new SlimDX.Color4(0.75f, 1f, 0.2f, 0.2f);
			m_FFTFreqsGraph.PeakLineColor = new SlimDX.Color4(0.75f, 1f, 0f, 1f);
			m_FFTFreqsGraph.ShowTicks = true;
			m_FFTFreqsGraph.ShowPeaks = true; 
			m_FFTFreqsGraph.TickSpace = ArtworkStaticObjects.Ensemble.AveragedFFTamplitudes.Length / 64; 			
            m_FFTFreqsGraph.ScaleEveryFrame = true;
            m_FFTGraph2.Graphs.Add(m_FFTFreqsGraph);

			#endregion
		}

		#endregion

		#region Create Menu

		private void CreateMenu()
		{
			#region Menu Bar

			int index = 100; 

			MenuBar menu = new MenuBar();

			menu.IsVisible = true;
			menu.Docking = System.Windows.Forms.DockStyle.Top;
			menu.Size = new System.Drawing.Size(20, 20);
			menu.RelitiveZIndex = index; 
			Controls.Add(menu);

			foreach (PanelBase panel in m_Panels)
			{
				panel.AddMenuItems(menu);
			}

			#endregion
		}
		
		#endregion

		#region Reset Control Values
		
		public void ResetControlValues()
		{
			foreach (PanelBase panel in m_Panels)
			{
				panel.ResetControlValues(); 
			}

			int newNumber = (int)ArtworkStaticObjects.Options.Simulation.NumberOfParticles;

			while (ArtworkStaticObjects.Ensemble.NumberOfParticles > 0)
			{
				ArtworkStaticObjects.Ensemble.Particles.Pop();				
			}

			while (ArtworkStaticObjects.Ensemble.NumberOfParticles < newNumber)
			{
				ArtworkStaticObjects.Ensemble.InitializeOneNewParticle();
			}

			this.Invalidate();
		}

		#endregion
	}
}

