
using arookas.IO.Binary;
using arookas.Xml;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Xml;

namespace arookas {

	public class bloWindow : bloPane {

		protected bloRectangle mContentRect;
		protected bloPalette mPalette;
		protected TextureSlot[] mTextures;
		protected bloTexture mContentTexture;
		protected bloColor mFromColor, mToColor;
		protected int mMinWidth;
		protected int mMinHeight;
		protected bool mTextured; // cached value, true if all textures are non-null

		internal bloWindow() {
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

			var finder = bloResourceFinder.getFinder();

			int x = reader.Read16();
			int y = reader.Read16();
			int width = reader.Read16();
			int height = reader.Read16();
			mContentRect.set(x, y, (x + width), (y + height));

			for (int i = 0; i < 4; ++i) {
				mTextures[i].texture = finder.find<bloTexture>(reader, "timg");
			}
			mPalette = finder.find<bloPalette>(reader, "tlut");

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

			var finder = bloResourceFinder.getFinder();

			int numparams = reader.Read8();

			int x = reader.Read16();
			int y = reader.Read16();
			int width = reader.Read16();
			int height = reader.Read16();
			mContentRect.set(x, y, (x + width), (y + height));

			for (int i = 0; i < 4; ++i) {
				mTextures[i].texture = finder.find<bloTexture>(reader, "timg");
			}
			mPalette = finder.find<bloPalette>(reader, "tlut");

			int bits = reader.Read8();
			for (int i = 0; i < 4; ++i) {
				mTextures[i].mirror = (bloMirror)((bits >> (6 - (i * 2))) & 3);
			}

			for (int i = 0; i < 4; ++i) {
				mTextures[i].color.rgba = reader.Read32();
			}

			numparams -= 14;

			if (numparams > 0) {
				mContentTexture = finder.find<bloTexture>(reader, "timg");
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
		protected override void loadXml(xElement element) {
			base.loadXml(element);

			var finder = bloResourceFinder.getFinder();

			var content = element.Element("content");
			mContentRect = bloXml.loadRectangle(content.Element("rectangle"));
			mContentTexture = finder.find<bloTexture>(content.Element("texture"), "timg");

			mPalette = finder.find<bloPalette>(element.Element("palette"), "tlut");
			
			var corners = element.Element("corners");
			loadCornerXml(mTextures[cTopLeft], finder, corners.Element("top-left"), 0);
			loadCornerXml(mTextures[cTopRight], finder, corners.Element("top-right"), bloMirror.X);
			loadCornerXml(mTextures[cBottomLeft], finder, corners.Element("bottom-left"), bloMirror.Y);
			loadCornerXml(mTextures[cBottomRight], finder, corners.Element("bottom-right"), (bloMirror.X | bloMirror.Y));

			bloXml.loadGradient(element.Element("gradient"), out mFromColor, out mToColor);

			postLoad();
		}
		void postLoad() {
			mTextured = true;
			for (int i = 0; i < 4; ++i) {
				if (mTextures[i].texture == null) {
					mTextured = false;
					break;
				}
			}
			if (mTextured) {
				mMinWidth = (mTextures[cTopLeft].texture.getWidth() + mTextures[cTopRight].texture.getWidth());
				mMinHeight = (mTextures[cTopLeft].texture.getHeight() + mTextures[cBottomLeft].texture.getHeight());
			} else {
				mMinWidth = 1;
				mMinHeight = 1;
			}
		}

		internal override void saveCompact(aBinaryWriter writer) {
			base.saveCompact(writer);

			writer.Write16((ushort)mContentRect.left);
			writer.Write16((ushort)mContentRect.top);
			writer.Write16((ushort)mContentRect.width);
			writer.Write16((ushort)mContentRect.height);

			for (int i = 0; i < 4; ++i) {
				bloResource.save(mTextures[i].texture, writer);
			}
			bloResource.save(mPalette, writer);

			byte bits = 0;
			for (int i = 0; i < 4; ++i) {
				bits <<= 2;
				bits |= (byte)mTextures[i].mirror;
			}
			writer.Write8(bits);

			for (int i = 0; i < 4; ++i) {
				writer.Write32(mTextures[i].color.rgba);
			}

			writer.WritePadding(4, 0);
		}
		internal override void saveBlo1(aBinaryWriter writer) {
			base.saveBlo1(writer);

			byte numparams;

			if (mToColor.rgba != bloColor.cOne) {
				numparams = 17;
			} else if (mFromColor.rgba != bloColor.cZero) {
				numparams = 16;
			} else if (mContentTexture != null) {
				numparams = 15;
			} else {
				numparams = 14;
			}

			writer.Write8(numparams);
			writer.Write16((ushort)mContentRect.left);
			writer.Write16((ushort)mContentRect.top);
			writer.Write16((ushort)mContentRect.width);
			writer.Write16((ushort)mContentRect.height);

			for (int i = 0; i < 4; ++i) {
				bloResource.save(mTextures[i].texture, writer);
			}
			bloResource.save(mPalette, writer);

			byte bits = 0;
			for (int i = 0; i < 4; ++i) {
				bits <<= 2;
				bits |= (byte)mTextures[i].mirror;
			}
			writer.Write8(bits);

			for (int i = 0; i < 4; ++i) {
				writer.Write32(mTextures[i].color.rgba);
			}

			numparams -= 14;

			if (numparams > 0) {
				bloResource.save(mContentTexture, writer);
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
		internal override void saveXml(XmlWriter writer) {
			base.saveXml(writer);

			writer.WriteStartElement("content");
			bloXml.saveRectangle(writer, mContentRect, "rectangle");
			bloResource.save(mContentTexture, "texture", writer);
			writer.WriteEndElement();

			bloResource.save(mPalette, "palette", writer);

			writer.WriteStartElement("corners");
			writer.WriteStartElement("top-left");
			bloResource.save(mTextures[cTopLeft].texture, "texture", writer);
			writer.WriteElementString("mirror", mTextures[cTopLeft].mirror.ToString());
			bloXml.saveColor(writer, mTextures[cTopLeft].color, "color");
			writer.WriteEndElement();
			writer.WriteStartElement("top-right");
			bloResource.save(mTextures[cTopRight].texture, "texture", writer);
			writer.WriteElementString("mirror", mTextures[cTopRight].mirror.ToString());
			bloXml.saveColor(writer, mTextures[cTopRight].color, "color");
			writer.WriteEndElement();
			writer.WriteStartElement("bottom-left");
			bloResource.save(mTextures[cBottomLeft].texture, "texture", writer);
			writer.WriteElementString("mirror", mTextures[cBottomLeft].mirror.ToString());
			bloXml.saveColor(writer, mTextures[cBottomLeft].color, "color");
			writer.WriteEndElement();
			writer.WriteStartElement("bottom-right");
			bloResource.save(mTextures[cBottomRight].texture, "texture", writer);
			writer.WriteElementString("mirror", mTextures[cBottomRight].mirror.ToString());
			bloXml.saveColor(writer, mTextures[cBottomRight].color, "color");
			writer.WriteEndElement();
			writer.WriteEndElement();

			bloXml.saveGradient(writer, mFromColor, mToColor, "gradient");
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

			var context = bloContext.getContext();

			context.useProgram();
			context.setProgramInt("textureCount", 1);
			context.setProgramInt("texture[0]", 0);
			context.setProgramColor("fromColor", new bloColor(bloColor.cZero));
			context.setProgramColor("toColor", new bloColor(bloColor.cOne));
			context.setProgramInt("transparency[0]", mContentTexture.getTransparency());

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

			mContentTexture.bind(0);

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

			context.unuseProgram();

		}

		static void loadCornerXml(TextureSlot slot, bloResourceFinder finder, xElement element, bloMirror defMirror) {
			slot.texture = finder.find<bloTexture>(element.Element("texture"), "timg");
			slot.color = bloXml.loadColor(element.Element("color"), new bloColor(bloColor.cWhite));
			if (!Enum.TryParse<bloMirror>(element.Element("mirror"), out slot.mirror)) {
				slot.mirror = 0;
			}
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

				var context = bloContext.getContext();

				context.useProgram();
				context.setProgramInt("textureCount", 1);
				context.setProgramInt("texture[0]", 0);
				context.setProgramColor("fromColor", fromColor);
				context.setProgramColor("toColor", toColor);
				context.setProgramInt("transparency[0]", texture.getTransparency());

				int left = x;
				int top = y;
				int right = (left + width);
				int bottom = (top + height);

				bloColor whiteColor = bloMath.scaleAlpha(new bloColor(bloColor.cWhite), alpha);

				texture.bind(0);

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

				context.unuseProgram();

			}
			
		}

	}

}
