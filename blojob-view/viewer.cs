
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;

namespace arookas {

	class bloViewer : GameWindow {

		string mInput;
		bloFormat mFormat;
		bloScreen mScreen;
		bloContext mContext;
		glShader mFragmentShader, mVertexShader;
		bool mShowPanes;
		bool mShowAll;

		public bloViewer(string input, bloFormat format) {
			mInput = input;
			mFormat = format;
			Title = String.Format("blojob-view v{0} - {1}", blojob.getVersion(), Path.GetFileName(mInput));
			initScreen();
			initSize();
			initContext();
		}

		void initScreen() {
			bloScreen screen;
			using (Stream stream = File.OpenRead(mInput)) {
				screen = bloScreen.loadBlo1(stream);
			}
			if (screen != null) {
				mScreen = screen;
			}
		}
		void initSize() {
			bloRectangle rectangle = mScreen.getRectangle();
			rectangle.normalize();
			ClientRectangle = rectangle;
			WindowBorder = WindowBorder.Fixed;
		}
		void initContext() {
			mContext = new bloContext();
			mVertexShader = glShader.fromFile(ShaderType.VertexShader, "shader/gradient-map.vp");
			mFragmentShader = glShader.fromFile(ShaderType.FragmentShader, "shader/gradient-map.fp");
			mContext.setShaders(mFragmentShader, mVertexShader);
			bloContext.setContext(mContext);
		}

		void clearBuffer(Color4 color) {
			GL.ClearColor(color);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}
		void initLine() {
			GL.Enable(EnableCap.LineSmooth);
			GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
			GL.LineWidth(1.5f);
		}
		void initBlend() {
			GL.Enable(EnableCap.Blend);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
		}
		void initPolyMode() {
			GL.FrontFace(FrontFaceDirection.Cw);
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
		}
		void initOrtho(bloRectangle viewport) {
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Viewport(viewport.left, viewport.top, (viewport.right - viewport.left), (viewport.bottom - viewport.top));
			GL.Ortho(viewport.left, viewport.right, viewport.bottom, viewport.top, -1.0d, 1.0d);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
		}
		void initRenderFlags() {
			bloRenderFlags flags = 0;
			if (mShowPanes) {
				flags |= bloRenderFlags.PaneWireframe;
			}
			if (mShowAll) {
				flags |= bloRenderFlags.ShowInvisible;
			}
			mContext.setRenderFlags(flags);
		}

		public void run() {
			Run(30.0d, 30.0d);
		}

		protected override void OnKeyPress(KeyPressEventArgs e) {
			switch (e.KeyChar) {
				case 'p': mShowPanes = !mShowPanes; break;
				case 'v': mShowAll = !mShowAll; break;
			}
		}
		protected override void OnLoad(EventArgs e) {
			mScreen.loadGL();
		}
		protected override void OnRenderFrame(FrameEventArgs e) {
			clearBuffer(Color4.CornflowerBlue);
			initPolyMode();
			initLine();
			initBlend();
			initOrtho(mScreen.getRectangle());
			initRenderFlags();
			mScreen.draw();
			SwapBuffers();
		}

	}

}
