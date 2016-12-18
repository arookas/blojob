
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

	}

}
