
using arookas.IO.Binary;
using System;
using System.IO;

namespace arookas {

	public class bloPalette : bloResource {

		protected gxTlutFormat mFormat;
		protected int mTransparency;
		protected int mEntryCount;
		protected aRGBA[] mData;

		public bloPalette() {
			mFormat = gxTlutFormat.IA8;
			mTransparency = 0;
			mEntryCount = 0;
			mData = new aRGBA[0];
		}
		public bloPalette(gxTlutFormat format, int transparency, int entrycount, aBinaryReader reader) {
			mFormat = format;
			mTransparency = transparency;
			mEntryCount = entrycount;
			mData = new aRGBA[mEntryCount];
			loadPaletteData(reader);
		}
		public bloPalette(gxTlutFormat format, int transparency, aRGBA[] data) {
			mFormat = format;
			mTransparency = transparency;
			mEntryCount = data.Length;
			mData = data;
		}

		void loadPaletteData(aBinaryReader reader) {
			switch (mFormat) {
				case gxTlutFormat.IA8: loadPaletteDataIA8(reader); break;
				case gxTlutFormat.RGB565: loadPaletteDataRGB565(reader); break;
				case gxTlutFormat.RGB5A3: loadPaletteDataRGB5A3(reader); break;
				default: {
					throw new NotImplementedException("Encountered unimplemented TLUT format.");
				}
			}
		}
		void loadPaletteDataIA8(aBinaryReader reader) {
			for (int i = 0; i < mEntryCount; ++i) {
				var i8 = reader.Read8();
				var a8 = reader.Read8();
				mData[i] = new aRGBA(i8, a8);
			}
		}
		void loadPaletteDataRGB565(aBinaryReader reader) {
			for (int i = 0; i < mEntryCount; ++i) {
				var rgb565 = reader.Read16();
				mData[i] = aRGBA.FromRGB565(rgb565);
			}
		}
		void loadPaletteDataRGB5A3(aBinaryReader reader) {
			for (int i = 0; i < mEntryCount; ++i) {
				var rgb5a3 = reader.Read16();
				mData[i] = (
					(rgb5a3 & 0x8000) != 0 ?
					aRGBA.FromRGB5(rgb5a3) :
					aRGBA.FromRGB4A3(rgb5a3)
				);
			}
		}

		public void attachPalette(short[] indata, aRGBA[] outdata) {
			for (var i = 0; i < indata.Length; ++i) {
				outdata[i] = mData[indata[i]];
			}
		}

		public override void load(Stream stream) {
			var reader = new aBinaryReader(stream, Endianness.Big);
			reader.PushAnchor();
			mFormat = (gxTlutFormat)reader.Read8();
			mTransparency = reader.Read8();
			mEntryCount = reader.Read16();
			reader.Goto(0x20);
			loadPaletteData(reader);
		}

		public gxTlutFormat getFormat() {
			return mFormat;
		}
		public int getEntryCount() {
			return mEntryCount;
		}

	}

}
