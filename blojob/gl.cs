
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;

namespace arookas {

	static class gl {

		public static int genTexObj() {
			return GL.GenTexture();
		}
		public static void initTexObj(int textureName, aRGBA[] data, int width, int height, gxWrapMode wrapS, gxWrapMode wrapT, bool mipmap) {
			GL.BindTexture(TextureTarget.Texture2D, textureName);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)convertWrapMode(wrapS));
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)convertWrapMode(wrapT));
		}
		public static void initTexObjLOD(int textureName, gxTextureFilter minFilter, gxTextureFilter magFilter, double minLod, double maxLod, double lodBias, bool biasClamp, bool doEdgeLod, gxAnisotropy maxAniso) {
			GL.BindTexture(TextureTarget.Texture2D, textureName);
			// GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, lodBias);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)convertMinFilter(minFilter));
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)convertMagFilter(magFilter));
			// GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinLod, minLod);
			// GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLod, maxLod);
		}
		public static void loadTexObj(int textureName) {
			GL.Enable(EnableCap.Texture2D);
			GL.BindTexture(TextureTarget.Texture2D, textureName);
		}
		public static void unloadTexObj() {
			GL.BindTexture(TextureTarget.Texture2D, 0);
		}

		public static void setCullMode(gxCullMode mode) {
			GL.Enable(EnableCap.CullFace);
			switch (mode) {
				case gxCullMode.None: GL.Disable(EnableCap.CullFace); break;
				case gxCullMode.Front: GL.CullFace(CullFaceMode.Front); break;
				case gxCullMode.Back: GL.CullFace(CullFaceMode.Back); break;
				case gxCullMode.All: GL.CullFace(CullFaceMode.FrontAndBack); break;
			}
		}

		static TextureWrapMode convertWrapMode(gxWrapMode mode) {
			switch (mode) {
				case gxWrapMode.Clamp: return TextureWrapMode.ClampToEdge;
				case gxWrapMode.Repeat: return TextureWrapMode.Repeat;
				case gxWrapMode.Mirror: return TextureWrapMode.MirroredRepeat;
			}
			throw new NotImplementedException("Unknown wrap mode.");
		}

		static TextureMinFilter convertMinFilter(gxTextureFilter minFilter) {
			switch (minFilter) {
				case gxTextureFilter.Near: return TextureMinFilter.Nearest;
				case gxTextureFilter.Linear: return TextureMinFilter.Linear;
				case gxTextureFilter.NearMipNear: return TextureMinFilter.NearestMipmapNearest;
				case gxTextureFilter.LinearMipNear: return TextureMinFilter.LinearMipmapNearest;
				case gxTextureFilter.NearMipLinear: return TextureMinFilter.NearestMipmapLinear;
				case gxTextureFilter.LinearMipLinear: return TextureMinFilter.LinearMipmapLinear;
			}
			throw new NotImplementedException("Unknown minification filter.");
		}
		static TextureMagFilter convertMagFilter(gxTextureFilter magFilter) {
			switch (magFilter) {
				case gxTextureFilter.Near: return TextureMagFilter.Nearest;
				case gxTextureFilter.Linear: return TextureMagFilter.Linear;
			}
			throw new ArgumentOutOfRangeException("magFilter", "Invalid magnification filter.");
		}

	}

}
