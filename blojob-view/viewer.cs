
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.IO;

namespace arookas {

	class bloViewer : GameWindow {

		bloScreen mScreen;
		bloContext mContext;
		glShader mFragmentShader, mVertexShader;
		bool mShowPanes;
		bool mShowAll;

		public bloViewer(bloScreen screen) {
			var rectangle = screen.getRectangle();
			initScreen(screen, rectangle.width, rectangle.height);
			initContext();
		}
		public bloViewer(bloScreen screen, int width, int height) {
			initScreen(screen, width, height);
			initContext();
		}

		void initScreen(bloScreen screen, int width, int height) {
			mScreen = screen;
			setSize(width, height);
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
			GL.Viewport(((ClientRectangle.Width - viewport.width) / 2), ((ClientRectangle.Height - viewport.height) / 2), viewport.width, viewport.height);
			GL.Ortho(viewport.left, viewport.right, (viewport.bottom + 0.5d), viewport.top, -1.0d, 1.0d);
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
		public void setTitle(string title) {
			if (String.IsNullOrEmpty(title)) {
				Title = String.Format("pablo v{0}", blojob.getVersion());
			} else {
				Title = String.Format("pablo v{0} - {1}", blojob.getVersion(), title);
			}
		}
		public void setSize(int width, int height) {
			ClientRectangle = new Rectangle(0, 0, width, height);
			WindowBorder = WindowBorder.Fixed;
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
