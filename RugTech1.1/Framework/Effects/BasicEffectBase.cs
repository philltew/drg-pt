using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rug.Cmd;
using RugTech1.Framework.Objects;

namespace RugTech1.Framework.Effects
{
	public abstract class BasicEffectBase : IResourceManager
	{
		private SlimDX.D3DCompiler.ShaderBytecode m_Bytecode; 

		public abstract string ShaderLocation { get; }

		public SlimDX.D3DCompiler.ShaderBytecode Bytecode 
		{ 
			get
			{
				if (m_Bytecode == null)
				{
					string compilationErrors;
					//m_Bytecode = SlimDX.D3DCompiler.ShaderBytecode.CompileFromFile(Helper.ResolvePath(ShaderLocation), "fx_4_0", SlimDX.D3DCompiler.ShaderFlags.EnableStrictness, SlimDX.D3DCompiler.EffectFlags.None);
					m_Bytecode = SlimDX.D3DCompiler.ShaderBytecode.CompileFromFile(Helper.ResolvePath(ShaderLocation), "fx_5_0", SlimDX.D3DCompiler.ShaderFlags.None, SlimDX.D3DCompiler.EffectFlags.None, null, null, out compilationErrors);

					if (Helper.IsNotNullOrEmpty(compilationErrors) == true)
					{
						RC.WriteLine(ConsoleVerbosity.Debug, ConsoleThemeColor.ErrorColor1, "Errors compiling shader: " + ShaderLocation);
						RC.WriteLine(ConsoleVerbosity.Debug, ConsoleThemeColor.ErrorColor2, compilationErrors);
					}
				}				

				return m_Bytecode; 
			}
		}

		#region IResourceManager Members

		public abstract bool Disposed { get; }

		public abstract void LoadResources();

		public abstract void UnloadResources();

		#endregion

		#region IDisposable Members

		public virtual void Dispose()
		{
			if (m_Bytecode != null && m_Bytecode.Disposed == false)
			{
				m_Bytecode.Dispose();
				m_Bytecode = null; 
			}
		}

		#endregion
	}
}
