using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects;
using RugTech1.Framework.Objects.Simple;
using RugTech1.Framework.Objects.Text;

namespace RugTech1.Framework.Test
{
	public class TestScene : IScene, IResourceManager
	{
		private bool m_Disposed = true;
		//FontMatrix m_Matrix = new FontMatrix(); 
		TestImage m_Image = new TestImage(@"C:\Code\CSharp\RugProjects\Play\ArtAppBase\ArtAppBase\Test.png");
		TestParticles m_Parts = new TestParticles(); 

		public TestImage Image { get { return m_Image; } }

		public TestScene()
		{
			//m_Matrix.FontName = "Arial";
			//m_Matrix.TextureSize = 1024; 
			//m_Matrix.Size = 70f;
			//m_Matrix.Style = System.Drawing.FontStyle.Regular;
			//m_Matrix.Unit = System.Drawing.GraphicsUnit.Pixel; 			
		}

		public void Render(View3D view)
		{
			m_Image.Render();
			m_Parts.Render(view); 
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
				//m_Matrix.LoadResources(); 
				m_Image.LoadResources();

				//m_Image.TextureView = m_Matrix.TexureView; 

				m_Parts.LoadResources();
				m_Parts.Update(null); 

				m_Disposed = false;
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				//m_Matrix.Dispose(); 
				m_Image.UnloadResources();
				m_Parts.Dispose(); 

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
