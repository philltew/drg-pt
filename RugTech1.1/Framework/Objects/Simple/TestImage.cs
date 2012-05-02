using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Effects;
using SlimDX.Direct3D11;

namespace RugTech1.Framework.Objects.Simple
{
	public class TestImage : IResourceManager
	{
		protected static ImposterEffect Effect;

		private bool m_Disposed = true;
		private string m_Filename;
		private Texture2D m_ImposterTexture;
		public ShaderResourceView TextureView;

		public TestImage(string filename)
		{
			if (Effect == null)
			{
				Effect = SharedEffects.Effects["Imposter"] as ImposterEffect;
			}

			m_Filename = filename; 
		}

		public void Render()
		{
			Effect.RenderImposter(ImposterOverlayType.None, TextureView); 
		}

		#region IResourceManager Members

		public bool Disposed
		{
			get { return m_Disposed; }
		}

		public void LoadResources()
		{
			if (m_Disposed == true)
			{
				if (m_Filename != null)
				{
					m_ImposterTexture = Texture2D.FromFile(GameEnvironment.Device, Helper.ResolvePath(m_Filename));
					TextureView = new ShaderResourceView(GameEnvironment.Device, m_ImposterTexture);
				}

				m_Disposed = false;
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				if (m_Filename != null)
				{
					m_ImposterTexture.Dispose();			
					TextureView.Dispose();
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
	}
}
