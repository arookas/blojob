
using arookas.IO.Binary;
using arookas.Xml;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Xml;

namespace arookas {

	public class bloPicture : bloPane {

		protected bloTexture[] mTextures;
		protected double[] mBlendColorFactors, mBlendAlphaFactors;
		protected int mTextureCount;
		protected bloPalette mPalette;
		protected bloBinding mBinding;
		protected bloMirror mMirror;
		protected bool mRotate90;
		protected bloWrapMode mWrapS, mWrapT;
		protected bloColor mFromColor, mToColor;
		protected Vector4d mKonstColor, mKonstAlpha;
		protected bloColor[] mColors;

		internal bloPicture() {
			mTextures = new bloTexture[cTextureSlots];
			mBlendColorFactors = new double[cTextureSlots];
			mBlendAlphaFactors = new double[cTextureSlots];
			mTextureCount = 0;
			mColors = new bloColor[4];
			mFromColor = new bloColor(bloColor.cZero);
			mToColor = new bloColor(bloColor.cOne);
			mBinding = (bloBinding.Left | bloBinding.Top | bloBinding.Right | bloBinding.Bottom);
		}
		public bloPicture(bloTexture texture) {
			if (texture == null) {
				throw new ArgumentNullException("texture");
			}
			mTextures[0] = texture;
			mTextureCount = 1;
			mRect.set(0, 0, texture.getWidth(), texture.getHeight());
		}
		public bloPicture(uint name, bloRectangle rectangle, bloTexture texture, bloPalette palette) {
			if (texture == null) {
				throw new ArgumentNullException("texture");
			}
			mName = name;
			mRect = rectangle;
			mTextures[0] = texture;
			mTextureCount = 1;
			texture.attachPalette(palette);
		}

		protected override void loadCompact(aBinaryReader reader) {
			base.loadCompact(reader);

			var finder = bloResourceFinder.getFinder();

			mTextureCount = 1;
			mTextures[0] = finder.find<bloTexture>(reader, "timg");
			mPalette = finder.find<bloPalette>(reader, "tlut");

			mBinding = (bloBinding)reader.Read8();
			
			int bits = reader.Read8();
			mMirror = (bloMirror)((bits >> 0) & 3);
			mRotate90 = ((bits & 4) != 0);
			mWrapS = (bloWrapMode)((bits >> 3) & 3);
			mWrapT = mWrapS;

			reader.Skip(4);

			for (int i = 0; i < 4; ++i) {
				mColors[i] = new bloColor(bloColor.cWhite);
			}

			setBlendKonstColor();
			setBlendKonstAlpha();
		}
		protected override void loadBlo1(aBinaryReader reader) {
			base.loadBlo1(reader);

			var finder = bloResourceFinder.getFinder();

			int numparams = reader.Read8();
			mTextureCount = 1;
			mTextures[0] = finder.find<bloTexture>(reader, "timg");
			mPalette = finder.find<bloPalette>(reader, "tlut");
			mBinding = (bloBinding)reader.Read8();

			numparams -= 3;

			if (numparams > 0) {
				int bits = reader.Read8();
				mMirror = (bloMirror)(bits & 3);
				mRotate90 = ((bits & 4) != 0);
				--numparams;
			} else {
				mMirror = 0;
				mRotate90 = false;
			}

			if (numparams > 0) {
				int bits = reader.Read8();
				mWrapS = (bloWrapMode)((bits >> 2) & 3);
				mWrapT = (bloWrapMode)((bits >> 0) & 3);
				--numparams;
			} else {
				mWrapS = bloWrapMode.None;
				mWrapT = bloWrapMode.None;
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

			for (int i = 0; i < 4; ++i) {
				if (numparams > 0) {
					mColors[i] = new bloColor(reader.Read32());
					--numparams;
				} else {
					mColors[i] = new bloColor(bloColor.cWhite);
				}
			}

			reader.Skip(4);

			setBlendKonstColor();
			setBlendKonstAlpha();
		}
		protected override void loadXml(xElement element) {
			base.loadXml(element);

			var finder = bloResourceFinder.getFinder();

			mTextureCount = 1;
			mTextures[0] = finder.find<bloTexture>(element.Element("texture"), "timg");
			mPalette = finder.find<bloPalette>(element.Element("palette"), "tlut");

			if (!Enum.TryParse<bloBinding>(element.Element("binding"), true, out mBinding)) {
				mBinding = (bloBinding.Left | bloBinding.Top | bloBinding.Right | bloBinding.Bottom);
			}

			if (!Enum.TryParse<bloMirror>(element.Element("mirror"), true, out mMirror)) {
				mMirror = 0;
			}

			mRotate90 = (element.Element("rotate-90") | false);

			if (!Enum.TryParse<bloWrapMode>(element.Element("wrap-s"), true, out mWrapS)) {
				mWrapS = bloWrapMode.None;
			}

			if (!Enum.TryParse<bloWrapMode>(element.Element("wrap-t"), true, out mWrapT)) {
				mWrapT = bloWrapMode.None;
			}

			bloXml.loadGradient(element.Element("gradient"), out mFromColor, out mToColor);

			var white = new bloColor(bloColor.cWhite);
			var colors = element.Element("colors");

			mColors[cTopLeft] = bloXml.loadColor(colors.Element("top-left"), white);
			mColors[cTopRight] = bloXml.loadColor(colors.Element("top-right"), white);
			mColors[cBottomLeft] = bloXml.loadColor(colors.Element("bottom-left"), white);
			mColors[cBottomRight] = bloXml.loadColor(colors.Element("bottom-right"), white);

			setBlendKonstColor();
			setBlendKonstAlpha();
		}

		internal override void saveCompact(aBinaryWriter writer) {
			base.saveCompact(writer);

			if (mTextureCount > 0) {
				bloResource.save(mTextures[0], writer);
			} else {
				bloResource.save(null, writer);
			}
			bloResource.save(mPalette, writer);

			writer.Write8((byte)mBinding);

			byte bits = 0;
			bits |= (byte)mWrapS;
			bits <<= 1;
			bits |= (byte)(mRotate90 ? 1 : 0);
			bits <<= 2;
			bits |= (byte)mMirror;
			writer.Write8(bits);

			writer.WritePadding(4, 0);

			for (int i = 0; i < 4; ++i) {
				writer.Write32(mColors[i].rgba);
			}
		}
		internal override void saveBlo1(aBinaryWriter writer) {
			base.saveBlo1(writer);

			byte numparams;

			if (mColors[cBottomRight].rgba != bloColor.cWhite) {
				numparams = 11;
			} else if (mColors[cBottomLeft].rgba != bloColor.cWhite) {
				numparams = 10;
			} else if (mColors[cTopRight].rgba != bloColor.cWhite) {
				numparams = 9;
			} else if (mColors[cTopLeft].rgba != bloColor.cWhite) {
				numparams = 8;
			} else if (mToColor.rgba != bloColor.cOne) {
				numparams = 7;
			} else if (mFromColor.rgba != bloColor.cZero) {
				numparams = 6;
			} else if (mWrapS != bloWrapMode.None || mWrapT != bloWrapMode.None) {
				numparams = 5;
			} else if (mMirror != 0 || mRotate90) {
				numparams = 4;
			} else {
				numparams = 3;
			}

			writer.Write8(numparams);
			if (mTextureCount > 0) {
				bloResource.save(mTextures[0], writer);
			} else {
				bloResource.save(null, writer);
			}
			bloResource.save(mPalette, writer);
			writer.Write8((byte)mBinding);

			numparams -= 3;

			if (numparams > 0) {
				byte bits = (byte)mMirror;
				if (mRotate90) {
					bits |= 4;
				}
				writer.Write8(bits);
				--numparams;
			}

			if (numparams > 0) {
				byte bits = 0;
				bits |= (byte)mWrapS;
				bits <<= 2;
				bits |= (byte)mWrapT;
				writer.Write8(bits);
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

			for (int i = 0; i < 4; ++i) {
				if (numparams > 0) {
					writer.Write32(mColors[i].rgba);
					--numparams;
				}
			}

			writer.WritePadding(4, 0);
		}
		internal override void saveXml(XmlWriter writer) {
			base.saveXml(writer);

			if (mTextureCount > 0) {
				bloResource.save(mTextures[0], "texture", writer);
			}
			bloResource.save(mPalette, "palette", writer);
			writer.WriteElementString("binding", mBinding.ToString());

			if (mMirror != 0) {
				writer.WriteElementString("mirror", mMirror.ToString());
			}

			if (mRotate90) {
				writer.WriteElementString("rotate-90", mRotate90.ToString());
			}

			if (mWrapS != bloWrapMode.None) {
				writer.WriteElementString("wrap-s", mWrapS.ToString());
			}

			if (mWrapT != bloWrapMode.None) {
				writer.WriteElementString("wrap-t", mWrapT.ToString());
			}

			bloXml.saveGradient(writer, mFromColor, mToColor, "gradient");

			var saveColors = false;
			for (int i = 0; i < 4; ++i) {
				if (mColors[i].rgba != bloColor.cWhite) {
					saveColors = true;
					break;
				}
			}

			if (saveColors) {
				writer.WriteStartElement("colors");
				bloXml.saveColor(writer, mColors[cTopLeft], "top-left");
				bloXml.saveColor(writer, mColors[cTopRight], "top-right");
				bloXml.saveColor(writer, mColors[cBottomLeft], "bottom-left");
				bloXml.saveColor(writer, mColors[cBottomRight], "bottom-right");
				writer.WriteEndElement();
			}
		}

		protected override void loadGLSelf() {
			base.loadGLSelf();
			if (mTextureCount > 0 && mTextures[0] != null) {
				mTextures[0].loadGL();
			}
		}

		protected override void drawSelf() {
			var context = bloContext.getContext();

			if (context.hasRenderFlags(bloRenderFlags.PictureWireframe)) {
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

				var rect = new bloRectangle(0, 0, mRect.width, mRect.height);
				var white = new bloColor(bloColor.cWhite);

				GL.Disable(EnableCap.Texture2D);

				GL.Begin(PrimitiveType.Quads);
				GL.Color4(white);
				GL.Vertex2(rect.topleft);
				GL.Color4(white);
				GL.Vertex2(rect.topright);
				GL.Color4(white);
				GL.Vertex2(rect.bottomright);
				GL.Color4(white);
				GL.Vertex2(rect.bottomleft);
				GL.End();

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}
			if (mTextureCount > 0 && mTextures[0] != null) {
				drawSelf(0, 0, mRect.width, mRect.height, mBinding, mMirror, mRotate90, mWrapS, mWrapT);
			}
		}
		void drawSelf(int x, int y, int width, int height, bloBinding binding, bloMirror mirror, bool rotate90, bloWrapMode wrapS, bloWrapMode wrapT) {

			var texture = mTextures[0];

			// wrapping
			int left = x;
			int top = y;

			if (wrapS == bloWrapMode.None) {
				double cap = (rotate90 ? texture.getHeight() : texture.getWidth());
				if (!binding.hasFlag(bloBinding.Left)) {
					if (width > cap) {
						left = (int)(
							binding.hasFlag(bloBinding.Right) ?
							(x + width - cap) : (x + ((width - cap)) / 2)
						);
						width = (int)cap;
					}
				} else if (!binding.hasFlag(bloBinding.Right)) {
					if (width > cap) {
						width = (int)cap;
					}
				}
			}

			if (mWrapT == bloWrapMode.None) {
				double cap = (rotate90 ? texture.getWidth() : texture.getHeight());
				if (!binding.hasFlag(bloBinding.Top)) {
					if (height > cap) {
						top = (int)(
							binding.hasFlag(bloBinding.Bottom) ?
							(y + height - cap) : (y + ((height - cap)) / 2)
						);
						height = (int)cap;
					}
				} else if (!binding.hasFlag(bloBinding.Bottom)) {
					if (height > cap) {
						height = (int)cap;
					}
				}
			}

			// binding
			bool bindLeft, bindRight, bindTop, bindBottom;
			if (rotate90) {
				bindLeft = (mirror.hasFlag(bloMirror.X) ? binding.hasFlag(bloBinding.Bottom) : binding.hasFlag(bloBinding.Top));
				bindRight = (mirror.hasFlag(bloMirror.X) ? binding.hasFlag(bloBinding.Top) : binding.hasFlag(bloBinding.Bottom));
				bindTop = (mirror.hasFlag(bloMirror.Y) ? binding.hasFlag(bloBinding.Left) : binding.hasFlag(bloBinding.Right));
				bindBottom = (mirror.hasFlag(bloMirror.Y) ? binding.hasFlag(bloBinding.Right) : binding.hasFlag(bloBinding.Left));
			} else {
				bindLeft = (mirror.hasFlag(bloMirror.X) ? binding.hasFlag(bloBinding.Right) : binding.hasFlag(bloBinding.Left));
				bindRight = (mirror.hasFlag(bloMirror.X) ? binding.hasFlag(bloBinding.Left) : binding.hasFlag(bloBinding.Right));
				bindTop = (mirror.hasFlag(bloMirror.Y) ? binding.hasFlag(bloBinding.Bottom) : binding.hasFlag(bloBinding.Top));
				bindBottom = (mirror.hasFlag(bloMirror.Y) ? binding.hasFlag(bloBinding.Top) : binding.hasFlag(bloBinding.Bottom));
			}

			// U mapping
			int rectWidth = (rotate90 ? height : width);
			double texWidth = texture.getWidth();
			double widthFactor = (rectWidth / texWidth);
			double fLeft, fRight;

			if (bindLeft) {
				fLeft = 0.0d;
				fRight = (bindRight ? 1.0d : widthFactor);
			} else if (bindRight) {
				fLeft = (1.0d - widthFactor);
				fRight = 1.0d;
			} else {
				fLeft = (0.5d - (widthFactor * 0.5d));
				fRight = (0.5d + (widthFactor * 0.5d));
			}

			// V mapping
			int rectHeight = (rotate90 ? width : height);
			double texHeight = texture.getHeight();
			double heightFactor = (rectHeight / texHeight);
			double fTop, fBottom;

			if (bindTop) {
				fTop = 0.0d;
				fBottom = (bindBottom ? 1.0d : heightFactor);
			} else if (bindBottom) {
				fTop = (1.0d - heightFactor);
				fBottom = 1.0d;
			} else {
				fTop = (0.5d - (heightFactor * 0.5d));
				fBottom = (0.5d + (heightFactor * 0.5d));
			}

			// mirror
			if (mirror.hasFlag(bloMirror.X)) {
				bloMath.swap(ref fLeft, ref fRight);
			}

			if (mirror.hasFlag(bloMirror.Y)) {
				bloMath.swap(ref fTop, ref fBottom);
			}

			// render
			if (rotate90) {
				drawSelf(
					(left - x), (top - y), width, height,
					new Vector2d(fLeft, fBottom),
					new Vector2d(fLeft, fTop),
					new Vector2d(fRight, fBottom),
					new Vector2d(fRight, fTop)
				);
			} else {
				drawSelf(
					(left - x), (top - y), width, height,
					new Vector2d(fLeft, fTop),
					new Vector2d(fRight, fTop),
					new Vector2d(fLeft, fBottom),
					new Vector2d(fRight, fBottom)
				);
			}
		}
		void drawSelf(int x, int y, int width, int height, Vector2d uvTopLeft, Vector2d uvTopRight, Vector2d uvBottomLeft, Vector2d uvBottomRight) {
			
			var context = bloContext.getContext();

			context.useProgram();
			context.setProgramInt("textureCount", mTextureCount);
			for (var i = 0; i < mTextureCount; ++i) {
				context.setProgramInt(String.Format("texture[{0}]", i), i);
				context.setProgramInt(String.Format("transparency[{0}]", i), mTextures[i].getTransparency());
			}
			context.setProgramVector("blendColorFactor", mKonstColor);
			context.setProgramVector("blendAlphaFactor", mKonstAlpha);
			context.setProgramColor("fromColor", mFromColor);
			context.setProgramColor("toColor", mToColor);

			bloRectangle rect = new bloRectangle(x, y, (x + width), (y + height));

			var topLeftColor = bloMath.scaleAlpha(mColors[cTopLeft], mCumulativeAlpha);
			var topRightColor = bloMath.scaleAlpha(mColors[cTopRight], mCumulativeAlpha);
			var bottomLeftColor = bloMath.scaleAlpha(mColors[cBottomLeft], mCumulativeAlpha);
			var bottomRightColor = bloMath.scaleAlpha(mColors[cBottomRight], mCumulativeAlpha);

			for (var i = 0; i < mTextureCount; ++i) {
				mTextures[i].bind(i);
			}

			GL.Begin(PrimitiveType.Quads);
			GL.TexCoord2(uvTopLeft);
			GL.Color4(topLeftColor);
			GL.Vertex2(rect.topleft);
			GL.TexCoord2(uvTopRight);
			GL.Color4(topRightColor);
			GL.Vertex2(rect.topright);
			GL.TexCoord2(uvBottomRight);
			GL.Color4(bottomRightColor);
			GL.Vertex2(rect.bottomright);
			GL.TexCoord2(uvBottomLeft);
			GL.Color4(bottomLeftColor);
			GL.Vertex2(rect.bottomleft);
			GL.End();

			context.unuseProgram();

		}

		public int getTextureCount() {
			return mTextureCount;
		}

		public bool insert(bloTexture texture, int slot, double factor) {
			if (texture == null) {
				return false;
			}
			if (slot < 0 || slot >= cTextureSlots || slot > mTextureCount) {
				return false;
			}
			if (mTextureCount >= cTextureSlots) {
				return false;
			}
			for (var i = (cTextureSlots - 1); i > slot; --i) {
				mTextures[i] = mTextures[i - 1];
				mBlendColorFactors[i] = mBlendColorFactors[i - 1];
				mBlendAlphaFactors[i] = mBlendAlphaFactors[i - 1];
			}
			mTextures[slot] = texture;
			mBlendColorFactors[slot] = factor;
			mBlendAlphaFactors[slot] = factor;
			if (mTextureCount == 0) {
				mRect.set(0, 0, texture.getWidth(), texture.getHeight());
			}
			++mTextureCount;
			setBlendKonstColor();
			setBlendKonstAlpha();
			return true;
		}
		public bloTexture changeTexture(bloTexture texture, int slot) {
			if (texture == null) {
				return null;
			}
			if (slot < 0 || slot >= cTextureSlots || slot >= mTextureCount) {
				return null;
			}
			var old = mTextures[slot];
			mTextures[slot] = texture;
			return old;
		}
		public bool remove(int slot) {
			if (slot < 0 || slot >= cTextureSlots || slot >= mTextureCount) {
				return false;
			}
			for (var i = slot; i < (mTextureCount - 1); ++i) {
				mTextures[i] = mTextures[i + 1];
				mBlendColorFactors[i] = mBlendColorFactors[i + 1];
				mBlendAlphaFactors[i] = mBlendAlphaFactors[i + 1];
			}
			--mTextureCount;
			setBlendKonstColor();
			setBlendKonstAlpha();
			return true;
		}
		
		public void setBlendFactor(double factor, int slot) {
			setBlendFactor(factor, factor, slot);
		}
		public void setBlendFactor(double colorFactor, double alphaFactor, int slot) {
			setBlendColorFactor(colorFactor, slot);
			setBlendAlphaFactor(alphaFactor, slot);
		}
		public void setBlendColorFactor(double factor, int slot) {
			if (slot < 0 || slot >= cTextureSlots || slot >= mTextureCount) {
				return;
			}
			mBlendColorFactors[slot] = factor;
		}
		public void setBlendAlphaFactor(double factor, int slot) {
			if (slot < 0 || slot >= cTextureSlots || slot >= mTextureCount) {
				return;
			}
			mBlendAlphaFactors[slot] = factor;
		}
		public void setBlendColorFactor(double a, double b, double c, double d) {
			mBlendColorFactors[0] = a;
			mBlendColorFactors[1] = b;
			mBlendColorFactors[2] = c;
			mBlendColorFactors[3] = d;
		}
		public void setBlendAlphaFactor(double a, double b, double c, double d) {
			mBlendAlphaFactors[0] = a;
			mBlendAlphaFactors[1] = b;
			mBlendAlphaFactors[2] = c;
			mBlendAlphaFactors[3] = d;
		}

		public void setBlendKonstColor() {
			var konst = new double[4];
#if BLOJOB_KONST
			for (int slot = 1; slot < mTextureCount; ++slot) {
				double sum = 0.0d;
				for (int prev = 0; prev < slot; ++prev) {
					sum += mBlendColorFactors[prev];
				}
				double factor = (mBlendColorFactors[slot] + sum);
				if (factor != 0.0d) {
					konst[slot] = (1.0d - (sum / factor));
				}
			}
#else
			double sum = 0.0d;
			for (var i = 0; i < mTextureCount; ++i) {
				sum += mBlendColorFactors[i];
			}
			for (var i = 0; i < mTextureCount; ++i) {
				konst[i] = (mBlendColorFactors[i] / sum);
			}
#endif
			mKonstColor = new Vector4d(konst[0], konst[1], konst[2], konst[3]);
		}
		public void setBlendKonstAlpha() {
			var konst = new double[4];
#if BLOJOB_KONST
			for (int slot = 1; slot < mTextureCount; ++slot) {
				double sum = 0.0d;
				for (int prev = 0; prev < slot; ++prev) {
					sum += mBlendAlphaFactors[prev];
				}
				double factor = (mBlendAlphaFactors[slot] + sum);
				if (factor != 0.0d) {
					konst[slot] = (1.0d - (sum / factor));
				}
			}
#else
			double sum = 0.0d;
			for (var i = 0; i < mTextureCount; ++i) {
				sum += mBlendAlphaFactors[i];
			}
			for (var i = 0; i < mTextureCount; ++i) {
				konst[i] = (mBlendAlphaFactors[i] / sum);
			}
#endif
			mKonstAlpha = new Vector4d(konst[0], konst[1], konst[2], konst[3]);
		}

		protected const int cTopLeft = 0;
		protected const int cTopRight = 1;
		protected const int cBottomLeft = 2;
		protected const int cBottomRight = 3;

		protected const int cTextureSlots = 4;

	}

}
