using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects;
using RugTech1;
using System.Xml;
using System.Drawing;

namespace DS
{
	class ArtworkOptions
	{
		#region Option Object Members

		public readonly KinectOptions Kinect = new KinectOptions();
		public readonly SimulationOptions Simulation = new SimulationOptions();
		public readonly VisualOptions Visual = new VisualOptions();
		//public readonly ControlOptions Controls = new ControlOptions();		
		//public readonly MeshOptions Mesh = new MeshOptions();
		//public readonly JointsOptions Joints = new JointsOptions();
		public readonly OscOptions Osc = new OscOptions();
		//public readonly RecordOptions Record = new RecordOptions();
		public readonly FFTOptions FFT = new FFTOptions(); 

		//public readonly CommunicationOptions Communications = new CommunicationOptions();

		public readonly ScenePresets Scenes = new ScenePresets();

		#endregion

		#region Load All Options

		public void Load(XmlNode node)
		{
			XmlNode KinectNode = node.SelectSingleNode("Kinect");
			if (KinectNode != null) Kinect.Load(KinectNode);

			XmlNode SimulationNode = node.SelectSingleNode("Simulation");
			if (SimulationNode != null) Simulation.Load(SimulationNode);

			XmlNode VisualNode = node.SelectSingleNode("Visual");
			if (VisualNode != null) Visual.Load(VisualNode);
			
			//XmlNode ControlNode = node.SelectSingleNode("Controls");
			//if (ControlNode != null) Controls.Load(ControlNode);

			//XmlNode MeshNode = node.SelectSingleNode("Mesh");
			//if (MeshNode != null) Mesh.Load(MeshNode);

			//XmlNode JointsNode = node.SelectSingleNode("Joints");
			//if (JointsNode != null) Joints.Load(JointsNode);

			XmlNode OscNode = node.SelectSingleNode("Osc");
			if (OscNode != null) Osc.Load(OscNode);

			//XmlNode RecordNode = node.SelectSingleNode("Record");
			//if (RecordNode != null) Record.Load(RecordNode);

			//XmlNode CommunicationsNode = node.SelectSingleNode("Communications");
			//if (CommunicationsNode != null) Communications.Load(CommunicationsNode);

			XmlNode FFTNode = node.SelectSingleNode("FFT");
			if (FFTNode != null) FFT.Load(FFTNode);
			

			XmlNode ScenesNode = node.SelectSingleNode("Scenes");
			if (ScenesNode != null) Scenes.Load(ScenesNode);
		}

		#endregion

		#region Save All Options

		public void Save(XmlNode node)
		{
			XmlElement KinectNode = Helper.CreateElement(node, "Kinect");
			Kinect.Save(KinectNode);
			node.AppendChild(KinectNode);

			XmlElement SimulationNode = Helper.CreateElement(node, "Simulation");
			Simulation.Save(SimulationNode);
			node.AppendChild(SimulationNode);

			XmlElement VisualNode = Helper.CreateElement(node, "Visual");
			Visual.Save(VisualNode);
			node.AppendChild(VisualNode);
			
			//XmlElement ControlNode = Helper.CreateElement(node, "Controls");
			//Controls.Save(ControlNode);
			//node.AppendChild(ControlNode);

			//XmlElement MeshNode = Helper.CreateElement(node, "Mesh");
			//Mesh.Save(MeshNode);
			//node.AppendChild(MeshNode);

			//XmlElement JointsNode = Helper.CreateElement(node, "Joints");
			//Joints.Save(JointsNode);
			//node.AppendChild(JointsNode);

			XmlElement OscNode = Helper.CreateElement(node, "Osc");
			Osc.Save(OscNode);
			node.AppendChild(OscNode);

			//XmlElement RecordNode = Helper.CreateElement(node, "Record");
			//Record.Save(RecordNode);
			//node.AppendChild(RecordNode);

			//XmlElement CommunicationsNode = Helper.CreateElement(node, "Communications");
			//Communications.Save(CommunicationsNode);
			//node.AppendChild(CommunicationsNode);

			XmlElement FFTNode = Helper.CreateElement(node, "FFT");
			FFT.Save(FFTNode);
			node.AppendChild(FFTNode);

			XmlElement ScenesNode = Helper.CreateElement(node, "Scenes");
			Scenes.Save(ScenesNode);
			node.AppendChild(ScenesNode);
		}

		#endregion

		#region Copy Options

		public void CopyTo(ScenePresets.ScenePreset scenePreset)
		{
			scenePreset.HasBeenStored = true;
			//Kinect.CopyTo(scenePreset.Kinect);
			Simulation.CopyTo(scenePreset.Simulation);
			Visual.CopyTo(scenePreset.Visual);
			FFT.CopyTo(scenePreset.FFT); 
			//Controls.CopyTo(scenePreset.Controls);			
			//Mesh.CopyTo(scenePreset.Mesh);
			//Joints.CopyTo(scenePreset.Joints);
			//Osc.CopyTo(scenePreset.Osc);
		}

		public void CopyFrom(ScenePresets.ScenePreset scenePreset)
		{
			//scenePreset.Kinect.CopyTo(Kinect);
			scenePreset.Simulation.CopyTo(Simulation);
			scenePreset.Visual.CopyTo(Visual);
			scenePreset.FFT.CopyTo(FFT); 
			//scenePreset.Controls.CopyTo(Controls);			
			//scenePreset.Mesh.CopyTo(Mesh);
			//scenePreset.Joints.CopyTo(Joints);
			//scenePreset.Osc.CopyTo(Osc);
		}

		#endregion


		#region Scenes

		public class ScenePresets
		{
			public class ScenePreset
			{
				public bool HasBeenStored = false;				
				
				//public readonly KinectOptions Kinect = new KinectOptions();
				public readonly SimulationOptions Simulation = new SimulationOptions();
				public readonly VisualOptions Visual = new VisualOptions();
				public readonly FFTOptions FFT = new FFTOptions();

				//public readonly ControlOptions Controls = new ControlOptions();				
				//public readonly MeshOptions Mesh = new MeshOptions();
				//public readonly JointsOptions Joints = new JointsOptions();
				//public readonly OscOptions Osc = new OscOptions();

				public void Load(XmlNode node)
				{
					HasBeenStored = true;

					//XmlNode KinectNode = node.SelectSingleNode("Kinect");
					//if (KinectNode != null) Kinect.Load(KinectNode);

					XmlNode SimulationNode = node.SelectSingleNode("Simulation");
					if (SimulationNode != null) Simulation.Load(SimulationNode);

					XmlNode VisualNode = node.SelectSingleNode("Visual");
					if (VisualNode != null) Visual.Load(VisualNode);

					XmlNode FFTNode = node.SelectSingleNode("FFT");
					if (FFTNode != null) FFT.Load(FFTNode);

					//XmlNode ControlNode = node.SelectSingleNode("Controls");
					//if (ControlNode != null) Controls.Load(ControlNode);

					//XmlNode MeshNode = node.SelectSingleNode("Mesh");
					//if (MeshNode != null) Mesh.Load(MeshNode);

					//XmlNode JointsNode = node.SelectSingleNode("Joints");
					//if (JointsNode != null) Joints.Load(JointsNode);

					//XmlNode OscNode = node.SelectSingleNode("Osc");
					//if (OscNode != null) Osc.Load(OscNode);
				}

				public void Save(XmlNode node)
				{
					//XmlElement KinectNode = Helper.CreateElement(node, "Kinect");
					//Kinect.Save(KinectNode);
					//node.AppendChild(KinectNode);

					XmlElement SimulationNode = Helper.CreateElement(node, "Simulation");
					Simulation.Save(SimulationNode);
					node.AppendChild(SimulationNode);

					XmlElement VisualNode = Helper.CreateElement(node, "Visual");
					Visual.Save(VisualNode);
					node.AppendChild(VisualNode);

					XmlElement FFTNode = Helper.CreateElement(node, "FFT");
					FFT.Save(FFTNode);
					node.AppendChild(FFTNode);

					//XmlElement ControlNode = Helper.CreateElement(node, "Controls");
					//Controls.Save(ControlNode);
					//node.AppendChild(ControlNode);

					//XmlElement MeshNode = Helper.CreateElement(node, "Mesh");
					//Mesh.Save(MeshNode);
					//node.AppendChild(MeshNode);

					//XmlElement JointsNode = Helper.CreateElement(node, "Joints");
					//Joints.Save(JointsNode);
					//node.AppendChild(JointsNode);

					//XmlElement OscNode = Helper.CreateElement(node, "Osc");
					//Osc.Save(OscNode);
					//node.AppendChild(OscNode);
				}
			}

			public readonly ScenePreset[] Scenes = new ScenePreset[40];

			public ScenePresets()
			{
				for (int i = 0; i < Scenes.Length; i++)
				{
					Scenes[i] = new ScenePreset();
				}
			}

			public void Load(XmlNode node)
			{
				for (int i = 0; i < Scenes.Length; i++)
				{
					XmlNode config = node.SelectSingleNode("Scene_" + i.ToString());
					if (config != null) Scenes[i].Load(config);
				}
			}

			public void Save(XmlElement node)
			{
				for (int i = 0; i < Scenes.Length; i++)
				{
					if (Scenes[i].HasBeenStored == true)
					{
						XmlElement config = Helper.CreateElement(node, "Scene_" + i.ToString());

						node.AppendChild(config);

						Scenes[i].Save(config);
					}
				}
			}

			public void Store(int i)
			{
				ArtworkStaticObjects.Options.CopyTo(Scenes[i]);
			}

			public void Recall(int i)
			{
				if (Scenes[i].HasBeenStored == true)
				{
					ArtworkStaticObjects.Options.CopyFrom(Scenes[i]);
				}
			}
		}
		#endregion		

		#region Kinect Options

		public class KinectOptions
		{
			private int m_ElevationAngle = 0;
			public int ElevationAngle { get { return m_ElevationAngle; } set { m_ElevationAngle = value; } }

			private short m_NearClippingPlane = 0;
			public short NearClippingPlane { get { return m_NearClippingPlane; } set { m_NearClippingPlane = value; } }

			private short m_FarClippingPlane = 8000;
			public short FarClippingPlane { get { return m_FarClippingPlane; } set { m_FarClippingPlane = value; } }

			private double m_NoiseTolerance = 0;
			public double NoiseTolerance { get { return m_NoiseTolerance; } set { m_NoiseTolerance = value; } }		

			private short m_BackgroundCalibarationFrames = 1000;
			public short BackgroundCalibarationFrames { get { return m_BackgroundCalibarationFrames; } set { m_BackgroundCalibarationFrames = value; } }

			private Rectangle m_Bounds = new Rectangle(0, 0, 800, 400);
			public Rectangle Bounds { get { return m_Bounds; } set { m_Bounds = value; } } 

			public readonly List<KinectCameraData> Cameras = new List<KinectCameraData>();

			public KinectOptions()
			{
				Cameras.Add(new KinectCameraData());
				Cameras.Add(new KinectCameraData());
				Cameras.Add(new KinectCameraData());
				Cameras.Add(new KinectCameraData());
			}

			public void Load(XmlNode node)
			{				
				m_ElevationAngle = Helper.GetAttributeValue(node, "ElevationAngle", m_ElevationAngle);
				m_NearClippingPlane = Helper.GetAttributeValue(node, "NearClippingPlane", m_NearClippingPlane);
				m_FarClippingPlane = Helper.GetAttributeValue(node, "FarClippingPlane", m_FarClippingPlane);
				m_NoiseTolerance = Helper.GetAttributeValue(node, "NoiseTolerance", m_NoiseTolerance);
				m_BackgroundCalibarationFrames = Helper.GetAttributeValue(node, "BackgroundCalibarationFrames", m_BackgroundCalibarationFrames);
				m_Bounds = Helper.DeserializeRectangle(Helper.GetAttributeValue(node, "Bounds", Helper.SerializeRectangle(m_Bounds)));

				LoadCameras(node);
			}

			public void Save(XmlElement node)
			{				
				Helper.AppendAttributeAndValue(node, "ElevationAngle", m_ElevationAngle);
				Helper.AppendAttributeAndValue(node, "NearClippingPlane", m_NearClippingPlane);
				Helper.AppendAttributeAndValue(node, "FarClippingPlane", m_FarClippingPlane);
				Helper.AppendAttributeAndValue(node, "NoiseTolerance", m_NoiseTolerance);
				Helper.AppendAttributeAndValue(node, "BackgroundCalibarationFrames", m_BackgroundCalibarationFrames);
				Helper.AppendAttributeAndValue(node, "Bounds", Helper.SerializeRectangle(m_Bounds));

				SaveCameras(node); 
			}

			public void CopyTo(KinectOptions other)
			{
				//other.ElevationAngle = this.ElevationAngle;
				other.NearClippingPlane = this.m_NearClippingPlane;
				other.FarClippingPlane = this.m_FarClippingPlane;
				other.NoiseTolerance = this.m_NoiseTolerance; 
			}

			public void LoadCameras(XmlNode node)
			{
				for (int i = 0; i < Cameras.Count; i++)
				{
					XmlNode config = node.SelectSingleNode("Camera_" + i.ToString());
					if (config != null) Cameras[i].Load(config);
				}
			}

			public void SaveCameras(XmlElement node)
			{
				for (int i = 0; i < Cameras.Count; i++)
				{
					XmlElement config = Helper.CreateElement(node, "Camera_" + i.ToString());

					node.AppendChild(config);

					Cameras[i].Save(config);
				}
			}

			public class KinectCameraData
			{
				private float m_X = 0;
				private float m_Y = 0;

				public float X
				{
					get { return m_X; }
					set { m_X = value; }
				}

				public float Y
				{
					get { return m_Y; }
					set { m_Y = value; }
				}

				public void Load(XmlNode node)
				{
					m_X = Helper.GetAttributeValue(node, "X", m_X);
					m_Y = Helper.GetAttributeValue(node, "Y", m_Y);					
				}

				public void Save(XmlElement node)
				{
					Helper.AppendAttributeAndValue(node, "X", m_X.ToString());
					Helper.AppendAttributeAndValue(node, "Y", m_Y.ToString());					
				}
			}
		}

		#endregion

		#region Simulation Options

		public class SimulationOptions
		{			
			private int m_NumberOfParticles = 1;
			public int NumberOfParticles { get { return m_NumberOfParticles; } set { m_NumberOfParticles = value; } }

			private double m_ParticleScale = 1;
			public double ParticleScale { get { return m_ParticleScale; } set { m_ParticleScale = value; } }

			private double m_BerendsenThermostatCoupling = 1;
			public double BerendsenThermostatCoupling { get { return m_BerendsenThermostatCoupling; } set { m_BerendsenThermostatCoupling = value; } }

			private double m_EquilibriumTemperature = 3000;
			public double EquilibriumTemperature { get { return m_EquilibriumTemperature; } set { m_EquilibriumTemperature = value; } }

			private double m_GradientScaleFactor = 3000;
			public double GradientScaleFactor { get { return m_GradientScaleFactor; } set { m_GradientScaleFactor = value; } }

			private double m_PotentialEnergy = 0;
			public double PotentialEnergy { get { return m_PotentialEnergy; } set { m_PotentialEnergy = value; } }

			public readonly List<ParticleSetupData> ParticleTypes = new List<ParticleSetupData>();

			public SimulationOptions()
			{
				ParticleTypes.Add(new ParticleSetupData());
				ParticleTypes.Add(new ParticleSetupData());
				ParticleTypes.Add(new ParticleSetupData());
				ParticleTypes.Add(new ParticleSetupData());
				ParticleTypes.Add(new ParticleSetupData());
			}


			public void Load(XmlNode node)
			{
				m_NumberOfParticles = Helper.GetAttributeValue(node, "NumberOfParticles", m_NumberOfParticles);
				m_ParticleScale = Helper.GetAttributeValue(node, "ParticleScale", m_ParticleScale); 
				m_BerendsenThermostatCoupling = Helper.GetAttributeValue(node, "BerendsenThermostatCoupling", m_BerendsenThermostatCoupling);
				m_EquilibriumTemperature = Helper.GetAttributeValue(node, "EquilibriumTemperature", m_EquilibriumTemperature);
				m_GradientScaleFactor = Helper.GetAttributeValue(node, "GradientScaleFactor", m_GradientScaleFactor);
				m_PotentialEnergy = Helper.GetAttributeValue(node, "PotentialEnergy", m_PotentialEnergy);

				LoadParticleTypes(node);
			}

			public void Save(XmlElement node)
			{
				Helper.AppendAttributeAndValue(node, "NumberOfParticles", m_NumberOfParticles);
				Helper.AppendAttributeAndValue(node, "ParticleScale", m_ParticleScale);				
				Helper.AppendAttributeAndValue(node, "BerendsenThermostatCoupling", m_BerendsenThermostatCoupling);
				Helper.AppendAttributeAndValue(node, "EquilibriumTemperature", m_EquilibriumTemperature);
				Helper.AppendAttributeAndValue(node, "GradientScaleFactor", m_GradientScaleFactor);
				Helper.AppendAttributeAndValue(node, "PotentialEnergy", m_PotentialEnergy);

				SaveParticleTypes(node);
			}

			public void CopyTo(SimulationOptions other)
			{
				other.NumberOfParticles = this.m_NumberOfParticles;
				other.ParticleScale = this.m_ParticleScale;
				other.BerendsenThermostatCoupling = this.m_BerendsenThermostatCoupling;
				other.EquilibriumTemperature = this.m_EquilibriumTemperature;
				other.GradientScaleFactor = this.m_GradientScaleFactor;
				other.PotentialEnergy = this.m_PotentialEnergy;

				for (int i = 0; i < ParticleTypes.Count; i++)
				{
					ParticleTypes[i].CopyTo(other.ParticleTypes[i]); 
				}
			}


			public void LoadParticleTypes(XmlNode node)
			{
				for (int i = 0; i < ParticleTypes.Count; i++)
				{
					XmlNode config = node.SelectSingleNode("ParticleType_" + i.ToString());
					if (config != null) ParticleTypes[i].Load(config);
				}
			}

			public void SaveParticleTypes(XmlElement node)
			{
				for (int i = 0; i < ParticleTypes.Count; i++)
				{
					XmlElement config = Helper.CreateElement(node, "ParticleType_" + i.ToString());

					node.AppendChild(config);

					ParticleTypes[i].Save(config);
				}
			}


			public class ParticleSetupData
			{
				private bool m_AttractiveOrRepulsive = true;
				private bool m_IsSoundOn = true;
				private bool m_IsEnabled = true;

				public bool AttractiveOrRepulsive
				{
					get { return m_AttractiveOrRepulsive; }
					set { m_AttractiveOrRepulsive = value; }
				}

				public bool IsSoundOn
				{
					get { return m_IsSoundOn; }
					set { m_IsSoundOn = value; }
				}

				public bool IsEnabled
				{
					get { return m_IsEnabled; }
					set { m_IsEnabled = value; }
				} 

				public void Load(XmlNode node)
				{
					m_AttractiveOrRepulsive = Helper.GetAttributeValue(node, "AttractiveOrRepulsive", m_AttractiveOrRepulsive);
					m_IsSoundOn = Helper.GetAttributeValue(node, "IsSoundOn", m_IsSoundOn);
					m_IsEnabled = Helper.GetAttributeValue(node, "IsEnabled", m_IsEnabled);
				}

				public void Save(XmlElement node)
				{
					Helper.AppendAttributeAndValue(node, "AttractiveOrRepulsive", m_AttractiveOrRepulsive);
					Helper.AppendAttributeAndValue(node, "IsSoundOn", m_IsSoundOn);
					Helper.AppendAttributeAndValue(node, "IsEnabled", m_IsEnabled);
				}

				public void CopyTo(ParticleSetupData other)
				{
					other.AttractiveOrRepulsive = this.m_AttractiveOrRepulsive;
					other.IsSoundOn = this.m_IsSoundOn;
					other.IsEnabled = this.m_IsEnabled;
				}
			}
		}

		#endregion

		#region Visual Options

		public class VisualOptions
		{
			private float m_FeedbackLevel = 0.2f;
			public float FeedbackLevel { get { return m_FeedbackLevel; } set { m_FeedbackLevel = value; } }

			private float m_ParticleFeedbackLevel = 1f;
			public float ParticleFeedbackLevel { get { return m_ParticleFeedbackLevel; } set { m_ParticleFeedbackLevel = value; } }

			private float m_WarpPropagation = 0.2f;
			public float WarpPropagation { get { return m_WarpPropagation; } set { m_WarpPropagation = value; } }

			private float m_WarpVariance = 0.2f;
			public float WarpVariance { get { return m_WarpVariance; } set { m_WarpVariance = value; } }

			private float m_WarpPersistence = 0.2f;
			public float WarpPersistence { get { return m_WarpPersistence; } set { m_WarpPersistence = value; } }

			private float m_SelfImage = 0.2f;
			public float SelfImage { get { return m_SelfImage; } set { m_SelfImage = value; } }

			private float m_SelfFeedback = 0.2f;
			public float SelfFeedback { get { return m_SelfFeedback; } set { m_SelfFeedback = value; } }

			private float m_SelfColor = 0.5f;			
			public float SelfColor { get { return m_SelfColor; } set { m_SelfColor = value; } }

			private float m_SelfFeedbackColor = 0.5f;
			public float SelfFeedbackColor { get { return m_SelfFeedbackColor; } set { m_SelfFeedbackColor = value; } }

			public void Load(XmlNode node)
			{
				m_FeedbackLevel = Helper.GetAttributeValue(node, "FeedbackLevel", m_FeedbackLevel);
				m_ParticleFeedbackLevel = Helper.GetAttributeValue(node, "ParticleFeedbackLevel", m_ParticleFeedbackLevel);
				m_WarpPropagation = Helper.GetAttributeValue(node, "WarpPropagation", m_WarpPropagation);
				m_WarpVariance = Helper.GetAttributeValue(node, "WarpVariance", m_WarpVariance);
				m_WarpPersistence = Helper.GetAttributeValue(node, "WarpPersistence", m_WarpPersistence);
				m_SelfImage = Helper.GetAttributeValue(node, "SelfImage", m_SelfImage);
				m_SelfFeedback = Helper.GetAttributeValue(node, "SelfFeedback", m_SelfFeedback);
				m_SelfColor = Helper.GetAttributeValue(node, "SelfColor", m_SelfColor);
				m_SelfFeedbackColor = Helper.GetAttributeValue(node, "SelfFeedbackColor", m_SelfFeedbackColor);
			}

			public void Save(XmlElement node)
			{
				Helper.AppendAttributeAndValue(node, "FeedbackLevel", m_FeedbackLevel);
				Helper.AppendAttributeAndValue(node, "ParticleFeedbackLevel", m_ParticleFeedbackLevel);
				Helper.AppendAttributeAndValue(node, "WarpPropagation", m_WarpPropagation);
				Helper.AppendAttributeAndValue(node, "WarpVariance", m_WarpVariance);
				Helper.AppendAttributeAndValue(node, "WarpPersistence", m_WarpPersistence);
				Helper.AppendAttributeAndValue(node, "SelfImage", m_SelfImage);
				Helper.AppendAttributeAndValue(node, "SelfFeedback", m_SelfFeedback);
				Helper.AppendAttributeAndValue(node, "SelfColor", m_SelfColor);
				Helper.AppendAttributeAndValue(node, "SelfFeedbackColor", m_SelfFeedbackColor);

			}

			public void CopyTo(VisualOptions other)
			{
				other.FeedbackLevel = this.m_FeedbackLevel;
				other.ParticleFeedbackLevel = this.m_ParticleFeedbackLevel;
				other.WarpPropagation = this.m_WarpPropagation;
				other.WarpVariance = this.m_WarpVariance;
				other.WarpPersistence = this.m_WarpPersistence;
				other.SelfImage = this.m_SelfImage;
				other.SelfFeedback = this.m_SelfFeedback;
				other.SelfColor = this.m_SelfColor;
				other.SelfFeedbackColor = this.m_SelfFeedbackColor;
			}
		}

		#endregion

		#region Osc Options

		public class OscOptions
		{
			private float m_SpeedThreshold = 0.115f;
			private float m_DistanceThreshold = 2;
			private string m_Address = "10.24.1.160";
			private int m_Port = 5432;
			private int m_PortalID = 1;

			public float SpeedThreshold
			{
				get { return m_SpeedThreshold; }
				set { m_SpeedThreshold = value; }
			}

			public float DistanceThreshold
			{
				get { return m_DistanceThreshold; }
				set { m_DistanceThreshold = value; }
			}

			public string Address
			{
				get { return m_Address; }
				set { m_Address = value; }
			}

			public int Port
			{
				get { return m_Port; }
				set { m_Port = value; }
			}

			public int PortalID
			{
				get { return m_PortalID; }
				set { m_PortalID = value; }
			}

			public void Load(XmlNode node)
			{
				m_SpeedThreshold = Helper.GetAttributeValue(node, "SpeedThreshold", m_SpeedThreshold);
				m_DistanceThreshold = Helper.GetAttributeValue(node, "DistanceThreshold", m_DistanceThreshold);
				m_Address = Helper.GetAttributeValue(node, "Address", m_Address);
				m_Port = Helper.GetAttributeValue(node, "Port", m_Port);
				m_PortalID = Helper.GetAttributeValue(node, "PortalID", m_PortalID);
			}

			public void Save(XmlElement node)
			{
				Helper.AppendAttributeAndValue(node, "SpeedThreshold", m_SpeedThreshold.ToString());
				Helper.AppendAttributeAndValue(node, "DistanceThreshold", m_DistanceThreshold.ToString());
				Helper.AppendAttributeAndValue(node, "Address", m_Address);
				Helper.AppendAttributeAndValue(node, "Port", m_Port.ToString());
				Helper.AppendAttributeAndValue(node, "PortalID", m_PortalID.ToString());
			}

			public void CopyTo(OscOptions other)
			{

			}
		}

		#endregion

		#region FFT Options

		public class FFTOptions
		{
			/* 
			FFT 
			 - FFT Options (should be user controled) 
			 - FFT Freq (how often we do the FFT) 
			 - FFT Enabled
			 - Particle Events Enabled
			 - FFT (number of Frames) 
			 - How Often particle update there correlation function 
			 - Turn off FFT for some particle types
			*/

			private int m_FFTFrequency = 1; // once per frame
			private int m_CorrelationFunctionUpdateFrequency = 1; // once per frame
			private bool m_FFTEnabled = true;
			private bool m_ParticleEventsEnabled = true;

			private int m_PeakCount = 16;
			private int m_SendFFTFrequency = 10;

			public int FFTFrequency
			{
				get { return m_FFTFrequency; }
				set { m_FFTFrequency = value; }
			}

			public int CorrelationFunctionUpdateFrequency
			{
				get { return m_CorrelationFunctionUpdateFrequency; }
				set { m_CorrelationFunctionUpdateFrequency = value; }
			}


			public bool FFTEnabled
			{
				get { return m_FFTEnabled; }
				set { m_FFTEnabled = value; }
			}

			public bool ParticleEventsEnabled
			{
				get { return m_ParticleEventsEnabled; }
				set { m_ParticleEventsEnabled = value; }
			}

			public int PeakCount
			{
				get { return m_PeakCount; }
				set { m_PeakCount = value; }
			}

			public int SendFFTFrequency
			{
				get { return m_SendFFTFrequency; }
				set { m_SendFFTFrequency = value; }
			}


			public void Load(XmlNode node)
			{
				m_FFTFrequency = Helper.GetAttributeValue(node, "FFTFreqency", m_FFTFrequency);
				m_CorrelationFunctionUpdateFrequency = Helper.GetAttributeValue(node, "CorrelationFunctionUpdateFrequency", m_CorrelationFunctionUpdateFrequency);				
				m_FFTEnabled = Helper.GetAttributeValue(node, "FFTEnabled", m_FFTEnabled);
				m_ParticleEventsEnabled = Helper.GetAttributeValue(node, "ParticleEventsEnabled", m_ParticleEventsEnabled);

				m_PeakCount = Helper.GetAttributeValue(node, "PeakCount", m_PeakCount);
				m_SendFFTFrequency = Helper.GetAttributeValue(node, "SendFFTFrequency", m_SendFFTFrequency);
			}

			public void Save(XmlElement node)
			{
				Helper.AppendAttributeAndValue(node, "FFTFreqency", m_FFTFrequency.ToString());
				Helper.AppendAttributeAndValue(node, "CorrelationFunctionUpdateFrequency", m_CorrelationFunctionUpdateFrequency.ToString());				
				Helper.AppendAttributeAndValue(node, "FFTEnabled", m_FFTEnabled.ToString());
				Helper.AppendAttributeAndValue(node, "ParticleEventsEnabled", m_ParticleEventsEnabled.ToString());

				Helper.AppendAttributeAndValue(node, "PeakCount", m_PeakCount);
				Helper.AppendAttributeAndValue(node, "SendFFTFrequency", m_SendFFTFrequency);
			}

			public void CopyTo(FFTOptions other)
			{
				other.FFTFrequency = this.FFTFrequency;
				other.CorrelationFunctionUpdateFrequency = this.CorrelationFunctionUpdateFrequency;				
				other.FFTEnabled = this.FFTEnabled;
				other.ParticleEventsEnabled = this.ParticleEventsEnabled;

				other.PeakCount = this.m_PeakCount; 
				other.SendFFTFrequency = this.SendFFTFrequency;
			}
		}

		#endregion
	}
}
