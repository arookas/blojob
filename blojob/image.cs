
using System.IO;
using arookas.Collections;
using arookas.IO.Binary;
using System;

namespace arookas {

	static class bloImage {

		static int[] sNybbleToByte;

		static bloImage() {
			sNybbleToByte = aCollection.Initialize(16, (i) => (i * 0x11));
		}

		// these return an instance of type aRGBA[] if the format is a non-paletted texture format;
		// for indexed texture formats, these functions return a short[] containing the palette-index data
		public static object loadImageData(byte[] data, Endianness endianness, int width, int height, gxTextureFormat format) {
			return loadImageData(data, 0, endianness, width, height, format);
		}
		public static object loadImageData(byte[] data, long index, Endianness endianness, int width, int height, gxTextureFormat format) {
			using (var stream = new MemoryStream(data)) {
				stream.Seek(index, SeekOrigin.Begin);
				return loadImageData(stream, endianness, width, height, format);
			}
		}
		public static object loadImageData(Stream stream, Endianness endianness, int width, int height, gxTextureFormat format) {
			var reader = new aBinaryReader(stream, endianness);
			return loadImageData(reader, width, height, format);
		}
		public static object loadImageData(aBinaryReader reader, int width, int height, gxTextureFormat format) {
			switch (format) {
				case gxTextureFormat.I4: return loadI4(reader, width, height);
				case gxTextureFormat.I8: return loadI8(reader, width, height);
				case gxTextureFormat.IA4: return loadIA4(reader, width, height);
				case gxTextureFormat.IA8: return loadIA8(reader, width, height);
				case gxTextureFormat.RGB565: return loadRGB565(reader, width, height);
				case gxTextureFormat.RGB5A3: return loadRGB5A3(reader, width, height);
				case gxTextureFormat.RGBA8: return loadRGBA8(reader, width, height);
				case gxTextureFormat.CI4: return loadCI4(reader, width, height);
				case gxTextureFormat.CI8: return loadCI8(reader, width, height);
				case gxTextureFormat.CI14X2: return loadCI14X2(reader, width, height);
				case gxTextureFormat.CMPR: return loadCMPR(reader, width, height);
				default: {
					throw new NotImplementedException(String.Format("Encountered an unimplemented texture format {0}.", format));
				}
			}
		}

		static aRGBA[] loadI4(aBinaryReader reader, int width, int height) {
			var data = new aRGBA[width * height];
			const int cBlockWidth = 8, cBlockHeight = 8;
			for (var y = 0; y < height; y += cBlockHeight) {
				for (var x = 0; x < width; x += cBlockWidth) {
					for (var by = 0; by < cBlockHeight; ++by) {
						for (var bx = 0; bx < cBlockWidth; bx += 2) {
							var i4 = reader.Read8();
							var index = (width * (y + by) + (x + bx));
							if ((by + y) >= height) {
								continue;
							}
							if ((bx + x) < width) {
								data[index] = new aRGBA(sNybbleToByte[(i4 >> 4) & 0xF]);
							}
							if ((bx + x + 1) < width) {
								data[index + 1] = new aRGBA(sNybbleToByte[i4 & 0xF]);
							}
						}
					}
				}
			}
			return data;
		}
		static aRGBA[] loadI8(aBinaryReader reader, int width, int height) {
			var data = new aRGBA[width * height];
			const int cBlockWidth = 8, cBlockHeight = 4;
			for (var y = 0; y < height; y += cBlockHeight) {
				for (var x = 0; x < width; x += cBlockWidth) {
					for (var by = 0; by < cBlockHeight; ++by) {
						for (var bx = 0; bx < cBlockWidth; ++bx) {
							var i8 = reader.Read8();
							if ((bx + x) >= width || (by + y) >= height) {
								continue;
							}
							data[width * (y + by) + (x + bx)] = new aRGBA(i8);
						}
					}
				}
			}
			return data;
		}
		static aRGBA[] loadIA4(aBinaryReader reader, int width, int height) {
			var data = new aRGBA[width * height];
			const int cBlockWidth = 8, cBlockHeight = 4;
			for (var y = 0; y < height; y += cBlockHeight) {
				for (var x = 0; x < width; x += cBlockWidth) {
					for (var by = 0; by < cBlockHeight; ++by) {
						for (var bx = 0; bx < cBlockWidth; ++bx) {
							var ia4 = reader.Read8();
							if ((bx + x) >= width || (by + y) >= height) {
								continue;
							}
							data[width * (y + by) + (x + bx)] = new aRGBA(sNybbleToByte[ia4 & 0xF], sNybbleToByte[(ia4 >> 4) & 0xF]);
						}
					}
				}
			}
			return data;
		}
		static aRGBA[] loadIA8(aBinaryReader reader, int width, int height) {
			var data = new aRGBA[width * height];
			const int cBlockWidth = 4, cBlockHeight = 4;
			for (var y = 0; y < height; y += cBlockHeight) {
				for (var x = 0; x < width; x += cBlockWidth) {
					for (var by = 0; by < cBlockHeight; ++by) {
						for (var bx = 0; bx < cBlockWidth; ++bx) {
							var intensity = reader.Read8();
							var alpha = reader.Read8();
							if ((bx + x) >= width || (by + y) >= height) {
								continue;
							}
							data[width * (y + by) + (x + bx)] = new aRGBA(intensity, alpha);
						}
					}
				}
			}
			return data;
		}
		static aRGBA[] loadRGB565(aBinaryReader reader, int width, int height) {
			var data = new aRGBA[width * height];
			const int cBlockWidth = 4, cBlockHeight = 4;
			for (var y = 0; y < height; y += cBlockHeight) {
				for (var x = 0; x < width; x += cBlockWidth) {
					for (var by = 0; by < cBlockHeight; ++by) {
						for (var bx = 0; bx < cBlockWidth; ++bx) {
							var color = reader.Read16();
							if ((bx + x) >= width || (by + y) >= height) {
								continue;
							}
							data[width * (y + by) + (x + bx)] = aRGBA.FromRGB565(color);
						}
					}
				}
			}
			return data;
		}
		static aRGBA[] loadRGB5A3(aBinaryReader reader, int width, int height) {
			var data = new aRGBA[width * height];
			const int cBlockWidth = 4, cBlockHeight = 4;
			for (var y = 0; y < height; y += cBlockHeight) {
				for (var x = 0; x < width; x += cBlockWidth) {
					for (var by = 0; by < cBlockHeight; ++by) {
						for (var bx = 0; bx < cBlockWidth; ++bx) {
							var color = reader.Read16();
							if ((bx + x) >= width || (by + y) >= height) {
								continue;
							}
							data[(width * (y + by)) + (x + bx)] = (
								(color & 0x8000) != 0 ?
								aRGBA.FromRGB5(color) :
								aRGBA.FromRGB4A3(color)
							);
						}
					}
				}
			}
			return data;
		}
		static aRGBA[] loadRGBA8(aBinaryReader reader, int width, int height) {
			var data = new aRGBA[width * height];
			const int cBlockWidth = 4, cBlockHeight = 4;
			var colors = new uint[16];
			for (var y = 0; y < height; y += cBlockHeight) {
				for (var x = 0; x < width; x += cBlockWidth) {
					for (var by = 0; by < cBlockHeight && (y + by) < height; ++by) { // AR
						for (var bx = 0; bx < cBlockWidth && (x + bx) < width; ++bx) {
							colors[(cBlockWidth * by) + bx] = (uint)(reader.Read16() << 16);
						}
					}
					for (var by = 0; by < cBlockHeight && (y + by) < height; ++by) { // GB
						for (var bx = 0; bx < cBlockWidth && (x + bx) < width; ++bx) {
							colors[(cBlockWidth * by) + bx] |= reader.Read16();
						}
					}
					for (var by = 0; by < cBlockHeight; ++by) {
						for (var bx = 0; bx < cBlockWidth; ++bx) {
							if ((x + bx) >= width || (y + by) >= height) {
								continue;
							}
							data[width * (y + by) + (x + bx)] = aRGBA.FromARGB8(colors[(cBlockWidth * by) + bx]);
						}
					}
				}
			}
			return data;
		}
		static short[] loadCI4(aBinaryReader reader, int width, int height) {
			var data = new short[width * height];
			const int cBlockWidth = 8, cBlockHeight = 8;
			for (var y = 0; y < height; y += cBlockHeight) {
				for (var x = 0; x < width; x += cBlockWidth) {
					for (var by = 0; by < cBlockHeight; ++by) {
						for (var bx = 0; bx < cBlockWidth; bx += 2) {
							var ci4 = reader.Read8();
							var index = (width * (y + by) + (x + bx));
							if ((by + y) >= height) {
								continue;
							}
							if ((bx + x) < width) {
								data[index] = (short)((ci4 >> 4) & 0xF);
							}
							if ((bx + x + 1) < width) {
								data[index + 1] = (short)((ci4 >> 0) & 0xF);
							}
						}
					}
				}
			}
			return data;
		}
		static short[] loadCI8(aBinaryReader reader, int width, int height) {
			var data = new short[width * height];
			const int cBlockWidth = 8, cBlockHeight = 4;
			for (var y = 0; y < height; y += cBlockHeight) {
				for (var x = 0; x < width; x += cBlockWidth) {
					for (var by = 0; by < cBlockHeight; ++by) {
						for (var bx = 0; bx < cBlockWidth; ++bx) {
							var ci8 = reader.Read8();
							if ((bx + x) >= width || (by + y) >= height) {
								continue;
							}
							data[width * (y + by) + (x + bx)] = ci8;
						}
					}
				}
			}
			return data;
		}
		static short[] loadCI14X2(aBinaryReader reader, int width, int height) {
			var data = new short[width * height];
			const int cBlockWidth = 4, cBlockHeight = 4;
			for (var y = 0; y < height; y += cBlockHeight) {
				for (var x = 0; x < width; x += cBlockWidth) {
					for (var by = 0; by < cBlockHeight; ++by) {
						for (var bx = 0; bx < cBlockWidth; ++bx) {
							var ci14x2 = reader.Read16();
							if ((bx + x) >= width || (by + y) >= height) {
								continue;
							}
							data[width * (y + by) + (x + bx)] = (short)(ci14x2 & 0x3FFF);
						}
					}
				}
			}
			return data;
		}
		static aRGBA[] loadCMPR(aBinaryReader reader, int width, int height) {
			var data = new aRGBA[width * height];
			const int cTileWidth = 8, cTileHeight = 8;
			const int cBlockWidth = 4, cBlockHeight = 4;
			for (var y = 0; y < height; y += cTileHeight) { // tile
				for (var x = 0; x < width; x += cTileWidth) {
					for (var by = 0; by < cTileHeight; by += cBlockHeight) { // block
						for (var bx = 0; bx < cTileWidth; bx += cBlockWidth) {
							var colors = aRGBA.FromST3C1(reader.Read64());
							for (var ty = 0; ty < cBlockHeight && y + by + ty < height; ty++) { // texel
								for (var tx = 0; tx < cBlockWidth && x + bx + tx < width; tx++) {
									data[width * (y + by + ty) + (x + bx + tx)] = colors[(ty * cBlockWidth) + tx];
								}
							}
						}
					}
				}
			}
			return data;
		}

	}

}
