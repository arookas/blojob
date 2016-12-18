
using arookas.IO.Binary;

namespace arookas {

	class bloTextbox : bloPane {

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

		protected override void loadBlo1(aBinaryReader reader) {
			base.loadBlo1(reader);

			int numparams = reader.Read8();
			
			mFont = blojob.sResourceFinder.find<bloResFont>(reader, "font");
			
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
			return mFont.createStringBuffer(buffer, out mText);
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
