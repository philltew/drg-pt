using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects;
using RugTech1.Framework.Compute;
using SlimDX.Direct3D11;
using RugTech1.Framework;

namespace DS.Simulation
{
	class FieldGradiantCS : ComputeBase
	{
		private bool m_Disposed = true;
		private ComputeShader m_ComputeShader; 

		public override string ShaderLocation
		{
			get { return "~/Shaders/ComputeField.hlsl"; }
		}

		public override string EntryPoint
		{
			get { return "main"; }
		}

		#region IResourceManager Members

		public override bool Disposed { get { return m_Disposed; } }

		public override void LoadResources()
		{
			if (m_Disposed == true)
			{
				m_ComputeShader = new ComputeShader(GameEnvironment.Device, Bytecode);

				m_Disposed = false;
			}
		}

		public override void UnloadResources()
		{
			if (m_Disposed == false)
			{
				m_ComputeShader.Dispose(); 

				m_Disposed = true;
			}
		}

		#endregion

		#region IDisposable Members

		public override void Dispose()
		{
			UnloadResources();

			base.Dispose(); 
		}

		#endregion
	}
}
