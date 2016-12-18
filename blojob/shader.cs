
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace arookas {

	class glProgram : IEnumerable<glShader>, IDisposable {

		int mId;
		List<glShader> mShaders;
		bool mDisposed;

		public glShader this[int index] {
			get { return mShaders[index]; }
		}
		public glShader this[ShaderType type] {
			get { return mShaders.FirstOrDefault(shader => shader.getShaderType() == type); }
		}
		public int this[ProgramParameter parameter] {
			get {
				int value;
				GL.GetProgram(mId, parameter, out value);
				return value;
			}
		}

		glProgram(int id) {
			mShaders = new List<glShader>(5);
			mId = id;
		}

		public void attach(glShader shader) {
			if (shader == null) {
				throw new ArgumentNullException("shader");
			}

			GL.AttachShader(mId, shader);
			mShaders.Add(shader);
		}
		public void link() {
			GL.LinkProgram(mId);
			if (this[ProgramParameter.LinkStatus] != 1) {
				throw new InvalidOperationException(String.Format("The GLProgram failed to be linked. The info log is:\n{0}", getInfoLog()));
			}
		}
		public void use() {
			GL.UseProgram(mId);
		}
		public string getInfoLog() {
			return GL.GetProgramInfoLog(mId);
		}
		public void Dispose() {
			if (!mDisposed) {
				int status;
				GL.DeleteProgram(mId);
				GL.GetProgram(mId, ProgramParameter.DeleteStatus, out status);
				if (status != 1) {
					throw new InvalidOperationException("The GL program failed to be deleted.");
				}
				mDisposed = true;
			}
		}
		public void unuse() {
			glProgram.unuseAll();
		}

		public IEnumerator<glShader> GetEnumerator() {
			return mShaders.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public static glProgram create() {
			return new glProgram(GL.CreateProgram());
		}
		public static void unuseAll() {
			GL.UseProgram(0);
		}

		public static implicit operator int(glProgram program) {
			return program.mId;
		}

	}

	class glShader {

		int mId;
		ShaderType mShaderType;
		string mSource;
		bool mDisposed;

		public int this[ShaderParameter param] {
			get {
				int value;
				GL.GetShader(mId, param, out value);
				return value;
			}
		}

		glShader(int id, ShaderType type, string source) {
			mId = id;
			mShaderType = type;
			mSource = source;
		}

		public ShaderType getShaderType() {
			return mShaderType;
		}
		public string getInfoLog() {
			return GL.GetShaderInfoLog(mId);
		}
		public void Dispose() {
			if (!mDisposed) {
				int status;
				GL.DeleteShader(mId);
				GL.GetShader(mId, ShaderParameter.DeleteStatus, out status);
				if (status != 1) {
					throw new InvalidOperationException("The GL shader failed to be deleted.");
				}
				mDisposed = true;
			}
		}

		public static glShader fromFile(ShaderType type, string path) {
			if (path == null) {
				throw new ArgumentNullException("path");
			}
			return fromSource(type, File.ReadAllText(path));
		}
		public static glShader fromSource(ShaderType type, string source) {
			if (!type.IsDefined()) {
				throw new ArgumentOutOfRangeException("type", type, "The specified shader type was not a defined ShaderType value.");
			}
			if (source == null) {
				throw new ArgumentNullException("source");
			}

			var id = GL.CreateShader(type);
			GL.ShaderSource(id, source);
			GL.CompileShader(id);

			int result;
			GL.GetShader(id, ShaderParameter.CompileStatus, out result);

			if (result != 1) {
				var log = GL.GetShaderInfoLog(id);
				GL.DeleteShader(id);
				throw new ArgumentException(String.Format("The GLSL shader of type {0} failed to compile. The info log is:\n{1}", type, log), "source");
			}

			return new glShader(id, type, source);
		}
		public static glShader fromSource(ShaderType type, string[] source) {
			if (source == null) {
				throw new ArgumentNullException("source");
			}
			return fromSource(type, String.Concat(source));
		}

		public static implicit operator int(glShader shader) {
			return shader.mId;
		}

	}

}
