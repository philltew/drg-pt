using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RugTech1.Framework.Effects;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Runtime.InteropServices;
using RugTech1.Framework.Data;

namespace RugTech1.Framework.Objects.Simple
{
	public class Cube : IResourceManager 
	{
		private enum CoordLocationIndex
		{
			TopLeftBack = 0, TopRightBack = 1, TopLeftFront = 2, TopRightFront = 3,
			BottomLeftBack = 4, BottomRightBack = 5, BottomLeftFront = 6, BottomRightFront = 7,
		}

		private enum FaceIndex
		{
			Left = 0, Front = 1, Right = 2, Back = 3, Top = 4, Bottom = 5
		}

		protected static MaterialEffect Effect;

		private bool m_Disposed = true;

		private string m_DiffuseTexturePath;
		private Texture2D m_DiffuseTexture;
		private ShaderResourceView m_DiffuseTextureView;

		private string m_DiffuseLightMapPath;
		private Texture2D m_DiffuseLightMapTexture;
		private ShaderResourceView m_DiffuseLightMapTextureView;

		private string m_SpecularLightMapPath;
		private Texture2D m_SpecularLightMapTexture;
		private ShaderResourceView m_SpecularLightMapTextureView;

		private SlimDX.Direct3D11.Buffer m_Vertices;
		private SlimDX.Direct3D11.Buffer m_Indices;
		private VertexBufferBinding m_CubeBindings;
		private int m_IndexCount; 

		private Vector3 m_Size; 
		private Vector3 m_Location;
		private Quaternion m_Rotation;

		public Vector3 Location
		{
			get { return m_Location; }
			set { m_Location = value; }
		}

		public Quaternion Rotation
		{
			get { return m_Rotation; }
			set { m_Rotation = value; }
		}

		public Cube(float width, float height, float depth, string diffuseTexturePath, string diffuseLightMapPath, string specularLightMapPath)
		{
			if (Effect == null)
			{
				Effect = SharedEffects.Effects["Material"] as MaterialEffect; 
			}

			m_Size = new Vector3(width, height, depth); 

			m_DiffuseTexturePath = diffuseTexturePath;
			m_DiffuseLightMapPath = diffuseLightMapPath;
			m_SpecularLightMapPath = specularLightMapPath; // DiffuseCube.dds
			m_Disposed = true;			
		}

		public void Render(View3D view)
		{
			Matrix model = Matrix.RotationQuaternion(m_Rotation) * Matrix.Translation(m_Location);

			Effect.Render(view, model, m_DiffuseTextureView, m_DiffuseLightMapTextureView, m_SpecularLightMapTextureView, m_Vertices, m_Indices, m_IndexCount, m_CubeBindings);
			//Effect.RenderImposter(m_OverlayType, m_DiffuseTextureView); 
		}

		#region IResourceManager Members
		
		public bool Disposed { get { return m_Disposed; } }

		public void LoadResources()
		{
			if (m_Disposed == true)
			{
				m_DiffuseTexture = Texture2D.FromFile(GameEnvironment.Device, Helper.ResolvePath(m_DiffuseTexturePath));

				m_DiffuseTextureView = new ShaderResourceView(GameEnvironment.Device, m_DiffuseTexture);

				m_DiffuseLightMapTexture = Texture2D.FromFile(GameEnvironment.Device, Helper.ResolvePath(m_DiffuseLightMapPath));

				m_DiffuseLightMapTextureView = new ShaderResourceView(GameEnvironment.Device, m_DiffuseLightMapTexture);

				m_SpecularLightMapTexture = Texture2D.FromFile(GameEnvironment.Device, Helper.ResolvePath(m_SpecularLightMapPath));

				m_SpecularLightMapTextureView = new ShaderResourceView(GameEnvironment.Device, m_SpecularLightMapTexture);

				#region Create Volume Bounding Box

				float right = (float)m_Size.X * 0.5f;
				float front = (float)m_Size.Y * 0.5f;
				float top = (float)m_Size.Z * 0.5f;

				Vector3 TopLeftBack		= new Vector3(-right, -front,  top);
				Vector3 TopRightBack	= new Vector3( right, -front,  top);
				Vector3 TopLeftFront	= new Vector3(-right,  front,  top);
				Vector3 TopRightFront	= new Vector3( right,  front,  top);
				
				Vector3 BottomLeftBack	 = new Vector3(-right, -front, -top);
				Vector3 BottomRightBack  = new Vector3( right, -front, -top);
				Vector3 BottomLeftFront  = new Vector3(-right,  front, -top);
				Vector3 BottomRightFront = new Vector3( right,  front, -top);

				Vector3 NormalUp = new Vector3(0, 0, 1);
				Vector3 NormalDown = new Vector3(0, 0, -1);
				
				Vector3 NormalLeft = new Vector3(-1, 0, 0);
				Vector3 NormalRight = new Vector3(1, 0, 0);

				Vector3 NormalBack = new Vector3(0, -1, 0);
				Vector3 NormalFront = new Vector3(0, 1, 0);

				//Vector3[] faceNormals = new Vector3[6];

				//faceNormals[(int)FaceIndex.Top] = NormalUp;
				//faceNormals[(int)FaceIndex.Bottom] = NormalDown;
				//faceNormals[(int)FaceIndex.Left] = NormalLeft;
				//faceNormals[(int)FaceIndex.Right] = NormalRight;
				//faceNormals[(int)FaceIndex.Back] = NormalBack;
				//faceNormals[(int)FaceIndex.Front] = NormalFront;

				Vector3 TopLeftBack_Normal = MergeNomrals(NormalLeft, NormalUp, NormalBack);
				Vector3 TopRightBack_Normal = MergeNomrals(NormalRight, NormalUp, NormalBack);
				Vector3 TopLeftFront_Normal = MergeNomrals(NormalLeft, NormalUp, NormalFront);
				Vector3 TopRightFront_Normal = MergeNomrals(NormalRight, NormalUp, NormalFront);

				Vector3 BottomLeftBack_Normal = MergeNomrals(NormalLeft, NormalDown, NormalBack);
				Vector3 BottomRightBack_Normal = MergeNomrals(NormalRight, NormalDown, NormalBack);
				Vector3 BottomLeftFront_Normal = MergeNomrals(NormalLeft, NormalDown, NormalFront);
				Vector3 BottomRightFront_Normal = MergeNomrals(NormalRight, NormalDown, NormalFront);


				Vector2 TexTopLeft		= new Vector2(0, 0);
				Vector2 TexTopRight		= new Vector2(0, 1);
				Vector2 TexBottomLeft	= new Vector2(1, 0);
				Vector2 TexBottomRight	= new Vector2(1, 1);

				MaterialVertex[] verts = new MaterialVertex[4 * 6];

				int virt = 0;
				int faces = 0;

				faces++;
				//virt = faces++ * 4; //  (int)FaceIndex.Top * 4;
				virt = (int)FaceIndex.Top * 4;
				verts[virt++] = new MaterialVertex() { Position = TopLeftBack, DiffuseTextureCoords = TexTopLeft, Normal = NormalUp };
				verts[virt++] = new MaterialVertex() { Position = TopRightBack, DiffuseTextureCoords = TexBottomLeft, Normal = NormalUp };
				verts[virt++] = new MaterialVertex() { Position = TopLeftFront, DiffuseTextureCoords = TexTopRight, Normal = NormalUp };
				verts[virt++] = new MaterialVertex() { Position = TopRightFront, DiffuseTextureCoords = TexBottomRight, Normal = NormalUp };

				faces++;
				//virt = faces++ * 4; // (int)FaceIndex.Front * 4;
				virt = (int)FaceIndex.Front * 4;
				verts[virt++] = new MaterialVertex() { Position = TopLeftFront, DiffuseTextureCoords = TexTopLeft, Normal = NormalFront };
				verts[virt++] = new MaterialVertex() { Position = TopRightFront, DiffuseTextureCoords = TexTopRight, Normal = NormalFront };
				verts[virt++] = new MaterialVertex() { Position = BottomLeftFront, DiffuseTextureCoords = TexBottomLeft, Normal = NormalFront };
				verts[virt++] = new MaterialVertex() { Position = BottomRightFront, DiffuseTextureCoords = TexBottomRight, Normal = NormalFront };	 

				faces++;
				//virt = faces++ * 4; // (int)FaceIndex.Back * 4;
				virt = (int)FaceIndex.Back * 4;
				verts[virt++] = new MaterialVertex() { Position = TopRightBack, DiffuseTextureCoords = TexTopRight, Normal = NormalBack };
				verts[virt++] = new MaterialVertex() { Position = TopLeftBack, DiffuseTextureCoords = TexTopLeft, Normal = NormalBack };
				verts[virt++] = new MaterialVertex() { Position = BottomRightBack, DiffuseTextureCoords = TexBottomRight, Normal = NormalBack };
				verts[virt++] = new MaterialVertex() { Position = BottomLeftBack, DiffuseTextureCoords = TexBottomLeft, Normal = NormalBack };

				faces++;
				//virt = faces++ * 4; // (int)FaceIndex.Left * 4;
				virt = (int)FaceIndex.Left * 4;
				verts[virt++] = new MaterialVertex() { Position = TopLeftBack, DiffuseTextureCoords = TexTopLeft, Normal = NormalLeft };
				verts[virt++] = new MaterialVertex() { Position = TopLeftFront, DiffuseTextureCoords = TexTopRight, Normal = NormalLeft };
				verts[virt++] = new MaterialVertex() { Position = BottomLeftBack, DiffuseTextureCoords = TexBottomLeft, Normal = NormalLeft };
				verts[virt++] = new MaterialVertex() { Position = BottomLeftFront, DiffuseTextureCoords = TexBottomRight, Normal = NormalLeft };

				faces++;
				//virt = faces++ * 4; // (int)FaceIndex.Right * 4;				
				virt = (int)FaceIndex.Right * 4;
				verts[virt++] = new MaterialVertex() { Position = TopRightFront, DiffuseTextureCoords = TexTopRight, Normal = NormalRight };
				verts[virt++] = new MaterialVertex() { Position = TopRightBack, DiffuseTextureCoords = TexTopLeft, Normal = NormalRight };				
				verts[virt++] = new MaterialVertex() { Position = BottomRightFront, DiffuseTextureCoords = TexBottomRight, Normal = NormalRight };
				verts[virt++] = new MaterialVertex() { Position = BottomRightBack, DiffuseTextureCoords = TexBottomLeft, Normal = NormalRight };

				faces++;
				//virt = faces++ * 4; // (int)FaceIndex.Bottom * 4;
				virt = (int)FaceIndex.Bottom * 4;
				verts[virt++] = new MaterialVertex() { Position = BottomRightBack, DiffuseTextureCoords = TexTopLeft, Normal = NormalDown };		
				verts[virt++] = new MaterialVertex() { Position = BottomLeftBack, DiffuseTextureCoords = TexTopRight, Normal = NormalDown };				
				verts[virt++] = new MaterialVertex() { Position = BottomRightFront, DiffuseTextureCoords = TexBottomLeft, Normal = NormalDown };	
				verts[virt++] = new MaterialVertex() { Position = BottomLeftFront, DiffuseTextureCoords = TexBottomRight, Normal = NormalDown };				


				/* 
				faces++;
				//virt = faces++ * 4; //  (int)FaceIndex.Top * 4;
				virt = (int)FaceIndex.Top * 4;
				verts[virt++] = new MaterialVertex() { Position = TopLeftBack, DiffuseTextureCoords = TexTopLeft, Normal = TopLeftBack_Normal }; // NormalUp };
				verts[virt++] = new MaterialVertex() { Position = TopRightBack, DiffuseTextureCoords = TexBottomLeft, Normal = TopRightBack_Normal }; // NormalUp };
				verts[virt++] = new MaterialVertex() { Position = TopLeftFront, DiffuseTextureCoords = TexTopRight, Normal = TopLeftFront_Normal }; // NormalUp };
				verts[virt++] = new MaterialVertex() { Position = TopRightFront, DiffuseTextureCoords = TexBottomRight, Normal = TopRightFront_Normal }; //  NormalUp };

				faces++;
				//virt = faces++ * 4; // (int)FaceIndex.Front * 4;
				virt = (int)FaceIndex.Front * 4;
				verts[virt++] = new MaterialVertex() { Position = TopLeftFront, DiffuseTextureCoords = TexTopLeft, Normal = TopLeftFront_Normal }; // NormalFront };
				verts[virt++] = new MaterialVertex() { Position = TopRightFront, DiffuseTextureCoords = TexTopRight, Normal = TopRightFront_Normal }; // NormalFront };
				verts[virt++] = new MaterialVertex() { Position = BottomLeftFront, DiffuseTextureCoords = TexBottomLeft, Normal = BottomLeftFront_Normal }; // NormalFront };
				verts[virt++] = new MaterialVertex() { Position = BottomRightFront, DiffuseTextureCoords = TexBottomRight, Normal = BottomRightFront_Normal }; // NormalFront };	 

				faces++;
				//virt = faces++ * 4; // (int)FaceIndex.Back * 4;
				virt = (int)FaceIndex.Back * 4;
				verts[virt++] = new MaterialVertex() { Position = TopRightBack, DiffuseTextureCoords = TexTopRight, Normal = TopRightBack_Normal }; // NormalBack };
				verts[virt++] = new MaterialVertex() { Position = TopLeftBack, DiffuseTextureCoords = TexTopLeft, Normal = TopLeftBack_Normal }; // NormalBack };
				verts[virt++] = new MaterialVertex() { Position = BottomRightBack, DiffuseTextureCoords = TexBottomRight, Normal = BottomRightBack_Normal }; // NormalBack };
				verts[virt++] = new MaterialVertex() { Position = BottomLeftBack, DiffuseTextureCoords = TexBottomLeft, Normal = BottomLeftBack_Normal }; // NormalBack };

				faces++;
				//virt = faces++ * 4; // (int)FaceIndex.Left * 4;
				virt = (int)FaceIndex.Left * 4;
				verts[virt++] = new MaterialVertex() { Position = TopLeftBack, DiffuseTextureCoords = TexTopLeft, Normal = TopLeftBack_Normal }; // NormalLeft };
				verts[virt++] = new MaterialVertex() { Position = TopLeftFront, DiffuseTextureCoords = TexTopRight, Normal = TopLeftFront_Normal }; // NormalLeft };
				verts[virt++] = new MaterialVertex() { Position = BottomLeftBack, DiffuseTextureCoords = TexBottomLeft, Normal = BottomLeftBack_Normal }; // NormalLeft };
				verts[virt++] = new MaterialVertex() { Position = BottomLeftFront, DiffuseTextureCoords = TexBottomRight, Normal = BottomLeftFront_Normal }; // NormalLeft };

				faces++;
				//virt = faces++ * 4; // (int)FaceIndex.Right * 4;				
				virt = (int)FaceIndex.Right * 4;
				verts[virt++] = new MaterialVertex() { Position = TopRightFront, DiffuseTextureCoords = TexTopRight, Normal = TopRightFront_Normal }; // NormalRight };
				verts[virt++] = new MaterialVertex() { Position = TopRightBack, DiffuseTextureCoords = TexTopLeft, Normal = TopRightBack_Normal }; // NormalRight };				
				verts[virt++] = new MaterialVertex() { Position = BottomRightFront, DiffuseTextureCoords = TexBottomRight, Normal = BottomRightFront_Normal }; // NormalRight };
				verts[virt++] = new MaterialVertex() { Position = BottomRightBack, DiffuseTextureCoords = TexBottomLeft, Normal = BottomRightBack_Normal }; // NormalRight };

				faces++;
				//virt = faces++ * 4; // (int)FaceIndex.Bottom * 4;
				virt = (int)FaceIndex.Bottom * 4;
				verts[virt++] = new MaterialVertex() { Position = BottomRightBack, DiffuseTextureCoords = TexTopLeft, Normal = BottomRightBack_Normal }; // NormalDown };		
				verts[virt++] = new MaterialVertex() { Position = BottomLeftBack, DiffuseTextureCoords = TexTopRight, Normal = BottomLeftBack_Normal }; // NormalDown };				
				verts[virt++] = new MaterialVertex() { Position = BottomRightFront, DiffuseTextureCoords = TexBottomLeft, Normal = BottomRightFront_Normal }; // NormalDown };	
				verts[virt++] = new MaterialVertex() { Position = BottomLeftFront, DiffuseTextureCoords = TexBottomRight, Normal = BottomLeftFront_Normal }; // NormalDown };				
				*/ 
				int[] indices = new int[faces * 6];

				int ind = 0;
				for (int i = 0; i < faces; i++)
				{
					int baseIndex = i * 4;
					indices[ind++] = baseIndex + 0;
					indices[ind++] = baseIndex + 2;
					indices[ind++] = baseIndex + 1;

					indices[ind++] = baseIndex + 3;
					indices[ind++] = baseIndex + 1;
					indices[ind++] = baseIndex + 2;
				}


				/*
				verts[0] = new MaterialVertex() { Position = new Vector3(-right, -front, -top), DiffuseTextureCoords = new Vector2(0, 0), Normal = new Vector3() };
				verts[1] = new MaterialVertex() { Position = new Vector3(right, -front, -top), DiffuseTextureCoords = new Vector2(1, 0), Normal = new Vector3() };
				verts[2] = new MaterialVertex() { Position = new Vector3(-right, front, -top), DiffuseTextureCoords = new Vector2(0, 1), Normal = new Vector3() };
				verts[3] = new MaterialVertex() { Position = new Vector3(right, front, -top), DiffuseTextureCoords = new Vector2(1, 1), Normal = new Vector3() };

				verts[4] = new MaterialVertex() { Position = new Vector3(-right, -front,  top), DiffuseTextureCoords = new Vector2(0, 1), Normal = new Vector3() };
				verts[5] = new MaterialVertex() { Position = new Vector3( right, -front,  top), DiffuseTextureCoords = new Vector2(1, 1), Normal = new Vector3() };
				verts[6] = new MaterialVertex() { Position = new Vector3(-right,  front,  top), DiffuseTextureCoords = new Vector2(0, 0), Normal = new Vector3() };
				verts[7] = new MaterialVertex() { Position = new Vector3( right,  front,  top), DiffuseTextureCoords = new Vector2(1, 0), Normal = new Vector3() };

				int[] indices = new int[] { 
					// 0
					0, 1, 2, 
					1, 3, 2, 

					// 1
					0, 6, 4, 
					0, 2, 6, 

					// 2
					2, 7, 6, 
					3, 7, 2, 

					// 3
					1, 5, 7, 
					7, 3, 1, 

					// 4
					0, 4, 5, 
					5, 1, 0, 
 
					// 5
					4, 6, 5, 
					6, 7, 5
				};

				int[] indexToFaceMapping = new int[8 * 3] 
				{
					// 0
					0, 1, 4, 

					// 1
					0, 3, 4,

					// 2
					0, 1, 2, 

					// 3
					0, 2, 3, 

					// 4
					1, 4, 5,

					// 5
					3, 4, 5,
					
					// 6
					1, 2, 5,

					// 7					
					2, 1, 5
				};
				/*
				int itf = 0; 

				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < 6; j++)
					{
						int faceOffset = j * 6; 

						for (int k = 0; k < 6; k++)
						{

						}
					}
				}
				* / 				

				Vector3[] faceNormals = new Vector3[6];
				int fi = 0;

				for (int i = 0; i < indices.Length; i += 6)
				{
					Vector3 u = verts[indices[i + 1]].Position - verts[indices[i]].Position;
					Vector3 v = verts[indices[i + 2]].Position - verts[indices[i]].Position;

					faceNormals[fi++] = Vector3.Cross(u, v);
				}

				Vector3[] vertNormals = new Vector3[verts.Length];

				fi = 0;
				for (int i = 0; i < verts.Length; i++)
				{
					Vector3 n = faceNormals[indexToFaceMapping[fi++]];
					n += faceNormals[indexToFaceMapping[fi++]];
					n += faceNormals[indexToFaceMapping[fi++]];

					vertNormals[i] = n; 
				}
				
				/* for (int i = 0; i < indices.Length; i += 3)
				{
					Vector3 n = faceNormals[fi++];
					vertNormals[indices[i]] += n;
					vertNormals[indices[i + 1]] += n;
					vertNormals[indices[i + 2]] += n;
				}* / 

				for (int i = 0; i < verts.Length; i++)
				{
					vertNormals[i].Normalize();
					verts[i].Normal = vertNormals[i]; // *-1f; 
				}

				/* 
				Vector3[] faceNormals = new Vector3[12];
				int fi = 0; 

				for (int i = 0; i < indices.Length; i += 3)
				{
					Vector3 u = verts[indices[i + 1]].Position - verts[indices[i]].Position;
					Vector3 v = verts[indices[i + 2]].Position - verts[indices[i]].Position;

					faceNormals[fi++] = Vector3.Cross(u, v);
				}

				Vector3[] vertNormals = new Vector3[verts.Length];

				fi = 0; 
				for (int i = 0; i < indices.Length; i += 3)
				{
					Vector3 n = faceNormals[fi++]; 
					vertNormals[indices[i]] += n;
					vertNormals[indices[i + 1]] += n;
					vertNormals[indices[i + 2]] += n;
				}

				for (int i = 0; i < verts.Length; i++)
				{
					vertNormals[i].Normalize();
					verts[i].Normal = vertNormals[i]; // *-1f; 
				}
				*/

				m_IndexCount = indices.Length;

				using (DataStream stream = new DataStream(verts.Length * Marshal.SizeOf(typeof(MaterialVertex)), false, true))
				{
					stream.WriteRange(verts);

					stream.Position = 0;

					m_Vertices = new SlimDX.Direct3D11.Buffer(GameEnvironment.Device, stream, new BufferDescription()
					{
						BindFlags = BindFlags.VertexBuffer,
						CpuAccessFlags = CpuAccessFlags.None,
						OptionFlags = ResourceOptionFlags.None,
						SizeInBytes = verts.Length * Marshal.SizeOf(typeof(MaterialVertex)),
						Usage = ResourceUsage.Default
					});
				}

				m_CubeBindings = new VertexBufferBinding(m_Vertices, Marshal.SizeOf(typeof(MaterialVertex)), 0);

				using (DataStream stream = new DataStream(indices.Length * sizeof(int), true, true))
				{
					stream.WriteRange(indices);
					stream.Position = 0;

					m_Indices = new SlimDX.Direct3D11.Buffer(GameEnvironment.Device, stream, new BufferDescription()
					{
						BindFlags = BindFlags.IndexBuffer,
						CpuAccessFlags = CpuAccessFlags.None,
						OptionFlags = ResourceOptionFlags.None,
						SizeInBytes = indices.Length * sizeof(int),
						Usage = ResourceUsage.Default
					});
				}

				#endregion
				
			}
		}

		private Vector3 MergeNomrals(Vector3 NormalLeft, Vector3 NormalDown, Vector3 NormalBack)
		{
			Vector3 n = NormalLeft + NormalDown + NormalBack;
			
			n.Normalize(); 

			return n; 
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				m_Disposed = true;
				
				m_DiffuseTexture.Dispose();
				m_DiffuseTextureView.Dispose();
				
				m_DiffuseLightMapTexture.Dispose();
				m_DiffuseLightMapTextureView.Dispose();

				m_SpecularLightMapTexture.Dispose();
				m_SpecularLightMapTextureView.Dispose(); 
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			UnloadResources();
		}

		#endregion

		/* 
		void ModelClass::CalculateTangentBinormal(TempVertexType vertex1, TempVertexType vertex2, TempVertexType vertex3,
										  VectorType& tangent, VectorType& binormal)
		{
			float vector1[3], vector2[3];
			float tuVector[2], tvVector[2];
			float den;
			float length;


			// Calculate the two vectors for this face.
			vector1[0] = vertex2.x - vertex1.x;
			vector1[1] = vertex2.y - vertex1.y;
			vector1[2] = vertex2.z - vertex1.z;

			vector2[0] = vertex3.x - vertex1.x;
			vector2[1] = vertex3.y - vertex1.y;
			vector2[2] = vertex3.z - vertex1.z;

			// Calculate the tu and tv texture space vectors.
			tuVector[0] = vertex2.tu - vertex1.tu;
			tvVector[0] = vertex2.tv - vertex1.tv;

			tuVector[1] = vertex3.tu - vertex1.tu;
			tvVector[1] = vertex3.tv - vertex1.tv;

			// Calculate the denominator of the tangent/binormal equation.
			den = 1.0f / (tuVector[0] * tvVector[1] - tuVector[1] * tvVector[0]);

			// Calculate the cross products and multiply by the coefficient to get the tangent and binormal.
			tangent.x = (tvVector[1] * vector1[0] - tvVector[0] * vector2[0]) * den;
			tangent.y = (tvVector[1] * vector1[1] - tvVector[0] * vector2[1]) * den;
			tangent.z = (tvVector[1] * vector1[2] - tvVector[0] * vector2[2]) * den;

			binormal.x = (tuVector[0] * vector2[0] - tuVector[1] * vector1[0]) * den;
			binormal.y = (tuVector[0] * vector2[1] - tuVector[1] * vector1[1]) * den;
			binormal.z = (tuVector[0] * vector2[2] - tuVector[1] * vector1[2]) * den;

			// Calculate the length of this normal.
			length = sqrt((tangent.x * tangent.x) + (tangent.y * tangent.y) + (tangent.z * tangent.z));
			
			// Normalize the normal and then store it
			tangent.x = tangent.x / length;
			tangent.y = tangent.y / length;
			tangent.z = tangent.z / length;

			// Calculate the length of this normal.
			length = sqrt((binormal.x * binormal.x) + (binormal.y * binormal.y) + (binormal.z * binormal.z));
			
			// Normalize the normal and then store it
			binormal.x = binormal.x / length;
			binormal.y = binormal.y / length;
			binormal.z = binormal.z / length;

			return;
		}


		void ModelClass::CalculateNormal(VectorType tangent, VectorType binormal, VectorType& normal)
		{
			float length;


			// Calculate the cross product of the tangent and binormal which will give the normal vector.
			normal.x = (tangent.y * binormal.z) - (tangent.z * binormal.y);
			normal.y = (tangent.z * binormal.x) - (tangent.x * binormal.z);
			normal.z = (tangent.x * binormal.y) - (tangent.y * binormal.x);

			// Calculate the length of the normal.
			length = sqrt((normal.x * normal.x) + (normal.y * normal.y) + (normal.z * normal.z));

			// Normalize the normal.
			normal.x = normal.x / length;
			normal.y = normal.y / length;
			normal.z = normal.z / length;

			return;
		}
		 */
		
	}
}
