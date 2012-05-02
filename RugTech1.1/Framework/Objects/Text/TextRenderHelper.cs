using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using RugTech1.Framework.Data;
using RugTech1.Framework.Effects;
using RugTech1.Framework.Objects.Text;
using RugTech1.Framework.Objects.UI;
using SlimDX;
using Rug.Cmd;
using System.Text.RegularExpressions;

namespace RugTech1.Framework.Objects.Text
{
	public static class TextRenderHelper 
	{
		public static UiEffect Effect;

		static TextRenderHelper()
		{
			Effect = UiScene.Effect; 
		}

		#region MessureString

		public static SizeF MessureString(string str, FontType fontType, float scale)
		{
			float width = 0, height = 0;

			foreach (char @char in str)
			{
				RectangleF rect = Effect.TextFont[fontType, @char];

				if (rect.Height > height)
				{
					height = rect.Height;
				}

				width += rect.Width;
			}

			width *= Effect.TextFont.TextureSize * scale;
			height *= Effect.TextFont.TextureSize * scale;

			return new SizeF(width, height);
		}

		public static SizeF MessureString(string str, FontType fontType, View3D view, float scale)
		{
			float width = 0, height = 0;

			foreach (char @char in str)
			{
				RectangleF rect = Effect.TextFont[fontType, @char];

				if (rect.Height > height)
				{
					height = rect.Height; 
				}

				width += rect.Width; 
			}

			width *= Effect.TextFont.TextureSize * scale;
			height *= Effect.TextFont.TextureSize * scale;

			return new SizeF(width, height);
		}

		public static SizeF MessureString(int maxLength, FontType fontType, View3D view, float scale)
		{
			float width = 0, height = 0;

			Rectangle charSize = Effect.TextFont[fontType];

			height = (float)charSize.Height;
			width = (float)charSize.Width * maxLength; 

			width *= scale;
			height *= scale;

			return new SizeF(width, height);
		}
		
		#endregion

		#region Get Total Element Counts

		public static void GetTotalElementCounts(string str, out int indexCount, out int triangleCount)
		{
			int len = str.Length;

			indexCount = len * 6;
			triangleCount = len * 4; 
		}

		public static void GetTotalElementCounts(int maxLength, out int indexCount, out int triangleCount)
		{
			indexCount = maxLength * 6;
			triangleCount = maxLength * 4;
		}

		#endregion

		#region Write String

		public static void WriteString(View3D view,
													RectangleF RemainingBounds,
													string str, FontType fontType, Vector3 location, float scale, Color4 color, 
													SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount, 
													SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			float x, y, w, h;

			UiStyleHelper.CovertToVertCoords(RemainingBounds, view.WindowSize, view.PixelSize, out x, out y, out w, out h);

			x += location.X;
			y += location.Y; 
			float z = location.Z;

			foreach (char @char in str)
			{
				RectangleF rect = Effect.TextFont[fontType, @char];

				w = rect.Width * Effect.TextFont.TextureSize * view.PixelSize.X * scale;
				h = rect.Height * Effect.TextFont.TextureSize * view.PixelSize.Y * scale; 

				TriangleVerts.WriteRange(new UIVertex[] { 
					new UIVertex() { Color = color, Position = new Vector3(x, y, z), TextureCoords = new Vector2(rect.Left, rect.Top) },
					new UIVertex() { Color = color, Position = new Vector3(x + w, y, z), TextureCoords = new Vector2(rect.Right, rect.Top) },
					new UIVertex() { Color = color, Position = new Vector3(x, y - h, z), TextureCoords = new Vector2(rect.Left, rect.Bottom) },
					new UIVertex() { Color = color, Position = new Vector3(x + w, y - h, z), TextureCoords = new Vector2(rect.Right, rect.Bottom) }
				});

				int i = TriangleVertsCount;

				TriangleIndices.WriteRange(new int[] { 
					i + 0, i + 1, i + 2,
					i + 1, i + 3, i + 2
				});

				TriangleVertsCount += 4;
				TriangleIndicesCount += 6;

				x += w; 
			}
		}

		public static void WriteString(View3D view,
											RectangleF RemainingBounds,
											string str, FontType fontType, ref float px, ref float py, float z, float scale, Color4 color,
											SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount,
											SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			float x, y, w, h;
			float pw, ph; 

			//UiStyleHelper.CovertToVertCoords(px, py, view.WindowSize, view.PixelSize, out x, out y);
			UiStyleHelper.CovertToVertCoords(RemainingBounds, view.WindowSize, view.PixelSize, out x, out y, out w, out h);

			x += px;
			y += py; 

			foreach (char @char in str)
			{
				RectangleF rect = Effect.TextFont[fontType, @char];

				//pw = rect.Width * Effect.TextFont.TextureSize * scale;
				//ph = rect.Height * Effect.TextFont.TextureSize * scale;

				w = rect.Width * Effect.TextFont.TextureSize * view.PixelSize.X * scale;
				h = rect.Height * Effect.TextFont.TextureSize * view.PixelSize.Y * scale;

				TriangleVerts.WriteRange(new UIVertex[] { 
					new UIVertex() { Color = color, Position = new Vector3(x, y, z), TextureCoords = new Vector2(rect.Left, rect.Top) },
					new UIVertex() { Color = color, Position = new Vector3(x + w, y, z), TextureCoords = new Vector2(rect.Right, rect.Top) },
					new UIVertex() { Color = color, Position = new Vector3(x, y - h, z), TextureCoords = new Vector2(rect.Left, rect.Bottom) },
					new UIVertex() { Color = color, Position = new Vector3(x + w, y - h, z), TextureCoords = new Vector2(rect.Right, rect.Bottom) }
				});

				int i = TriangleVertsCount;

				TriangleIndices.WriteRange(new int[] { 
					i + 0, i + 1, i + 2,
					i + 1, i + 3, i + 2
				});

				TriangleVertsCount += 4;
				TriangleIndicesCount += 6;

				x += w;
				px += w;
			}
		}

		#endregion

		#region Write Interpreted

		public static void WriteInterpreted(View3D view,
											RectangleF RemainingBounds, SizeF lineSize, int maxWidth, 
											string buffer, FontType fontType, Vector3 start, float scale, 
											ConsoleColorExt foregroundColor, Color4[] colorLookup,
											SlimDX.DataStream TriangleVerts, ref int TriangleVertsCount,
											SlimDX.DataStream TriangleIndices, ref int TriangleIndicesCount)
		{
			string prefix = ""; 
			int lastIndex = 0;
			Stack<ConsoleColorExt> stack = new Stack<ConsoleColorExt>();

			ConsoleColorExt activeForegroundColor = foregroundColor;

			float x = start.X, y = start.Y; 

			foreach (Match match in ConsoleFormatter.FormatRegex.Matches(buffer))
			{
				if (match.Groups["Tag"].Success)
				{
					// start tag 
					// parse the tags inner value
					// e.g. c:# (where # is the name or ID of the colour to use (From ConsoleColourExt) 
					// add the parsed colour to the stack 

					ConsoleColorExt col = ConsoleFormatter.ParseColour(match.Groups["Inner"].Value, RC.Theme);

					stack.Push(activeForegroundColor);

					activeForegroundColor = col;
				}
				else if (match.Groups["EndTag"].Success)
				{
					// end tag 
					// handle stack changes
					if (stack.Count >= 0)
					{
						activeForegroundColor = stack.Pop();
					}
					else
					{
						throw new Exception(string.Format("Unexpected end tag at index {0}", match.Index));
					}
				}
				else if (match.Groups["Text"].Success)
				{
					string wholeMessage = ConsoleFormatter.UnescapeString(match.Value);

					List<string> lines = ConsoleFormatter.SplitLinebreaks(wholeMessage);

					foreach (string line in lines)
					{
						string str = line;

						if (str == "\n" || str == Environment.NewLine)
						{
							//BufferWriteLine("");
							x = start.X;
							y += lineSize.Height; 

							lastIndex = 0;
						}
						else
						{
							while (str.Length > 0)
							{
								if (lastIndex + str.Length > maxWidth)
								{
									int lastWord = str.LastIndexOf(' ', maxWidth - lastIndex, maxWidth - lastIndex);

									if (lastWord <= 0)
									{
										lastWord = maxWidth - lastIndex;
									}

									string toRender;

									if (lastIndex > 0)
									{
										toRender = str.Substring(0, lastWord);
									}
									else
									{
										toRender = prefix + str.Substring(0, lastWord);
									}

									if (maxWidth == lastIndex + toRender.Length)
									{
										WriteString(view,
													RemainingBounds,
													toRender, fontType, ref x, ref y, start.Z, scale, colorLookup[(int)activeForegroundColor],
													TriangleVerts, ref TriangleVertsCount,
													TriangleIndices, ref TriangleIndicesCount);
										
										//BufferWrite(toRender);
									}
									else
									{
										WriteString(view,
													RemainingBounds,
													toRender, fontType, ref x, ref y, start.Z, scale, colorLookup[(int)activeForegroundColor],
													TriangleVerts, ref TriangleVertsCount,
													TriangleIndices, ref TriangleIndicesCount);

//										BufferWriteLine(toRender);
										
										x = start.X;
										y += lineSize.Height; 
									}

									str = str.Substring(lastWord + 1);

									lastIndex = 0;
								}
								else
								{
									string toRender = str;

									if (lastIndex <= 0)
									{
										toRender = prefix + str;
									}

									//BufferWrite(toRender);
									WriteString(view,
													RemainingBounds,
													toRender, fontType, ref x, ref y, start.Z, scale, colorLookup[(int)activeForegroundColor],
													TriangleVerts, ref TriangleVertsCount,
													TriangleIndices, ref TriangleIndicesCount);
										
									lastIndex += str.Length;

									str = "";
								}
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
