using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RugTech1.Framework.Objects
{
	public interface IResourceManager : IDisposable
	{
		bool Disposed { get; }

		void LoadResources();

		void UnloadResources(); 
	}
}
