
using OpenTK;

namespace arookas {
	
	public class bloCoord2D {

		Vector2d mCurrent, mGoal, mStep;

		public bool update() {
			bool result = false;
			bloMath.chase(ref mCurrent.X, mGoal.X, mStep.X);
			bloMath.chase(ref mCurrent.Y, mGoal.Y, mStep.Y);
			if (mCurrent == mGoal) {
				mStep = Vector2d.Zero;
				result = true;
			}
			return result;
		}

		public Vector2d getValue() {
			return mCurrent;
		}

		public void setValue(int steps, double xTo, double yTo, double xFrom, double yFrom) {
			setValue(steps, new Vector2d(xTo, yTo), new Vector2d(xFrom, yFrom));
		}
		public void setValue(int steps, Vector2d to, Vector2d from) {
			mGoal = to;
			mCurrent = from;
			if (steps > 0) {
				mStep = ((to - from) / steps);
			} else {
				mStep = Vector2d.Zero;
			}
		}

		public static int round(float x) {
			return (int)(x + (x > 0.0f ? 0.5f : -0.5f));
		}
		public static int round(double x) {
			return (int)(x + (x > 0.0d ? 0.5d : -0.5d));
		}
		public static int round(decimal x) {
			return (int)(x + (x > 0.0m ? 0.5m : -0.5m));
		}

	}

}
