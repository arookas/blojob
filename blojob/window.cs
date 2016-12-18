
using arookas.IO.Binary;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace arookas {

	class bloWindow : bloPane {

		protected bloRectangle mContentRect;
		protected bloPalette mPalette;
		protected TextureSlot[] mTextures;
		protected bloTexture mContentTexture;
		protected bloColor mFromColor, mToColor;
		protected int mMinWidth;
		protected int mMinHeight;
		protected bool mTextured; // cached value, true if all textures are non-null

		public bloWindow() {
			mTextures = new TextureSlot[4];
			for (int i = 0; i < 4; ++i) {
				mTextures[i] = new TextureSlot();
			}
		}

		public override void resize(int width, int height) {
			int oldwidth = mRect.width;
			int oldheight = mRect.height;
			base.resize(width, height);
			int diffwidth = (width - oldwidth);
			int diffheight = (height - oldheight);

			// TODO: figure out how to do this cleanly (fuck you nintendo)
			// C# doesn't let protected members be accessed from types
			// unless the type is derived from the _calling_ type
			/*
			foreach (var pane in mChildren) {
				if (pane is bloTextbox) {
					pane.mRect.resize(
						(diffwidth + pane.mRect.width),
						(diffheight + pane.mRect.height)
					);
				}
			}
			*/
		}
		
		protected override void loadCompact(aBinaryReader reader) {
			base.loadCompact(reader);

			int x = reader.Read16();
			int y = reader.Read16();
			int width = reader.Read16();
			int height = reader.Read16();
			mContentRect.set(x, y, (x + width), (y + height));

			for (int i = 0; i < 4; ++i) {
				mTextures[i].texture = blojob.sResourceFinder.find<bloTexture>(reader, "timg");
			}
			mPalette = blojob.sResourceFinder.find<bloPalette>(reader, "tlut");

			int bits = reader.Read8();
			for (int i = 0; i < 4; ++i) {
				mTextures[i].mirror = (bloMirror)((bits >> (6 - (i * 2))) & 3);
			}

			for (int i = 0; i < 4; ++i) {
				mTextures[i].color.rgba = reader.Read32();
			}

			mContentTexture = null;
			mFromColor.rgba = bloColor.cZero;
			mToColor.rgba = bloColor.cOne;

			reader.Skip(4);
			postLoad();
		}
		protected override void loadBlo1(aBinaryReader reader) {
			base.loadBlo1(reader);

			int numparams = reader.Read8();

			int x = reader.Read16();
			int y = reader.Read16();
			int width = reader.Read16();
			int height = reader.Read16();
			mContentRect.set(x, y, (x + width), (y + height));

			mTextured = true;
			for (int i = 0; i < 4; ++i) {
				mTextures[i].texture = blojob.sResourceFinder.find<bloTexture>(reader, "timg");
				if (mTextures[i].texture == null) {
					mTextured = false;
				}
			}
			mPalette = blojob.sResourceFinder.find<bloPalette>(reader, "tlut");

			int bits = reader.Read8();
			for (int i = 0; i < 4; ++i) {
				mTextures[i].mirror = (bloMirror)((bits >> (6 - (i * 2))) & 3);
			}

			for (int i = 0; i < 4; ++i) {
				mTextures[i].color.rgba = reader.Read32();
			}

			numparams -= 14;

			if (numparams > 0) {
				mContentTexture = blojob.sResourceFinder.find<bloTexture>(reader, "timg");
				--numparams;
			} else {
				mContentTexture = null;
			}

			if (numparams > 0) {
				mFromColor.rgba = reader.Read32();
				--numparams;
			} else {
				mFromColor.rgba = bloColor.cZero;
			}

			if (numparams > 0) {
				mToColor.rgba = reader.Read32();
				--numparams;
			} else {
				mToColor.rgba = bloColor.cOne;
			}

			reader.Skip(4);
			postLoad();
		}
		void postLoad() {
			if (mTextured) {
				mMinWidth = (mTextures[cTopLeft].texture.getWidth() + mTextures[cTopRight].texture.getWidth());
				mMinHeight = (mTextures[cTopLeft].texture.getHeight() + mTextures[cBottomLeft].texture.getHeight());
			} else {
				mMinWidth = 1;
				mMinHeight = 1;
			}
		}

		protected override void loadGLSelf() {
			base.loadGLSelf();
			for (int i = 0; i < 4; ++i) {
				if (mTextures[i].texture != null) {
					mTextures[i].texture.loadGL();
				}
			}
			if (mContentTexture != null) {
				mContentTexture.loadGL();
			}
		}

		protected override void drawSelf() {
			if (mRect.width < mMinWidth || mRect.height < mMinHeight) {
				return;
			}
			drawContents();
			if (mTextured) {
				TextureSlot slot;

				// fill rectangle (not necessarily content rectangle)
				bloRectangle fill = new bloRectangle(
					mTextures[cTopLeft].getWidth(),
					mTextures[cTopLeft].getHeight(),
					(mRect.width - mTextures[cBottomRight].getWidth()),
					(mRect.height - mTextures[cBottomRight].getHeight())
				);
				
				// corners
				drawCorner(mTextures[cTopLeft], 0, 0);
				drawCorner(mTextures[cTopRight], fill.right, 0);
				drawCorner(mTextures[cBottomLeft], 0, fill.bottom);
				drawCorner(mTextures[cBottomRight], fill.right, fill.bottom);
				
				// edges
				slot = mTextures[cTopRight]; // top
				slot.draw(
					fill.left, 0, fill.width, slot.getHeight(),
					(slot.mirror.hasFlag(bloMirror.X) ? 1.0d : 0.0d),
					(slot.mirror.hasFlag(bloMirror.Y) ? 0.0d : 1.0d),
					(slot.mirror.hasFlag(bloMirror.X) ? 1.0d : 0.0d),
					(slot.mirror.hasFlag(bloMirror.Y) ? 1.0d : 0.0d),
					mCumulativeAlpha, mFromColor, mToColor
				);
				slot = mTextures[cBottomRight]; // bottom
				slot.draw(
					fill.left, fill.bottom, fill.width, slot.getHeight(),
					(slot.mirror.hasFlag(bloMirror.X) ? 1.0d : 0.0d),
					(slot.mirror.hasFlag(bloMirror.Y) ? 0.0d : 1.0d),
					(slot.mirror.hasFlag(bloMirror.X) ? 1.0d : 0.0d),
					(slot.mirror.hasFlag(bloMirror.Y) ? 1.0d : 0.0d),
					mCumulativeAlpha, mFromColor, mToColor
				);
				slot = mTextures[cBottomLeft]; // left
				slot.draw(
					0, fill.top, slot.getWidth(), fill.height,
					(slot.mirror.hasFlag(bloMirror.X) ? 0.0d : 1.0d),
					(slot.mirror.hasFlag(bloMirror.Y) ? 1.0d : 0.0d),
					(slot.mirror.hasFlag(bloMirror.X) ? 1.0d : 0.0d),
					(slot.mirror.hasFlag(bloMirror.Y) ? 1.0d : 0.0d),
					mCumulativeAlpha, mFromColor, mToColor
				);
				slot = mTextures[cBottomRight]; // right
				slot.draw(
					fill.right, fill.top, slot.getWidth(), fill.height,
					(slot.mirror.hasFlag(bloMirror.X) ? 0.0d : 1.0d),
					(slot.mirror.hasFlag(bloMirror.Y) ? 1.0d : 0.0d),
					(slot.mirror.hasFlag(bloMirror.X) ? 1.0d : 0.0d),
					(slot.mirror.hasFlag(bloMirror.Y) ? 1.0d : 0.0d),
					mCumulativeAlpha, mFromColor, mToColor
				);
			}
		}
		void drawCorner(TextureSlot slot, int x, int y) {
			slot.draw(x, y, slot.mirror.hasFlag(bloMirror.X), slot.mirror.hasFlag(bloMirror.Y), mCumulativeAlpha, mFromColor, mToColor);
		}
		void drawContents() {

			var topLeftColor = bloMath.scaleAlpha(mTextures[cTopLeft].color, mCumulativeAlpha);
			var topRightColor = bloMath.scaleAlpha(mTextures[cTopRight].color, mCumulativeAlpha);
			var bottomLeftColor = bloMath.scaleAlpha(mTextures[cBottomLeft].color, mCumulativeAlpha);
			var bottomRightColor = bloMath.scaleAlpha(mTextures[cBottomRight].color, mCumulativeAlpha);

			GL.Disable(EnableCap.Texture2D);

			GL.Begin(PrimitiveType.Quads);
			GL.Color4(topLeftColor);
			GL.Vertex3(mContentRect.left, mContentRect.top, 0.0d);
			GL.Color4(topRightColor);
			GL.Vertex3(mContentRect.right, mContentRect.top, 0.0d);
			GL.Color4(bottomRightColor);
			GL.Vertex3(mContentRect.right, mContentRect.bottom, 0.0d);
			GL.Color4(bottomLeftColor);
			GL.Vertex3(mContentRect.left, mContentRect.bottom, 0.0d);
			GL.End();

			if (mContentTexture != null) {
				drawContentsTexture(mContentRect.left, mContentRect.top, mContentRect.width, mContentRect.height, mCumulativeAlpha);
			}

		}
		void drawContentsTexture(int x, int y, int width, int height, int alpha) {

			var hFactor = ((double)width / (double)mContentTexture.getWidth());
			var vFactor = ((double)height / (double)mContentTexture.getHeight());
			var sLeft = (-(hFactor - 1.0d) * 0.5d);
			var tTop = (-(vFactor - 1.0d) * 0.5d);
			var sRight = (sLeft + hFactor);
			var tBottom = (tTop + vFactor);

			var left = x;
			var top = y;
			var right = (left + width);
			var bottom = (top + height);

			var whiteColor = bloMath.scaleAlpha(new bloColor(bloColor.cWhite), alpha);

			GL.Enable(EnableCap.Texture2D);
			mContentTexture.bind();

			GL.Begin(PrimitiveType.Quads);
			GL.TexCoord2(sLeft, tTop);
			GL.Color4(whiteColor);
			GL.Vertex2(left, top);
			GL.TexCoord2(sRight, tTop);
			GL.Color4(whiteColor);
			GL.Vertex2(right, top);
			GL.TexCoord2(sRight, tBottom);
			GL.Color4(whiteColor);
			GL.Vertex2(right, bottom);
			GL.TexCoord2(sLeft, tBottom);
			GL.Color4(whiteColor);
			GL.Vertex2(left, bottom);
			GL.End();

		}

		protected const int cTopLeft = 0;
		protected const int cTopRight = 1;
		protected const int cBottomLeft = 2;
		protected const int cBottomRight = 3;

		protected class TextureSlot {

			public bloMirror mirror;
			public bloColor color;
			public bloTexture texture;

			public int getWidth() {
				return texture.getWidth();
			}
			public int getHeight() {
				return texture.getHeight();
			}

			public void draw(int x, int y, bool flipX, bool flipY, int alpha, bloColor fromColor, bloColor toColor) {
				draw(
					x, y, getWidth(), getHeight(),
					(flipX ? 0.0d : 1.0d),
					(flipY ? 0.0d : 1.0d),
					(flipX ? 1.0d : 0.0d),
					(flipY ? 1.0d : 0.0d),
					alpha, fromColor, toColor
				);
			}
			public void draw(int x, int y, int width, int height, double sRight, double tBottom, double sLeft, double tTop, int alpha, bloColor fromColor, bloColor toColor) {

				bool gradient = (
					fromColor.rgba != bloColor.cZero ||
					toColor.rgba != bloColor.cOne
				);

				if (gradient) {
					gl.useProgram();
					gl.setProgramColor("fromColor", fromColor);
					gl.setProgramColor("toColor", toColor);
					gl.setProgramInt("transparency", texture.getTransparency());
				}

				int left = x;
				int top = y;
				int right = (left + width);
				int bottom = (top + height);

				bloColor whiteColor = bloMath.scaleAlpha(new bloColor(bloColor.cWhite), alpha);

				GL.Enable(EnableCap.Texture2D);
				texture.bind();

				GL.Begin(PrimitiveType.Quads);
				GL.TexCoord2(sLeft, tTop);
				GL.Color4(whiteColor);
				GL.Vertex2(left, top);
				GL.TexCoord2(sRight, tTop);
				GL.Color4(whiteColor);
				GL.Vertex2(right, top);
				GL.TexCoord2(sRight, tBottom);
				GL.Color4(whiteColor);
				GL.Vertex2(right, bottom);
				GL.TexCoord2(sLeft, tBottom);
				GL.Color4(whiteColor);
				GL.Vertex2(left, bottom);
				GL.End();

				if (gradient) {
					gl.unuseProgram();
				}

			}
			
		}

	}

}
