using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RugTech1.Framework.Objects.UI
{
	public class UiSceneControlCollection : IEnumerable<UiControl>
	{
		private UiScene m_Owner;

		private List<UiControl> m_Inner = new List<UiControl>();
		private int m_MinZindex = 0;
		private int m_MaxZindex = 0;

		public int MinZindex { get { return m_MinZindex; } }
		public int MaxZindex { get { return m_MaxZindex; } }

		public UiControl this[int index] { get { return m_Inner[index]; } }

		public int Count { get { return m_Inner.Count; } }

		public UiSceneControlCollection(UiScene owner)
		{
			this.m_Owner = owner; 
		}

		public void Add(UiControl item)
		{
			m_Inner.Add(item);

			item.Parent = null;
			item.Scene = m_Owner;
		}

		public void Remove(UiControl item)
		{
			m_Inner.Remove(item);

			item.Parent = null;
			item.Scene = null;

			if (item is IDynamicUiControl)
			{
				m_Owner.DynamicControls.Remove(item as IDynamicUiControl);
			}
		}

		public void Clear()
		{
			DetachDynamicControls();

			foreach (UiControl item in m_Inner)
			{
				item.Parent = null; 
			}

			m_Inner.Clear(); 
		}

		public void Sort()
		{
			m_Inner.Sort();
 			
			if (m_Inner.Count > 0)
			{
				m_MinZindex = m_Inner[0].ZIndex;

				m_MaxZindex = m_Inner[m_Inner.Count - 1].ZIndex;
			}
		}

		public void AttachDynamicControls()
		{
			foreach (UiControl item in m_Inner)
			{
				if (item is IUiControlContainer)
				{
					(item as IUiControlContainer).AttachDynamicControls();
				}

				if (item is IDynamicUiControl)
				{
					m_Owner.DynamicControls.Add(item as IDynamicUiControl);
				}

				if (item is UiSubScene)
				{
					m_Owner.SubScenes.Add(item as UiSubScene);
				}

				if (item is IResourceManager)
				{
					m_Owner.ResourceManagers.Add(item as IResourceManager);
				}
			}
		}

		public void DetachDynamicControls()
		{
			foreach (UiControl item in m_Inner)
			{
				if (item is IUiControlContainer)
				{
					(item as IUiControlContainer).DetachDynamicControls();
				}
				
				if (item is IDynamicUiControl)
				{
					m_Owner.DynamicControls.Remove(item as IDynamicUiControl);
				}

				if (item is UiSubScene)
				{
					m_Owner.SubScenes.Remove(item as UiSubScene);
				}

				if (item is IResourceManager)
				{
					m_Owner.ResourceManagers.Remove(item as IResourceManager);
				}
			}
		}

		public IEnumerator<UiControl> GetEnumerator()
		{
			return m_Inner.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return (m_Inner as System.Collections.IEnumerable).GetEnumerator();
		}
	}
}
