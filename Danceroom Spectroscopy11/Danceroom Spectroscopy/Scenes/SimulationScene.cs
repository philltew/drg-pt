using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects;
using RugTech1.Framework.Objects.Simple;
using DSParticles3;
using DS.Kinect;
using RugTech1.Framework.Data;
using SlimDX;
using DS.Panels;
using RugTech1.Framework.Objects.UI;
using System.Drawing;
using RugTech1.Framework;
using DS.Simulation;
using DS.Objects;
using SlimDX.Direct3D11;
using RugTech1.Framework.Effects;
using SlimDX.DXGI;
using DS.Audio;
using RugTech1.Framework.Contextual;

namespace DS.Scenes
{
	class SimulationScene : IScene, IResourceManager
	{
		private bool m_Disposed = true;
		private ParticleRender2 m_Particles;
		private int m_DrawFrequency = 1;
		private StarInstanceVertex[] m_Points;
		private CompositeFieldImageCS m_Composite;
		private WarpGrid m_WarpGrid;

		private Imposter[] m_FeedbackImposters = new Imposter[2];
		private int m_ActiveImposter = 0;

		private FeedbackScene m_FeedbackScene;
		private int m_ParticleCorrelationFunctionTime;
		private int m_FFTFrequencyTime;

		private RenderContext m_RenderContext;
		private ImageBox m_TargetBox;

		//private float m_HeightScale = 1.4f; 

		// private int m
		public SimulationScene()
		{
            m_Particles = new ParticleRender2(RugTech1.Helper.ResolvePath("~/Assets/particle2.png"), ArtworkStaticObjects.Ensemble.MaxNumberOfParticles);
			m_Particles.ColorScale = 1f;
			m_Particles.ParticleScale = 0.01f;
			m_Particles.MinDistance = 0;
			m_Particles.MaxDistance = 1000;
			m_Points = new StarInstanceVertex[m_Particles.MaxCount]; 
			m_Composite = new CompositeFieldImageCS();
			m_WarpGrid = new WarpGrid();
			m_WarpGrid.FeedbackLevel = 0.99f;

			m_FeedbackImposters[0] = new Imposter(GameConfiguration.WindowWidth, GameConfiguration.WindowHeight, Format.R16G16B16A16_Float, new Color4(0f, 0f, 0f, 0f), ImposterOverlayType.Add);
			m_FeedbackImposters[1] = new Imposter(GameConfiguration.WindowWidth, GameConfiguration.WindowHeight, Format.R16G16B16A16_Float, new Color4(0f, 0f, 0f, 0f), ImposterOverlayType.Add);

			m_FeedbackScene = new FeedbackScene();
			m_FeedbackScene.Grid = m_WarpGrid;
			m_FeedbackScene.Particles = m_Particles;
			m_FeedbackScene.Composite = m_Composite;

			m_RenderContext = new RenderContext(ArtworkStaticObjects.CompositeFieldImage.Width, ArtworkStaticObjects.CompositeFieldImage.Height);

			m_TargetBox = new ImageBox(null);
		}

		public bool NeedsResize()
		{
			float width = (float)ArtworkStaticObjects.CompositeFieldImage.Width;
			float height = (float)ArtworkStaticObjects.CompositeFieldImage.Height;

			float scale = (float)GameConfiguration.WindowWidth / width;

			int bufferWidth = (int)(width * scale);
			int bufferHeight = (int)(height * scale);

			if (m_Composite.CompositeFieldImage.Width != ArtworkStaticObjects.CompositeFieldImage.Width ||
				m_Composite.CompositeFieldImage.Height != ArtworkStaticObjects.CompositeFieldImage.Height || 
				m_RenderContext.Width != bufferWidth || m_RenderContext.Height != bufferHeight)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		#region IScene Members

		public void Render(View3D view)
		{
			
		}

		public void Render(FormContext formContext, View3D view)
		{
			if (NeedsResize() == true)
			{
				UnloadResources();
				LoadResources(); 
			}

			m_Composite.Update();

			float width = (float)ArtworkStaticObjects.CompositeFieldImage.Width;
			float height = (float)ArtworkStaticObjects.CompositeFieldImage.Height;

			float scaleX = (2f / width);
			float scaleY = (2f / height); 
			float scale = (2f / width);
			float offsetY = (height * scale) / 2f;

			m_Particles.ParticleScaleX = 1f;
			m_Particles.ParticleScaleY = view.WindowSize.X / view.WindowSize.Y; //  (width / height);// *(1 - scale); //; * m_HeightScale;
			 
			if (ArtworkStaticObjects.CompositeFieldImage.Update()) { }

			m_WarpGrid.Update(ArtworkStaticObjects.CompositeFieldImage, ArtworkStaticObjects.CompositeFieldImage.Width, ArtworkStaticObjects.CompositeFieldImage.Height);

			//m_WarpGrid.Propogate(GameConfiguration.WindowWidth, GameConfiguration.WindowHeight);

			ArtworkStaticObjects.Ensemble.ResetParticleCollisions();

			for (int j = 0; j < m_DrawFrequency; j++)
			{
				ArtworkStaticObjects.Ensemble.VelocityVerletPropagation(ArtworkStaticObjects.ExternalField);
			}

			#region Do FFT

			if (ArtworkStaticObjects.Options.FFT.FFTEnabled == true)
			{
				m_ParticleCorrelationFunctionTime++;
				m_FFTFrequencyTime++;

				if (m_ParticleCorrelationFunctionTime >= ArtworkStaticObjects.Options.FFT.CorrelationFunctionUpdateFrequency)
				{
					ArtworkStaticObjects.Ensemble.UpdateVelocityAutoCorrelationFunction();

					m_ParticleCorrelationFunctionTime = 0;
				}

				if (m_FFTFrequencyTime >= ArtworkStaticObjects.Options.FFT.FFTFrequency)
				{
					ArtworkStaticObjects.Ensemble.FFTVelocityAutoCorrelationFunction();

					m_FFTFrequencyTime = 0;

					ArtworkStaticObjects.FFTScanner.ShouldScan = true; 
				}
			}

			#endregion

			m_RenderContext.RenderBegin(); 

			//m_Composite.Rectangle = new RectangleF(-1, -offsetY, 2, height * scale);
			m_Composite.Rectangle = new RectangleF(-1, -1, 2, 2);			

			//m_WarpGrid.Render(m_Composite.TextureView); 
			OscOutput output = ArtworkStaticObjects.OscControler; 
			bool shouldSendParticleEvents = ArtworkStaticObjects.Options.FFT.ParticleEventsEnabled;

			for (int i = 0, ie = ArtworkStaticObjects.Ensemble.NumberOfParticles; i < ie; i++)
			{
				DSParticles3.Particle part = ArtworkStaticObjects.Ensemble.Particles[i];

				float partSpeed = 1f;

				if ((part.Velocity.X < 10f && part.Velocity.X > -10f) &&
					(part.Velocity.Y < 10f && part.Velocity.Y > -10f))
				{
					SlimDX.Vector2 length = new Vector2((float)part.Velocity.X, (float)part.Velocity.Y);

					float speedSquared = length.LengthSquared();

					if (speedSquared == float.NegativeInfinity || speedSquared <= 0)
					{
						partSpeed = 0; 
					}
					else if (speedSquared < 100f)
					{
						partSpeed = speedSquared * 0.01f; 
					}					
				}

				SlimDX.Vector4 color;

				if (part.DidParticleCollideWithParticle() == true)
				{
					color = part.ParticleType.ParticleCollisionColor;

					if (shouldSendParticleEvents == true && part.ParticleType.IsSoundOn == true)
					{
						output.SendPacket((float)part.Position.X, (float)part.Position.Y, (float)part.ParticleType.ID, (float)part.VInCollisionFrame);
					}
				}
				else if (part.DidParticleCollideWithWall() == true)
				{
					color = part.ParticleType.WallCollisionColor;
					
					if (shouldSendParticleEvents == true && part.ParticleType.IsSoundOn == true)
					{
						output.SendPacket((float)part.Position.X, (float)part.Position.Y, (float)part.ParticleType.ID, (float)part.VInCollisionFrame);
					}
				}
				else
				{
					color = part.ParticleType.RenderColor;
				}

				m_Points[i].Color = color;
				//m_Points[i].Position = new SlimDX.Vector3(-1 + ((float)part.Position.X * scale), -offsetY + ((height - (float)part.Position.Y) * scale), 1f);
				m_Points[i].Position = new SlimDX.Vector3(-1 + ((float)part.Position.X * scaleX), -1 + ((height - (float)part.Position.Y) * scaleY), partSpeed); 
			}


			DataBox box = GameEnvironment.Device.ImmediateContext.MapSubresource(m_Particles.Instances, SlimDX.Direct3D11.MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None);

			DataStream stream = box.Data;

			stream.WriteRange<StarInstanceVertex>(m_Points, 0, ArtworkStaticObjects.Ensemble.NumberOfParticles);

			m_Particles.InstanceCount = ArtworkStaticObjects.Ensemble.NumberOfParticles;

			GameEnvironment.Device.ImmediateContext.UnmapSubresource(m_Particles.Instances, 0); 

			m_Particles.Update(view);
			m_Particles.ColorScale = ArtworkStaticObjects.Options.Visual.ParticleFeedbackLevel;
			m_Particles.ScaleDistance = 0f;

			m_WarpGrid.WarpVariance = ArtworkStaticObjects.Options.Visual.WarpVariance; 
			m_WarpGrid.WarpPropagation = ArtworkStaticObjects.Options.Visual.WarpPropagation; 
			m_WarpGrid.WarpPersistence = ArtworkStaticObjects.Options.Visual.WarpPersistence; 
			m_WarpGrid.FeedbackLevel = ArtworkStaticObjects.Options.Visual.FeedbackLevel; 

			m_FeedbackScene.CurrentFeedbackTexture = m_FeedbackImposters[m_ActiveImposter].TextureView;
			
			m_ActiveImposter++; 
			if (m_ActiveImposter > 1)
			{
				m_ActiveImposter = 0;
			}
			
			m_FeedbackImposters[m_ActiveImposter].RenderToImposter(m_FeedbackScene, view);
			m_FeedbackImposters[m_ActiveImposter].Render();

			m_Composite.Render(view, ArtworkStaticObjects.Options.Visual.SelfImage, ArtworkStaticObjects.Options.Visual.SelfColor);

			m_Particles.ScaleDistance = 1f;
			m_Particles.ColorScale = 1f - ArtworkStaticObjects.Options.Visual.ParticleFeedbackLevel;
			m_Particles.Render(view); 

			m_RenderContext.RenderEnd(); 

			m_TargetBox.TextureView = m_RenderContext.TextureView;


			formContext.RenderBegin();

			//m_TargetBox.Rectangle = new RectangleF(-1, -offsetY * m_HeightScale, 2, height * scale * m_HeightScale);
            m_TargetBox.Rectangle = new RectangleF(-1, -1, 2, 2);
			//m_TargetBox.FlipHorizontal = true; 
			m_TargetBox.Render(); 
		}

		#endregion

		#region IResourceManager Members

		public bool Disposed
		{
			get { return m_Disposed; }
		}

		public void LoadResources()
		{
			if (m_Disposed == true)
			{
				float width = (float)ArtworkStaticObjects.CompositeFieldImage.Width;
				float height = (float)ArtworkStaticObjects.CompositeFieldImage.Height;

				float scale = (float)GameConfiguration.WindowWidth / width;

				int bufferWidth = (int)(width * scale);
				int bufferHeight = (int)(height * scale);

				//float scale = (2f / width);
				//float offsetY = (height * scale) / 2f;

				foreach (Imposter imp in m_FeedbackImposters)
				{
					imp.Resize(bufferWidth, bufferHeight);
					imp.LoadResources(); 					
				}

				m_Particles.LoadResources();

				m_Composite.Resize(ArtworkStaticObjects.CompositeFieldImage.Width, ArtworkStaticObjects.CompositeFieldImage.Height);
				m_Composite.LoadResources();

				m_WarpGrid.Setup((int)width, (int)height, 8, 1f);
				m_WarpGrid.LoadResources();
				//m_WarpGrid.WarpVariance = 0.02f;
				m_WarpGrid.Randomise((int)width, (int)height);

				m_RenderContext.Resize(bufferWidth, bufferHeight); 
				m_RenderContext.LoadResources();

				m_TargetBox.LoadResources(); 

				m_Disposed = false;
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				foreach (Imposter imp in m_FeedbackImposters)
				{
					imp.UnloadResources();
				}

				m_Particles.UnloadResources();
				m_Composite.UnloadResources();				

				m_WarpGrid.UnloadResources();

				m_RenderContext.UnloadResources();
				
				m_TargetBox.UnloadResources(); 

				m_Disposed = true;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			UnloadResources();

			m_Composite.Dispose(); 
		}

		#endregion

		class FeedbackScene : IScene
		{
			public ShaderResourceView CurrentFeedbackTexture;
			public WarpGrid Grid;
			public ParticleRender2 Particles;
			public CompositeFieldImageCS Composite;
			//public Imposter Feedback;

			#region IScene Members

			public void Render(View3D view)
			{
				Grid.Render(CurrentFeedbackTexture);
				Composite.Render(view, ArtworkStaticObjects.Options.Visual.SelfFeedback, ArtworkStaticObjects.Options.Visual.SelfFeedbackColor); 
				//Feedback.Render(); 
				Particles.Render(view); 
			}

			#endregion
		}
	}
}
