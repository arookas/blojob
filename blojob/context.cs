
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace arookas {
	
	public class bloContext {

		bloRenderFlags mRenderFlags;
		glShader mFragmentShader, mVertexShader;
		glProgram mProgram;

		public bloContext() {
			// empty
		}

		public bloRenderFlags getRenderFlags() {
			return mRenderFlags;
		}
		public bloRenderFlags setRenderFlags(bloRenderFlags flags) {
			bloRenderFlags old = mRenderFlags;
			mRenderFlags = flags;
			return old;
		}
		public bool hasRenderFlags(bloRenderFlags flags) {
			return ((mRenderFlags & flags) != 0);
		}

		public bool setShaders(glShader fragment, glShader vertex) {
			if (fragment == null || vertex == null) {
				return false;
			}
			if (mProgram != null) {
				mProgram.unuse();
				mProgram.Dispose();
				mProgram = null;
			}
			mFragmentShader = fragment;
			mVertexShader = vertex;
			try {
				mProgram = glProgram.create();
				mProgram.attach(fragment);
				mProgram.attach(vertex);
				mProgram.link();
			} catch {
				return false;
			}
			return true;
		}

		public void useProgram() {
			if (mProgram != null) {
				mProgram.use();
			}
		}
		public void unuseProgram() {
			if (mProgram != null) {
				mProgram.unuse();
			}
		}

		public void setProgramInt(string name, int integer) {
			if (mProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(mProgram, name);
			GL.Uniform1(location, integer);
		}

		public void setProgramColor(string name, bloColor color) {
			if (mProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(mProgram, name);
			GL.Uniform4(location, color);
		}
		public void setProgramColor(string name, Color4 color) {
			if (mProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(mProgram, name);
			GL.Uniform4(location, color);
		}

		public void setProgramVector(string name, bloPoint vector) {
			if (mProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(mProgram, name);
			GL.Uniform2(location, vector.x, vector.y);
		}
		public void setProgramVector(string name, Vector2 vector) {
			if (mProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(mProgram, name);
			GL.Uniform2(location, vector);
		}
		public void setProgramVector(string name, Vector2d vector) {
			if (mProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(mProgram, name);
			GL.Uniform2(location, vector.X, vector.Y);
		}
		public void setProgramVector(string name, Vector3 vector) {
			if (mProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(mProgram, name);
			GL.Uniform3(location, vector);
		}
		public void setProgramVector(string name, Vector3d vector) {
			if (mProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(mProgram, name);
			GL.Uniform3(location, vector.X, vector.Y, vector.Z);
		}
		public void setProgramVector(string name, Vector4 vector) {
			if (mProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(mProgram, name);
			GL.Uniform4(location, vector);
		}
		public void setProgramVector(string name, Vector4d vector) {
			if (mProgram == null) {
				return;
			}
			var location = GL.GetUniformLocation(mProgram, name);
			GL.Uniform4(location, vector.X, vector.Y, vector.Z, vector.W);
		}

		static bloContext sInstance;

		public static bloContext getContext() {
			return sInstance;
		}
		public static bloContext setContext(bloContext context) {
			bloContext old = sInstance;
			sInstance = context;
			return old;
		}

	}

}
