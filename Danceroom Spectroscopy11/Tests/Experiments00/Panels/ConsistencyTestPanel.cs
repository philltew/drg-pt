using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.UI;
using RugTech1.Framework.Objects;
using SlimDX.Direct3D10;
using RugTech1.Framework.Objects.Simple;
using RugTech1;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.Text;
using RugTech1.Framework.Objects.UI.Menus;
using RugTech1.Framework.Data;
using SlimDX;
using System.Threading;

namespace Experiments.Panels
{
	public class ConsistencyTestPanel : PanelBase 
	{
		InnerTestScene m_TestScene; 

		public override SplashSceenPanels ScreenPanel
		{
			get { return SplashSceenPanels.Consistency; }
		}

		public ConsistencyTestPanel(VisiblePanelControler controler, int index)
			: base(controler, index)
		{

		}

		public override void Initiate()
		{
			int index = 1;

			this.Size = new System.Drawing.Size(400, 10); 

			Label label = new Label();
			label.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			label.FixedSize = true;
			label.Size = new System.Drawing.Size(this.Size.Width - 10, 30);
			label.Location = new System.Drawing.Point(0, 5);
			label.Text = "Consistency Tests";
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

			m_TestScene = new InnerTestScene();

			m_TestScene.Size = new System.Drawing.Size(this.Size.Width - 10, this.Size.Width - 10);
			m_TestScene.Location = new System.Drawing.Point(5, verticalOffset);
			m_TestScene.RelitiveZIndex = index++;
			m_TestScene.IsVisible = true;

			//m_TestScene.ControlStyle = DisplayMode.Normal;
			//m_TestScene.InteractionType = ControlInteractionType.None; 
			
			this.Controls.Add(m_TestScene);

			verticalOffset += this.Size.Width - 10;

			this.Size = new System.Drawing.Size(this.Size.Width, verticalOffset + 5);
		}

		public override void AddMenuItems(RugTech1.Framework.Objects.UI.Menus.MenuBar menu)
		{
			MenuBarItem controlsConfig = menu.AddItem("Consistency Tests");
			controlsConfig.IsVisible = true;
			controlsConfig.FontType = FontType.Small;

			MenuItem controlProperties = controlsConfig.AddItem("Consistency");
			controlProperties.IsVisible = true;
			controlProperties.FontType = FontType.Small;
			controlProperties.Click += new EventHandler(Consistency_Click);
		}

		public override void ResetControlValues()
		{
		
		}

		public override void UpdateControls()
		{
			m_TestScene.Update(); 
		}

		void Consistency_Click(object sender, EventArgs e)
		{
			PanelControler.SetOrToggleCurrentPanel(ScreenPanel);
		}

		void button_Click(object sender, EventArgs e)
		{
			m_TestScene.Running = !m_TestScene.Running;
		}

		public class InnerTestScene : UiSubScene, IResourceManager
		{
			private TestImage m_Test;
			private ParticleRender m_Particles;	

			private bool m_Running = false;
			private bool m_Reset = true;

			DSParticles.Tester test1 = new DSParticles.Tester(1024, 1024, new double[1024 * 1024]);
			DSParticles2.Tester test2 = new DSParticles2.Tester(1024, 1024, new double[1024 * 1024]);
			DSParticles3.Tester test3 = new DSParticles3.Tester(1024, 1024, new double[1024 * 1024]);

			int m_ParticleCount = 100; 

			StarInstanceVertex[] m_Points; 

			public bool Running { get { return m_Running; } set { m_Running = value; } }
			public bool Reset { get { return m_Reset; } set { m_Reset = value; } }
			
			public bool Disposed { get { return m_Test.Disposed; } }

			public InnerTestScene()
			{
				m_Test = new TestImage(Helper.ResolvePath("~/Assets/Test.png"));
				m_Particles = new ParticleRender(Helper.ResolvePath(@"~/Assets/star4-out.png"), m_ParticleCount * 3);

				m_Particles.ColorScale = 0.25f;
				m_Particles.ParticleScale = 0.05f;
				m_Particles.MinDistance = 0;
				m_Particles.MaxDistance = 1000;

				ControlStyle = DisplayMode.Normal;
				InteractionType = ControlInteractionType.None; 

			}

			public void Update()
			{
				if (Running == true)
				{
					if (Reset == true)
					{
						test1.SetupSingleFrame(m_ParticleCount);
						test2.SetupSingleFrame(m_ParticleCount);
						test3.SetupSingleFrame(m_ParticleCount);

						m_Points = new StarInstanceVertex[m_ParticleCount * 3]; 

						Reset = false;

						Thread.CurrentThread.Priority = ThreadPriority.Highest; 
					}

					bool Run_test1 = true;
					bool Run_test2 = true;
					bool Run_test3 = true;

					if (Run_test1)
					{
						test1.RunSingleFrame(3);
					}

					if (Run_test2)
					{
						test2.RunSingleFrame(3);
					}

					if (Run_test3)
					{
						test3.RunSingleFrame(3);
					}


					int j = 0;

					if (Run_test1)
					{
						for (int i = 0, ie = test1.Ensemble1.GetNumberOfParticles(); i < ie; i++)
						{
							DSParticles.Particle part = test1.Ensemble1.pParticleVector[i];

							m_Points[j++] = new StarInstanceVertex()
							{
								Color = new SlimDX.Vector4(1, 0, 0, 1),
								Position = new SlimDX.Vector3(((float)part.getpx() / 512f) - 1f, ((float)part.getpy() / 550f) - 1f, 2f) // (float)part.getRadius())
							};
						}
					}

					if (Run_test2)
					{
						/* for (int i = 0, ie = test2.Ensemble1.GetNumberOfParticles(); i < ie; i++)
						{
							DSParticlesOpt.Particle part = test2.Ensemble1.pParticleVector[i];

							m_Points[j++] = new StarInstanceVertex()
							{
								Color = new SlimDX.Vector4(0, 1, 0, 1),
								Position = new SlimDX.Vector3(((float)part.getpx() / 512f) - 1f, ((float)part.getpy() / 512f) - 1f, 2f) // (float)part.getRadius())
							};
						}
						 * */

						for (int i = 0, ie = test2.Ensemble1.GetNumberOfParticles(); i < ie; i++)
						{
							DSParticles2.Particle part = test2.Ensemble1.pParticleVector[i];

							m_Points[j++] = new StarInstanceVertex()
							{
								Color = new SlimDX.Vector4(0, 1, 0, 1),
								Position = new SlimDX.Vector3(((float)part.getpx() / 512f) - 1f, ((float)part.getpy() / 550f) - 1f, 2f) // (float)part.getRadius())
							};
						}
					}

					if (Run_test3)
					{
						/* 
						for (int i = 0, ie = test3.Ensemble1.NumberOfParticles; i < ie; i++)
						{
							DSParticlesOpt3.Particle part = test3.Ensemble1.Particles[i];

							m_Points[j++] = new StarInstanceVertex()
							{
								Color = new SlimDX.Vector4(0, 0, 1, 1),
								Position = new SlimDX.Vector3(((float)part.Position.X / 512f) - 1f, ((float)part.Position.Y / 550f) - 1f, 2f) // (float)part.Radius)
							};
						}
						 */

						for (int i = 0, ie = test3.Ensemble1.NumberOfParticles; i < ie; i++)
						{
							DSParticles3.Particle part = test3.Ensemble1.Particles[i];

							m_Points[j++] = new StarInstanceVertex()
							{
								Color = new SlimDX.Vector4(0, 0, 1, 1),
								Position = new SlimDX.Vector3(((float)part.Position.X / 512f) - 1f, ((float)part.Position.Y / 550f) - 1f, 2f) // (float)part.getRadius())
							};
						}
					}

					DataStream stream = m_Particles.Instances.Map(SlimDX.Direct3D10.MapMode.WriteDiscard, SlimDX.Direct3D10.MapFlags.None);

					stream.WriteRange<StarInstanceVertex>(m_Points, 0, j);

					m_Particles.InstanceCount = j;

					m_Particles.Instances.Unmap();					
				}
				else
				{
					Reset = true; 
				}
			}

			public override void Render(View3D view, Viewport viewport)
			{
				//m_Test.Render();

				m_Particles.Update(view);

				m_Particles.Render(view); 
			}

			#region ResourceManager Members

			public void LoadResources()
			{
				m_Test.LoadResources();
				m_Particles.LoadResources();
			}

			public void UnloadResources()
			{
				m_Test.UnloadResources();
				m_Particles.UnloadResources(); 
			}

			public void Dispose()
			{
				m_Test.Dispose();
				m_Particles.Dispose(); 
			}

			#endregion
		}
	}
}
