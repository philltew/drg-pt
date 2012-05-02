using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using RugTech1.Framework.Objects;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Buffer = SlimDX.Direct3D11.Buffer;
using Device = SlimDX.Direct3D11.Device;

namespace RugTech1.Framework.Objects.Simple
{
	public struct CoordSubRect
	{
		public float Left, Right; // U 
		public float Top, Bottom; // V
	}

	public class BloomQuad : IResourceManager
	{
		private int m_Width;
		private int m_Height;
		private CoordSubRect m_Coords;
		private Vector2[] m_Offsets;
		private int m_NumberOfOffsets;

		private bool m_Disposed = true;

		public Buffer VBdata;
		public Viewport Viewport;
		public int stride;
		public int offset;
		public int nElements;

		public BloomQuad(int width, int height, CoordSubRect coords, Vector2[] offsets, int nOffsets)
		{
			m_Width = width;
			m_Height = height;

			m_Coords = coords;
			m_Offsets = offsets;
			m_NumberOfOffsets = nOffsets;

			Viewport = new SlimDX.Direct3D11.Viewport()
			{
				Width = m_Width,
				Height = m_Height,
				MinZ = 0f,
				MaxZ = 1.0f,
				X = 0,
				Y = 0
			};

			m_Disposed = true; 
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
				float tWidth = m_Coords.Right - m_Coords.Left;
				float tHeight = m_Coords.Bottom - m_Coords.Top;

				nElements = 1 + m_NumberOfOffsets;
				stride = Marshal.SizeOf(typeof(Vector2)) * nElements;
				offset = 0;

				Vector2[] vertices = new Vector2[3 * nElements];

				// Positions
				vertices[0 * nElements] = new Vector2(-1.0f, 1.0f);  // Top left
				vertices[1 * nElements] = new Vector2(3.0f, 1.0f);  // Top right
				vertices[2 * nElements] = new Vector2(-1.0f, -3.0f);  // Bottom left

				// Offsets
				float[] tempX = new float[3], tempY = new float[3];

				tempX[0] = m_Coords.Left;
				tempY[0] = m_Coords.Top;

				tempX[1] = m_Coords.Left + (tWidth * 2.0f);
				tempY[1] = m_Coords.Top;

				tempX[2] = m_Coords.Left;
				tempY[2] = m_Coords.Top + (tHeight * 2.0f);

				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j < m_NumberOfOffsets; j++)
					{
						vertices[i * nElements + j + 1] = new Vector2(tempX[i] + m_Offsets[j].X, tempY[i] + m_Offsets[j].Y);
					}
				}

				using (DataStream stream = new DataStream(vertices, false, false))
				{
					VBdata = new Buffer(GameEnvironment.Device, stream, new BufferDescription()
					{
						Usage = ResourceUsage.Immutable,
						SizeInBytes = stride * 3,
						BindFlags = SlimDX.Direct3D11.BindFlags.VertexBuffer,
						CpuAccessFlags = SlimDX.Direct3D11.CpuAccessFlags.None,
						OptionFlags = ResourceOptionFlags.None
					});
				}

				m_Disposed = false; 
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				VBdata.Dispose();

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
