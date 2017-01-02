
namespace arookas {

	public class bloExPane : bloPaneInterpolator {

		protected bloCoord2D mOffset, mSize;
		protected double mAlphaCurrent, mAlphaStep, mAlphaGoal;
		protected bool mOffsetActive, mSizeActive, mAlphaActive;

		public bloExPane(bloPane pane)
			: base(pane) {
			initialize();
		}
		public bloExPane(bloScreen screen, uint name)
			: base(screen, name) {
			initialize();
		}

		void initialize() {
			mAlphaCurrent = 255.0d;
			mAlphaStep = 0.0d;
			mAlphaGoal = 0.0d;

			mOffset = new bloCoord2D();
			mSize = new bloCoord2D();

			mOffsetActive = false;
			mSizeActive = false;
			mAlphaActive = false;
		}

		public override bool update() {
			if (mOffsetActive) {
				if (mOffset.update()) {
					mOffsetActive = false;
				}
				var value = mOffset.getValue();
				mPane.move(
					(mRect.left + bloCoord2D.round(value.X)),
					(mRect.top + bloCoord2D.round(value.Y))
				);
			}
			if (mSizeActive) {
				if (mSize.update()) {
					mSizeActive = false;
				}
				var value = mSize.getValue();
				mPane.resize(bloCoord2D.round(value.X), bloCoord2D.round(value.Y));
			}
			if (mAlphaActive) {
				if ((mAlphaStep < 0.0d && mAlphaCurrent <= mAlphaGoal) || (mAlphaStep >= 0.0d && mAlphaCurrent >= mAlphaGoal)) {
					mAlphaActive = false;
				}
				mAlphaCurrent += mAlphaStep;
				mPane.setAlpha((byte)bloMath.clamp((int)mAlphaCurrent, 0, 255));
			}
			return (!mOffsetActive && !mSizeActive && !mAlphaActive);
		}

		public void setPaneOffset(int steps, int xTo, int yTo, int xFrom, int yFrom) {
			mOffset.setValue(steps, xTo, yTo, xFrom, yFrom);
			mPane.move((mRect.left + xFrom), (mRect.top + yFrom));
			mOffsetActive = true;
		}
		public void setPaneSize(int steps, int wTo, int hTo, int wFrom, int hFrom) {
			mSize.setValue(steps, wTo, hTo, wFrom, hFrom);
			mPane.resize(wFrom, hFrom);
			mSizeActive = true;
		}
		public void setPaneAlpha(int steps, int to, int from) {
			var alpha = (byte)bloMath.clamp(from, 0, 255);
			mPane.setAlpha(alpha);
			mAlphaCurrent = from;
			mAlphaStep = ((double)(to - from) / steps);
			mAlphaGoal = to;
			mAlphaActive = true;
		}

	}

}
