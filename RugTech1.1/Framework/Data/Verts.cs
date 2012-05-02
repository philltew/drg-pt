using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace RugTech1.Framework.Data
{
	public struct MeshVertex
	{
		public Vector3 Position;
		public Vector4 Normal;
		public Color4 Color;
	}

	public struct StarInstanceVertex
	{
		public Vector3 Position;
		public Vector4 Color;		
	}

	public struct LineVertex
	{
		public Vector4 Position;
		public Vector4 Color;
	}

	public struct TexturedVertex
	{
		public Vector4 Position;
		public Vector2 TextureCoords;
	}

	public struct UIVertex
	{
		public Vector3 Position;
		public Vector2 TextureCoords;
		public Color4 Color;		
	}

	public struct Vertex2D
	{
		public Vector2 Position;
		public Vector2 TextureCoords;		
	}

	public struct VolumeVertex
	{
		public Vector3 Position;
		public Vector3 Color;
	}

	public struct MaterialVertex
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Vector2 DiffuseTextureCoords;		
	}
}
