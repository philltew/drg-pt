using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SlimDX;

namespace RugTech1.Framework.Objects.UI
{
	public enum PickType { None, Hover, Focus, UnHover, UnFocus };
	public enum ControlInteractionType { NonSelectable = -1, None = 0, Focus = 1, Click = 2, Drag = 4, }

	public abstract class UiControl : IComparable<UiControl>
	{
		private bool m_IsHovering = false;
		private bool m_IsFocused = false;
		private UiControl m_Parent;
		private UiScene m_Scene;
		private int m_ZIndex = 0;
		private int m_RelitiveZIndex = 0;
		private ControlInteractionType m_InteractionType = ControlInteractionType.None;
		private object m_Tag = null;

		public abstract bool IsVisible { get; set; }

		public virtual ControlInteractionType InteractionType { get { return m_InteractionType; } protected set { m_InteractionType = value; } } 
		public virtual bool IsHovering { get { return m_IsHovering; } set { m_IsHovering = value; } }
		public virtual bool IsFocused { get { return m_IsFocused; } set { m_IsFocused = value; } }

		public virtual UiControl Parent 
		{ 
			get { return m_Parent; } 
			set 
			{ 
				m_Parent = value;

				if (m_Parent != null)
				{
					m_ZIndex = m_Parent.ZIndex + RelitiveZIndex;
				}
				else
				{
					m_ZIndex = RelitiveZIndex; 
				}
			} 
		}

		public virtual UiScene Scene { get { return m_Scene; } set { m_Scene = value; } }

		public abstract RectangleF Bounds { get; }

		public virtual int ZIndex { get { return m_ZIndex; } set { m_ZIndex = value; } }
		public virtual int RelitiveZIndex { get { return m_RelitiveZIndex; } set { m_RelitiveZIndex = value; } }

		public float ZIndex_Float { get { return ((float)(1000 - ZIndex) * 0.001f); } }
		public float ZIndexForOver_Float { get { return ZIndex_Float - 0.00025f; } }
		public float ZIndexForText_Float { get { return ZIndex_Float - 0.0005f; } } 
		public float ZIndexForLines_Float { get { return ZIndex_Float - 0.00075f; } }
		public float ZIndexForUnder_Float { get { return ZIndex_Float - 0.0008f; } }

		public virtual object Tag { get { return m_Tag; } set { m_Tag = value; } }

		public abstract void GetTotalElementCounts(ref int LineVerts, ref int LinesIndices, ref int TriangleVerts, ref int TriangleIndices);
		
		public abstract void WriteVisibleElements(View3D view,
													RectangleF ClientBounds, ref RectangleF RemainingBounds,
													DataStream LineVerts, ref int LineVertsCount,
													DataStream LinesIndices, ref int LinesIndicesCount,
													DataStream TriangleVerts, ref int TriangleVertsCount,
													DataStream TriangleIndices, ref int TriangleIndicesCount);

		public virtual bool BeginPick(Vector2 mousePos, out UiControl control)
		{
			if (this.InteractionType == ControlInteractionType.NonSelectable)
			{
				control = null; 
				return false; 
			}
			else if (Bounds.Contains(mousePos.X, mousePos.Y))
			{
				control = this;
				return true;
			}
			else
			{
				control = null;
				return false;
			}
		}

		public virtual void EndPick(Vector2 mousePos, PickType pickType, UiControl control)
		{
			//if (control == this)
			{
				switch (pickType)
				{
					case PickType.None:
						break;
					case PickType.Hover:
						IsHovering = true; 
						break;
					case PickType.Focus:
						IsFocused = true; 
						break;
					case PickType.UnHover:
						IsHovering = false; 
						break;
					case PickType.UnFocus:
						IsFocused = false;
						break;
					default:
						break;
				}
			}
		}

		public virtual void DoMouseInteraction(MouseState mouseState, System.Windows.Forms.MouseButtons mouseButtons, Vector2 mousePos, out bool shouldUpdate)
		{
			shouldUpdate = false;
		}

		public virtual void DoKeyInteraction(KeyInteractionType keyInteractionType, char @char, System.Windows.Forms.KeyEventArgs eventArgs, out bool shouldUpdate)
		{
			shouldUpdate = false;
		}

		#region IComparable<UiControl> Members

		public int CompareTo(UiControl other)
		{
			return this.ZIndex - other.ZIndex; 
		}

		#endregion

		public bool IsDescendantOf(UiControl potentialParent)
		{
			UiControl parent = this;

			while (parent != potentialParent && parent != null)
			{
				parent = parent.Parent; 
			}

			return parent == potentialParent; 
		}
	}
}
