
using arookas.Collections;
using arookas.IO.Binary;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.IO;

namespace arookas {

	public class bloResFont : bloFont {

		FontBlock mFontHeader;
		InfoBlock mInfoBlock;
		WidthBlock[] mWidthBlocks;
		GlyphBlock[] mGlyphBlocks;
		MapBlock[] mMapBlocks;
		int mWidthCount, mGlyphCount, mMapCount;
		int mFirstChar;
		DecoderMethod mDecoderMethod;
		bloColor mFromColor, mToColor;
		int mTextureName;
		int mGlyphIndex;

		public bloResFont() {
			mSheetStartX = 0;
			mSheetStartY = 0;
			mSheetIndex = -1;
		}

		public override void load(Stream stream) {
			aBinaryReader reader = new aBinaryReader(stream, Endianness.Big);

			mFontHeader = new FontBlock(reader);

			Debug.Assert(mFontHeader.typeID == cFONT);
			Debug.Assert(mFontHeader.version == cBFN1);

			countBlock(reader);
			if (mWidthCount > 0) {
				mWidthBlocks = new WidthBlock[mWidthCount];
			}
			if (mGlyphCount > 0) {
				mGlyphBlocks = new GlyphBlock[mGlyphCount];
			}
			if (mMapCount > 0) {
				mMapBlocks = new MapBlock[mMapCount];
			}
			setBlock(reader);
		}

		void countBlock(aBinaryReader reader) {
			reader.Goto(0x20u);
			for (var i = 0u; i < mFontHeader.blockCount; ++i) {
				var blockStart = reader.Position;
				var typeID = reader.Read32();
				var size = reader.Read32();
				switch (typeID) {
					case cINF1: break;
					case cWID1: ++mWidthCount; break;
					case cMAP1: ++mMapCount; break;
					case cGLY1: ++mGlyphCount; break;
					default: {
						Debug.Fail("Unknown data block.");
						break;
					}
				}
				reader.Goto(blockStart + size);
			}
		}
		void setBlock(aBinaryReader reader) {
			int widthBlocks = 0;
			int glyphBlocks = 0;
			int mapBlocks = 0;
			reader.Goto(0x20u);
			for (uint i = 0; i < mFontHeader.blockCount; ++i) {
				var blockStart = reader.Position;
				var typeID = reader.Read32();
				var size = reader.Read32();
				switch (typeID) {
					case cINF1: {
						mInfoBlock = new InfoBlock(reader);
						mDecoderMethod = sAboutEncoding[(int)mInfoBlock.fontType];
						break;
					}
					case cWID1: {
						mWidthBlocks[widthBlocks++] = new WidthBlock(reader);
						break;
					}
					case cGLY1: {
						mGlyphBlocks[glyphBlocks++] = new GlyphBlock(reader);
						break;
					}
					case cMAP1: {
						mMapBlocks[mapBlocks] = new MapBlock(reader);
						int firstChar = mMapBlocks[mapBlocks].firstChar;
						if (mFirstChar > firstChar) {
							mFirstChar = firstChar;
						}
						++mapBlocks;
						break;
					}
					default: {
						Debug.Fail("Unknown data block.");
						break;
					}
				}
				reader.Goto(blockStart + size);
			}
		}

		public override void loadGL() {
			if (mTextureName == 0) {
				mTextureName = gl.genTexObj();
			}
		}

		void loadFont(int character, out WidthEntry widthEntry) {
			widthEntry = new WidthEntry();
			int code = getFontCode(character);
			if (code != 0) {
				int index;
				for (index = 0; index < mWidthCount; ++index) {
					var block = mWidthBlocks[index];
					if (block.firstCode <= code && code <= block.lastCode) {
						widthEntry = block.entries[code - block.firstCode];
						break;
					}
				}
				if (index == mWidthCount) {
					widthEntry = new WidthEntry(0, getWidth());
				}
			}
			loadImage(code);
		}
		void loadImage(int code) {
			int index;
			for (index = 0; index < mGlyphCount; ++index) {
				var block = mGlyphBlocks[index];
				if (block.firstCode <= code && code <= block.lastCode) {
					code -= block.firstCode;
					break;
				}
			}
			if (index != mGlyphCount) {
				var block = mGlyphBlocks[index];
				int cellCount = (block.sheetRow * block.sheetColumn);
				int sheetIndex = (code / cellCount);
				code %= cellCount;
				mSheetStartX = (block.cellWidth * (code % block.sheetRow));
				mSheetStartY = (block.cellHeight * (code / block.sheetRow));
				if (sheetIndex != mSheetIndex || index != mGlyphIndex) {
					var data = (bloImage.loadImageData(block.sheets[sheetIndex], Endianness.Big, block.sheetWidth, block.sheetHeight, block.sheetFormat) as aRGBA[]);
					if (data == null) {
						throw new Exception("Index texture formats are not supported for fonts.");
					}
					gl.initTexObj(mTextureName, data, block.sheetWidth, block.sheetHeight, gxWrapMode.Clamp, gxWrapMode.Clamp, false);
					gl.initTexObjLOD(mTextureName, gxTextureFilter.Linear, gxTextureFilter.Linear, 0.0d, 0.0d, 0.0d, false, false, gxAnisotropy.Aniso1);
					mSheetIndex = sheetIndex;
					mGlyphIndex = index;
				}
				gl.loadTexObj(mTextureName);
			}
		}

		public int getFontCode(int character) {
			int code = mInfoBlock.invalChar;
			// the font does not have half-width characters, convert them to full-width
			if (getFontType() == bloFontType.FontSJIS && mFirstChar >= 0x8000 && character >= 0x20 && character <= 0x7F) {
				character = sHalfToFull[character - 0x20];
			}
			for (int i = 0; i < mMapCount; ++i) {
				var block = mMapBlocks[i];
				if (block.firstChar > character || character > block.lastChar) {
					continue;
				}
				switch (block.mapType) {
					case MapType.LinearMap: {
						code = (character - block.firstChar);
						break;
					}
					case MapType.TableMap: {
						code = block.data[character - block.firstChar];
						break;
					}
					case MapType.MapMap: {
						int upper = (block.dataCount - 1), lower = 0;
						while (upper >= lower) {
							int index = (upper + lower);
							if (character < block.data[index]) {
								upper = (index - 1);
							} else if (character > block.data[index]) {
								lower = (index + 1);
							} else {
								code = block.code[index];
								break;
							}
						}
						break;
					}
					case MapType.KanjiMap: {
						// in TWW individual MAP1 blocks may override the baseCode value.
						// SMS doesn't check the data count and is hardcoded instead,
						// but it is probably safe to support TWW's implementation here.
						ushort? baseCode = null;
						if (block.dataCount == 1) {
							baseCode = block.data[0];
						}
						code = convertSjis(character, baseCode);
						break;
					}
				}
				break;
			}
			return code;
		}

		public override bloFontType getFontType() {
			return mInfoBlock.fontType;
		}
		public override bool isLeadByte(int character) {
			return mDecoderMethod(character);
		}

		public override int getAscent() {
			return mInfoBlock.ascent;
		}
		public override int getDescent() {
			return mInfoBlock.descent;
		}
		public override int getWidth() {
			return mInfoBlock.width;
		}
		public override int getHeight() {
			return (getDescent() + getAscent());
		}
		public override int getLeading() {
			return mInfoBlock.leading;
		}

		public override void getWidthEntry(int character, out WidthEntry widthEntry) {
			int code = getFontCode(character);
			widthEntry = new WidthEntry(0, mInfoBlock.width);
			for (int i = 0; i < mWidthCount; ++i) {
				var block = mWidthBlocks[i];
				if (block.firstCode <= code && code <= block.lastCode) {
					widthEntry = block.entries[code - block.firstCode];
					break;
				}
			}
		}

		public override double drawChar(double x, double y, double width, double height, int character, bool monospace) {

			var context = bloContext.getContext();

			context.useProgram();
			context.setProgramColor("fromColor", mFromColor);
			context.setProgramColor("toColor", mToColor);
			context.setProgramInt("transparency", 1);

			WidthEntry widthEntry;
			loadFont(character, out widthEntry);

			double hScale = (width / getWidth());
			double vScale = (height / getHeight());

			double totalWidth = (mMonoWidth * hScale);
			if (!mMonoFlag) {
				if (!monospace) {
					totalWidth = ((widthEntry.width + widthEntry.kerning) * hScale);
				} else {
					totalWidth = (widthEntry.width * hScale);
				}
			}

			double fLeft, fTop, fRight, fBottom;
			if (mMonoFlag || !monospace) {
				fLeft = x;
			} else {
				fLeft = (x - (widthEntry.kerning * hScale));
			}
			fRight = (fLeft + width);
			fTop = (y - (getAscent() * vScale));
			fBottom = (y + (getDescent() * vScale));

			var block = mGlyphBlocks[mGlyphIndex];
			double sLeft = ((double)mSheetStartX / (double)block.sheetWidth);
			double tTop = ((double)mSheetStartY / (double)block.sheetHeight);
			double sRight = (sLeft + ((double)block.cellWidth / (double)block.sheetWidth));
			double tBottom = (tTop + ((double)block.cellHeight / (double)block.sheetHeight));

			GL.Begin(PrimitiveType.Quads);
			GL.TexCoord2(sLeft, tTop);
			GL.Color4(mColorTopLeft);
			GL.Vertex3(fLeft, fTop, 0.0d);
			GL.TexCoord2(sRight, tTop);
			GL.Color4(mColorTopRight);
			GL.Vertex3(fRight, fTop, 0.0d);
			GL.TexCoord2(sRight, tBottom);
			GL.Color4(mColorBottomRight);
			GL.Vertex3(fRight, fBottom, 0.0d);
			GL.Color4(mColorBottomLeft);
			GL.TexCoord2(sLeft, tBottom);
			GL.Vertex3(fLeft, fBottom, 0.0d);
			GL.End();

			context.unuseProgram();

			return totalWidth;
		}

		public override void setGL() {
			mFromColor = new bloColor(bloColor.cZero);
			mToColor = new bloColor(bloColor.cOne);
		}
		public override void setGL(bloColor fromColor, bloColor toColor) {
			mFromColor = fromColor;
			mToColor = toColor;
		}

		static DecoderMethod[] sAboutEncoding = {
			isLeadByte_1Byte,
			isLeadByte_2Byte,
			isLeadByte_ShiftJIS,
		};

		static int[] sHalfToFull = {
			0x8140, 0x8149, 0x8168, 0x8194, 0x8190, 0x8193, 0x8195, 0x8166, 0x8169, 0x816A,
			0x8196, 0x817B, 0x8143, 0x817C, 0x8144, 0x815E, 0x824F, 0x8250, 0x8251, 0x8252,
			0x8253, 0x8254, 0x8255, 0x8256, 0x8257, 0x8258, 0x8146, 0x8147, 0x8183, 0x8181,
			0x8184, 0x8148, 0x8197, 0x8260, 0x8261, 0x8262, 0x8263, 0x8264, 0x8265, 0x8266,
			0x8267, 0x8268, 0x8269, 0x826A, 0x826B, 0x826C, 0x826D, 0x826E, 0x826F, 0x8270,
			0x8271, 0x8272, 0x8273, 0x8274, 0x8275, 0x8276, 0x8277, 0x8278, 0x8279, 0x816D,
			0x818F, 0x816E, 0x814F, 0x8151, 0x8165, 0x8281, 0x8282, 0x8283, 0x8284, 0x8285,
			0x8286, 0x8287, 0x8288, 0x8289, 0x828A, 0x828B, 0x828C, 0x828D, 0x828E, 0x828F,
			0x8290, 0x8291, 0x8292, 0x8293, 0x8294, 0x8295, 0x8296, 0x8297, 0x8298, 0x8299,
			0x829A, 0x816F, 0x8162, 0x8170, 0x8160,
		};

		static int convertSjis(int character, ushort? overrideCode) {
			int leadByte = ((character >> 8) & 0xFF);
			int trailByte = ((character >> 0) & 0xFF);
			int index = (trailByte - 64);
			if (index >= 64) {
				--index;
			}
			// to allow loading TWW fonts, we use TWW's implementation
			// and allow overriding the base code from within the MAP1.
			// SMS hardcodes the parameter to null so we should be fine.
			// the SDK's implementation has even stricter error handling.
			int baseCode = 796;
			if (overrideCode != null) {
				baseCode = overrideCode.Value;
			}
			return (baseCode + index + ((leadByte - 136) * 188 - 94));
		}

		const uint cFONT = 0x464F4E54u;
		const uint cBFN1 = 0x62666E31u;
		const uint cINF1 = 0x494E4631u;
		const uint cWID1 = 0x57494431u;
		const uint cGLY1 = 0x474C5931u;
		const uint cMAP1 = 0x4D415031u;

		enum MapType {
			LinearMap,
			KanjiMap,
			TableMap,
			MapMap,
		}

		class FontBlock {

			public uint typeID;
			public uint version;
			public uint size;
			public uint blockCount;

			public FontBlock(aBinaryReader reader) {
				typeID = reader.Read32();
				version = reader.Read32();
				size = reader.Read32();
				blockCount = reader.Read32();
			}

		}

		class InfoBlock {

			public bloFontType fontType;
			public int ascent, descent;
			public int width;
			public int leading;
			public int invalChar;

			public InfoBlock(aBinaryReader reader) {
				fontType = (bloFontType)reader.Read16();
				ascent = reader.Read16();
				descent = reader.Read16();
				width = reader.Read16();
				leading = reader.Read16();
				invalChar = reader.Read16();
			}

		}

		class WidthBlock {

			public int firstCode, lastCode;
			public WidthEntry[] entries;

			public WidthBlock(aBinaryReader reader) {
				firstCode = reader.Read16();
				lastCode = reader.Read16();
				entries = aCollection.Initialize((lastCode - firstCode), () => new WidthEntry(reader));
			}

		}

		class GlyphBlock {

			public int firstCode, lastCode;
			public int sheetRow, sheetColumn;
			public int sheetSize;
			public gxTextureFormat sheetFormat;
			public int cellWidth, cellHeight;
			public int sheetWidth, sheetHeight;
			public byte[][] sheets;

			public GlyphBlock(aBinaryReader reader) {
				firstCode = reader.Read16(); // 0008
				lastCode = reader.Read16(); // 000A
				cellWidth = reader.Read16(); // 000C
				cellHeight = reader.Read16(); // 000E
				sheetSize = reader.ReadS32(); // 0010
				sheetFormat = (gxTextureFormat)reader.Read16(); // 0014
				sheetRow = reader.Read16(); // 0016
				sheetColumn = reader.Read16(); // 0018
				sheetWidth = reader.Read16(); // 001A
				sheetHeight = reader.Read16(); // 001C
				reader.Step(2); // 001E

				// we have to manually calculate how many sheets there are
				int sheetCount = (((lastCode - firstCode) / (sheetRow * sheetColumn)) + 1);
				sheets = aCollection.Initialize(sheetCount, () => reader.Read8s(sheetSize));
			}

		}

		class MapBlock {

			public MapType mapType;
			public int firstChar, lastChar, dataCount;
			public ushort[] data;
			public ushort[] code;

			public MapBlock(aBinaryReader reader) {
				mapType = (MapType)reader.Read16();
				firstChar = reader.Read16();
				lastChar = reader.Read16();
				dataCount = reader.Read16();
				if (mapType == MapType.MapMap) {
					data = new ushort[dataCount];
					code = new ushort[dataCount];
					for (int i = 0; i < dataCount; ++i) {
						data[i] = reader.Read16();
						code[i] = reader.Read16();
					}
				} else {
					data = reader.Read16s(dataCount);
				}
			}

		}

	}

}
