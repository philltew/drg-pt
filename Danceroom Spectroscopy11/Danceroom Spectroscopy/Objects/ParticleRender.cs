using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using RugTech1.Framework.Data;
using RugTech1.Framework.Effects;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D11.Buffer; 

namespace RugTech1.Framework.Objects.Simple
{
	public class ParticleRender2 : IResourceManager
	{
		static ParticleEffect2 Effect; 

		private bool m_Disposed = true;
		private string m_ParticleTexturePath;
		private Texture2D m_ParticleTexture;
		private ShaderResourceView m_ParticleTextureView;
		private Buffer m_BillboardVertices;
		private Buffer m_Instances;
		private VertexBufferBinding[] m_DataBindings; 

		private int m_MaxCount = 100;
		private int m_InstanceCount = 0;

		private float m_ColorScale = 1f;
		private float m_ParticleScale = 1f;
		private float m_MinDistance = 0f;
		private float m_MaxDistance = 100f;
		private float m_ScaleDistance = 1f;
		private float m_ParticleScaleX = 1f;
		private float m_ParticleScaleY = 1f;
		
		public int MaxCount { get { return m_MaxCount; } set { m_MaxCount = value; } }
		public int InstanceCount { get { return m_InstanceCount; } set { m_InstanceCount = value; } }

		public float ColorScale { get { return m_ColorScale; } set { m_ColorScale = value; } }
		public float ParticleScale { get { return m_ParticleScale; } set { m_ParticleScale = value; } }
		public float MinDistance { get { return m_MinDistance; } set { m_MinDistance = value; } }
		public float MaxDistance { get { return m_MaxDistance; } set { m_MaxDistance = value; } }
		public float ScaleDistance { get { return m_ScaleDistance; } set { m_ScaleDistance = value; } }

		public float ParticleScaleX { get { return m_ParticleScaleX; } set { m_ParticleScaleX = value; } }
		public float ParticleScaleY { get { return m_ParticleScaleY; } set { m_ParticleScaleY = value; } }


		public Buffer Instances { get { return m_Instances; } }

		public ParticleRender2(string particleTexture, int maxCount)
		{
			if (Effect == null)
			{
				Effect = SharedEffects.Effects["Particle2"] as ParticleEffect2; 
			}

			m_ParticleTexturePath = particleTexture; 
			m_MaxCount = maxCount; 
		}

		public virtual void Update(View3D view)
		{


		}

		public void Render(View3D view)
		{
			Matrix viewProjWorld = (view.World * view.View) * view.Projection;

			Effect.Render(m_DataBindings, m_InstanceCount, viewProjWorld, m_ParticleTextureView, m_ColorScale, m_ParticleScale * m_ParticleScaleX, m_ParticleScale * m_ParticleScaleY, m_MinDistance, m_MaxDistance, m_ScaleDistance); 
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
				m_ParticleTexture = Texture2D.FromFile(GameEnvironment.Device, m_ParticleTexturePath);
				m_ParticleTextureView = new ShaderResourceView(GameEnvironment.Device, m_ParticleTexture);

				#region Billboard

				using (DataStream stream = new DataStream(4 * Marshal.SizeOf(typeof(TexturedVertex)), true, true))
				{
					stream.WriteRange(new TexturedVertex[] {					
						new TexturedVertex() { Position = new Vector4(1f, -1f, 0.0f, 1f), TextureCoords =  new Vector2(1.0f, 0.0f) }, 
						new TexturedVertex() { Position = new Vector4(-1f, -1f, 0.0f, 1f), TextureCoords =  new Vector2(0.0f, 0.0f) }, 
						new TexturedVertex() { Position = new Vector4(1f, 1f, 0.0f, 1f), TextureCoords = new Vector2(1.0f, 1.0f) },  
						new TexturedVertex() { Position = new Vector4(-1f, 1f, 0.0f, 1f), TextureCoords =  new Vector2(0.0f, 1.0f) } 
					});

					stream.Position = 0;

					m_BillboardVertices = new SlimDX.Direct3D11.Buffer(GameEnvironment.Device, stream, new BufferDescription()
					{
						BindFlags = BindFlags.VertexBuffer,
						CpuAccessFlags = CpuAccessFlags.None,
						OptionFlags = ResourceOptionFlags.None,
						SizeInBytes = 4 * Marshal.SizeOf(typeof(TexturedVertex)),
						Usage = ResourceUsage.Default					
					});
				}

				#endregion

				#region Instances

				m_Instances = new SlimDX.Direct3D11.Buffer(GameEnvironment.Device, new BufferDescription()
				{
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = m_MaxCount * Marshal.SizeOf(typeof(StarInstanceVertex)),
					Usage = ResourceUsage.Dynamic
				});

				#endregion

				#region Bindings

				m_DataBindings = new VertexBufferBinding[2];

				m_DataBindings[0] = new VertexBufferBinding(m_BillboardVertices, Marshal.SizeOf(typeof(TexturedVertex)), 0);
				m_DataBindings[1] = new VertexBufferBinding(m_Instances, Marshal.SizeOf(typeof(StarInstanceVertex)), 0);

				#endregion

				m_Disposed = false; 
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				m_ParticleTexture.Dispose();
				m_ParticleTextureView.Dispose();

				m_BillboardVertices.Dispose();
				m_Instances.Dispose(); 

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
