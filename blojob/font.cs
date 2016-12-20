
using arookas.IO.Binary;
using System;
using System.Text;

namespace arookas {

	public enum bloFontType {
		Font8Bit, // 8-bit [00-FF] range
		Font16Bit, // 16-bit [0000-FFFF] range
		FontSJIS, // 8-bit [00-7F] range + 16-bit [8000-FFFF] range
	}

	public abstract class bloFont : bloResource {

		protected bool mMonoFlag;
		protected int mMonoWidth;
		protected bloColor mColorTopLeft, mColorTopRight;
		protected bloColor mColorBottomLeft, mColorBottomRight;
		protected int mSheetIndex;
		protected int mSheetStartX, mSheetStartY;

		protected bloFont() {
			setCharColor(new bloColor(bloColor.cWhite));
			mMonoFlag = false;
			mMonoWidth = 0;
		}

		public abstract void loadGL();

		public bool getMonoFlag() {
			return mMonoFlag;
		}
		public int getMonoWidth() {
			return mMonoWidth;
		}

		public abstract bloFontType getFontType();
		public abstract bool isLeadByte(int character);

		public abstract int getAscent();
		public abstract int getDescent();
		public abstract int getWidth();
		public abstract int getHeight();
		public abstract int getLeading();

		public abstract void getWidthEntry(int character, out WidthEntry widthEntry);

		public void setCharColor(bloColor color) {
			mColorTopLeft = color;
			mColorTopRight = color;
			mColorBottomLeft = color;
			mColorBottomRight = color;
		}
		public void setGradColor(bloColor topColor, bloColor bottomColor) {
			mColorTopLeft = topColor;
			mColorTopRight = topColor;
			mColorBottomLeft = bottomColor;
			mColorBottomRight = bottomColor;
		}

		public void drawString(double x, double y, double width, double height, ushort[] buffer, bool monospace) {
			int index = 0;
			while (index < buffer.Length) {
				x += drawChar(x, y, width, height, buffer[index++], monospace);
				monospace = true;
			}
		}
		public abstract double drawChar(double x, double y, double width, double height, int character, bool monospace);

		public abstract void setGL();
		public virtual void setGL(bloColor fromColor, bloColor toColor) {
			setGL();
		}

		public ushort[] encode(string text) {
			return encode(null, text);
		}
		public ushort[] encode(string format, params object[] args) {
			return encode(null, format, args);
		}
		public ushort[] encode(Encoding encoding, string text) {
			return encode(encoding, "{0}", text);
		}
		public ushort[] encode(Encoding encoding, string format, params object[] args) {
			if (encoding == null) {
				switch (getFontType()) {
					case bloFontType.Font8Bit: {
						encoding = Encoding.GetEncoding(1252); // Latin-1 / ISO-8859-1
						break;
					}
					case bloFontType.Font16Bit: {
						encoding = Encoding.BigEndianUnicode; // BE UTF-16
						break;
					}
					case bloFontType.FontSJIS: {
						encoding = Encoding.GetEncoding(932); // S-JIS
						break;
					}
				}
			}
			var text = String.Format(format, args);
			var encoded = encoding.GetBytes(text);
			ushort[] buffer;
			encode(encoded, out buffer);
			return buffer;
		}
		public int encode(byte[] inBuffer, out ushort[] outBuffer) {
			outBuffer = new ushort[inBuffer.Length + 1];
			int index = 0, newSize = 0;
			while (index < inBuffer.Length) {
				ushort character = inBuffer[index++];
				if (isLeadByte(character)) {
					character <<= 8;
					character |= inBuffer[index++];
				}
				outBuffer[newSize++] = character;
			}
			outBuffer[newSize++] = 0;
			Array.Resize(ref outBuffer, newSize);
			return (newSize - 1);
		}

		public byte[] decodeToBytes(ushort[] buffer) {
			var decoded = new byte[buffer.Length * 2];
			var newSize = 0;
			switch (getFontType()) {
				case bloFontType.Font8Bit: {
					for (int i = 0; i < buffer.Length; ++i) {
						decoded[newSize++] = (byte)buffer[i];
					}
					break;
				}
				case bloFontType.Font16Bit: {
					for (int i = 0; i < buffer.Length; ++i) {
						decoded[newSize++] = (byte)(buffer[i] >> 8);
						decoded[newSize++] = (byte)(buffer[i] >> 0);
					}
					break;
				}
				case bloFontType.FontSJIS: {
					for (int i = 0; i < buffer.Length; ++i) {
						if (buffer[i] >= 0x8000) {
							decoded[newSize++] = (byte)(buffer[i] >> 8);
						}
						decoded[newSize++] = (byte)buffer[i];
					}
					break;
				}
			}
			Array.Resize(ref decoded, newSize);
			return decoded;
		}
		public string decodeToUtf16(ushort[] buffer) {
			var decoded = decodeToBytes(buffer);
			switch (getFontType()) {
				case bloFontType.Font8Bit: {
					var encoding = Encoding.GetEncoding(1252);
					return encoding.GetString(decoded);
				}
				case bloFontType.Font16Bit: {
					var encoding = Encoding.BigEndianUnicode;
					return encoding.GetString(decoded);
				}
				case bloFontType.FontSJIS: {
					var encoding = Encoding.GetEncoding(932);
					return encoding.GetString(decoded);
				}
			}
			return "";
		}

		protected static bool isLeadByte_1Byte(int character) {
			return false;
		}
		protected static bool isLeadByte_2Byte(int character) {
			return true;
		}
		protected static bool isLeadByte_ShiftJIS(int character) {
			return (
				(character >= 0x81 && character <= 0x9F) ||
				(character >= 0xE0 && character <= 0xFC)
			);
		}

		protected delegate bool DecoderMethod(int character);

		public struct WidthEntry {

			public byte kerning;
			public byte width;

			public WidthEntry(int kerning, int width) {
				this.kerning = (byte)kerning;
				this.width = (byte)width;
			}
			public WidthEntry(aBinaryReader reader) {
				kerning = reader.Read8();
				width = reader.Read8();
			}

		}

	}

}
