using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RugTech1.Framework.Objects
{
	public abstract class SceneObject : IResourceManager
	{
		private bool m_Disposed = true; 

		public abstract void Render(View3D view);

		#region IResourceManager Members

		public bool Disposed { get { return m_Disposed; } protected set { m_Disposed = value; } }

		public abstract void LoadResources();

		public abstract void UnloadResources();

		#endregion

		#region IDisposable Members

		public abstract void Dispose();

		#endregion
	}
}
