
using System;

namespace arookas {

	public enum bloFormat {
		Compact,
		Blo1,
	}

	public enum bloAnchor {
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
	public enum bloBinding {
		Bottom = (1 << 0),
		Top = (1 << 1),
		Right = (1 << 2),
		Left = (1 << 3),
	}

	public enum bloTextboxHBinding {
		Center,
		Right,
		Left,
	}

	public enum bloTextboxVBinding {
		Center,
		Bottom,
		Top,
	}

	[Flags]
	public enum bloMirror {
		Y = (1 << 0),
		X = (1 << 1),
	}

	public enum bloWrapMode {
		None,
		Clamp,
		Repeat,
		Mirror,
	}

	public enum gxTextureFilter {
		Near,
		Linear,
		NearMipNear,
		LinearMipNear,
		NearMipLinear,
		LinearMipLinear,
	}

	public enum gxTextureFormat {
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

	public enum gxTlutFormat {
		IA8,
		RGB565,
		RGB5A3,
	}

	public enum gxCullMode {
		None,
		Front,
		Back,
		All,
	}

	public enum gxWrapMode {
		Clamp,
		Repeat,
		Mirror,
	}

	public enum gxAnisotropy {
		Aniso1,
		Aniso2,
		Aniso4,
	}

	[Flags]
	public enum bloRenderFlags {
		ShowInvisible = (1 << 0),
		PaneWireframe = (1 << 1),
		PictureWireframe = (1 << 2),
	}

	public static class bloEnum {

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
