
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace arookas {
	
	public class bloContext {

		bloRenderFlags mRenderFlags;
		glShader mFragmentShader, mVertexShader;
		glProgram mProgram;
		Dictionary<string, int> mUniformCache;

		public bloContext() {
			mUniformCache = new Dictionary<string, int>(100);
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
				mUniformCache.Clear();
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

		int getUniformLocation(string name) {
			int location;
			if (!mUniformCache.TryGetValue(name, out location)) {
				location = GL.GetUniformLocation(mProgram, name);
				mUniformCache[name] = location;
			}
			return location;
		}

		public void setProgramInt(string name, int integer) {
			if (mProgram == null) {
				return;
			}
			GL.Uniform1(getUniformLocation(name), integer);
		}

		public void setProgramColor(string name, bloColor color) {
			if (mProgram == null) {
				return;
			}
			GL.Uniform4(getUniformLocation(name), color);
		}
		public void setProgramColor(string name, Color4 color) {
			if (mProgram == null) {
				return;
			}
			GL.Uniform4(getUniformLocation(name), color);
		}

		public void setProgramVector(string name, bloPoint vector) {
			if (mProgram == null) {
				return;
			}
			GL.Uniform2(getUniformLocation(name), vector.x, vector.y);
		}
		public void setProgramVector(string name, Vector2 vector) {
			if (mProgram == null) {
				return;
			}
			GL.Uniform2(getUniformLocation(name), vector);
		}
		public void setProgramVector(string name, Vector2d vector) {
			setProgramVector(name, (Vector2)vector);
		}
		public void setProgramVector(string name, Vector3 vector) {
			if (mProgram == null) {
				return;
			}
			GL.Uniform3(getUniformLocation(name), vector);
		}
		public void setProgramVector(string name, Vector3d vector) {
			setProgramVector(name, (Vector3)vector);
		}
		public void setProgramVector(string name, Vector4 vector) {
			if (mProgram == null) {
				return;
			}
			GL.Uniform4(getUniformLocation(name), vector);
		}
		public void setProgramVector(string name, Vector4d vector) {
			setProgramVector(name, (Vector4)vector);
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
