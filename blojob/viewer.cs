
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
		glShader mGradientFrag, mGradientVert;
		glProgram mGradientProgram;
		bool mShowPanes;
		bool mShowAll;

		public bloViewer(string input, bloFormat format) {
			mInput = input;
			mFormat = format;
			Title = String.Format("blojob v{0} - {1}", blojob.sVersion, Path.GetFileName(mInput));
			initScreen();
			initSize();
			initShader();
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
		void initShader() {
			mGradientVert = glShader.fromFile(ShaderType.VertexShader, "shader/gradient-map.vp");
			mGradientFrag = glShader.fromFile(ShaderType.FragmentShader, "shader/gradient-map.fp");
			mGradientProgram = glProgram.create();
			mGradientProgram.attach(mGradientVert);
			mGradientProgram.attach(mGradientFrag);
			mGradientProgram.link();
			gl.setProgram(mGradientProgram);
		}

		void initGL() {
			bloRenderFlags flags = 0;
			if (mShowPanes) {
				flags |= bloRenderFlags.PaneWireframe;
			}
			if (mShowAll) {
				flags |= bloRenderFlags.ShowInvisible;
			}
			gl.setRenderFlags(flags);
			gl.setProgram(mGradientProgram);
		}

		public bloScreen getScreen() {
			return mScreen;
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
			gl.clearBuffer(Color4.CornflowerBlue);
			gl.initPolyMode();
			gl.initLine();
			gl.initBlend();
			gl.initOrtho(mScreen.getRectangle());
			initGL();
			mScreen.draw();
			SwapBuffers();
		}

	}

}
