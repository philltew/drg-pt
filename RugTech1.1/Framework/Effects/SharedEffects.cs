using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects;

namespace RugTech1.Framework.Effects
{
	public static class SharedEffects
	{
		public static readonly Dictionary<string, IResourceManager> Effects = new Dictionary<string, IResourceManager>();
		private static bool m_Disposed = true;

		static SharedEffects()
		{
			Effects.Add("Imposter", new ImposterEffect());
			Effects.Add("Bloom", new BloomEffect());
			Effects.Add("Particle", new ParticleEffect());
			Effects.Add("UI", new UiEffect());
			Effects.Add("Volume", new VolumeEffect());
			Effects.Add("Material", new MaterialEffect()); 
		}

		public static void AddEffects() 
		{
			// just ensures that the effect system has been accessed. 
		}

		public static void LoadResources()
		{
			if (m_Disposed == true)
			{
				foreach (IResourceManager manager in Effects.Values)
				{
					manager.LoadResources();
				}
				m_Disposed = false;
			}
		}

		public static void UnloadResources()
		{
			if (m_Disposed == false)
			{
				foreach (IResourceManager manager in Effects.Values)
				{
					manager.UnloadResources();
				}
				m_Disposed = true;
			}
		}


		public static bool Disposed { get { return m_Disposed; }  }

		public static void Dispose()
		{
			foreach (IResourceManager manager in Effects.Values)
			{
				manager.Dispose();
			}

			Effects.Clear();

			m_Disposed = true; 
		}

	}
}
