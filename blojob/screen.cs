
using arookas.IO.Binary;
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;
using System.Text;

namespace arookas {

	class bloScreen : bloPane {
		
		protected bloColor mTintColor;

		public bloScreen() {
			mVisible = true;
			mAlpha = 255;
		}

		public override bloPane search(uint name) {
			if (name == 0) {
				return null;
			}
			return base.search(name);
		}

		public static bloScreen loadBlo1(Stream stream) {
			aBinaryReader reader = new aBinaryReader(stream, Endianness.Big, Encoding.GetEncoding(1252));
			bloScreen scrn = new bloScreen();
			if (reader.Read32() != cSCRN) {
				return null;
			}
			if (reader.Read32() != cBLO1) {
				return null;
			}
			reader.Step(24);
			if (reader.Read32() != cINF1) {
				return null;
			}
			long inf1size = reader.Read32();
			int width = reader.ReadS16();
			int height = reader.ReadS16();
			scrn.mRect.set(0, 0, width, height);
			scrn.mTintColor = new bloColor(reader.Read32());
			if (!loadBlo1(scrn, reader)) {
				return null;
			}
			return scrn;
		}
		static bool loadBlo1(bloPane parent, aBinaryReader reader) {
			bloPane lastPane = parent;
			for (;;) {
				long start = reader.Position;
				uint typeID = reader.Read32();
				long length = reader.Read32();
				long end = (start + length);

				switch (typeID) {
					case cPAN1: {
						lastPane = new bloPane();
						lastPane.load(parent, reader, bloFormat.Blo1);
						if (reader.Position != end) {
							Console.WriteLine(">>> Bad '{0}' section at {1:X6} (header size {2:X6} actual size {3:X6})", idToString(typeID), start, length, (reader.Position - start));
							reader.Goto(end);
						}
						break;
					}
					case cPIC1: {
						lastPane = new bloPicture();
						lastPane.load(parent, reader, bloFormat.Blo1);
						if (reader.Position != end) {
							Console.WriteLine(">>> Bad '{0}' section at {1:X6} (header size {2:X6} actual size {3:X6})", idToString(typeID), start, length, (reader.Position - start));
							reader.Goto(end);
						}
						break;
					}
					case cWIN1: {
						lastPane = new bloWindow();
						lastPane.load(parent, reader, bloFormat.Blo1);
						if (reader.Position != end) {
							Console.WriteLine(">>> Bad '{0}' section at {1:X6} (header size {2:X6} actual size {3:X6})", idToString(typeID), start, length, (reader.Position - start));
							reader.Goto(end);
						}
						break;
					}
					case cTBX1: {
						lastPane = new bloTextbox();
						lastPane.load(parent, reader, bloFormat.Blo1);
						reader.Goto(end);
						break;
					}
					case cBGN1: {
						reader.Goto(end);
						if (!loadBlo1(lastPane, reader)) {
							return false;
						}
						break;
					}
					case cEND1: {
						reader.Goto(end);
						return true;
					}
					case cEXT1: {
						// we should skip to the end of this section just in case,
						// but SMS doesn't so to keep compatibility neither do we
						return true;
					}
					default: {
						Console.WriteLine(">>> Unknown '{0}' section at {1:X6} (size {2:X6})", idToString(typeID), start, length);
						return false;
					}
				}
			}
		}

		protected override void drawSelf() {
			GL.Disable(EnableCap.Texture2D);
			GL.Begin(PrimitiveType.Quads);
			GL.Color4(mTintColor);
			GL.Vertex3(mRect.left, mRect.top, 0.0d);
			GL.Color4(mTintColor);
			GL.Vertex3(mRect.right, mRect.top, 0.0d);
			GL.Color4(mTintColor);
			GL.Vertex3(mRect.right, mRect.bottom, 0.0d);
			GL.Color4(mTintColor);
			GL.Vertex3(mRect.left, mRect.bottom, 0.0d);
			GL.End();
		}

		static string idToString(uint id) {
			var idstr = new char[4];
			idstr[0] = (char)((id >> 24) & 255);
			idstr[1] = (char)((id >> 16) & 255);
			idstr[2] = (char)((id >> 08) & 255);
			idstr[3] = (char)((id >> 00) & 255);
			return new String(idstr);
		}

		const uint cSCRN = 0x5343524Eu; // 'SCRN'
		const uint cBLO1 = 0x626C6F31u; // 'blo1'
		const uint cINF1 = 0x494E4631u; // 'INF1'
		const uint cPAN1 = 0x50414E31u; // 'PAN1'
		const uint cPIC1 = 0x50494331u; // 'PIC1'
		const uint cWIN1 = 0x57494E31u; // 'WIN1'
		const uint cTBX1 = 0x54425831u; // 'TBX1'
		const uint cBGN1 = 0x42474E31u; // 'BGN1'
		const uint cEND1 = 0x454E4431u; // 'END1'
		const uint cEXT1 = 0x45585431u; // 'EXT1'

	}
}
