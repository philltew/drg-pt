using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace RugTech1
{
    public static class Helper
	{
		#region Path Helpers

		public static string ApplicationRootPath { get { return new FileInfo(System.Windows.Forms.Application.ExecutablePath).DirectoryName + @"\"; } }
		public static string ApplicationDataPath_Common { get { return System.Windows.Forms.Application.CommonAppDataPath + @"\"; } }
		public static string ApplicationDataPath_ForUser { get { return System.Windows.Forms.Application.LocalUserAppDataPath + @"\"; } }

		public static string ResolvePath(string path)
		{
			string p = path;			

			p = p.Replace("~/", ApplicationRootPath);
			p = p.Replace("$/", ApplicationDataPath_Common);
			p = p.Replace("&/", ApplicationDataPath_ForUser);

			p = p.Replace('/', '\\');

			p = Environment.ExpandEnvironmentVariables(p); 

			return p;
		}

		public static string UnResolvePath(string path)
		{
			string p = path;
			
			p = p.Replace(ApplicationRootPath, "~/");
			p = p.Replace(ApplicationDataPath_Common, "$/");
			p = p.Replace(ApplicationDataPath_ForUser, "&/");

			System.Collections.IDictionary vars = Environment.GetEnvironmentVariables();

			foreach (object keyObj in vars.Keys)
			{
				string key = keyObj.ToString();
				string value = Environment.GetEnvironmentVariable(key);

				if (p.Contains(value))
				{
					p = p.Replace(value, key);
				}
			}

			p = p.Replace('\\', '/');

			return p;
		}

		public static void EnsurePathExists(string filePath)
		{
			FileInfo fileInfo = new FileInfo(filePath);

			if (fileInfo.Directory.Exists == false)
			{
				fileInfo.Directory.Create(); 
			}
		}

		#endregion

		#region String Helpers

		public static bool IsNullOrEmpty(string str)
        {
            if (str == null)
                return true;

            if (str.Trim().Length == 0)
                return true;

            return false;
        }

        public static bool IsNotNullOrEmpty(string str)
        {
            if (str == null)
                return false;

            if (str.Trim().Length == 0)
                return false;

            return true;
        }

        #endregion 

        #region Node helpers

        public static XmlNode FindChild(string name, XmlNode node)
        {
            XmlNode child = null;

            foreach (System.Xml.XmlNode sub in node.ChildNodes)
            {
                if (sub.Name == name)
                {
                    child = sub;
                    break;
                }
            }

            return child;
        }

		public static void AppendAttributeAndValue(XmlElement element, string name, bool value) { AppendAttributeAndValue(element, name, value.ToString()); }
		public static void AppendAttributeAndValue(XmlElement element, string name, int value) { AppendAttributeAndValue(element, name, value.ToString()); }
		public static void AppendAttributeAndValue(XmlElement element, string name, uint value) { AppendAttributeAndValue(element, name, value.ToString()); }
		public static void AppendAttributeAndValue(XmlElement element, string name, short value) { AppendAttributeAndValue(element, name, value.ToString()); }
		public static void AppendAttributeAndValue(XmlElement element, string name, ushort value) { AppendAttributeAndValue(element, name, value.ToString()); }
		public static void AppendAttributeAndValue(XmlElement element, string name, float value) { AppendAttributeAndValue(element, name, value.ToString()); }
		public static void AppendAttributeAndValue(XmlElement element, string name, double value) { AppendAttributeAndValue(element, name, value.ToString()); }
		public static void AppendAttributeAndValue(XmlElement element, string name, decimal value) { AppendAttributeAndValue(element, name, value.ToString()); }		

        public static void AppendAttributeAndValue(XmlElement element, string name, string value)
        {
            if (IsNotNullOrEmpty(value))
            {
                element.Attributes.Append(element.OwnerDocument.CreateAttribute(name));
                element.Attributes[name].Value = value;
            }
        }

		public static TResult GetAttributeValue<TResult>(XmlNode node, string name, TResult @default) where TResult : struct, IConvertible
        {
			if (!typeof(TResult).IsEnum)
			{
				throw new NotSupportedException("TResult must be an Enum");
			}

			if ((node.Attributes[name] != null))
			{
				try
				{
					return (TResult)Enum.Parse(typeof(TResult), node.Attributes[name].Value, true);
				}
				catch
				{
					return @default;
				}
			}
			else
			{
				return @default;
			}
        }

        public static bool IsAttributeValueTrue(XmlNode node, string name, bool @default)
        {
            if ((node.Attributes[name] != null))
                return Helper.IsTrueValue(node.Attributes[name].Value);
            else
                return @default; 
        }

        public static string GetAttributeValue(XmlNode node, string name, string @default)
        {
            if ((node.Attributes[name] != null))
                return node.Attributes[name].Value;
            else
                return @default;
        }

        public static int GetAttributeValue(XmlNode node, string name, int @default)
        {
            int @return;

            if ((node.Attributes[name] != null) && int.TryParse(node.Attributes[name].Value, out @return))
                return @return;
            else
                return @default;
        }

		public static uint GetAttributeValue(XmlNode node, string name, uint @default)
		{
			uint @return;

			if ((node.Attributes[name] != null) && uint.TryParse(node.Attributes[name].Value, out @return))
				return @return;
			else
				return @default;
		}

		public static short GetAttributeValue(XmlNode node, string name, short @default)
        {
			short @return;

			if ((node.Attributes[name] != null) && short.TryParse(node.Attributes[name].Value, out @return))
                return @return;
            else
                return @default;
        }

		public static ushort GetAttributeValue(XmlNode node, string name, ushort @default)
		{
			ushort @return;

			if ((node.Attributes[name] != null) && ushort.TryParse(node.Attributes[name].Value, out @return))
				return @return;
			else
				return @default;
		}

		public static float GetAttributeValue(XmlNode node, string name, float @default)
		{
			float @return;

			if ((node.Attributes[name] != null) && float.TryParse(node.Attributes[name].Value, out @return))
				return @return;
			else
				return @default;
		}

		public static double GetAttributeValue(XmlNode node, string name, double @default)
		{
			double @return;

			if ((node.Attributes[name] != null) && double.TryParse(node.Attributes[name].Value, out @return))
				return @return;
			else
				return @default;
		}

		public static decimal GetAttributeValue(XmlNode node, string name, decimal @default)
		{
			decimal @return;

			if ((node.Attributes[name] != null) && decimal.TryParse(node.Attributes[name].Value, out @return))
				return @return;
			else
				return @default;
		}

        public static bool GetAttributeValue(XmlNode node, string name, bool @default)
        {
            bool @return;

            if ((node.Attributes[name] != null) && bool.TryParse(node.Attributes[name].Value, out @return))
                return @return;
            else
                return @default;
        }

        #endregion

        #region Color Format lifted from Rug.Settings

        /*public enum ColorFormat
        {
            NamedColor,
            ARGBColor
        }

        public static string SerializeColor(Color color)
        {
            if (color.IsNamedColor)
                return string.Format("{0}:{1}", ColorFormat.NamedColor, color.Name);
            else
                return string.Format("{0}:{1},{2},{3},{4}", ColorFormat.ARGBColor, color.A, color.R, color.G, color.B);
        }

        public static Color DeserializeColor(string color)
        {
            byte a, r, g, b;

            string[] pieces = color.Split(new char[] { ':', ',' });

            ColorFormat colorType = (ColorFormat)Enum.Parse(typeof(ColorFormat), pieces[0], true);

            switch (colorType)
            {
                case ColorFormat.NamedColor:
                    return Color.FromName(pieces[1]);

                case ColorFormat.ARGBColor:
                    a = byte.Parse(pieces[1]);
                    r = byte.Parse(pieces[2]);
                    g = byte.Parse(pieces[3]);
                    b = byte.Parse(pieces[4]);

                    return Color.FromArgb(a, r, g, b);
            }
            return Color.Empty;
        }*/


		public static string SerializeColor3(SlimDX.Color3 color)
		{
			return string.Format("{0},{1},{2}", color.Red, color.Green, color.Blue);
		}

		public static SlimDX.Color3 DeserializeColor3(string value)
		{
			string[] parts = value.Split(',');

			float r, g, b;

			r = float.Parse(parts[0]);
			g = float.Parse(parts[1]);
			b = float.Parse(parts[2]);

			return new SlimDX.Color3(r, g, b);
		}

		public static string SerializeColor4(SlimDX.Color4 color)
		{
			return string.Format("{0},{1},{2},{3}", color.Alpha, color.Red, color.Green, color.Blue);
		}

		public static SlimDX.Color4 DeserializeColor4(string value)
		{
			string[] parts = value.Split(',');

			float a, r, g, b;

			a = float.Parse(parts[0]);
			r = float.Parse(parts[1]);
			g = float.Parse(parts[2]);
			b = float.Parse(parts[3]);

			return new SlimDX.Color4(a, r, g, b); 
		}

        public static string SerializeColor(System.Drawing.Color color)
        {
            return Color.ToARGBHtmlString(color);
        }

        public static System.Drawing.Color DeserializeColor(string color)
        {
            return Color.FromHtmlString(color, false);
        }

        #region Color
        public static class Color
        {
            private readonly static List<string> Colors = new List<string>(Enum.GetNames(typeof(System.Drawing.KnownColor)));

            public static string ToHtmlString(System.Drawing.Color color)
            {
                return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            }

            public static string ToARGBHtmlString(System.Drawing.Color color)
            {
                return "#" + color.A.ToString("X2") + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            }

            public static System.Drawing.Color FromHtmlString(string color)
            {
                return FromHtmlString(color, true);
            }

            public static System.Drawing.Color FromHtmlString(string color, bool throwException)
            {
                string str = color;

                if (IsNullOrEmpty(str))
                    throw new ArgumentNullException("color");

                str = str.Trim();

                if (str.StartsWith("#"))
                    str = str.Substring(1);

                if (str.Length == 8)
                {
                    byte a, r, g, b;

                    if ((byte.TryParse(str.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out a)) &&
                        (byte.TryParse(str.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out r)) &&
                        (byte.TryParse(str.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out g)) &&
                        (byte.TryParse(str.Substring(6, 2), System.Globalization.NumberStyles.HexNumber, null, out b)))
                        return System.Drawing.Color.FromArgb(a, r, g, b);
                }

                if (str.Length == 6)
                {
                    byte r, g, b;

                    if ((byte.TryParse(str.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out r)) &&
                        (byte.TryParse(str.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out g)) &&
                        (byte.TryParse(str.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out b)))
                        return System.Drawing.Color.FromArgb(r, g, b);
                }

                if (str.Length == 3)
                {
                    byte r, g, b;

                    if ((byte.TryParse(str.Substring(0, 1), System.Globalization.NumberStyles.HexNumber, null, out r)) &&
                        (byte.TryParse(str.Substring(1, 1), System.Globalization.NumberStyles.HexNumber, null, out g)) &&
                        (byte.TryParse(str.Substring(2, 1), System.Globalization.NumberStyles.HexNumber, null, out b)))
                        return System.Drawing.Color.FromArgb(r * 16, g * 16, b * 16);
                }

                if (Colors.Contains(str))
                    return System.Drawing.Color.FromName(str);

                if (throwException)
                    throw new Exception("Color is of unknown format '" + color + "'");
                else
                    return System.Drawing.Color.Transparent;
            }
        }
        #endregion 

        #endregion

        #region Rectangle Helpers

        public static string SerializeRectangle(Rectangle rect)
        {
            return String.Format("{0},{1},{2},{3}", rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static Rectangle DeserializeRectangle(string str)
        {
            string[] pieces = str.Split(new char[] { ',' });
            int x, y, width, height;

            x = int.Parse(pieces[0]);
            y = int.Parse(pieces[1]);
            width = int.Parse(pieces[2]);
            height = int.Parse(pieces[3]);

            return new Rectangle(x, y, width, height);
        }

        public static string SerializeRectangleF(RectangleF rect)
        {
            return String.Format("{0},{1},{2},{3}", rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static RectangleF DeserializeRectangleF(string str)
        {
            string[] pieces = str.Split(new char[] { ',' });
            float x, y, width, height;

            x = float.Parse(pieces[0]);
            y = float.Parse(pieces[1]);
            width = float.Parse(pieces[2]);
            height = float.Parse(pieces[3]);

            return new RectangleF(x, y, width, height);
        }

        #endregion

        #region Point Helpers

        public static string SerializePoint(Point point)
        {
            return String.Format("{0},{1}", point.X, point.Y);
        }

        public static Point DeserializePoint(string str)
        {
            string[] pieces = str.Split(new char[] { ',' });
            int x, y;

            x = int.Parse(pieces[0]);
            y = int.Parse(pieces[1]);

            return new Point(x, y);
        }

        public static string SerializePointF(PointF point)
        {
            return String.Format("{0},{1}", point.X, point.Y);
        }

        public static PointF DeserializePointF(string str)
        {
            string[] pieces = str.Split(new char[] { ',' });
            float x, y;

            x = float.Parse(pieces[0]);
            y = float.Parse(pieces[1]);

            return new PointF(x, y);
        }

        #endregion 

        #region Size Helpers

        public static string SerializeSize(Size size)
        {
            return String.Format("{0},{1}", size.Width, size.Height);
        }

        public static Size DeserializeSize(string str)
        {
            string[] pieces = str.Split(new char[] { ',' });
            int x, y;           

            x = int.Parse(pieces[0]);
            y = int.Parse(pieces[1]);

            return new Size(x, y);
        }

        public static string SerializeSizeF(SizeF size)
        {
            return String.Format("{0},{1}", size.Width, size.Height);
        }

        public static SizeF DeserializeSizeF(string str)
        {
            string[] pieces = str.Split(new char[] { ',' });
            float x, y;

            x = float.Parse(pieces[0]);
            y = float.Parse(pieces[1]);

            return new SizeF(x, y);
        }

        #endregion 
    
        public static bool IsTrueValue(string value)
        {
            if (IsNullOrEmpty(value))
                return false; 

            string val = value.Trim(); 

            bool result = false; 
            if (bool.TryParse(val, out result))
                return result; 
            else if ("yes".Equals(val, StringComparison.CurrentCultureIgnoreCase))
                return true; 
            else if ("1".Equals(val))
                return true;

            return false; 
        }

		public static XmlElement CreateElement(XmlNode node, string tag)
		{
			XmlDocument doc = node.OwnerDocument;
			if (node is XmlDocument)
			{
				doc = (XmlDocument)node;
			}

			return doc.CreateElement(tag);
		}
	}
}
