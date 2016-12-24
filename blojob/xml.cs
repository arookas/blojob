
using arookas.Xml;
using System;
using System.Globalization;
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
			} else if (UInt32.TryParse(element.Value, NumberStyles.AllowHexSpecifier, null, out rgba)) {
				color.rgba = rgba;
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
			writer.WriteString(font.decodeToUtf16(buffer));
			writer.WriteEndElement();
		}

	}

}
