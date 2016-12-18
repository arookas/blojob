
using arookas.IO.Binary;
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;

namespace arookas {

	class bloTexture : bloResource {

		protected gxTextureFormat mFormat;
		protected int mTransparency; // quick "does this texture have alpha" lookup
		protected int mWidth, mHeight;
		protected gxWrapMode mWrapS, mWrapT;
		protected gxTlutFormat mTlutFormat;
		protected gxAnisotropy mMaxAniso;
		protected int mMinFilter, mMagFilter;
		protected int mMinLod, mMaxLod;
		protected int mLodBias;
		protected bool mMipMap, mEdgeLOD, mBiasClamp;
		protected int mImageCount;
		protected short[] mPaletteData;
		protected aRGBA[] mImageData;
		protected bloPalette mBasePalette, mAttachedPalette;
		protected bool mLoadedGL;

		protected int mTextureName;

		public void save(string filename) {
			System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(mWidth, mHeight);
			for (var y = 0; y < mHeight; ++y) {
				for (var x = 0; x < mWidth; ++x) {
					bmp.SetPixel(x, y, mImageData[(mWidth * y) + x]);
				}
			}
			bmp.Save(filename);
		}

		public override void load(Stream stream) {
			aBinaryReader reader = new aBinaryReader(stream, Endianness.Big);
			
			mFormat = (gxTextureFormat)reader.Read8(); // 0000
			mTransparency = reader.Read8(); // 0001
			mWidth = reader.Read16(); // 0002
			mHeight = reader.Read16(); // 0004
			mWrapS = (gxWrapMode)reader.Read8(); // 0006
			mWrapT = (gxWrapMode)reader.Read8(); // 0007
			reader.Step(1); // 0008 (0001)
			mTlutFormat = (gxTlutFormat)reader.Read8(); // 0009
			int tlutentrycount = reader.Read16(); // 000A
			long tlutoffset = reader.Read32(); // 000C
			mMipMap = (reader.Read8() != 0); // 0010
			mEdgeLOD = (reader.Read8() != 0); // 0011
			mBiasClamp = (reader.Read8() != 0); // 0012
			mMaxAniso = (gxAnisotropy)reader.Read8(); // 0013
			mMinFilter = reader.Read8(); // 0014
			mMagFilter = reader.Read8(); // 0015
			mMinLod = reader.ReadS8(); // 0016
			mMaxLod = reader.ReadS8(); // 0017
			mImageCount = reader.Read8(); // 0018 (0001)
			mLodBias = reader.ReadS16(); // 001A
			long texoffset = reader.Read32(); // 001C
			loadImageData(reader, texoffset);
			if (tlutentrycount > 0) {
				loadPaletteData(reader, tlutentrycount, tlutoffset);
			}
		}

		public int loadGL() {
			if (!mLoadedGL) {
				mTextureName = gl.genTexObj();
			}
			gl.initTexObj(mTextureName, mImageData, mWidth, mHeight, mWrapS, mWrapT, false);
			gl.initTexObjLOD(mTextureName, gxTextureFilter.Linear, gxTextureFilter.Linear, 0.0d, 0.0d, 0.0d, false, false, gxAnisotropy.Aniso1);
			mLoadedGL = true;
			return mTextureName;
		}
		public void bind() {
			gl.loadTexObj(mTextureName);
		}

		public void attachPalette(bloPalette palette) {
			if (palette != null || mBasePalette == null) {
				mAttachedPalette = palette;
			} else {
				mAttachedPalette = mBasePalette;
			}
			mAttachedPalette.attachPalette(mPaletteData, mImageData);
			if (mLoadedGL) {
				loadGL();
			}
		}

		public gxTextureFormat getFormat() {
			return mFormat;
		}
		public int getTransparency() {
			return mTransparency;
		}
		public int getWidth() {
			return mWidth;
		}
		public int getHeight() {
			return mHeight;
		}

		protected void loadImageData(aBinaryReader reader, long texoffset) {
			if (texoffset == 0) {
				texoffset = 32;
			}
			reader.Goto(texoffset);
			var data = bloImage.loadImageData(reader, mWidth, mHeight, mFormat);
			if (data is aRGBA[]) {
				mImageData = (data as aRGBA[]);
				mPaletteData = null;
			} else if (data is short[]) {
				mImageData = new aRGBA[mWidth * mHeight];
				mPaletteData = (data as short[]);
			}
		}
		protected void loadPaletteData(aBinaryReader reader, int entrycount, long tlutoffset) {
			reader.Goto(tlutoffset);
			mBasePalette = new bloPalette(mTlutFormat, mTransparency, entrycount, reader);
			attachPalette(mBasePalette);
		}

	}

}
