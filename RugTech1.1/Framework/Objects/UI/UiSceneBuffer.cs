using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using RugTech1.Framework.Data;
using SlimDX;
using SlimDX.Direct3D11;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace RugTech1.Framework.Objects.UI
{
	public class UiSceneBuffer : IResourceManager
	{
		#region Private Members
		
		private bool m_Disposed = true;

		private int m_MaxLineCount = 0;
		private int m_MaxLineIndicesCount = 0;

		private int m_MaxTriangleCount = 0;
		private int m_MaxTriangleIndicesCount = 0;

		private int m_LineCount = 0;
		private int m_LineIndicesCount = 0;

		private int m_TriangleCount = 0;
		private int m_TriangleIndicesCount = 0;

		private Buffer m_Lines;
		private Buffer m_LineIndices;

		private Buffer m_Triangles;
		private Buffer m_TriangleIndices;

		private VertexBufferBinding m_LinesBinding; 
		private VertexBufferBinding m_TriangleBinding; 

		#endregion

		#region Properties
		
		public int MaxLineCount { get { return m_MaxLineCount; } }
		public int MaxLineIndicesCount { get { return m_MaxLineIndicesCount; } }

		public int MaxTriangleCount { get { return m_MaxTriangleCount; } }
		public int MaxTriangleIndicesCount { get { return m_MaxTriangleIndicesCount; } }

		public int LineCount { get { return m_LineCount; } }
		public int LineIndicesCount { get { return m_LineIndicesCount; } }

		public int TriangleCount { get { return m_TriangleCount; } }
		public int TriangleIndicesCount { get { return m_TriangleIndicesCount; } }

		public VertexBufferBinding Lines { get { return m_LinesBinding; } }

		public Buffer LineIndices { get { return m_LineIndices; } }

		public VertexBufferBinding Triangles { get { return m_TriangleBinding; } }

		public Buffer TriangleIndices { get { return m_TriangleIndices; } }

		#endregion

		#region Resize
		
		public void Resize(int lineCount, int lineIndicesCount, int triangleCount, int triangleIndicesCount)
		{
			if (m_Disposed == false && 
				m_MaxLineCount > lineCount &&
				m_MaxLineIndicesCount > lineIndicesCount &&
				m_MaxTriangleCount > triangleCount &&
				m_MaxTriangleIndicesCount > triangleIndicesCount)
			{
				return; 
			}

			bool wasDisposed = m_Disposed;

			UnloadResources();

			m_MaxLineCount = lineCount < 2 ? 2 : lineCount;
			m_MaxLineIndicesCount = lineIndicesCount < 2 ? 2 : lineIndicesCount;

			m_MaxTriangleCount = triangleCount < 3 ? 3 : triangleCount;
			m_MaxTriangleIndicesCount = triangleIndicesCount < 3 ? 3 : triangleIndicesCount;


			if (wasDisposed == false)
			{
				LoadResources(); 
			}
		}
		
		#endregion

		public void MapStreams(MapMode mode, out DataStream Lines, out DataStream LineIndices, out DataStream Triangles, out DataStream TriangleIndices)
		{
			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext; 

			//Lines = m_Lines.Map(mode, MapFlags.None);
			//LineIndices = m_LineIndices.Map(mode, MapFlags.None);
			//Triangles = m_Triangles.Map(mode, MapFlags.None);
			//TriangleIndices = m_TriangleIndices.Map(mode, MapFlags.None);

			Lines = context.MapSubresource(m_Lines, mode, MapFlags.None).Data;
			LineIndices = context.MapSubresource(m_LineIndices, mode, MapFlags.None).Data;
			Triangles = context.MapSubresource(m_Triangles, mode, MapFlags.None).Data;
			TriangleIndices = context.MapSubresource(m_TriangleIndices, mode, MapFlags.None).Data;
		}

		public void UnmapStreams(int lineCount, int lineIndicesCount, int triangleCount, int triangleIndicesCount)
		{
			SlimDX.Direct3D11.DeviceContext context = GameEnvironment.Device.ImmediateContext; 

			m_LineCount = lineCount;
			m_LineIndicesCount = lineIndicesCount;
			m_TriangleCount = triangleCount;
			m_TriangleIndicesCount = triangleIndicesCount;

			//m_Lines.Unmap();
			//m_LineIndices.Unmap();
			//m_Triangles.Unmap();
			//m_TriangleIndices.Unmap();

			context.UnmapSubresource(m_Lines, 0);
			context.UnmapSubresource(m_LineIndices, 0);
			context.UnmapSubresource(m_Triangles, 0);
			context.UnmapSubresource(m_TriangleIndices, 0);
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
				m_Lines = new SlimDX.Direct3D11.Buffer(GameEnvironment.Device, new BufferDescription()
				{
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = m_MaxLineCount * Marshal.SizeOf(typeof(UIVertex)),
					Usage = ResourceUsage.Dynamic
				});

				m_LineIndices = new SlimDX.Direct3D11.Buffer(GameEnvironment.Device, new BufferDescription()
				{
					BindFlags = BindFlags.IndexBuffer,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = m_MaxLineIndicesCount * Marshal.SizeOf(typeof(int)),
					Usage = ResourceUsage.Dynamic
				});

				m_Triangles = new SlimDX.Direct3D11.Buffer(GameEnvironment.Device, new BufferDescription()
				{
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = m_MaxTriangleCount * Marshal.SizeOf(typeof(UIVertex)),
					Usage = ResourceUsage.Dynamic
				});

				m_TriangleIndices = new SlimDX.Direct3D11.Buffer(GameEnvironment.Device, new BufferDescription()
				{
					BindFlags = BindFlags.IndexBuffer,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = m_MaxTriangleIndicesCount * Marshal.SizeOf(typeof(int)),
					Usage = ResourceUsage.Dynamic
				});


				m_LinesBinding = new VertexBufferBinding(m_Lines, Marshal.SizeOf(typeof(UIVertex)), 0);
				m_TriangleBinding = new VertexBufferBinding(m_Triangles, Marshal.SizeOf(typeof(UIVertex)), 0); 

				m_Disposed = false;
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				m_Lines.Dispose();
				m_LineIndices.Dispose();
				m_Triangles.Dispose();
				m_TriangleIndices.Dispose(); 
		
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
