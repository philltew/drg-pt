using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Objects;
using RugTech1.Framework.Objects.Text;
using RugTech1.Framework.Objects.UI;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace RugTech1.Framework.Effects
{
	public class UiEffect : BasicEffectBase
	{
		#region Private Members

		private string m_ShaderLocation = @"~/Shaders/UserInterface.fx";

		private bool m_Disposed = true; 
		private Effect m_Effect;
		private EffectTechnique m_Technique;		
		private InputLayout m_Layout;
		private EffectPass m_Pass_BoxesAndText; 
		private EffectPass m_Pass_Lines;
		private EffectResourceVariable m_UiElementsTexture; 

		private FontMatrix m_TextFont; 

		#endregion

		public override string ShaderLocation { get { return m_ShaderLocation; } }

		public FontMatrix TextFont { get { return m_TextFont; } }

		public UiEffect()
		{			
			m_TextFont = FontMatrix.Read(Helper.ResolvePath(@"~/assets/font-matrix.fm"));
		}

		public void Render(UiSceneBuffer buffer)
		{
			if (m_Disposed == true)
			{
				return;
			}
			
			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext;

			context.InputAssembler.InputLayout = m_Layout;

			m_Pass_BoxesAndText.Apply(context);

			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
			context.InputAssembler.SetVertexBuffers(0, buffer.Triangles);
			context.InputAssembler.SetIndexBuffer(buffer.TriangleIndices, Format.R32_UInt, 0);
			context.DrawIndexed(buffer.TriangleIndicesCount, 0, 0);

			m_Pass_Lines.Apply(context);

			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
			context.InputAssembler.SetVertexBuffers(0, buffer.Lines);
			context.InputAssembler.SetIndexBuffer(buffer.LineIndices, Format.R32_UInt, 0);
			context.DrawIndexed(buffer.LineIndicesCount, 0, 0);			
		}

		#region IResourceManager

		public override bool Disposed { get { return m_Disposed; } }

		public override void LoadResources()
		{
			if (m_Disposed == true)
			{
				m_TextFont.LoadResources();

				//SlimDX.D3DCompiler.ShaderBytecode blob = SlimDX.D3DCompiler.ShaderBytecode.CompileFromFile(Helper.ResolvePath(m_ShaderLocation), "fx_4_0", SlimDX.D3DCompiler.ShaderFlags.EnableStrictness, SlimDX.D3DCompiler.EffectFlags.None);

				m_Effect = new Effect(GameEnvironment.Device, Bytecode);

				m_Technique = m_Effect.GetTechniqueByName("LinesAndBoxes");
				m_Pass_BoxesAndText = m_Technique.GetPassByName("BoxesAndText");
				m_Pass_Lines = m_Technique.GetPassByName("Lines");
				
				m_Layout = new InputLayout(GameEnvironment.Device, m_Pass_Lines.Description.Signature, new[] {
					new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0),
					new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 20, 0),				
				});

				m_UiElementsTexture = m_Effect.GetVariableByName("UiElementsTexture").AsResource();
				m_UiElementsTexture.SetResource(m_TextFont.TexureView); 

				m_Disposed = false;
			}
		}

		public override void UnloadResources()
		{
			if (m_Disposed == false)
			{
				m_TextFont.UnloadResources(); 

				m_Layout.Dispose();
				m_Effect.Dispose();

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
