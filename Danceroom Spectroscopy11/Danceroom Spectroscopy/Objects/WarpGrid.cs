using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using RugTech1.Framework.Objects;
using DS.Simulation;
using SlimDX.Direct3D11;
using RugTech1.Framework;
using System.Runtime.InteropServices;
using RugTech1.Framework.Effects;

namespace DS.Objects
{
	public struct WarpVert
	{
		public Vector2 Position;
		public Vector2 Texture; 
	}

	internal class WarpGrid : IResourceManager
	{
		protected static WarpGridEffect Effect;

		#region Private Members
		
		private bool m_Disposed;

		private int m_Divide;
		private int m_Width;
		private int m_Height;

		private int m_GridSizeX;
		private int m_GridSizeY;

		private WarpVert[] m_Vertices;
		private Vector2[] m_TexCoordsOrig;
		private int[] m_Indices;

		private Vector2[] m_DistortionA;
		private Vector2[] m_DistortionB;

		private int m_QuadCount;
		private float m_WarpVariance;
		private float m_WarpPropagation;
		private bool m_EnableWarpPropagation;
		private float m_WarpPersistence;

		private SlimDX.Direct3D11.Buffer m_VerticesBuffer;
		private SlimDX.Direct3D11.Buffer m_IndicesBuffer;
		private VertexBufferBinding m_Bindings;
		private int m_IndexCount;
		private float m_FeedbackLevel; 

		#endregion

		#region Public Properties		

		public float WarpVariance
		{
			get { return m_WarpVariance; }
			set { m_WarpVariance = value; }
		}

		public float WarpPropagation
		{
			get { return m_WarpPropagation; }
			set { m_WarpPropagation = value; }
		}

		public bool EnableWarpPropagation
		{
			get { return m_EnableWarpPropagation; }
			set { m_EnableWarpPropagation = value; }
		}

		public float WarpPersistence
		{
			get { return m_WarpPersistence; }
			set { m_WarpPersistence = value; }
		}

		public float FeedbackLevel
		{
			get { return m_FeedbackLevel; }
			set { m_FeedbackLevel = value; }
		}

		#endregion

		public WarpGrid()
		{
			if (Effect == null)
			{
				Effect = SharedEffects.Effects["WarpGridEffect"] as WarpGridEffect;
			}

			m_Disposed = true;

			m_Divide = 2;
			m_Width = -1;
			m_Height = -1;

			//m_Vertices = NULL;
			//m_TexCoords = NULL;
			//m_Indices = NULL;

			m_QuadCount = 0;
			m_WarpVariance = 0.01f; //  0.42f;

			m_WarpPropagation = 0.02f; // 0; //  0.2f;
			m_EnableWarpPropagation = true;
			m_WarpPersistence = 0.02f; //  0.55f; 
		}

		public void Setup(int width, int height, int divide, float yScale)
		{
			if (m_Disposed == false)
			{
				UnloadResources(); 
			}

			m_Divide = divide;

			int gridSizeX = (width / divide);
			int gridSizeY = (height / divide);

			m_GridSizeX = gridSizeX;
			m_GridSizeY = gridSizeY;

			m_QuadCount = gridSizeX * gridSizeY;

			m_Vertices = new WarpVert[((gridSizeX + 1) * (gridSizeY + 1))];
			//m_TexCoords = new Vector2[((gridSizeX + 1) * (gridSizeY + 1))];
			m_TexCoordsOrig = new Vector2[((gridSizeX + 1) * (gridSizeY + 1))];

			m_DistortionA = new Vector2[((gridSizeX + 1) * (gridSizeY + 1))];
			m_DistortionB = new Vector2[((gridSizeX + 1) * (gridSizeY + 1))];

			m_Indices = new int[gridSizeY * (gridSizeX * 6)];

			int v = 0, i = 0; // , t = 0, i = 0, d = 0;
			//float xInc = (float)width / (float)gridSizeX;
			//float yInc = (float)height / (float)gridSizeY;

			// ofTextureData texData = texture->getTextureData();

			float offsetw = 0;
			float offseth = 0;

			/*if (texData.textureTarget == GL_TEXTURE_2D)
			{
				offsetw = 1.0f / (texData.tex_w);
				offseth = 1.0f / (texData.tex_h);
			}*/

			float tx0 = 1f;
			float ty0 = 0f;
			float tx1 = 0f;
			float ty1 = 1f;

			float txInc = ((float)(tx1 - tx0)) / (float)gridSizeX;
			float tyInc = ((float)(ty1 - ty0)) / (float)gridSizeY;

			float xInc = 2f / (float)gridSizeX;
			float yInc = (2f * yScale) / (float)gridSizeY;


			for (int y = 0; y <= gridSizeY; y++)
			{
				for (int x = 0; x <= gridSizeX; x++)
				{
					// precompute x,y
					m_TexCoordsOrig[v] = new Vector2(tx0 + ((float)x * txInc), ty0 + ((float)y * tyInc));

					m_Vertices[v] = new WarpVert()
					{
						//Position = new Vector2((float)x * xInc, (float)y * yInc),
						Position = new Vector2(1 - ((float)x * xInc), (1 * yScale) - ((float)y * yInc)),
						Texture = new Vector2(tx0 + ((float)x * txInc), ty0 + ((float)y * tyInc))
					};					
					// m_TexCoords[t++] = new Vector2(tx0 + ((float)x * txInc), ty0 + ((float)y * tyInc));

					m_DistortionA[v] = new Vector2(0.0f, 0.0f);
					m_DistortionB[v++] = new Vector2(0.0f, 0.0f);
				}
			}

			int span = (gridSizeX + 1) * 1;
			int RowOff = 0;
			
			for (int yref = 0; yref < gridSizeY; yref++)
			{
				for (int xref = 0; xref < gridSizeX; xref++)
				{
					m_Indices[i++] = (int)(RowOff + xref);
					m_Indices[i++] = (int)(RowOff + xref + span);
					m_Indices[i++] = (int)(RowOff + xref + 1);

					m_Indices[i++] = (int)(RowOff + xref + span);
					m_Indices[i++] = (int)(RowOff + xref + 1);
					m_Indices[i++] = (int)(RowOff + xref + span + 1);
				}

				RowOff += span;
			}

			//LoadResources(); 
		}
		
		public void Randomise(int width, int height) {

			if (m_Disposed == true)
			{
				return;
			}

			Random rand = new Random(); 

			int t = 0; 
			float xStep = (float)width / (float)(m_GridSizeX); 
			float yStep = (float)height / (float)(m_GridSizeY); 
			float xVariance = (1.0f / 255.0f) * (xStep * m_WarpVariance); 
			float yVariance = (1.0f / 255.0f) * (yStep * m_WarpVariance); 

			float oX, oY; 

			oX = 0.0f; 
			oY = 0.0f; 

			int currentRow = 0, currentCol = 0, nextRow = 0, nextCol = 0; 
			int rowInt = 0; 

			for (int y = 0; y <= m_GridSizeY; y++) 
			{
				oX = 0.0f; 

				currentCol = 0; 
				nextCol = (int)(oX + xStep); 
				rowInt = (int)(oY + yStep);

				if (rowInt >= height) rowInt = height - 1; 	

				nextRow = rowInt * width;

				for (int x = 0; x <= m_GridSizeX; x++) {
			
					nextCol = (int)(oX + xStep);
					
					if (nextCol >= width)
					{
						nextCol = width - 1;
					}

					m_DistortionA[t] = new Vector2((float)((rand.NextDouble() - 0.5) * 2 * xVariance), (float)((rand.NextDouble() - 0.5) * 2 * yVariance));
					m_Vertices[t].Texture = new Vector2(m_TexCoordsOrig[t].X - m_DistortionA[t].X, m_TexCoordsOrig[t].Y + m_DistortionA[t].Y);
					t++;

					oX += xStep; 
					currentCol = nextCol; 
				}

				oY += yStep; 
				currentRow = nextRow;
			}

			DataBox box = GameEnvironment.Device.ImmediateContext.MapSubresource(m_VerticesBuffer, MapMode.WriteDiscard, MapFlags.None);

			DataStream stream = box.Data;

			stream.WriteRange<WarpVert>(m_Vertices, 0, m_Vertices.Length);

			GameEnvironment.Device.ImmediateContext.UnmapSubresource(m_VerticesBuffer, 0); 			
		}


		public void Update(CompositeFieldImage image, int width, int height)
		{
			if (m_Disposed == true)
			{
				return;
			}

			int t = 0;
			float xStep = (float)width / (float)(m_GridSizeX);
			float yStep = (float)height / (float)(m_GridSizeY);
			float xVariance = (1.0f / 255.0f) * (xStep * m_WarpVariance);
			float yVariance = (1.0f / 255.0f) * (yStep * m_WarpVariance);

			float oX, oY;

			oX = 0.0f;
			oY = 0.0f;

			int currentRow = 0, currentCol = 0, nextRow = 0, nextCol = 0;
			int rowInt = 0;

			for (int y = 0; y <= m_GridSizeY; y++)
			{
				oX = width - 1; //  0.0f;

				currentCol = (int)(oX); //  width - 1;
				nextCol = (int)(oX - xStep);
				rowInt = (int)(oY + yStep);

				if (rowInt >= height)
				{
					rowInt = height - 1;
				}

				nextRow = rowInt; //  * width;
								

				for (int x = 0; x <= m_GridSizeX; x++)
				{
					t = (x + (y * (m_GridSizeX + 1)));

					nextCol = (int)(oX - xStep);
					
					if (nextCol >= width)
					{
						nextCol = width - 1;
					}

					if (nextCol < 0)
					{
						nextCol = 0; 
					}

					float value0 = (float)image.GetValueAt(currentCol, currentRow);
					float valueX = (float)image.GetValueAt(nextCol, currentRow);
					float valueY = (float)image.GetValueAt(currentCol, nextRow);					

					m_DistortionA[t] = new Vector2(((value0 - valueX) * xVariance), ((value0 - valueY) * yVariance));
					// m_TexCoords[t] = m_TexCoordsOrig[t] - m_DistortionB[t];
					m_Vertices[t].Texture = new Vector2(m_TexCoordsOrig[t].X - m_DistortionB[t].X, m_TexCoordsOrig[t].Y + m_DistortionB[t].Y); 
					//t++;

					/* 
					m_DistortionA[t] = ((pixels[currentCol + currentRow] - pixels[nextCol + currentRow]) * xVariance);
					m_TexCoords[t] = m_TexCoordsOrig[t] - m_DistortionB[t];
					t++;

					m_DistortionA[t] = ((pixels[currentCol + currentRow] - pixels[currentCol + nextRow]) * yVariance);
					m_TexCoords[t] = m_TexCoordsOrig[t] + m_DistortionB[t];
					t++;
					*/ 

					oX -= xStep;
					currentCol = nextCol;
				}

				oY += yStep;
				currentRow = nextRow;
			}
		
			if (m_EnableWarpPropagation)
			{
				int gridStride = m_GridSizeX; //  *2;

				for (int y = 1; y < m_GridSizeY; y++)
				{
					for (int x = 1; x < m_GridSizeX; x++)
					{
						int thisPix = (x + (y * (m_GridSizeX + 1)));

						m_DistortionA[thisPix] = (m_DistortionA[thisPix] * m_WarpPersistence) + ((m_DistortionB[thisPix - 1] + m_DistortionB[thisPix + 1] + m_DistortionB[thisPix - gridStride] + m_DistortionB[thisPix + gridStride]) * m_WarpPropagation);
						//thisPix++;
						//m_DistortionA[thisPix] = (m_DistortionA[thisPix] * m_WarpPersistence) + (m_DistortionB[thisPix - 2] + m_DistortionB[thisPix + 2] + m_DistortionB[thisPix - gridStride] + m_DistortionB[thisPix + gridStride]) * m_WarpPropagation;
					}
				}
			}
			 
			// swap distortion buffers 
			Vector2[] temp = m_DistortionA;
			m_DistortionA = m_DistortionB;
			m_DistortionB = temp;

			DataBox box = GameEnvironment.Device.ImmediateContext.MapSubresource(m_VerticesBuffer, MapMode.WriteDiscard, MapFlags.None); 
			
			DataStream stream = box.Data;

			stream.WriteRange<WarpVert>(m_Vertices, 0, m_Vertices.Length);

			GameEnvironment.Device.ImmediateContext.UnmapSubresource(m_VerticesBuffer, 0); 			
		}

		public void Render(ShaderResourceView texture)
		{
			if (m_Disposed == true)
			{
				return;
			}

			int count = (m_GridSizeY) * (m_GridSizeX) * 2;

			Effect.RenderGrid(texture, m_FeedbackLevel, m_Bindings, m_IndicesBuffer, count * 3); 			
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
				using (DataStream stream = new DataStream(m_Vertices.Length * Marshal.SizeOf(typeof(WarpVert)), true, true))
				{
					stream.WriteRange(m_Vertices);
					stream.Position = 0;

					m_VerticesBuffer = new SlimDX.Direct3D11.Buffer(GameEnvironment.Device, stream, new BufferDescription()
					{
						BindFlags = BindFlags.VertexBuffer,
						CpuAccessFlags = CpuAccessFlags.Write,
						OptionFlags = ResourceOptionFlags.None,
						SizeInBytes = m_Vertices.Length * Marshal.SizeOf(typeof(WarpVert)),
						StructureByteStride = Marshal.SizeOf(typeof(WarpVert)), 
						Usage = ResourceUsage.Dynamic
					});
				}

				m_Bindings = new VertexBufferBinding(m_VerticesBuffer, Marshal.SizeOf(typeof(WarpVert)), 0);

				using (DataStream stream = new DataStream(m_Indices.Length * sizeof(int), true, true))
				{
					stream.WriteRange(m_Indices);
					stream.Position = 0;

					m_IndicesBuffer = new SlimDX.Direct3D11.Buffer(GameEnvironment.Device, stream, new BufferDescription()
					{
						BindFlags = BindFlags.IndexBuffer,
						CpuAccessFlags = CpuAccessFlags.None,
						OptionFlags = ResourceOptionFlags.None,
						SizeInBytes = m_Indices.Length * sizeof(int),
						StructureByteStride = sizeof(int),
						Usage = ResourceUsage.Default
					});
				}

				m_Disposed = false;
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				m_VerticesBuffer.Dispose();
				m_IndicesBuffer.Dispose();				
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
