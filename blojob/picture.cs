
using arookas.IO.Binary;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Xml;

namespace arookas {

	public class bloPicture : bloPane {

		protected bloTexture mTexture;
		protected bloPalette mPalette;
		protected bloBinding mBinding;
		protected bloMirror mMirror;
		protected bool mRotate90;
		protected bloWrapMode mWrapS, mWrapT;
		protected bloColor mFromColor, mToColor;
		protected bloColor[] mColors;

		public bloPicture() {
			mColors = new bloColor[4];
			mFromColor = new bloColor(bloColor.cZero);
			mToColor = new bloColor(bloColor.cOne);
		}

		protected override void loadCompact(aBinaryReader reader) {
			base.loadCompact(reader);

			var finder = bloResourceFinder.getFinder();

			mTexture = finder.find<bloTexture>(reader, "timg");
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
		}
		protected override void loadBlo1(aBinaryReader reader) {
			base.loadBlo1(reader);

			var finder = bloResourceFinder.getFinder();

			int numparams = reader.Read8();
			mTexture = finder.find<bloTexture>(reader, "timg");
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
		}

		public override void saveBlo1(aBinaryWriter writer) {
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
			bloResource.save(mTexture, writer);
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
		public override void saveXml(XmlWriter writer) {
			base.saveXml(writer);

			bloResource.save(mTexture, "texture", writer);
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
			if (mTexture != null) {
				mTexture.loadGL();
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
			if (mTexture != null) {
				drawSelf(0, 0, mRect.width, mRect.height, mBinding, mMirror, mRotate90, mWrapS, mWrapT);
			}
		}
		void drawSelf(int x, int y, int width, int height, bloBinding binding, bloMirror mirror, bool rotate90, bloWrapMode wrapS, bloWrapMode wrapT) {

			// wrapping
			int left = x;
			int top = y;

			if (wrapS == bloWrapMode.None) {
				double cap = (rotate90 ? mTexture.getHeight() : mTexture.getWidth());
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
				double cap = (rotate90 ? mTexture.getWidth() : mTexture.getHeight());
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
			double texWidth = mTexture.getWidth();
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
			double texHeight = mTexture.getHeight();
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
			
			bool gradient = (
				mFromColor.rgba != bloColor.cZero ||
				mToColor.rgba != bloColor.cOne
			);

			var context = bloContext.getContext();
			
			if (gradient) {
				context.useProgram();
				context.setProgramColor("fromColor", mFromColor);
				context.setProgramColor("toColor", mToColor);

				// if transparency is disabled on the texture, the alpha channel is then
				// blended to TEVREG2, which is hardcoded to store an opaque white color
				context.setProgramInt("transparency", mTexture.getTransparency());
			}

			bloRectangle rect = new bloRectangle(x, y, (x + width), (y + height));

			var topLeftColor = bloMath.scaleAlpha(mColors[cTopLeft], mCumulativeAlpha);
			var topRightColor = bloMath.scaleAlpha(mColors[cTopRight], mCumulativeAlpha);
			var bottomLeftColor = bloMath.scaleAlpha(mColors[cBottomLeft], mCumulativeAlpha);
			var bottomRightColor = bloMath.scaleAlpha(mColors[cBottomRight], mCumulativeAlpha);

			GL.Enable(EnableCap.Texture2D);
			mTexture.bind();

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

			if (mTexture.getTransparency() == 0) {
				// gl.enableBlend();
			}

			if (gradient) {
				context.unuseProgram();
			}

		}

		public override void info() {
			base.info();
			Console.WriteLine("Textured : {0}", (mTexture != null));
			if (mTexture != null) {
				Console.WriteLine("  Format : {0}", mTexture.getFormat());
				Console.WriteLine("  Transparency : {0}", mTexture.getTransparency());
				Console.WriteLine("  Size : {0}x{1}", mTexture.getWidth(), mTexture.getHeight());
			}
			Console.WriteLine("Paletted : {0}", (mPalette != null));
			if (mPalette != null) {
				Console.WriteLine("  Format : {0}", mPalette.getFormat());
				Console.WriteLine("  Entry Count : {0}", mPalette.getEntryCount());
			}
			Console.Write("Orientation :");
			if (mMirror.hasFlag(bloMirror.X)) {
				Console.Write(" (MirrorX)");
			}
			if (mMirror.hasFlag(bloMirror.Y)) {
				Console.Write(" (MirrorY)");
			}
			if (mRotate90) {
				Console.Write(" (Rotate90)");
			}
			Console.WriteLine();
			Console.WriteLine("Wrap S/T : {0} / {1}", mWrapS, mWrapT);
			Console.WriteLine("Colors : {0:X8} {1:X8} {2:X8} {3:X8}", mColors[cTopLeft].rgba, mColors[cTopRight].rgba, mColors[cBottomLeft].rgba, mColors[cBottomRight].rgba);
			if (mFromColor.rgba != bloColor.cZero || mToColor.rgba != bloColor.cOne) {
				Console.WriteLine("Gradient : {0:X8} > {1:X8}", mFromColor.rgba, mToColor.rgba);
			}
		}

		protected const int cTopLeft = 0;
		protected const int cTopRight = 1;
		protected const int cBottomLeft = 2;
		protected const int cBottomRight = 3;

	}

}
