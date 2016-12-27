
namespace arookas {

	public class bloBoundPane {

		protected bloPane mPane;
		protected bloRectangle mRect;
		protected bloPoint mPosition, mSize;
		protected bloPoint mPositionTop, mPositionMid, mPositionBot;
		protected bloPoint mSizeTop, mSizeMid, mSizeBot;
		protected double mPositionTime, mPositionStep;
		protected double mSizeTime, mSizeStep;
		protected bool mPositionActive, mSizeActive;

		public bloBoundPane(bloPane pane) {
			initialize(pane);
		}
		public bloBoundPane(bloScreen screen, uint name) {
			initialize(screen.search(name));
		}

		void initialize(bloPane pane) {
			mPane = pane;
			mRect = pane.getRectangle();

			mPositionTime = 0.0d;
			mPositionStep = 0.0d;
			mPositionActive = false;

			mSizeTime = 0.0d;
			mSizeStep = 0.0d;
			mSizeActive = false;
		}

		public bloPane getPane() {
			return mPane;
		}

		public virtual bool update() {
			if (mPositionActive) {
				if (mPositionTime > 1.0d) {
					mPositionTime = 1.0d;
					mPositionActive = false;
				}
				makeNewPosition(mPositionTime, mPositionTop, mPositionMid, mPositionBot, out mPosition);
				mPane.move((mRect.left + mPosition.x), (mRect.top + mPosition.y));
				mPositionTime += mPositionStep;
			}
			if (mSizeActive) {
				if (mSizeTime > 1.0d) {
					mSizeTime = 1.0d;
					mSizeActive = false;
				}
				makeNewPosition(mSizeTime, mSizeTop, mSizeMid, mSizeBot, out mSize);
				mPane.resize((mRect.width + mSize.x), (mRect.height + mSize.y));
				mSizeTime += mSizeStep;
			}
			if (mPositionActive || mSizeActive) {
				return false;
			}
			return true;
		}

		public void setPanePosition(int steps, bloPoint top, bloPoint mid, bloPoint bot) {
			mPositionTime = 0.0d;
			mPositionStep = (1.0d / steps);
			mPositionTop = top;
			mPositionMid = mid;
			mPositionBot = bot;
			mPositionActive = true;
		}
		public void setPaneSize(int steps, bloPoint top, bloPoint mid, bloPoint bot) {
			mSizeTime = 0.0d;
			mSizeStep = (1.0d / steps);
			mSizeTop = top;
			mSizeMid = mid;
			mSizeBot = bot;
			mSizeActive = true;
		}

		static void makeNewPosition(double time, bloPoint top, bloPoint mid, bloPoint bot, out bloPoint result) {
			double botFactor = (time * time);
			double midFactor = (2.0d * (1.0d - time) * time);
			double topFactor = ((1.0d - time) * (1.0d - time));
			double x = ((top.x * topFactor) + (mid.x * midFactor) + (bot.x * botFactor));
			double y = ((top.y * topFactor) + (mid.y * midFactor) + (bot.y * botFactor));
			result = new bloPoint(bloCoord2D.round(x), bloCoord2D.round(y));
		}

	}

}
