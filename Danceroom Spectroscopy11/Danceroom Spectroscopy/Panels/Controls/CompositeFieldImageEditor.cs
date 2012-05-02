using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects.UI;
using RugTech1.Framework.Objects;
using DS.Kinect;
using System.Drawing;
using SlimDX.Direct3D11;

namespace DS.Panels
{
	class CompositeFieldImageEditor : UiSubScene, IResourceManager
	{
		public static List<Color> DefaultColors = new List<Color>(new Color[] { Color.Green, Color.Blue, Color.Red, Color.Purple });

		private bool m_Disposed = true;
		private List<KinectFieldImageTexture> m_FieldImages = new List<KinectFieldImageTexture>();
		private OverlayScene m_Scene; 

		private KinectFieldImageType m_FieldImageMode = KinectFieldImageType.BlendMap;


		public KinectFieldImageType FieldImageMode
		{
			get { return m_FieldImageMode; }
			set { m_FieldImageMode = value; }			
		}

		public bool RebuildImages { get { return m_RebuildImages; } set { m_RebuildImages = value; } } 

		private View3D m_View;
		private bool m_RebuildImages;

		public bool Disposed { get { return m_Disposed; } }

		public CompositeFieldImageEditor()
		{
			ControlStyle = DisplayMode.Normal;

			InteractionType = ControlInteractionType.None;

			m_View = new View3D(new System.Drawing.Rectangle(0, 0, 1, 1), 8, 8, (float)Math.PI / 4, 1f);

			int i = 0; 
			foreach (DS.Simulation.CompositeFieldImage.KinectFieldImage image in ArtworkStaticObjects.CompositeFieldImage.Images)
			{
				KinectFieldImageTexture texture = new KinectFieldImageTexture(image, DefaultColors[i++], KinectFieldImageType.BlendMap);
				texture.FlipVertical = true; 

				m_FieldImages.Add(texture); 
			}
			
			m_Scene = new OverlayScene();
			m_Scene.Initialize();
		}

		public void Update()
		{
			if (m_RebuildImages == true)
			{
				ArtworkStaticObjects.CompositeFieldImage.RecalculateComposite();

				m_RebuildImages = false; 
			}
			
			ArtworkStaticObjects.CompositeFieldImage.Update(); 
		}

		public void UpdateRegionBounds()
		{
			m_Scene.UpdateMasterRegion();
		}

		public override void Render(View3D view, Viewport viewport)
		{
			m_View.Resize(new System.Drawing.Rectangle(0, 0, (int)viewport.Width, (int)viewport.Height), (int)viewport.Width, (int)viewport.Height);

			float x, y, w, h;

			foreach (KinectFieldImageTexture image in m_FieldImages)
			{
				image.ImageType = m_FieldImageMode;

				UiStyleHelper.CovertToVertCoords(new RectangleF(image.Source.X, image.Source.Y, image.Source.Stride, image.Source.Height), m_View.WindowSize, m_View.PixelSize, out x, out y, out w, out h);

				image.Rectangle = new RectangleF(x, y, w, h);

				image.Update();

				image.Render(m_View);
			}

			m_Scene.Update(); 
			m_Scene.Render(m_View); 
		}

		#region ResourceManager Members

		public void LoadResources()
		{
			if (m_Disposed == true)
			{
				foreach (KinectFieldImageTexture image in m_FieldImages)
				{
					image.LoadResources();
				}

				m_Scene.LoadResources();

				m_Disposed = false;
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				foreach (KinectFieldImageTexture image in m_FieldImages)
				{
					image.UnloadResources();
				}

				m_Scene.UnloadResources();

				m_Disposed = true;
			}
		}

		public void Dispose()
		{
			UnloadResources();

			foreach (KinectFieldImageTexture image in m_FieldImages)
			{
				image.Dispose();
			}

			m_Scene.Dispose();
		}

		#endregion


		public override void UnfocusControls()
		{
			m_Scene.UnfocusControls();
		}

		public override void UnhoverControls()
		{
			m_Scene.UnhoverControls();
		}

		public override void OnMouseDown(View3D view, SlimDX.Vector2 mousePosition, System.Windows.Forms.MouseButtons mouseButtons, out bool shouldSubUpdate)
		{
			m_Scene.OnMouseDown(m_View, mousePosition - MouseOffset, mouseButtons, out shouldSubUpdate);

			if (shouldSubUpdate)
			{
				m_Scene.Invalidate();
			}
		}

		public override void OnMouseUp(View3D view, SlimDX.Vector2 mousePosition, System.Windows.Forms.MouseButtons mouseButtons, out bool shouldSubUpdate)
		{
			m_Scene.OnMouseUp(m_View, mousePosition - MouseOffset, mouseButtons, out shouldSubUpdate);

			if (shouldSubUpdate)
			{
				m_Scene.Invalidate();
			}
		}

		public override void OnMouseMoved(View3D view, SlimDX.Vector2 mousePosition, out bool shouldSubUpdate)
		{
			m_Scene.OnMouseMoved(m_View, mousePosition - MouseOffset, out shouldSubUpdate);

			if (shouldSubUpdate)
			{
				m_Scene.Invalidate();
			}
		}

		public override void OnKeyPress(char @char, out bool shouldUpdate)
		{
			m_Scene.OnKeyPress(@char, out shouldUpdate);

			if (shouldUpdate)
			{
				m_Scene.Invalidate();
			}
		}

		public override void OnKeyDown(System.Windows.Forms.KeyEventArgs args, out bool shouldUpdate)
		{
			m_Scene.OnKeyDown(args, out shouldUpdate);

			if (shouldUpdate)
			{
				m_Scene.Invalidate();
			}
		}

		public override void OnKeyUp(System.Windows.Forms.KeyEventArgs args, out bool shouldUpdate)
		{
			m_Scene.OnKeyUp(args, out shouldUpdate);

			if (shouldUpdate)
			{
				m_Scene.Invalidate();
			}
		}
		
		class OverlayScene : UiScene
		{
			NonDragableRegion m_MasterRegion; 

			List<DragableRegion> m_Regions = new List<DragableRegion>(); 

			protected override void InitializeControls()
			{
				base.InitializeControls();
				
				m_MasterRegion = new NonDragableRegion();
				m_MasterRegion.Location = new Point(ArtworkStaticObjects.CompositeFieldImage.Bounds.X, ArtworkStaticObjects.CompositeFieldImage.Bounds.Y);
				m_MasterRegion.Size = new Size(ArtworkStaticObjects.CompositeFieldImage.Bounds.Width, ArtworkStaticObjects.CompositeFieldImage.Bounds.Height);
				m_MasterRegion.RelitiveZIndex = 902;
				this.Controls.Add(m_MasterRegion); 

				int i = 1; 

				foreach (DS.Simulation.CompositeFieldImage.KinectFieldImage image in ArtworkStaticObjects.CompositeFieldImage.Images)
				{
					DragableRegion region = new DragableRegion(); 

					region.Text = i.ToString();
					region.ShowBackground = false; 
					region.Size = new Size(image.Width, image.Height);
					region.Location = new Point((int)image.X, (int)image.Y);
					region.RelitiveZIndex = 900;
					region.Moving += new EventHandler(region_Moving);
					region.MoveEnded += new EventHandler(region_MoveEnded);
					region.Tag = image;
						 
					m_Regions.Add(region);
					
					this.Controls.Add(region); 

					i++; 
				}					
			}

			public void UpdateMasterRegion()
			{
				m_MasterRegion.Location = new Point(ArtworkStaticObjects.CompositeFieldImage.Bounds.X, ArtworkStaticObjects.CompositeFieldImage.Bounds.Y);
				m_MasterRegion.Size = new Size(ArtworkStaticObjects.CompositeFieldImage.Bounds.Width, ArtworkStaticObjects.CompositeFieldImage.Bounds.Height);

				Invalidate(); 
			}

			void region_MoveEnded(object sender, EventArgs e)
			{
				DragableRegion region = sender as DragableRegion;

				DS.Simulation.CompositeFieldImage.KinectFieldImage image = region.Tag as DS.Simulation.CompositeFieldImage.KinectFieldImage;

				image.X = region.Location.X;
				image.Y = region.Location.Y;

				ArtworkStaticObjects.CompositeFieldImage.RecalculateComposite();
			}

			void region_Moving(object sender, EventArgs e)
			{
				DragableRegion region = sender as DragableRegion;

				DS.Simulation.CompositeFieldImage.KinectFieldImage image = region.Tag as DS.Simulation.CompositeFieldImage.KinectFieldImage;

				image.X = region.Location.X;
				image.Y = region.Location.Y;

				ArtworkStaticObjects.CompositeFieldImage.RecalculateComposite();
			}
		}
	}
}
