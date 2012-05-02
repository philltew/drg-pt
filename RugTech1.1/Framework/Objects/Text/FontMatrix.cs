using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;
using System.ComponentModel;

namespace RugTech1.Framework.Objects.Text
{
	public enum FontType { XLarge = 0, Large = 1, Heading = 2, Regular = 3, Small = 4, Monospaced = 5 }

	public class FontMatrix : IResourceManager
	{
		public static readonly char[] AllChars = new char[] { 
				'0','1','2','3','4','5','6','7','8','9', 
				'a', 'b', 'c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
				'A', 'B', 'C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
				'!','"','$','%','^','&','*','(',')','-','+','_','=','`','¬','\\','|',',','.','/','<','>','?',';',':','\'','@',
				'~','#','[',']','{','}',' ',
			};
	
		#region Private Members

		private bool m_Disposed = true;
		private int m_TextureSize = 512;

		private Rectangle[] m_BiggestCharSize = new Rectangle[6]; 
		private Dictionary<char, RectangleF>[] m_CharLookup = new Dictionary<char, RectangleF>[6];
		private Texture2D m_Texture;
		private ShaderResourceView m_TextureView;

		private FontResource m_XLarge = new FontResource(FontType.XLarge);
		private FontResource m_Large = new FontResource(FontType.Large);
		private FontResource m_Heading = new FontResource(FontType.Heading);
		private FontResource m_Regular = new FontResource(FontType.Regular);
		private FontResource m_Small = new FontResource(FontType.Small);
		private FontResource m_Monospaced = new FontResource(FontType.Monospaced);

		#endregion

		#region Public Properties / Members

		// [DefaultValue(true)]
		[Browsable(false)]
		public int TextureSize { get { return m_TextureSize; } set { m_TextureSize = value; } }

		[Browsable(false)]
		public Texture2D Texture { get { return m_Texture; } }
		
		[Browsable(false)]
		public ShaderResourceView TexureView { get { return m_TextureView; } }

		[Browsable(true)]
		public FontResource XLarge { get { return m_XLarge; } set { m_XLarge = value; } }
		[Browsable(true)]
		public FontResource Large { get { return m_Large; } set { m_Large = value; } }
		[Browsable(true)]
		public FontResource Heading { get { return m_Heading; } set { m_Heading = value; } }
		[Browsable(true)]
		public FontResource Regular { get { return m_Regular; } set { m_Regular = value; } }
		[Browsable(true)]
		public FontResource Small { get { return m_Small; } set { m_Small = value; } }
		[Browsable(true)]
		public FontResource Monospaced { get { return m_Monospaced; } set { m_Monospaced = value; } }

		[Browsable(false)]
		private Bitmap m_Bitmap;

		[Browsable(false)]
		public Rectangle this[FontType type]
		{
			get
			{
				return m_BiggestCharSize[(int)type]; 
			}
		}

		[Browsable(false)]
		public RectangleF this[FontType type, char @char]
		{
			get 
			{
				RectangleF rect;

				Dictionary<char, RectangleF> charLookup;

				charLookup = m_CharLookup[(int)type];
				
				if (charLookup.TryGetValue(@char, out rect))
				{
					return rect;
				}
				else
				{
					return charLookup['?'];
				}				
			}			
		}

		#endregion

		public FontMatrix() { }

		#region IResourceManager Members

		[Browsable(false)]
		public bool Disposed { get { return m_Disposed;  } }

		public void LoadResources()
		{
			if (m_Disposed == true)
			{
				XLarge.LoadResources();
				Large.LoadResources();
				Heading.LoadResources(); 
				Regular.LoadResources();
				Small.LoadResources();
				Monospaced.LoadResources(); 

				RenderToTexture(); 

				m_Disposed = false; 
			}
		}

		public void UnloadResources()
		{
			if (m_Disposed == false)
			{
				XLarge.UnloadResources();
				Large.UnloadResources();
				Heading.UnloadResources();				
				Regular.UnloadResources();
				Small.UnloadResources();
				Monospaced.UnloadResources(); 

				m_Texture.Dispose();
				m_TextureView.Dispose(); 

				m_Disposed = true;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			UnloadResources();

			if (m_Bitmap != null)
			{
				m_Bitmap.Dispose();
				m_Bitmap = null; 
			}
		}

		#endregion

		#region Render To Bitmap

		public void RenderToTexture()
		{
			if (m_Bitmap == null)
			{
				using (Bitmap bmp = RenderToBitmap())
				{
					RenderToTexture(bmp);
				}
			}
			else
			{
				RenderToTexture(m_Bitmap);
			}
		}

		public void RenderToTexture(Bitmap bmp)
		{
			BitmapData data = null;
				
			try 
			{
				data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

				byte[] dest = new byte[data.Stride * bmp.Height]; 
					
				Marshal.Copy(data.Scan0, dest, 0, dest.Length);
					
				m_Texture = new Texture2D(GameEnvironment.Device, new Texture2DDescription()
				{
					Format =  SlimDX.DXGI.Format.R16G16B16A16_Float, // .R16_Float,
					Width = bmp.Width,
					Height = bmp.Height,
					MipLevels = 1,
					ArraySize = 1, 
					BindFlags = BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					Usage = ResourceUsage.Dynamic, 
					SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0)
				});

				//DataRectangle dataRect = m_Texture.Map(0, MapMode.WriteDiscard, MapFlags.None);
				SlimDX.DXGI.Surface surface = m_Texture.AsSurface();
				DataRectangle dataRect = surface.Map(SlimDX.DXGI.MapFlags.Write | SlimDX.DXGI.MapFlags.Discard); ;
				float cScale = 1f / 255f;

				for (int i = 0; i < bmp.Width * bmp.Height * 4; i += 4)
				{
					dataRect.Data.Write(new Half4(new Half((float)dest[i + 0] * cScale), new Half((float)dest[i + 1] * cScale), new Half((float)dest[i + 2] * cScale), new Half((float)dest[i + 3] * cScale)));
				}

				//m_Texture.Unmap(0); 
				surface.Unmap();
				surface.Dispose(); 

				m_TextureView = new ShaderResourceView(GameEnvironment.Device, m_Texture, new ShaderResourceViewDescription()
				{
					Dimension = ShaderResourceViewDimension.Texture2D,
					Format = SlimDX.DXGI.Format.R16G16B16A16_Float, //.R16_Float, 
					ArraySize = 1,
					MipLevels = 1,
					MostDetailedMip = 0, 							 
				});
				
			}
			finally 
			{
				if (data != null) { bmp.UnlockBits(data); }
			}
		}


		public Bitmap RenderToBitmap()
		{
			/* 
			char[] AllChars = new char[] { 
				'0','1','2','3','4','5','6','7','8','9', 
				'a', 'b', 'c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
				'A', 'B', 'C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
				'!','"','$','%','^','&','*','(',')','-','+','_','=','`','¬','\\','|',',','.','/','<','>','?',';',':','\'','@',
				'~','#','[',']','{','}',' ',
			};
			*/ 
			int bmpSize = m_TextureSize;
			float tSize = 1f / (float)bmpSize;

			Bitmap bmp = new Bitmap(bmpSize, bmpSize, PixelFormat.Format32bppArgb);

			using (Graphics graphics = Graphics.FromImage(bmp))
			{
				graphics.Clear(Color.Transparent);

				FontResource[] resources = new FontResource[] { XLarge, Large, Heading, Regular, Small, Monospaced };

				float x = 0f, y = 0f;

				bool first = true;
				foreach (FontResource res in resources)
				{
					if (res.Disposed)
					{
						res.LoadResources(); 
					}

					graphics.TextRenderingHint = res.RenderingHint;

					float w = 0, h = 0;
					float hScaleFactor = 1.1f; 

					Dictionary<char, RectangleF> lookup = new Dictionary<char, RectangleF>();

					foreach (char @char in AllChars)
					{
						SizeF size = graphics.MeasureString(@char.ToString(), res.Font, bmpSize, StringFormat.GenericTypographic);
						if (size.Width > w) { w = size.Width; }
						if (size.Height > h) { h = size.Height; }
					}


					//Rectangle maxCharBounds = new Rectangle(0, 0, (int)(w + 0.5f), (int)(h + 0.5f));
					Rectangle maxCharBounds = new Rectangle(0, 0, (int)(w + 0.5f), (int)(h * hScaleFactor));

					if (first == true)
					{
						graphics.FillRectangle(Brushes.White, maxCharBounds);

						x += maxCharBounds.Width;

						int diff = 8 - (maxCharBounds.Width % 8);
						
						x += diff; 

						first = false;
					}

					float xScale = 1f;

					foreach (char @char in AllChars)
					{
						if (@char == ' ')
						{
							float spaceWidth;

							if (res.FontType == FontType.Monospaced)
							{
								spaceWidth = maxCharBounds.Width;
							}
							else
							{
								spaceWidth = maxCharBounds.Width * 0.33f;
							}

							lookup.Add(@char, new RectangleF(x * tSize, y * tSize, spaceWidth * tSize * xScale, (float)maxCharBounds.Height * tSize));

							x += (int)(spaceWidth + 0.5f);

							int diff = 8 - ((int)(spaceWidth + 0.5f) % 8);

							x += diff; 
						}
						else
						{
							SizeF currentSize = graphics.MeasureString(@char.ToString(), res.Font, maxCharBounds.Width, StringFormat.GenericTypographic);

							graphics.DrawString(@char.ToString(), res.Font, Brushes.White, new PointF(x, y), StringFormat.GenericTypographic);

							lookup.Add(@char, new RectangleF(x * tSize, y * tSize, currentSize.Width * tSize * xScale, (float)maxCharBounds.Height * tSize));

							x += (int)(currentSize.Width + 0.5f);

							int diff = 8 - ((int)(currentSize.Width + 0.5f) % 8);

							x += diff; 
						}

						if (x + maxCharBounds.Width + (8 - (maxCharBounds.Width % 8)) > bmpSize)
						{
							x = 0f;
							y += maxCharBounds.Height;

							int diff = 8 - (maxCharBounds.Height % 8);

							y += diff; 
						}
					}

					x = 0f;
					y += maxCharBounds.Height;

					int diffy = 8 - (maxCharBounds.Height % 8);

					y += diffy; 

					m_BiggestCharSize[(int)res.FontType] = maxCharBounds;
					m_CharLookup[(int)res.FontType] = lookup;
				}
			}

			/*
			BitmapData data = null;

			try
			{
				data = bmp.LockBits(new Rectangle(0, 0, bmpSize, bmpSize), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

				byte[] dest = new byte[data.Stride * bmpSize];

				Marshal.Copy(data.Scan0, dest, 0, dest.Length);

				m_Texture = new Texture2D(GameEnvironment.Device, new Texture2DDescription()
				{
					Format = SlimDX.DXGI.Format.R16G16B16A16_Float, // .R16_Float,
					Width = bmpSize,
					Height = bmpSize,
					MipLevels = 1,
					ArraySize = 1,
					BindFlags = BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					Usage = ResourceUsage.Dynamic,
					SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0)
				});

				DataRectangle dataRect = m_Texture.Map(0, MapMode.WriteDiscard, MapFlags.None);
				float cScale = 1f / 255f;

				for (int i = 0; i < bmpSize * bmpSize * 4; i += 4)
				{
					dataRect.Data.Write(new Half4(new Half((float)dest[i + 0] * cScale), new Half((float)dest[i + 1] * cScale), new Half((float)dest[i + 2] * cScale), new Half((float)dest[i + 3] * cScale)));
				}

				m_Texture.Unmap(0);

				m_TextureView = new ShaderResourceView(GameEnvironment.Device, m_Texture, new ShaderResourceViewDescription()
				{
					Dimension = ShaderResourceViewDimension.Texture2D,
					Format = SlimDX.DXGI.Format.R16G16B16A16_Float, //.R16_Float, 
					ArraySize = 1,
					MipLevels = 1,
					MostDetailedMip = 0,
				});

			}
			finally
			{
				if (data != null) { bmp.UnlockBits(data); }
			}
			*/ 
			//bmp.Save(@"c:\test.png", ImageFormat.Png);

			return bmp; 
		}

		#endregion

		#region Write / Read

		public void Write(string imagePath)
		{
			using (FileStream stream = new FileStream(imagePath, FileMode.Create))
			{
				using (GZipStream gzip = new GZipStream(stream, CompressionMode.Compress))
				{
					using (Bitmap bmp = RenderToBitmap())
					{
						using (BinaryWriter writer = new BinaryWriter(gzip))
						{
							writer.Write("FontMatrix001");
							writer.Write(m_TextureSize);

							FontResource[] resources = new FontResource[] { XLarge, Large, Heading, Regular, Small, Monospaced };

							writer.Write(resources.Length);
						
							for (int i = 0; i < resources.Length; i++)
							{
								resources[i].Write(writer);

								StructHelper.WriteStructure(writer, m_BiggestCharSize[i]);
							
								Dictionary<char, RectangleF> lookup = m_CharLookup[i];

								writer.Write(lookup.Count);

								foreach (KeyValuePair<char, RectangleF> rect in lookup)
								{
									writer.Write(rect.Key);

									StructHelper.WriteStructure(writer, rect.Value);
								}
							}

							using (Stream imageStream = new MemoryStream())
							{
								bmp.Save(imageStream, ImageFormat.Png);

								writer.Write((int)imageStream.Length);
								
								imageStream.Position = 0; 

								imageStream.CopyTo(gzip, 4096);
							}
						}
					}
				}
			}
		}

		public static FontMatrix Read(string imagePath)
		{
			using (FileStream stream = new FileStream(imagePath, FileMode.Open))
			{
				using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress))
				{
					FontMatrix matrix = new FontMatrix(); 

					using (BinaryReader reader = new BinaryReader(gzip))
					{
						string formatName = reader.ReadString();

						if (formatName != "FontMatrix001")
						{
							throw new Exception("Font Matrix format is incorrect '" + formatName + "'"); 
						}

						matrix.TextureSize = reader.ReadInt32(); 

						matrix.XLarge = new FontResource(); 
						matrix.Large = new FontResource(); 
						matrix.Heading = new FontResource(); 
						matrix.Regular = new FontResource(); 
						matrix.Small = new FontResource(); 
						matrix.Monospaced = new FontResource(); 

						FontResource[] resources = new FontResource[] { matrix.XLarge, matrix.Large, matrix.Heading, matrix.Regular, matrix.Small, matrix.Monospaced };
						reader.ReadInt32();

						for (int i = 0; i < resources.Length; i++)
						{
							resources[i].Read(reader);

							matrix.m_BiggestCharSize[i] = StructHelper.ReadStructure<Rectangle>(reader);

							int count = reader.ReadInt32();

							Dictionary<char, RectangleF> lookup = new Dictionary<char, RectangleF>(count);

							for (int c = 0; c < count; c++)
							{
								char @char = reader.ReadChar(); 

								RectangleF rect = StructHelper.ReadStructure<RectangleF>(reader);
								
								lookup.Add(@char, rect); 
							}

							matrix.m_CharLookup[i] = lookup; 
						}

						int length = reader.ReadInt32(); 

						using (Stream imageStream = new MemoryStream(reader.ReadBytes(length)))
						{
							Bitmap bmp = (Bitmap)Bitmap.FromStream(imageStream);

							// bmp.Save(@"c:\test2.png", ImageFormat.Png);

							matrix.m_Bitmap = bmp;
						}											
					}

					return matrix; 
				}
			}
		}

		#endregion
	}
}
