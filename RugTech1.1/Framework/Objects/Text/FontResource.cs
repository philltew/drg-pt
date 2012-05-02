using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace RugTech1.Framework.Objects.Text
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[DescriptionAttribute("Expand to edit font properties")]
	public class FontResource : IResourceManager
	{
		private bool m_Disposed = true;

		private string m_FontName = "Arial";
		private FontStyle m_Style = FontStyle.Regular;
		private float m_Size = 12f;
		private GraphicsUnit m_Unit = GraphicsUnit.Pixel;
		private FontType m_FontType = FontType.Small;
		private System.Drawing.Text.TextRenderingHint m_RenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

		private System.Drawing.Font m_Font;

		public System.Drawing.Text.TextRenderingHint RenderingHint
		{
			get { return m_RenderingHint; }
			set { m_RenderingHint = value; }
		} 

		[Browsable(false)]
		public FontType FontType
		{
			get { return m_FontType; }
			set { m_FontType = value; }
		} 
		
		public string FontName
		{
			get { return m_FontName; }
			set { m_FontName = value; }
		}

		public FontStyle Style
		{
			get { return m_Style; }
			set { m_Style = value; }
		}

		public float Size
		{
			get { return m_Size; }
			set { m_Size = value; }
		}

		public GraphicsUnit Unit
		{
			get { return m_Unit; }
			set { m_Unit = value; }
		}

		[Browsable(false)]
		public System.Drawing.Font Font
		{
			get { return m_Font; }
		}

		public FontResource()
		{

		}

		public FontResource(FontType type)
		{
			m_FontType = type; 
		}

		#region IResourceManager Members
		
		[Browsable(false)]
		public bool Disposed
		{
			get { return m_Disposed; }
		}

		public void LoadResources()
		{
			if (m_Disposed == true)
			{
				m_Font = new System.Drawing.Font(FontName, Size, Style, Unit);

				m_Disposed = false;
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				m_Font.Dispose(); 

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

		internal void Write(System.IO.BinaryWriter writer)
		{
			writer.Write(m_FontName);
			writer.Write((int)m_Style);
			writer.Write(m_Size);
			writer.Write((int)m_Unit);
			writer.Write((int)m_FontType);
			writer.Write((int)m_RenderingHint); 
		}

		internal void Read(System.IO.BinaryReader reader)
		{
			m_FontName = reader.ReadString();
			m_Style = (FontStyle)reader.ReadUInt32();
			m_Size = reader.ReadSingle();
			m_Unit = (GraphicsUnit)reader.ReadUInt32();
			m_FontType = (FontType)reader.ReadUInt32();
			m_RenderingHint = (System.Drawing.Text.TextRenderingHint)reader.ReadUInt32();
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}, {2}, {3}, {4}", m_FontName, m_Style, m_Size, m_Unit, m_RenderingHint);
		}
	}
}
