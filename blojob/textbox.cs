
using arookas.IO.Binary;
using System.Xml;

namespace arookas {

	public class bloTextbox : bloPane {

		protected bloFont mFont;
		protected ushort[] mText;
		protected int mFontSpacing, mFontLeading, mFontWidth, mFontHeight;
		protected bloColor mTopColor, mBottomColor;
		protected bloColor mFromColor, mToColor;
		protected bloTextboxHBinding mHBinding;
		protected bloTextboxVBinding mVBinding;

		public bloTextbox() {
			mFont = null;
			mText = new ushort[1] { 0 };
			mFromColor = new bloColor(bloColor.cZero);
			mToColor = new bloColor(bloColor.cOne);
			mTopColor = new bloColor(bloColor.cWhite);
			mBottomColor = new bloColor(bloColor.cWhite);
		}

		protected override void loadCompact(aBinaryReader reader) {
			base.loadCompact(reader);

			var finder = bloResourceFinder.getFinder();

			mFont = finder.find<bloResFont>(reader, "font");

			mTopColor = new bloColor(reader.Read32());
			mBottomColor = new bloColor(reader.Read32());

			int hbinding = reader.Read8();
			mHBinding = (bloTextboxHBinding)(hbinding & 127);
			mVBinding = (bloTextboxVBinding)reader.Read8();

			if ((hbinding & 0x80) != 0) {
				mFontSpacing = reader.ReadS16();
				mFontLeading = reader.ReadS16();
				mFontWidth = reader.Read16();
				mFontHeight = reader.Read16();
			} else if (mFont != null) {
				mFontSpacing = 0;
				mFontLeading = mFont.getLeading();
				mFontWidth = mFont.getWidth();
				mFontHeight = mFont.getHeight();
			} else {
				mFontSpacing = 0;
				mFontLeading = 0;
				mFontWidth = 0;
				mFontHeight = 0;
			}

			int strlen = reader.Read16();
			setString(reader.Read8s(strlen));

			mFromColor = new bloColor(bloColor.cZero);
			mToColor = new bloColor(bloColor.cOne);

			reader.Skip(4);
		}
		protected override void loadBlo1(aBinaryReader reader) {
			base.loadBlo1(reader);

			var finder = bloResourceFinder.getFinder();

			int numparams = reader.Read8();

			mFont = finder.find<bloResFont>(reader, "font");
			
			mTopColor = new bloColor(reader.Read32());
			mBottomColor = new bloColor(reader.Read32());

			int binding = reader.Read8();
			mHBinding = (bloTextboxHBinding)((binding >> 2) & 3);
			mVBinding = (bloTextboxVBinding)((binding >> 0) & 3);

			mFontSpacing = reader.ReadS16();
			mFontLeading = reader.ReadS16();
			mFontWidth = reader.Read16();
			mFontHeight = reader.Read16();
			
			int strlen = reader.Read16();
			setString(reader.Read8s(strlen));

			numparams -= 10;

			if (numparams > 0) {
				if (reader.Read8() != 0) {
					setConnectParent(true);
				}
				--numparams;
			}

			if (numparams > 0) {
				mFromColor = new bloColor(reader.Read32());
				--numparams;
			} else {
				mFromColor = new bloColor(bloColor.cZero);
			}

			if (numparams > 0) {
				mToColor = new bloColor(reader.Read32());
				--numparams;
			} else {
				mToColor = new bloColor(bloColor.cOne);
			}

			reader.Skip(4);
		}

		public override void saveBlo1(aBinaryWriter writer) {
			base.saveBlo1(writer);

			byte numparams;

			if (mToColor.rgba != bloColor.cOne) {
				numparams = 13;
			} else if (mFromColor.rgba != bloColor.cZero) {
				numparams = 12;
			} else if (mConnectParent) {
				numparams = 11;
			} else {
				numparams = 10;
			}

			bloResource.save(mFont, writer);
			writer.Write32(mTopColor.rgba);
			writer.Write32(mBottomColor.rgba);

			byte binding = 0;
			binding |= (byte)mHBinding;
			binding <<= 2;
			binding |= (byte)mVBinding;
			writer.Write8(binding);

			writer.WriteS16((short)mFontSpacing);
			writer.WriteS16((short)mFontLeading);
			writer.Write16((ushort)mFontWidth);
			writer.Write16((ushort)mFontHeight);

			if (mFont != null) {
				var strbuffer = mFont.decodeToBytes(mText);
				writer.Write16((ushort)strbuffer.Length);
				writer.Write8s(strbuffer);
			} else {
				writer.Write16(0);
			}

			numparams -= 10;

			if (numparams > 0) {
				writer.Write8((byte)(mConnectParent ? 1 : 0));
				--numparams;
			}

			if (numparams > 0) {
				writer.Write32(mFromColor.rgba);
				--numparams;
			}

			if (numparams > 0) {
				writer.Write32(mToColor.rgba);
				--numparams;
			}

			writer.WritePadding(4, 0);
		}
		public override void saveXml(XmlWriter writer) {
			base.saveXml(writer);

			bloResource.save(mFont, "font", writer);

			writer.WriteStartElement("colors");
			bloXml.saveColor(writer, mTopColor, "top");
			bloXml.saveColor(writer, mBottomColor, "bottom");
			writer.WriteEndElement();

			writer.WriteStartElement("binding");
			writer.WriteElementString("horizontal", mHBinding.ToString());
			writer.WriteElementString("vertical", mVBinding.ToString());
			writer.WriteEndElement();

			writer.WriteStartElement("typesetting");
			writer.WriteElementString("spacing", mFontSpacing.ToString());
			writer.WriteElementString("leading", mFontLeading.ToString());
			writer.WriteElementString("width", mFontWidth.ToString());
			writer.WriteElementString("height", mFontHeight.ToString());
			writer.WriteEndElement();

			bloXml.saveGradient(writer, mFromColor, mToColor, "gradient");
		}

		protected override void loadGLSelf() {
			base.loadGLSelf();
			if (mFont != null) {
				mFont.loadGL();
			}
		}

		protected override void drawSelf() {
			var rect = new bloRectangle(0, 0, mRect.width, mRect.height);
			var printer = new bloPrint(mFont, mFontSpacing, mFontLeading, mTopColor, mBottomColor);
			printer.setFontSize((mFontWidth >= 0 ? mFontWidth : 0), (mFontHeight >= 0 ? mFontHeight : 0));
			printer.setGradColor(mFromColor, mToColor);
			printer.initialize();
			printer.printReturn(mText, rect.width, rect.height, mHBinding, mVBinding, 0, 0, mCumulativeAlpha);
		}

		int setString(byte[] buffer) {
			if (mFont == null) {
				mText = new ushort[1] { 0 };
				return 0;
			}
			return mFont.encode(buffer, out mText);
		}

		public bloFont getFont() {
			return mFont;
		}
		public ushort[] getString() {
			return mText;
		}
		
		public bloFont setFont(bloFont font) {
			bloFont oldFont = mFont;
			mFont = font;
			return oldFont;
		}
		public ushort[] setString(ushort[] buffer) {
			ushort[] old = mText;
			if (buffer == null) {
				buffer = new ushort[1] { 0 };
			}
			mText = buffer;
			return old;
		}
		
		public override bool setConnectParent(bool set) {
			if (mParent == null || !(mParent is bloWindow)) {
				return false;
			}
			mConnectParent = set;
			return set;
		}

	}

}
