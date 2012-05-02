using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using RugTech1;
using RugTech1.Framework.Objects;
using DS.Kinect;
using Rug.Cmd;
using DS.Simulation;
using System.Drawing;
using DSParticles3;
using SlimDX;
using DS.Audio;

namespace DS
{
	class ArtworkStaticObjects
	{
		#region Private Members

		private static View3D m_View;

		//private static RecordingControler m_RecordingControler;
		private static OscOutput m_OscControler;
        
        /// <summary>
        /// All the options for the app that get loaded and saved  
        /// </summary>
		private static ArtworkOptions m_Options;  
        /// <summary>
        /// all the kinect devices
        /// </summary>
		private static KinectDeviceMaster m_KinectDevices;
        /// <summary>
        /// master external field
        /// </summary>
		private static CompositeFieldImage m_CompositeFieldImage;

		private static ParticleEnsemble m_Ensemble;
		private static ExternalField m_ExternalField;

		private static FFTScanner m_FFTScanner; 

		#endregion

		#region Public Properties

        /// <summary>
        /// This is the FFT to be repopulated every frame 
        /// </summary>
        //public static readonly float[] FFTData = new float[ParticleEnsemble.VelocityAutoCorrelationLength]; 


		public static string DefaultConfigFilePath { get { return RugTech1.Helper.ResolvePath(@"~/Config.xml"); } }

		public static View3D View
		{
			get { return ArtworkStaticObjects.m_View; }
			set { ArtworkStaticObjects.m_View = value; }
		}

		/*
		public static RecordingControler RecordingControler
		{
			get { return m_RecordingControler; }
		}
		*/

		public static OscOutput OscControler
		{
			get { return m_OscControler; }
		}

        /// <summary>
        /// All the options for the app that get loaded and saved  
        /// </summary>
		public static ArtworkOptions Options
		{
			get { return m_Options; }
		}

        /// <summary>
        /// all the kinect devices
        /// </summary>
		public static KinectDeviceMaster KinectDevices
		{
			get { return m_KinectDevices; }
		}

        /// <summary>
        /// master external field
        /// </summary>
		public static CompositeFieldImage CompositeFieldImage
		{
			get { return m_CompositeFieldImage; }
		}

		public static ParticleEnsemble Ensemble
		{
			get { return m_Ensemble; }
		}
		
		public static ExternalField ExternalField
		{
			get { return m_ExternalField; }
		}

		public static FFTScanner FFTScanner { get { return m_FFTScanner; } }

		/* 
		public static ClientThread Communicator
		{
			get { return m_Communicator; }
		}
		*/ 

		#endregion

		#region Initialize / Dispose

		public static void Initialize()
		{
			m_OscControler = new OscOutput();

			m_OscControler.FFTDataRecorder = new OscEventRecorder(RugTech1.Helper.ResolvePath("~/FFT_Record.txt"));

			m_Options = new ArtworkOptions();

			m_FFTScanner = new Simulation.FFTScanner();
			m_KinectDevices = new KinectDeviceMaster(); 
			m_KinectDevices.Initialize();

			m_CompositeFieldImage = new CompositeFieldImage();
			m_CompositeFieldImage.Bounds = new Rectangle(0, 0, 800, 400); 

			foreach (KinectDevice device in m_KinectDevices.Devices)
			{
				m_CompositeFieldImage.RegisterDevice(device);				
			}

			//m_KinectDevices.UseTestImage = true; 
		}

		public static void Dispose()
		{
			if (m_OscControler.FFTDataRecorder != null)
			{
				m_OscControler.FFTDataRecorder.Dispose(); 
			}
	
			m_KinectDevices.Dispose();

			m_OscControler.Disconnect();

			//m_RecordingControler.Dispose();
		}

		#endregion

		#region Load / Save Config Xml

		public static void LoadConfig(string filename)
		{
			#region Load the actual config
			
			if (File.Exists(filename))
			{
				RC.WriteLine(ConsoleThemeColor.TitleText1, "Loading config '" + filename + "'");

				XmlDocument doc = new XmlDocument();

				doc.Load(filename);

				m_Options.Load(doc.DocumentElement);
			}

			#endregion

			#region Setup The Field Image 
			
			m_CompositeFieldImage.Bounds = m_Options.Kinect.Bounds;

			int i = 0;

			foreach (DS.Simulation.CompositeFieldImage.KinectFieldImage field in m_CompositeFieldImage.Images)
			{
				field.X = m_Options.Kinect.Cameras[i].X;
				field.Y = m_Options.Kinect.Cameras[i++].Y;
			}

			#endregion

			#region Setup The Particle Simulation 
			
			double gradScaleFactor = 1000.0;
			double MinRad = 10.0;
			double MaxRad = 30.0;

			List<string> FFtype = new List<string>(new string[] { "SoftSpheres", "ExternalField" });
			
			ParticleStaticObjects.AtomPropertiesDefinition.Clear();
			ParticleStaticObjects.AtomPropertiesDefinition.AddParticleDefinition("A", 1, new Color3(0.0390625f, 0.1953125f, 0.99609375f), 0, 20); // neon blue  0xc00A32FF
			ParticleStaticObjects.AtomPropertiesDefinition.AddParticleDefinition("B", 4, new Color3(1f, 0.03921569f, 0.1960784f), 1, 49); // red 0xc0FF0A32
			ParticleStaticObjects.AtomPropertiesDefinition.AddParticleDefinition("C", 6, new Color3(0.1960784f, 0.03921569f, 1f), 2, 45); // purple 0xc0320AFF
			ParticleStaticObjects.AtomPropertiesDefinition.AddParticleDefinition("D", 9.2, new Color3(0.1490196f, 0.8470589f, 1f), 3, 43); // sky blue 0xc026D8FF // 0xc032FF0A, 3,  43); // green 
			ParticleStaticObjects.AtomPropertiesDefinition.AddParticleDefinition("E", 12.6, new Color3(1f, 0.1960784f, 0.03921569f), 4, 38); // orange 0xc0FF320A

			ParticleStaticObjects.ReSeedRandom(42);			

			m_Ensemble = new ParticleEnsemble(1, MinRad, MaxRad, FFtype, ArtworkStaticObjects.CompositeFieldImage.Height, ArtworkStaticObjects.CompositeFieldImage.Width, gradScaleFactor);
			m_ExternalField = new ExternalField(m_Ensemble, ArtworkStaticObjects.CompositeFieldImage);

			for (int j = 0; j < ParticleStaticObjects.AtomPropertiesDefinition.Count; j++)
			{
				ParticleStaticObjects.AtomPropertiesDefinition.SetAttractiveOrRepulsive(j, Options.Simulation.ParticleTypes[j].AttractiveOrRepulsive);
				ParticleStaticObjects.AtomPropertiesDefinition.SetEnabled(j, Options.Simulation.ParticleTypes[j].IsEnabled);
				ParticleStaticObjects.AtomPropertiesDefinition.SetSound(j, Options.Simulation.ParticleTypes[j].IsSoundOn);
			}

			m_Ensemble.BerendsenThermostatCoupling = Options.Simulation.BerendsenThermostatCoupling;
			m_Ensemble.EquilibriumTemperature = Options.Simulation.EquilibriumTemperature;
			m_Ensemble.GradientScaleFactor = Options.Simulation.GradientScaleFactor;
			
			int newNumber = Options.Simulation.NumberOfParticles;
			while (ArtworkStaticObjects.Ensemble.NumberOfParticles < newNumber)
			{
				ArtworkStaticObjects.Ensemble.InitializeOneNewParticle();
			}

			while (ArtworkStaticObjects.Ensemble.NumberOfParticles > newNumber)
			{
				ArtworkStaticObjects.Ensemble.Particles.Pop();
			}

			m_Ensemble.ParticleScale = Options.Simulation.ParticleScale; 
			m_Ensemble.PotentialEnergy = Options.Simulation.PotentialEnergy;

			//ParticleStaticObjects.AtomPropertiesDefinition.ToggleAttractiveOrRepulsive(0);
			
			#endregion
		}

		public static void SaveConfig(string filename)
		{
			if (m_CompositeFieldImage != null)
			{
				m_Options.Kinect.Bounds = m_CompositeFieldImage.Bounds;

				int i = 0;
				foreach (CompositeFieldImage.KinectFieldImage image in m_CompositeFieldImage.Images)
				{
					m_Options.Kinect.Cameras[i].X = image.X;
					m_Options.Kinect.Cameras[i++].Y = image.Y;
				}
			}

			RC.WriteLine(ConsoleThemeColor.TitleText1, "Saving config '" + filename + "'");

			XmlDocument doc = new XmlDocument();
			XmlElement config = RugTech1.Helper.CreateElement(doc, "Config");

			doc.AppendChild(config);

			m_Options.Save(config);

			doc.Save(filename);
		}

		#endregion
	}
}
