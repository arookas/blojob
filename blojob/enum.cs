
using System;

namespace arookas {

	enum bloFormat {
		Compact,
		Blo1,
	}

	enum bloAnchor {
		TopLeft,
		TopMiddle,
		TopRight,

		CenterLeft,
		CenterMiddle,
		CenterRight,

		BottomLeft,
		BottomMiddle,
		BottomRight,
	}

	[Flags]
	enum bloBinding {
		Bottom = (1 << 0),
		Top = (1 << 1),
		Right = (1 << 2),
		Left = (1 << 3),
	}

	enum bloTextboxHBinding {
		Center,
		Right,
		Left,
	}

	enum bloTextboxVBinding {
		Center,
		Bottom,
		Top,
	}

	[Flags]
	enum bloMirror {
		Y = (1 << 0),
		X = (1 << 1),
	}

	enum bloWrapMode {
		None,
		Clamp,
		Repeat,
		Mirror,
	}

	enum gxTextureFilter {
		Near,
		Linear,
		NearMipNear,
		LinearMipNear,
		NearMipLinear,
		LinearMipLinear,
	}

	enum gxTextureFormat {
		I4 = 0,
		I8 = 1,
		IA4 = 2,
		IA8 = 3,
		RGB565 = 4,
		RGB5A3 = 5,
		RGBA8 = 6,
		CI4 = 8,
		CI8 = 9,
		CI14X2 = 10,
		CMPR = 14,
	}

	enum gxTlutFormat {
		IA8,
		RGB565,
		RGB5A3,
	}

	enum gxCullMode {
		None,
		Front,
		Back,
		All,
	}

	enum gxWrapMode {
		Clamp,
		Repeat,
		Mirror,
	}

	enum gxAnisotropy {
		Aniso1,
		Aniso2,
		Aniso4,
	}

	[Flags]
	enum bloRenderFlags {
		ShowInvisible = (1 << 0),
		PaneWireframe = (1 << 1),
		PictureWireframe = (1 << 2),
	}

	static class bloEnum {

		public static bool hasFlag(this bloBinding value, bloBinding flag) {
			return ((value & flag) == flag);
		}
		public static bool hasFlag(this bloMirror value, bloMirror flag) {
			return ((value & flag) == flag);
		}
		public static bool hasFlag(this bloTextboxHBinding value, bloTextboxHBinding flag) {
			return ((value & flag) == flag);
		}
		public static bool hasFlag(this bloTextboxVBinding value, bloTextboxVBinding flag) {
			return ((value & flag) == flag);
		}
		public static bool hasFlag(this bloRenderFlags value, bloRenderFlags flag) {
			return ((value & flag) == flag);
		}

	}

}
