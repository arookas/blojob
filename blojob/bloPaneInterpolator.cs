
namespace arookas {

	abstract class bloPaneInterpolator {

		protected bloPane mPane;
		protected bloRectangle mRect;
		
		protected bloPaneInterpolator(bloPane pane) {
			initialize(pane);
		}
		protected bloPaneInterpolator(bloScreen screen, uint name) {
			initialize(screen.search(name));
		}

		void initialize(bloPane pane) {
			mPane = pane;
			resetRectangle();
		}

		public abstract bool update();

		public bloPane getPane() {
			return mPane;
		}
		public TPane getPane<TPane>()
			where TPane : bloPane {
			return (mPane as TPane);
		}

		public bloRectangle getRectangle() {
			return mRect;
		}
		public bloRectangle setRectangle(bloRectangle rectangle) {
			var old = mRect;
			mRect = rectangle;
			return old;
		}
		public void resetRectangle() {
			mRect = mPane.getRectangle();
		}

	}

}
