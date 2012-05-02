using System;
using System.Collections.Generic;
//using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Text;
using RugTech1.Framework.Effects;
using RugTech1.Framework.Objects.Text;
using RugTech1.Framework.Objects.UI.Controls;
using RugTech1.Framework.Objects.UI.Dynamic;
using RugTech1.Framework.Objects.UI.Menus;
using SlimDX;
using SlimDX.Direct3D11;

namespace RugTech1.Framework.Objects.UI
{
	public enum MouseState { None, Moving, Hovering, ClickStart, ClickEnd, DragStart, Dragging, DragEnd }
	public enum KeyInteractionType { KeyPress, KeyUp, KeyDown }

	public class UiScene : IScene, IResourceManager
	{
		public static UiEffect Effect;

		#region Private Members
	
		private bool m_Disposed = true;
		private UiSceneBuffer m_StaticBuffer;
		private UiSceneBuffer m_DynamicBuffer;

		private UiControl m_Hovered = null;
		private UiControl m_Focused = null;
		private UiControl m_Control_ClickStart = null;

		private MouseState m_MouseState = MouseState.None;
		private System.Windows.Forms.MouseButtons m_MouseButtons = System.Windows.Forms.MouseButtons.None;
		private bool m_HasBeenInvalidated = false;

		#endregion

		#region Public Members and Properties

		public readonly UiSceneControlCollection Controls;
		public readonly List<IDynamicUiControl> DynamicControls = new List<IDynamicUiControl>();
		public readonly List<UiSubScene> SubScenes = new List<UiSubScene>();
		public readonly List<IResourceManager> ResourceManagers = new List<IResourceManager>();
		
		public bool HasBeenInvalidated
		{
			get { return m_HasBeenInvalidated; }		
		}

		#endregion

		public UiScene()
		{
			if (Effect == null)
			{
				Effect = SharedEffects.Effects["UI"] as UiEffect;
			}

			Controls = new UiSceneControlCollection(this);

			m_StaticBuffer = new UiSceneBuffer();
			m_DynamicBuffer = new UiSceneBuffer();

		}

		public void Initialize()
		{
			InitializeControls();

			Controls.AttachDynamicControls();

			m_HasBeenInvalidated = true; 
		}

		public void Invalidate()
		{
			m_HasBeenInvalidated = true;
		}

		#region Methods To Override in Derived Classes
		
		protected virtual void InitializeControls()
		{
			
		}

		public virtual void Update()
		{

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
				int LineVerts = 0;
				int LinesIndices = 0;
				int TriangleVerts = 0;
				int TriangleIndices = 0;

				foreach (UiControl control in Controls)
				{
					control.GetTotalElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);
				}

				m_StaticBuffer.Resize(LineVerts, LinesIndices, TriangleVerts, TriangleIndices);

				m_StaticBuffer.LoadResources();

				LineVerts = 0;
				LinesIndices = 0;
				TriangleVerts = 0;
				TriangleIndices = 0;

				foreach (IDynamicUiControl control in DynamicControls)
				{
					control.GetTotalDynamicElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);
				}

				m_DynamicBuffer.Resize(LineVerts, LinesIndices, TriangleVerts, TriangleIndices);

				m_DynamicBuffer.LoadResources();

				foreach (IResourceManager manager in ResourceManagers)
				{
					manager.LoadResources(); 
				}
				
				m_Disposed = false;

				Invalidate();
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				m_StaticBuffer.UnloadResources();
				m_DynamicBuffer.UnloadResources();

				foreach (IResourceManager manager in ResourceManagers)
				{
					manager.UnloadResources();
				}

				m_Disposed = true;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			UnloadResources(); 
		}

		#endregion

		#region Render Methods

		public void Render(View3D view)
		{					
			if (HasBeenInvalidated)
			{
				UpdateStaticBuffers(view);

				m_HasBeenInvalidated = false;
			}

			Effect.Render(m_StaticBuffer);

			UpdateBuffers(view);

			Effect.Render(m_DynamicBuffer);

			foreach (UiSubScene scene in SubScenes)
			{
				if (scene.IsVisible == false)
				{
					continue;
				}

				if (scene.IsParentVisible == false)
				{
					continue;
				}

				scene.Render(view);
			}
		}

		#endregion

		#region Update

		protected void UpdateBuffers(View3D view)
		{
			if (m_Disposed == true)
			{
				return;
			}

			int LineVerts = 0;
			int LinesIndices = 0;
			int TriangleVerts = 0;
			int TriangleIndices = 0;

			DataStream LineVertsStream;
			DataStream LinesIndicesStream;
			DataStream TriangleVertsStream;
			DataStream TriangleIndicesStream;

			try
			{
				m_DynamicBuffer.MapStreams(MapMode.WriteDiscard, out LineVertsStream, out LinesIndicesStream, out TriangleVertsStream, out TriangleIndicesStream);

				foreach (IDynamicUiControl control in DynamicControls)
				{
					if (control.IsVisible == false)
					{
						continue; 
					}

					if (control.IsParentVisible == false)
					{
						continue;
					}

					control.WriteDynamicElements(view, LineVertsStream, ref LineVerts, LinesIndicesStream, ref LinesIndices, TriangleVertsStream, ref TriangleVerts, TriangleIndicesStream, ref TriangleIndices);
				}
			}
			finally
			{
				m_DynamicBuffer.UnmapStreams(LineVerts, LinesIndices, TriangleVerts, TriangleIndices);
			}

		}

		protected void UpdateStaticBuffers(View3D view)
		{
			if (m_Disposed == true)
			{
				return;
			}

			int LineVerts = 0;
			int LinesIndices = 0;
			int TriangleVerts = 0;
			int TriangleIndices = 0;

			DataStream LineVertsStream;
			DataStream LinesIndicesStream;
			DataStream TriangleVertsStream;
			DataStream TriangleIndicesStream;

			try
			{
				foreach (UiControl control in Controls)
				{
					control.GetTotalElementCounts(ref LineVerts, ref LinesIndices, ref TriangleVerts, ref TriangleIndices);
				}

				m_StaticBuffer.Resize(LineVerts, LinesIndices, TriangleVerts, TriangleIndices);

				LineVerts = 0;
				LinesIndices = 0;
				TriangleVerts = 0;
				TriangleIndices = 0;

				m_StaticBuffer.MapStreams(MapMode.WriteDiscard, out LineVertsStream, out LinesIndicesStream, out TriangleVertsStream, out TriangleIndicesStream);

				RectangleF rect = new RectangleF(view.Viewport.X + view.ViewportOffset.X, view.Viewport.Y + view.ViewportOffset.Y, view.Viewport.Width, view.Viewport.Height);
				RectangleF clientBounds = rect;

				foreach (UiControl control in Controls)
				{
					if (control.IsVisible == true)
					{
						control.WriteVisibleElements(view, clientBounds, ref rect, LineVertsStream, ref LineVerts, LinesIndicesStream, ref LinesIndices, TriangleVertsStream, ref TriangleVerts, TriangleIndicesStream, ref TriangleIndices);
					}
				}
			}
			finally
			{
				m_StaticBuffer.UnmapStreams(LineVerts, LinesIndices, TriangleVerts, TriangleIndices);
			}
		}

		#endregion

		public void UnfocusControls()
		{
			if (m_Focused != null)
			{
				m_Focused.EndPick(new Vector2(-1, -1), PickType.UnFocus, null);

				if (m_Focused is UiSubScene)
				{
					(m_Focused as UiSubScene).UnfocusControls(); 
				}

				m_Focused = null;

				m_MouseState = MouseState.None;
				
				m_MouseButtons = System.Windows.Forms.MouseButtons.None;

				m_HasBeenInvalidated = true; 
			}
		}

		public void UnhoverControls()
		{
			if (m_Hovered != null)
			{
				m_Hovered.EndPick(new Vector2(-1, -1), PickType.UnHover, null);

				if (m_Hovered is UiSubScene)
				{
					(m_Hovered as UiSubScene).UnhoverControls();
				}

				m_Hovered = null;

				m_MouseState = MouseState.None;

				m_MouseButtons = System.Windows.Forms.MouseButtons.None;

				m_HasBeenInvalidated = true;
			}
		}

		#region Handle Mouse Events

		#region Mosue Down

		public void OnMouseDown(View3D view, Vector2 mousePosition, System.Windows.Forms.MouseButtons mouseButtons, out bool shouldUpdate)
		{
			Vector2 mousePositionCorrected = mousePosition + view.ViewportOffset; // new Vector2(view.Viewport.X, view.Viewport.Y);

			shouldUpdate = false;

			UiControl foundTopMost = null;

			foreach (UiControl control in Controls)
			{
				UiControl found = null;

				if (control.IsVisible == true)
				{
					control.BeginPick(mousePositionCorrected, out found);
				}

				if (found != null)
				{
					if (foundTopMost == null)
					{
						foundTopMost = found;
					}
					else if (foundTopMost.ZIndex < found.ZIndex)
					{
						foundTopMost = found;
					}
				}
			}

			if (m_Focused != null && foundTopMost != null)
			{
				if (m_Focused != foundTopMost)
				{
					m_Focused.EndPick(mousePositionCorrected, PickType.UnFocus, foundTopMost);

					if (m_Focused is UiSubScene)
					{
						(m_Focused as UiSubScene).UnfocusControls();
					}

					m_Focused = foundTopMost;

					m_Focused.EndPick(mousePositionCorrected, PickType.Focus, m_Focused);

					shouldUpdate = true;
				}
			}
			else if (foundTopMost != null)
			{
				m_Focused = foundTopMost;

				m_Focused.EndPick(mousePositionCorrected, PickType.Focus, m_Focused);

				shouldUpdate = true;
			}
			else if (m_Focused != null && foundTopMost == null)
			{
				m_Focused.EndPick(mousePositionCorrected, PickType.UnFocus, null);

				if (m_Focused is UiSubScene)
				{
					(m_Focused as UiSubScene).UnfocusControls();
				}

				m_Focused = null;

				shouldUpdate = true;
			}

			if (m_Focused != null)
			{
				if (m_Focused is UiSubScene)
				{
					bool shouldSubUpdate;					
					(m_Focused as UiSubScene).OnMouseDown(view, mousePosition, mouseButtons, out shouldSubUpdate);

					m_MouseState = MouseState.Hovering;
					m_MouseButtons = System.Windows.Forms.MouseButtons.None;
				}
				else
				{
					if ((m_Focused.InteractionType & ControlInteractionType.Drag) == ControlInteractionType.Drag)
					{
						m_MouseState = MouseState.DragStart;
						m_MouseButtons = mouseButtons;

						m_Focused.DoMouseInteraction(m_MouseState, m_MouseButtons, mousePositionCorrected, out shouldUpdate);
					}
					else if ((m_Focused.InteractionType & ControlInteractionType.Click) == ControlInteractionType.Click)
					{
						m_MouseState = MouseState.ClickStart;
						m_MouseButtons = mouseButtons;
						m_Control_ClickStart = m_Focused;

						m_Focused.DoMouseInteraction(m_MouseState, m_MouseButtons, mousePositionCorrected, out shouldUpdate);
					}
					else
					{
						m_MouseState = MouseState.Hovering;
						m_MouseButtons = System.Windows.Forms.MouseButtons.None;

						m_Focused.DoMouseInteraction(m_MouseState, m_MouseButtons, mousePositionCorrected, out shouldUpdate);
					}
				}
			}
			else
			{
				m_MouseState = MouseState.None;
				m_MouseButtons = System.Windows.Forms.MouseButtons.None;
				m_Control_ClickStart = null;
			}
		}

		#endregion

		#region Mouse Up

		public void OnMouseUp(View3D view, Vector2 mousePosition, System.Windows.Forms.MouseButtons mouseButtons, out bool shouldUpdate)
		{
			Vector2 mousePositionCorrected = mousePosition + view.ViewportOffset; // new Vector2(view.Viewport.X, view.Viewport.Y);

			shouldUpdate = false;

			if (m_Focused != null)
			{
				if (m_Focused is UiSubScene)
				{
					bool shouldSubUpdate;

					(m_Focused as UiSubScene).OnMouseUp(view, mousePosition, mouseButtons, out shouldSubUpdate);
					
					m_MouseState = MouseState.Hovering;

					m_Control_ClickStart = null;
				}
				else
				{
					if (m_MouseState == MouseState.Dragging ||
						m_MouseState == MouseState.DragStart)
					{
						m_MouseState = MouseState.DragEnd;

						m_Focused.DoMouseInteraction(m_MouseState, m_MouseButtons, mousePositionCorrected, out shouldUpdate);

						m_Control_ClickStart = null;
					}
					else if (m_MouseState == MouseState.ClickStart)
					{
						m_MouseState = MouseState.ClickEnd;

						m_Focused.DoMouseInteraction(m_MouseState, m_MouseButtons, mousePositionCorrected, out shouldUpdate);
						
						m_Control_ClickStart = null;
					}
					else
					{
						m_MouseState = MouseState.Hovering;

						m_Control_ClickStart = null;
					}
				}
			}
			else
			{
				m_MouseState = MouseState.None;

				m_Control_ClickStart = null;
			}
		}

		#endregion
		
		#region Mouse Click

		public void OnMouseClick(View3D view, Vector2 mousePosition, System.Windows.Forms.MouseButtons mouseButtons, out bool shouldUpdate)
		{
			shouldUpdate = false;
		}

		#endregion 

		#region Mouse Moved

		public void OnMouseMoved(View3D view, Vector2 mousePosition, out bool shouldUpdate)
		{
			Vector2 mousePositionCorrected = mousePosition + view.ViewportOffset; // new Vector2(view.Viewport.X, view.Viewport.Y);

			shouldUpdate = false;

			if (m_Focused != null &&
				(m_MouseState == MouseState.Dragging ||
				m_MouseState == MouseState.DragStart))
			{
				m_MouseState = MouseState.Dragging;
				m_Focused.DoMouseInteraction(m_MouseState, m_MouseButtons, mousePositionCorrected, out shouldUpdate);
			}
			else
			{
				UiControl foundTopMost = null;

				foreach (UiControl control in Controls)
				{
					UiControl found = null;

					if (control.IsVisible == true)
					{
						control.BeginPick(mousePositionCorrected, out found);
					}

					if (found != null)
					{
						if (foundTopMost == null)
						{
							foundTopMost = found;
						}
						else if (foundTopMost.ZIndex < found.ZIndex)
						{
							foundTopMost = found;
						}
					}
				}

				if (foundTopMost != null && m_Control_ClickStart == foundTopMost)
				{
					m_MouseState = MouseState.ClickStart;
				}
				else
				{
					if (m_Hovered != null && foundTopMost != m_Hovered && m_Hovered is UiSubScene)
					{
						bool shouldSubUpdate;
						(m_Hovered as UiSubScene).OnMouseMoved(view, mousePosition, out shouldSubUpdate);
					}

					if (m_Hovered != null && foundTopMost != null)
					{
						if (m_Hovered != foundTopMost)
						{
							m_Hovered.EndPick(mousePositionCorrected, PickType.UnHover, foundTopMost);

							if (m_Hovered is UiSubScene)
							{
								(m_Hovered as UiSubScene).UnhoverControls();
							}

							m_Hovered = foundTopMost;

							m_Hovered.EndPick(mousePositionCorrected, PickType.Hover, m_Hovered);

							shouldUpdate = true;
						}
					}
					else if (foundTopMost != null)
					{
						m_Hovered = foundTopMost;

						m_Hovered.EndPick(mousePositionCorrected, PickType.Hover, m_Hovered);

						shouldUpdate = true;
					}
					else if (m_Hovered != null && foundTopMost == null)
					{
						m_Hovered.EndPick(mousePositionCorrected, PickType.UnHover, null);

						if (m_Hovered is UiSubScene)
						{
							(m_Hovered as UiSubScene).UnhoverControls();
						}

						m_Hovered = null;

						shouldUpdate = true;
					}

					if (m_Hovered != null && m_Hovered is UiSubScene)
					{
						bool shouldSubUpdate;
						(m_Hovered as UiSubScene).OnMouseMoved(view, mousePosition, out shouldSubUpdate);
					}

					if (m_Hovered != null)
					{
						m_MouseState = MouseState.Hovering;
					}
					else
					{
						m_MouseState = MouseState.Moving;
					}
				}
			}
		}
		
		#endregion

		#endregion

		#region Handle Key Events

		public void OnKeyPress(char @char, out bool shouldUpdate)
		{
			shouldUpdate = false;

			if (m_Focused != null)
			{
				if (m_Focused is UiSubScene)
				{
					bool shouldSubUpdate;
					(m_Focused as UiSubScene).OnKeyPress(@char, out shouldSubUpdate);
				}
				else
				{
					m_Focused.DoKeyInteraction(KeyInteractionType.KeyPress, @char, new System.Windows.Forms.KeyEventArgs(System.Windows.Forms.Keys.None), out shouldUpdate);
				}
			}
		}

		public void OnKeyUp(System.Windows.Forms.KeyEventArgs args, out bool shouldUpdate)
		{
			shouldUpdate = false;

			if (m_Focused != null)
			{
				if (m_Focused is UiSubScene)
				{
					bool shouldSubUpdate;
					(m_Focused as UiSubScene).OnKeyUp(args, out shouldSubUpdate);
				}
				else
				{
					m_Focused.DoKeyInteraction(KeyInteractionType.KeyUp, ' ', args, out shouldUpdate);
				}
			}
		}

		public void OnKeyDown(System.Windows.Forms.KeyEventArgs args, out bool shouldUpdate)
		{
			shouldUpdate = false;

			if (m_Focused != null)
			{
				if (m_Focused is UiSubScene)
				{
					bool shouldSubUpdate;
					(m_Focused as UiSubScene).OnKeyDown(args, out shouldSubUpdate);
				}
				else
				{
					m_Focused.DoKeyInteraction(KeyInteractionType.KeyDown, ' ', args, out shouldUpdate);
				}
			}
		}

		#endregion
	}
}
