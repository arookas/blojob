
using System;

namespace arookas {

	static class bloMath {

		public static bloColor scaleAlpha(bloColor color, int alpha) {
			return new bloColor(color.r, color.g, color.b, ((color.a * alpha) / 256));
		}

		public static void swap<T>(ref T a, ref T b) {
			var olda = a;
			var oldb = b;
			a = oldb;
			b = olda;
		}
		public static int round(double x) {
			return (int)(x + 0.5d);
		}
		public static T clamp<T>(T x, T min, T max)
			where T : IComparable {
			if (x.CompareTo(min) < 0) {
				return min;
			} else if (x.CompareTo(max) > 0) {
				return max;
			}
			return x;
		}

		public static bool chase(ref int from, int to, int step) {
			int distance = (to - from);
			if (step < 0) {
				step = -step;
			}
			if (distance > 0) {
				distance -= step;
				from = (distance > 0 ? (to - distance) : to);
			} else {
				distance += step;
				from = (distance < 0 ? (to - distance) : to);
			}
			if (from == to) {
				return false;
			}
			return true;
		}
		public static bool chase(ref float from, float to, float step) {
			float distance = (to - from);
			if (step < 0.0f) {
				step = -step;
			}
			if (distance > 0.0f) {
				distance -= step;
				from = (distance > 0.0f ? (to - distance) : to);
			} else {
				distance += step;
				from = (distance < 0.0f ? (to - distance) : to);
			}
			if (from == to) {
				return false;
			}
			return true;
		}
		public static bool chase(ref double from, double to, double step) {
			double distance = (to - from);
			if (step < 0.0d) {
				step = -step;
			}
			if (distance > 0.0d) {
				distance -= step;
				from = (distance > 0.0d ? (to - distance) : to);
			} else {
				distance += step;
				from = (distance < 0.0d ? (to - distance) : to);
			}
			if (from == to) {
				return false;
			}
			return true;
		}

	}

}
