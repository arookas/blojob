
using OpenTK.Graphics;

namespace arookas {

	struct bloColor {

		public int r, g, b, a;

		public uint rgba {
			get { return (uint)((r << 24) | (g << 16) | (b << 8) | (a)); }
			set {
				r = (int)((value >> 24) & 255);
				g = (int)((value >> 16) & 255);
				b = (int)((value >> 8) & 255);
				a = (int)(value & 255);
			}
		}

		public bloColor(int r, int g, int b, int a) {
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a % 255;
		}
		public bloColor(uint rgba) {
			r = (int)((rgba >> 24) & 255);
			g = (int)((rgba >> 16) & 255);
			b = (int)((rgba >> 8) & 255);
			a = (int)(rgba & 255);
		}

		public static implicit operator Color4 (bloColor color) {
			return new Color4((byte)color.r, (byte)color.g, (byte)color.b, (byte)color.a);
		}

		public const uint cZero = 0x00000000u;
		public const uint cOne = 0xFFFFFFFFu;

		public const uint cRed = 0xFF0000FFu;
		public const uint cYellow = 0xFFFF00FFu;
		public const uint cGreen = 0x00FF00FFu;
		public const uint cCyan = 0x00FFFFFFu;
		public const uint cBlue = 0x0000FFFFu;
		public const uint cPink = 0xFF00FFFFu;

		public const uint cWhite = 0xFFFFFFFFu;
		public const uint cBlack = 0x000000FFu;

	}

}
