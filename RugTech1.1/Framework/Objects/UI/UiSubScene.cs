using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;

namespace RugTech1.Framework.Objects.UI
{
	public abstract class UiSubScene : UiControlBase, IScene
	{
		private Vector2 m_MouseOffset;
		private Viewport m_InnerViewport;

		public Vector2 MouseOffset { get { return m_MouseOffset; } }

		public Viewport InnerViewport 
		{ 
			get { return m_InnerViewport; }
			protected set 
			{ 
				m_InnerViewport = value;

				m_MouseOffset = new Vector2(m_InnerViewport.X, m_InnerViewport.Y);
			}
		}

		public UiSubScene()
		{
			m_InnerViewport = new Viewport(0, 0, 1, 1);
			m_MouseOffset = new Vector2(m_InnerViewport.X, m_InnerViewport.Y);
		}

		public override void WriteVisibleElements(View3D view,
													RectangleF ClientBounds, ref RectangleF RemainingBounds,
													SlimDX.DataStream LineVerts, ref int LineVertsCount,
													SlimDX.DataStream LinesIndices, ref int LinesIndicesCount,
													SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount,
													SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			base.WriteVisibleElements(view, ClientBounds, ref RemainingBounds,
									LineVerts, ref LineVertsCount,
									LinesIndices, ref LinesIndicesCount,
									TriangleVerts, ref TriangleVertsCount,
									TriangleIndices, ref TriangleIndicesCount);

			m_InnerViewport.X = (int)Bounds.X - (int)view.ViewportOffset.X;
			m_InnerViewport.Y = (int)Bounds.Y - (int)view.ViewportOffset.Y;
			m_InnerViewport.Width = (int)Bounds.Width - 1; 
			m_InnerViewport.Height = (int)Bounds.Height - 1;
			m_MouseOffset = new Vector2(m_InnerViewport.X, m_InnerViewport.Y);
		}

		public virtual void Render(View3D view)
		{
			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext;

			Viewport[] ViewportsBackup = context.Rasterizer.GetViewports();

			context.Rasterizer.SetViewports(m_InnerViewport);

			try
			{
				Render(view, m_InnerViewport);
			}
			finally
			{
				context.Rasterizer.SetViewports(ViewportsBackup);
			}
		}

		public abstract void Render(View3D view, Viewport viewport);

		public abstract void UnfocusControls();

		public abstract void UnhoverControls();

		public abstract void OnMouseDown(View3D view, SlimDX.Vector2 mousePosition, System.Windows.Forms.MouseButtons mouseButtons, out bool shouldSubUpdate);

		public abstract void OnMouseUp(View3D view, SlimDX.Vector2 mousePosition, System.Windows.Forms.MouseButtons mouseButtons, out bool shouldSubUpdate);

		public abstract void OnMouseMoved(View3D view, SlimDX.Vector2 mousePosition, out bool shouldSubUpdate);

		public abstract void OnKeyDown(System.Windows.Forms.KeyEventArgs args, out bool shouldUpdate);

		public abstract void OnKeyUp(System.Windows.Forms.KeyEventArgs args, out bool shouldUpdate);

		public abstract void OnKeyPress(char @char, out bool shouldUpdate);

	}
}
