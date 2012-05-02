using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects;
using RugTech1.Framework.Objects.Simple;

namespace FieldTextureTest
{
	class TestFloatScene : IScene
	{
		public TestImage2 m_TestImage; 

		#region IScene Members

		public void Render(View3D view)
		{
			m_TestImage.Render();
		}

		#endregion
	}
}
