
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;

namespace arookas {

	static class gl {

		static bool sUseProgram;
		static glProgram sProgram;
		static bloRenderFlags sRenderFlags;

		public static bloRenderFlags getRenderFlags() {
			return sRenderFlags;
		}
		public static bool hasRenderFlags(bloRenderFlags flags) {
			return sRenderFlags.hasFlag(flags);
		}
		public static bloRenderFlags setRenderFlags(bloRenderFlags flags) {
			var oldRenderFlags = sRenderFlags;
			sRenderFlags = flags;
			return oldRenderFlags;
		}

		public static glProgram setProgram(glProgram program) {
			if (sUseProgram) {
				sProgram.unuse();
			}
			var oldProgram = sProgram;
			sProgram = program;
			if (sUseProgram) {
				sProgram.use();
			}
			return oldProgram;
		}
		public static glProgram unsetProgram() {
			unuseProgram();
			var oldProgram = sProgram;
			sProgram = null;
			return oldProgram;
		}

		public static void useProgram() {
			if (sProgram != null) {
				sUseProgram = true;
				sProgram.use();
			}
		}
		public static void unuseProgram() {
			if (sProgram != null) {
				sUseProgram = false;
				sProgram.unuse();
			}
		}

		public static void setProgramInt(string name, int integer) {
			if (sProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(sProgram, name);
			GL.Uniform1(location, integer);
		}

		public static void setProgramColor(string name, bloColor color) {
			if (sProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(sProgram, name);
			GL.Uniform4(location, color);
		}
		public static void setProgramColor(string name, Color4 color) {
			if (sProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(sProgram, name);
			GL.Uniform4(location, color);
		}

		public static void setProgramVector(string name, bloPoint vector) {
			if (sProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(sProgram, name);
			GL.Uniform2(location, vector.x, vector.y);
		}
		public static void setProgramVector(string name, Vector2 vector) {
			if (sProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(sProgram, name);
			GL.Uniform2(location, vector);
		}
		public static void setProgramVector(string name, Vector2d vector) {
			if (sProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(sProgram, name);
			GL.Uniform2(location, vector.X, vector.Y);
		}
		public static void setProgramVector(string name, Vector3 vector) {
			if (sProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(sProgram, name);
			GL.Uniform3(location, vector);
		}
		public static void setProgramVector(string name, Vector3d vector) {
			if (sProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(sProgram, name);
			GL.Uniform3(location, vector.X, vector.Y, vector.Z);
		}
		public static void setProgramVector(string name, Vector4 vector) {
			if (sProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(sProgram, name);
			GL.Uniform4(location, vector);
		}
		public static void setProgramVector(string name, Vector4d vector) {
			if (sProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(sProgram, name);
			GL.Uniform4(location, vector.X, vector.Y, vector.Z, vector.W);
		}

		public static int genTexObj() {
			return GL.GenTexture();
		}
		public static void initTexObj(int textureName, aRGBA[] data, int width, int height, gxWrapMode wrapS, gxWrapMode wrapT, bool mipmap) {
			GL.BindTexture(TextureTarget.Texture2D, textureName);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)convertGXToGL(wrapS));
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)convertGXToGL(wrapT));
		}
		public static void initTexObjLOD(int textureName, gxTextureFilter minFilter, gxTextureFilter magFilter, double minLod, double maxLod, double lodBias, bool biasClamp, bool doEdgeLod, gxAnisotropy maxAniso) {
			// GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, lodBias);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear); // minFilter);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Linear); // magFilter);
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

		public static void clearBuffer(Color4 color) {
			GL.ClearColor(color);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}

		public static void initLine() {
			GL.Enable(EnableCap.LineSmooth);
			GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
			GL.LineWidth(1.5f);
		}
		public static void initBlend() {
			GL.Enable(EnableCap.Blend);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
		}
		public static void initPolyMode() {
			GL.FrontFace(FrontFaceDirection.Cw);
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
		}

		public static void initOrtho(bloRectangle viewport) {
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Viewport(viewport.left, viewport.top, (viewport.right - viewport.left), (viewport.bottom - viewport.top));
			GL.Ortho(viewport.left, viewport.right, viewport.bottom, viewport.top, -1.0d, 1.0d);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
		}

		static TextureWrapMode convertGXToGL(gxWrapMode mode) {
			switch (mode) {
				case gxWrapMode.Clamp: return TextureWrapMode.ClampToEdge;
				case gxWrapMode.Repeat: return TextureWrapMode.Repeat;
				case gxWrapMode.Mirror: return TextureWrapMode.MirroredRepeat;
			}
			throw new NotImplementedException("Unknown wrap mode.");
		}

	}

}
