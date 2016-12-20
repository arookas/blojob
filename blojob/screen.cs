﻿
using arookas.IO.Binary;
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;
using System.Text;

namespace arookas {

	public class bloScreen : bloPane {
		
		protected bloColor mTintColor;

		public bloScreen() {
			mTintColor = new bloColor(bloColor.cZero);
			mVisible = true;
			mAlpha = 255;
		}

		public override bloPane search(uint name) {
			if (name == 0) {
				return null;
			}
			return base.search(name);
		}

		public static bloScreen loadCompact(Stream stream) {
			aBinaryReader reader = new aBinaryReader(stream, Endianness.Big, Encoding.GetEncoding(1252));
			bloScreen scrn = new bloScreen();
			if (!loadCompact(scrn, reader)) {
				return null;
			}
			return scrn;
		}
		static bool loadCompact(bloPane parent, aBinaryReader reader) {
			bloPane lastPane = parent;
			for (;;) {
				long start = reader.Position;
				ushort typeID = reader.Read16(); // this is actually a peek in SMS, but fuck that

				switch (typeID) {
					default: {
						Console.WriteLine(">>> Unknown '{0:X4}' section at 0x{1:X6}", typeID, start);
						return false;
					}
					case 0: {
						return true;
					}
					case 2: {
						reader.Step(2);
						return true;
					}
					case 1: {
						reader.Step(2);
						if (!loadCompact(lastPane, reader)) {
							return false;
						}
						break;
					}
					case 16: {
						lastPane = new bloPane();
						lastPane.load(parent, reader, bloFormat.Compact);
						if (parent is bloScreen) {
							var oldrect = lastPane.getRectangle();
							var newrect = new bloRectangle(0, 0, oldrect.width, oldrect.height);
							parent.setRectangle(newrect);
						}
						break;
					}
					case 17: {
						lastPane = new bloWindow();
						lastPane.load(parent, reader, bloFormat.Compact);
						break;
					}
					case 18: {
						lastPane = new bloPicture();
						lastPane.load(parent, reader, bloFormat.Compact);
						break;
					}
					case 19: {
						lastPane = new bloTextbox();
						lastPane.load(parent, reader, bloFormat.Compact);
						break;
					}
				}
			}
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

		public void saveBlo1(Stream stream) {
			aBinaryWriter writer = new aBinaryWriter(stream, Endianness.Big, Encoding.GetEncoding(1252));

			var blockstart = writer.Position;
			var blockcount = 0u;

			writer.Write32(cSCRN);
			writer.Write32(cBLO1);
			writer.Write32(0u); // dummy size
			writer.Write32(0u); // dummy block count
			writer.WritePadding(32, 0);

			writer.Write32(cINF1);
			writer.Write32(0x10u);
			writer.WriteS16((short)mRect.width);
			writer.WriteS16((short)mRect.height);
			writer.Write32(mTintColor.rgba);
			++blockcount;

			foreach (var childpane in this) {
				saveBlo1(childpane, writer, ref blockcount);
			}

			writer.Write32(cEXT1);
			writer.Write32(0x8u);
			++blockcount;

			writer.WritePadding(32, 0);

			var blockend = writer.Position;
			
			writer.Goto(blockstart + 8);
			writer.Write32((uint)(blockend - blockstart));
			writer.Write32(blockcount);
			writer.Goto(blockend);
		}
		void saveBlo1(bloPane pane, aBinaryWriter writer, ref uint blockcount) {
			var typeID = cPAN1;

			if (pane is bloTextbox) {
				typeID = cTBX1;
			} else if (pane is bloWindow) {
				typeID = cWIN1;
			} else if (pane is bloPicture) {
				typeID = cPIC1;
			}

			var blockstart = writer.Position;

			writer.Write32(typeID);
			writer.Write32(0u); // dummy size

			pane.saveBlo1(writer);

			writer.WritePadding(4, 0);

			var blockend = writer.Position;

			writer.Goto(blockstart + 4);
			writer.Write32((uint)(blockend - blockstart));
			writer.Goto(blockend);
			++blockcount;

			if (pane.getChildPane() > 0) {
				writer.Write32(cBGN1);
				writer.Write32(0x8u);
				++blockcount;

				foreach (var childpane in pane) {
					saveBlo1(childpane, writer, ref blockcount);
				}

				writer.Write32(cEND1);
				writer.Write32(0x8u);
				++blockcount;
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
