
using arookas.Xml;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace arookas {

	static class bloXml {

		public static bloRectangle loadRectangle(xElement element, bloRectangle defRectangle = default(bloRectangle)) {
			if (element == null) {
				return defRectangle;
			}
			var rectangle = new bloRectangle();
			if (element.Element("x") != null && element.Element("y") != null && element.Element("width") != null && element.Element("height") != null) {
				rectangle.move(element.Element("x"), element.Element("y"));
				rectangle.resize(element.Element("width"), element.Element("height"));
			} else if (element.Element("left") != null && element.Element("top") != null && element.Element("right") != null && element.Element("bottom") != null) {
				rectangle.set(element.Element("left"), element.Element("top"), element.Element("right"), element.Element("bottom"));
			} else {
				rectangle = defRectangle;
			}
			return rectangle;
		}
		public static void saveRectangle(XmlWriter writer, bloRectangle rectangle, string name, bool edges = false) {
			writer.WriteStartElement(name);
			if (edges) {
				writer.WriteElementString("left", rectangle.left.ToString());
				writer.WriteElementString("top", rectangle.top.ToString());
				writer.WriteElementString("right", rectangle.right.ToString());
				writer.WriteElementString("bottom", rectangle.bottom.ToString());
			} else {
				writer.WriteElementString("x", rectangle.left.ToString());
				writer.WriteElementString("y", rectangle.top.ToString());
				writer.WriteElementString("width", rectangle.width.ToString());
				writer.WriteElementString("height", rectangle.height.ToString());
			}
			writer.WriteEndElement();
		}

		public static bloColor loadColor(xElement element, bloColor defColor = default(bloColor)) {
			if (element == null) {
				return defColor;
			}
			uint rgba;
			var color = new bloColor();
			if (element.Element("r") != null && element.Element("g") != null && element.Element("b") != null) {
				color.r = bloMath.clamp(element.Element("r"), 0, 255);
				color.g = bloMath.clamp(element.Element("g"), 0, 255);
				color.b = bloMath.clamp(element.Element("b"), 0, 255);
				color.a = bloMath.clamp(element.Element("a") | 255, 0, 255);
			} else {
				var match = Regex.Match(element.Value, @"\s*(?<value>[0-9a-f]{1,8})\s*", (RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
				if (match.Success) {
					var valueGrp = match.Groups["value"];
					rgba = UInt32.Parse(valueGrp.Value, NumberStyles.AllowHexSpecifier);
					switch (valueGrp.Length) {
						case 1: color = new bloColor(255, 255, 255, (int)(rgba * 17)); break;
						case 2: color = new bloColor(255, 255, 255, (int)rgba); break;
						case 3: {
							color = new bloColor(
								(int)(((rgba >> 8) & 15) * 17),
								(int)(((rgba >> 4) & 15) * 17),
								(int)(((rgba >> 0) & 15) * 17),
								255
							);
							break;
						}
						case 4: {
							color = new bloColor(
								(int)(((rgba >> 12) & 15) * 17),
								(int)(((rgba >> 8) & 15) * 17),
								(int)(((rgba >> 4) & 15) * 17),
								(int)(((rgba >> 0) & 15) * 17)
							);
							break;
						}
						case 5: {
							color = new bloColor(
								(int)(((rgba >> 16) & 15) * 17),
								(int)(((rgba >> 12) & 15) * 17),
								(int)(((rgba >> 7) & 15) * 17),
								(int)((rgba >> 0) & 255)
							);
							break;
						}
						case 6: {
							color = new bloColor(
								(int)((rgba >> 16) & 255),
								(int)((rgba >> 8) & 255),
								(int)((rgba >> 0) & 255),
								255
							);
							break;
						}
						case 7: {
							color = new bloColor(
								(int)((rgba >> 20) & 255),
								(int)((rgba >> 12) & 255),
								(int)((rgba >> 4) & 255),
								(int)(((rgba >> 0) & 15) * 17)
							);
							break;
						}
						case 8: color = new bloColor(rgba); break;
					}
				}
			}
			return color;
		}
		public static void saveColor(XmlWriter writer, bloColor color, string name, bool separate = false) {
			writer.WriteStartElement(name);
			if (separate) {
				writer.WriteElementString("r", color.r.ToString());
				writer.WriteElementString("g", color.g.ToString());
				writer.WriteElementString("b", color.b.ToString());
				if (color.a != 255) {
					writer.WriteElementString("a", color.a.ToString());
				}
			} else {
				writer.WriteValue(color.rgba.ToString("X8"));
			}
			writer.WriteEndElement();
		}

		public static void loadGradient(xElement element, out bloColor fromColor, out bloColor toColor) {
			fromColor = loadColor(element.Element("from"), new bloColor(bloColor.cZero));
			toColor = loadColor(element.Element("to"), new bloColor(bloColor.cOne));
		}
		public static void saveGradient(XmlWriter writer, bloColor fromColor, bloColor toColor, string name, bool separate = false) {
			if (fromColor.rgba == bloColor.cZero && toColor.rgba == bloColor.cOne) {
				return;
			}
			writer.WriteStartElement(name);
			saveColor(writer, fromColor, "from", separate);
			saveColor(writer, toColor, "to", separate);
			writer.WriteEndElement();
		}

		public static ushort[] loadTextBuffer(xElement element, bloFont font) {
			if (element == null || font == null) {
				return new ushort[0];
			}
			return font.encode(element.Value);
		}
		public static void saveTextBuffer(XmlWriter writer, ushort[] buffer, bloFont font, string name) {
			if (font == null) {
				return;
			}
			writer.WriteStartElement(name);
			var text = font.decodeToUtf16(buffer);
			var sbuffer = new StringBuilder(text.Length);
			for (int i = 0; i < text.Length; ++i) {
				if (Char.IsHighSurrogate(text[i])) {
					if (i < (text.Length - 1) && Char.IsLowSurrogate(text[++i])) {
						if (sbuffer.Length > 0) {
							writer.WriteString(sbuffer.ToString());
							sbuffer.Clear();
						}
						writer.WriteSurrogateCharEntity(text[i], text[i - 1]);
					}
					continue;
				} else if (Char.IsControl(text[i])) {
					if (sbuffer.Length > 0) {
						writer.WriteString(sbuffer.ToString());
						sbuffer.Clear();
					}
					writer.WriteCharEntity(text[i]);
				} else {
					sbuffer.Append(text[i]);
				}
			}
			if (sbuffer.Length > 0) {
				writer.WriteString(sbuffer.ToString());
				sbuffer.Clear();
			}
			writer.WriteEndElement();
		}

	}

}
