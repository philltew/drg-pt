using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Effects;
using SlimDX.Direct3D11;
using SlimDX;
using RugTech1.Framework.Data;
using System.Runtime.InteropServices;
using System.Drawing;

namespace RugTech1.Framework.Objects.Simple
{
	public class ImageBox : IResourceManager
	{
		protected static ImposterEffect Effect;

		private bool m_Disposed = true;
		private string m_Filename;
		private Texture2D m_ImposterTexture;
		private SlimDX.Direct3D11.Buffer m_Vertices;
		private VertexBufferBinding m_VerticesBindings;
		private RectangleF m_CurrentRectangle; 
		
		public ShaderResourceView TextureView;
		public ImposterOverlayType OverlayType = ImposterOverlayType.None;
		public RectangleF Rectangle = new RectangleF(-1, -1, 2, 2);

		public bool FlipHorizontal = false;
		public bool FlipVertical = false;

		public ImageBox(string filename)
		{
			if (Effect == null)
			{
				Effect = SharedEffects.Effects["Imposter"] as ImposterEffect;
			}

			m_Filename = filename; 
		}

		public void Render()
		{
			if (Rectangle != m_CurrentRectangle)
			{
				WriteRectangle(); 
			}

			Effect.RenderImposter(OverlayType, m_VerticesBindings, TextureView); 
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

				m_Vertices = new SlimDX.Direct3D11.Buffer(GameEnvironment.Device, new BufferDescription()
				{
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = 4 * Marshal.SizeOf(typeof(Vertex2D)),
					Usage = ResourceUsage.Dynamic
				});

				m_VerticesBindings = new VertexBufferBinding(m_Vertices, Marshal.SizeOf(typeof(Vertex2D)), 0);

				WriteRectangle(); 		

				m_Disposed = false;
			}
		}

		private void WriteRectangle()
		{
			m_CurrentRectangle = Rectangle;

			float minX = m_CurrentRectangle.Left, minY = m_CurrentRectangle.Top, maxX = m_CurrentRectangle.Right, maxY = m_CurrentRectangle.Bottom;
			float minU = 0.0f, minV = 1.0f, maxU = 1.0f, maxV = 0.0f;

			if (FlipVertical == true)
			{
				float t = minV;
				minV = maxV;
				maxV = t;  
			}

			if (FlipHorizontal == true)
			{
				float t = minU;
				minU = maxU;
				maxU = t;
			}

			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext; 
			
			DataStream stream = context.MapSubresource(m_Vertices, MapMode.WriteDiscard, MapFlags.None).Data;

			stream.WriteRange(new Vertex2D[] {					
						new Vertex2D() { Position = new Vector2(maxX, minY), TextureCoords =  new Vector2(maxU, minV) }, 
						new Vertex2D() { Position = new Vector2(minX, minY), TextureCoords =  new Vector2(minU, minV) }, 
						new Vertex2D() { Position = new Vector2(maxX, maxY), TextureCoords = new Vector2(maxU, maxV) },  
						new Vertex2D() { Position = new Vector2(minX, maxY), TextureCoords =  new Vector2(minU, maxV) } 
					});

			context.UnmapSubresource(m_Vertices, 0); 
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				if (m_Filename != null)
				{
					m_ImposterTexture.Dispose();			
					TextureView.Dispose();

					m_Vertices.Dispose(); 					
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
