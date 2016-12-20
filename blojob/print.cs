
using OpenTK;
using System;

namespace arookas {

	public class bloPrint {

		bloFont mFont;
		PrintState mState, mWorkState;
		bloPoint mOrigin;
		Vector2d mCursor;
		double mCursorWidth;
		bloColor mFromColor, mToColor;

		public bloPrint(bloFont font) {
			var white = new bloColor(bloColor.cWhite);
			initialize(font, 0, Int32.MinValue, white, white);
		}
		public bloPrint(bloFont font, int spacing) {
			var white = new bloColor(bloColor.cWhite);
			initialize(font, spacing, Int32.MinValue, white, white);
		}
		public bloPrint(bloFont font, int spacing, int leading) {
			var white = new bloColor(bloColor.cWhite);
			initialize(font, spacing, leading, white, white);
		}
		public bloPrint(bloFont font, bloColor topColor, bloColor bottomColor) {
			initialize(font, 0, Int32.MinValue, topColor, bottomColor);
		}
		public bloPrint(bloFont font, int spacing, bloColor topColor, bloColor bottomColor) {
			initialize(font, spacing, Int32.MinValue, topColor, bottomColor);
		}
		public bloPrint(bloFont font, int spacing, int leading, bloColor topColor, bloColor bottomColor) {
			initialize(font, spacing, leading, topColor, bottomColor);
		}

		void initialize(bloFont font, int spacing, int leading, bloColor topColor, bloColor bottomColor) {
			mFont = font;
			mState.spacing = spacing;
			if (mFont != null) {
				mState.leading = (leading != Int32.MinValue ? leading : mFont.getLeading());
			} else {
				mState.leading = 32;
			}
			mState.gradientMode = true;
			mState.topColor = topColor;
			mState.bottomColor = bottomColor;
			mFromColor = new bloColor(bloColor.cZero);
			mToColor = new bloColor(bloColor.cOne);
			if (mFont != null) {
				mState.tabWidth = (short)(mFont.getWidth() * 4);
			} else {
				mState.tabWidth = 80;
			}
			if (mFont != null) {
				setFontSize();
				mFont.setGL(mFromColor, mToColor);
			} else {
				mState.fontWidth = Int32.MinValue;
				mState.fontHeight = Int32.MinValue;
			}
			locate(0, 0);
			initchar();
		}

		void initchar() {
			mWorkState = mState;
		}

		double parse(ushort[] buffer, int width, int[] lines, out Vector2d size, int alpha, bool draw) {
			size = new Vector2d();
			
			if (mFont == null) {
				return 0.0d;
			}

			int line = 0;
			Vector2d cursor = mCursor;
			double lineWidth = 0.0d;
			double fHeight = 0.0d;
			double fLeft = mCursor.X;
			double fRight = mCursor.X;
			double fTop = mCursor.Y;
			double fBottom = mCursor.Y;

			bloColor topColor = mWorkState.topColor;
			bloColor bottomColor = mWorkState.bottomColor;
			topColor.a = ((topColor.a * alpha) / 256);
			bottomColor.a = ((bottomColor.a * alpha) / 256);
			mFont.setGradColor(topColor, (mWorkState.gradientMode ? bottomColor : topColor));

			bool monospace = false; // r19
			bool advanceCursor = false; // r17
			double cursorStartX; // f18
			
			int stringPtr = 0;

			for (;;) {

				// 802C6930
				int character = buffer[stringPtr++]; // r20

				// 802C6970
				if (character == '\0' || stringPtr > buffer.Length) {
					if (!draw && lines != null) {
						lines[line] = bloMath.round(lineWidth);
					}
					++line;
					break;
				}

				// 802C69BC
				cursorStartX = mCursor.X;
				advanceCursor = true;

				if (character < 32) {
					if (character == 27) {
						// 802C69D4
						ushort escapeCode = doEscapeCode(buffer, ref stringPtr, alpha);
						if (escapeCode == cHM) {
							if (!draw && lines != null) {
								lines[line] = bloMath.round(lineWidth);
							}
							++line;
							mCursor.X = cursor.X;
							if (line == cMaxLines) {
								break; // -> 802C6F28
							}
							monospace = false;
							lineWidth = 0.0d;
						}
						if (escapeCode != 0) {
							advanceCursor = false;
						}
						// -> 802C6DEC
					} else {
						// 802C6A48
						doCtrlCode(character);
						advanceCursor = false;
						if (character == '\n') {
							if (!draw && lines != null) {
								lines[line] = bloMath.round(lineWidth);
							}
							++line;
							if (line == cMaxLines) {
								break; // -> 802C6F28
							}
							lineWidth = 0.0d;
							monospace = false;
							// -> 802C6DEC
						} else {
							switch (character) {
								case '\b':
								case '\t': {
									monospace = true;
									break;
								}
								case '\r': {
									monospace = false;
									break;
								}
							}
							// -> 802C6DEC
						}
					}
				} else {
					// 802C6AC8
					// NOTE: the multi-byte EOS check was removed because it will never occur
					// since as we pre-parse the bytes in the string to characters for the ushort[]
					// 802C6B0C
					if (mFont.getMonoFlag()) {
						mCursorWidth = mFont.getMonoWidth();
					} else {
						bloFont.WidthEntry entry;
						mFont.getWidthEntry(character, out entry);
						mCursorWidth = (monospace ? entry.width : (entry.width + entry.kerning));
					}
					mCursorWidth *= ((double)mWorkState.fontWidth / (double)mFont.getWidth());
					double totalWidth = ((mCursor.X + mCursorWidth) - mOrigin.x); // ((int)(cScalar * ((mCursor.X + mCursorWidth) - mOrigin.x)) / cScalar);
					if (totalWidth > width && mCursor.X > cursor.X) {
						// 802C6C5C
						--stringPtr; // since we optimized out the multibyte checks, -- is fine
						mCursor.Y += mWorkState.leading;
						if (!draw && lines != null) {
							lines[line] = bloMath.round(lineWidth);
						}
						++line;
						if (line == cMaxLines) {
							break; // -> 802C6F28
						}
						advanceCursor = false;
						monospace = false;
						mCursor.X = mOrigin.x;
						lineWidth = 0.0d;
						// -> 802C6DEC
					} else {
						// 802C6D04
						if (draw) {
							mFont.drawChar(
								(mCursor.X + (lines != null ? lines[line] : 0)),
								mCursor.Y,
								mWorkState.fontWidth,
								mWorkState.fontHeight,
								character, monospace
							);
						}
						monospace = true;
						mCursor.X += mCursorWidth;
						// -> 802C6DEC
					}
				}
				// 802C6DEC
				if (advanceCursor) {
					if ((mCursor.X - cursor.X) > lineWidth) {
						lineWidth = (mCursor.X - cursor.X);
					}
					mCursor.X += mWorkState.spacing;
					mCursorWidth += mWorkState.spacing;
					double height = (mCursor.Y + (mFont.getDescent() * ((double)mWorkState.fontHeight / (double)mFont.getHeight())));
					if (fHeight < height) {
						fHeight = height;
					}
					if (mCursor.X > fRight) {
						fRight = mCursor.X;
					}
					if (mCursor.X < fLeft) {
						fLeft = mCursor.X;
					}
					if (cursorStartX < fLeft) {
						fLeft = cursorStartX;
					}
					if (mCursor.Y > fBottom) {
						fBottom = mCursor.Y;
					}
					if (mCursor.Y < fTop) {
						fTop = mCursor.Y;
					}
				}
			}

			// 802C6F28
			if (lines != null) {
				lines[line] = -1;
			}

			size.X = (fRight - fLeft);
			size.Y = ((fBottom - fTop) + mFont.getLeading());

			if (!draw) {
				mCursor = cursor;
			}

			return fHeight;
		}
		ushort doEscapeCode(ushort[] buffer, ref int stringPtr, int alpha) {
			ushort escapeCode = 0;
			int saveStringPtr = stringPtr;

			for (int i = 0; i < 2; ++i) {
				var character = buffer[stringPtr++];
				if (character >= 128 || character < 32) {
					stringPtr = saveStringPtr;
					return 0;
				}
				escapeCode <<= 8;
				escapeCode |= character;
			}

			bloColor topColor = mWorkState.topColor;
			bloColor bottomColor = mWorkState.bottomColor;

			switch (escapeCode) {
				case cHM: break;
				case cCU: mCursor.Y -= getNumber(buffer, ref stringPtr, 1, 0); break;
				case cCD: mCursor.Y += getNumber(buffer, ref stringPtr, 1, 0); break;
				case cCL: mCursor.X -= getNumber(buffer, ref stringPtr, 1, 0); break;
				case cCR: mCursor.X += getNumber(buffer, ref stringPtr, 1, 0); break;
				case cLU: mCursor.Y -= mWorkState.leading; break;
				case cLD: mCursor.Y += mWorkState.leading; break;
				case cST: {
					int number = getNumber(buffer, ref stringPtr, mWorkState.tabWidth, mWorkState.tabWidth);
					if (number >= 0) {
						mWorkState.tabWidth = number;
					}
					break;
				}
				case cCC: {
					mWorkState.topColor = new bloColor(getNumber(buffer, ref stringPtr, mState.topColor.rgba, mWorkState.topColor.rgba));
					topColor = mWorkState.topColor;
					topColor.a = ((topColor.a * alpha) / 256);
					bottomColor.a = ((bottomColor.a * alpha) / 256);
					mFont.setGradColor(topColor, (mWorkState.gradientMode ? bottomColor : topColor));
					break;
				}
				case cGC: {
					mWorkState.bottomColor = new bloColor(getNumber(buffer, ref stringPtr, mState.bottomColor.rgba, mWorkState.bottomColor.rgba));
					bottomColor = mWorkState.topColor;
					topColor.a = ((topColor.a * alpha) / 256);
					bottomColor.a = ((bottomColor.a * alpha) / 256);
					mFont.setGradColor(topColor, (mWorkState.gradientMode ? bottomColor : topColor));
					break;
				}
				case cFX: {
					int fontWidth = getNumber(buffer, ref stringPtr, mState.fontWidth, mWorkState.fontWidth);
					if (fontWidth >= 0) {
						mWorkState.fontWidth = fontWidth;
					}
					break;
				}
				case cFY: {
					int fontHeight = getNumber(buffer, ref stringPtr, mState.fontHeight, mWorkState.fontHeight);
					if (fontHeight >= 0) {
						mWorkState.fontHeight = fontHeight;
					}
					break;
				}
				case cSH: {
					mWorkState.spacing = getNumber(buffer, ref stringPtr, mState.spacing, mWorkState.spacing);
					break;
				}
				case cSV: {
					mWorkState.leading = getNumber(buffer, ref stringPtr, mState.leading, mWorkState.leading);
					break;
				}
				case cGM: {
					mWorkState.gradientMode = (getNumber(buffer, ref stringPtr, (mWorkState.gradientMode ? 0 : 1), (mWorkState.gradientMode ? 1 : 0)) != 0);
					topColor.a = ((topColor.a * alpha) / 256);
					bottomColor.a = ((bottomColor.a * alpha) / 256);
					mFont.setGradColor(topColor, (mWorkState.gradientMode ? bottomColor : topColor));
					break;
				}
				default: {
					stringPtr = saveStringPtr;
					escapeCode = 0;
					break;
				}
			}
			return escapeCode;
		}
		void doCtrlCode(int ctrlCode) {
			switch (ctrlCode) {
				case '\b': {
					mCursor.X -= mCursorWidth;
					mCursorWidth = 0.0d;
					break;
				}
				case '\t': {
					if (mWorkState.tabWidth > 0) {
						var oldCursorX = mCursor.X;
						mCursor.X = (((int)mCursor.X / mWorkState.tabWidth * mWorkState.tabWidth) + mWorkState.tabWidth);
						mCursorWidth = (mCursor.X - oldCursorX);
					}
					break;
				}
				case '\n': {
					mCursor.Y += mWorkState.leading;
					goto case '\r';
				}
				case '\r': {
					mCursorWidth = 0.0d;
					mCursor.X = mOrigin.x;
					break;
				}
				case 28: {
					mCursor.X += 1.0d;
					break;
				}
				case 29: {
					mCursor.Y -= 1.0d;
					break;
				}
				case 30: {
					mCursor.Y -= 1.0d;
					break;
				}
				case 31: {
					mCursor.Y += 1.0d;
					break;
				}
			}
		}

		public void initialize() {
			if (mFont != null) {
				if (mState.fontWidth == Int32.MinValue && mState.fontHeight == Int32.MinValue) {
					setFontSize();
				}
				mFont.setGL(mFromColor, mToColor);
			}
		}

		public void setFontSize() {
			if (mFont != null) {
				mState.fontWidth = mFont.getWidth();
				mState.fontHeight = mFont.getHeight();
			}
		}
		public void setFontSize(int width, int height) {
			mState.fontWidth = width;
			mState.fontHeight = height;
		}
		public void setGradColor(bloColor fromColor, bloColor toColor) {
			mFromColor = fromColor;
			mToColor = toColor;
		}

		public void locate(int x, int y) {
			mOrigin = new bloPoint(x, y);
			mCursor = new Vector2d(x, y);
			mCursorWidth = 0.0d;
		}

		public void print(int x, int y, ushort[] buffer) {
			print(x, y, 255, buffer);
		}
		public void print(int x, int y, int alpha, ushort[] buffer) {
			locate(x, y);
			initchar();
			Vector2d size;
			parse(buffer, Int32.MaxValue, null, out size, alpha, true);
		}

		public void printReturn(ushort[] buffer, int width, int height, bloTextboxHBinding hbind, bloTextboxVBinding vbind, int x, int y, int alpha) {
			if (mFont == null) {
				return;
			}
			initchar();
			mOrigin = new bloPoint((int)mCursor.X, (int)mCursor.Y);
			int[] lines = new int[cMaxLines + 1];
			Vector2d size;
			double totalHeight = (parse(buffer, width, lines, out size, alpha, false) + mState.fontHeight);
			switch (vbind) {
				case bloTextboxVBinding.Top: break;
				case bloTextboxVBinding.Bottom: y += (bloMath.round(totalHeight) - height); break;
				case bloTextboxVBinding.Center: y += ((bloMath.round(totalHeight) - height) / 2); break;
			}
			int i = 0;
			while (lines[i] != -1) {
				switch (hbind) {
					case bloTextboxHBinding.Left: {
						lines[i] = 0;
						break;
					}
					case bloTextboxHBinding.Right: {
						lines[i] = (width - lines[i]);
						break;
					}
					case bloTextboxHBinding.Center: {
						lines[i] = ((width - lines[i]) / 2);
						break;
					}
				}
				++i;
			}
			initchar();
			mCursor += new Vector2d(x, (y + mState.fontHeight));
			mOrigin = new bloPoint((int)mCursor.X, (int)mCursor.Y);
			parse(buffer, width, lines, out size, alpha, true);
		}

		public void getSize(out Vector2d size, ushort[] buffer) {
			parse(buffer, Int32.MaxValue, null, out size, 255, false);
		}
		public double getWidth(ushort[] buffer) {
			Vector2d size;
			getSize(out size, buffer);
			return size.X;
		}
		public double getHeight(ushort[] buffer) {
			Vector2d size;
			getSize(out size, buffer);
			return size.Y;
		}
		
		static int getNumber(ushort[] buffer, ref int stringPtr, int missingNumber, int invalidNumber) {
			var saveStringPtr = stringPtr;

			if (buffer[stringPtr] != '[') {
				return missingNumber;
			}

			++stringPtr;

			var number = 0;
			var negative = false;

			while (Char.IsWhiteSpace((char)buffer[stringPtr])) {
				++stringPtr;
			}

			if (buffer[stringPtr] == '+') {
				++stringPtr;
			} else if (buffer[stringPtr] == '-') {
				++stringPtr;
				negative = true;
			}

			var digits = 0;
			while (buffer[stringPtr] >= '0' && buffer[stringPtr] <= '9') {
				number *= 10;
				number += (buffer[stringPtr] - '0');
				++stringPtr;
				++digits;
			}

			if (buffer[stringPtr] != ']') {
				stringPtr = saveStringPtr;
				return invalidNumber;
			}

			++stringPtr;

			if (digits == 0) {
				number = missingNumber;
			} else if (negative) {
				number = -number;
			}

			return number;
		}
		static uint getNumber(ushort[] buffer, ref int stringPtr, uint missingNumber, uint invalidNumber) {
			var saveStringPtr = stringPtr;

			if (buffer[stringPtr] != '[') {
				return missingNumber;
			}

			++stringPtr;

			// SMS uses (outStringPtr - endPtr) for the digit count, so the leading spaces,
			// optional sign and hexadecimal prefix characters are included
			var number = 0u;
			var digits = 0;
			var negative = false;

			while (Char.IsWhiteSpace((char)buffer[stringPtr])) {
				++stringPtr;
				++digits;
			}

			if (buffer[stringPtr] == '+') {
				++stringPtr;
				++digits;
			} else if (buffer[stringPtr] == '-') {
				++stringPtr;
				++digits;
				negative = true;
			}

			if (buffer[stringPtr] == '0' && (buffer[stringPtr + 1] == 'x' || buffer[stringPtr + 1] == 'X')) {
				stringPtr += 2;
				digits += 2;
			}

			while ((buffer[stringPtr] >= '0' && buffer[stringPtr] <= '9') ||
				(buffer[stringPtr] >= 'A' && buffer[stringPtr] <= 'F') ||
				(buffer[stringPtr] >= 'a' && buffer[stringPtr] <= 'f')) {
				number *= 16;
				if (buffer[stringPtr] >= 'a') {
					number += (uint)(10 + (buffer[stringPtr] - 'a'));
				} else if (buffer[stringPtr] >= 'A') {
					number += (uint)(10 + (buffer[stringPtr] - 'A'));
				} else {
					number += (uint)(buffer[stringPtr] - '0');
				}
				++stringPtr;
				++digits;
			}

			switch (digits) {
				case 8: break;
				case 6: {
					number <<= 8;
					number |= 255;
					break;
				}
				default: {
					stringPtr = saveStringPtr;
					return invalidNumber;
				}
			}

			if (buffer[stringPtr] != ']') {
				stringPtr = saveStringPtr;
				return invalidNumber;
			}

			++stringPtr;

			if (digits == 0) {
				number = missingNumber;
			} else if (negative) {
				number = (uint)-number;
			}

			return number;
		}

		const int cMaxLines = 256;
		const double cScalar = 10000.0d;

		const ushort cHM = 0x484D;
		const ushort cCU = 0x4355;
		const ushort cCD = 0x4344;
		const ushort cCL = 0x434C;
		const ushort cCR = 0x4352;
		const ushort cLU = 0x4C55;
		const ushort cLD = 0x4C44;
		const ushort cST = 0x5354;
		const ushort cCC = 0x4343;
		const ushort cGC = 0x4743;
		const ushort cFX = 0x4658;
		const ushort cFY = 0x4659;
		const ushort cSH = 0x5348;
		const ushort cSV = 0x5356;
		const ushort cGM = 0x474D;

		struct PrintState {

			public bool gradientMode;
			public bloColor topColor;
			public bloColor bottomColor;

			public int fontWidth;
			public int fontHeight;
			
			public int tabWidth;
			
			public int leading;
			public int spacing;

		}
	}

}
