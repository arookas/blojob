
using System.Drawing;
using OpenTK;

namespace arookas {

	public struct bloRectangle {

		public int left;
		public int top;
		public int right;
		public int bottom;

		public int width {
			get { return (right - left); }
		}
		public int height {
			get { return (bottom - top); }
		}

		public Vector2d topleft {
			get { return new Vector2d(left, top); }
		}
		public Vector2d topright {
			get { return new Vector2d(right, top); }
		}
		public Vector2d bottomleft {
			get { return new Vector2d(left, bottom); }
		}
		public Vector2d bottomright {
			get { return new Vector2d(right, bottom); }
		}

		public bloRectangle(int left, int top, int right, int bottom) {
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}

		public void set(int left, int top, int right, int bottom) {
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}
		public void copy(bloRectangle other) {
			left = other.left;
			top = other.top;
			right = other.right;
			bottom = other.bottom;
		}
		public void add(bloPoint point) {
			add(point.x, point.y);
		}
		public void add(int x, int y) {
			left += x;
			right += x;
			top += y;
			bottom += y;
		}
		public bool intersect(bloRectangle other) {
			if (left < other.left) {
				left = other.left;
			}
			if (top < other.top) {
				top = other.top;
			}
			if (right > other.right) {
				right = other.right;
			}
			if (bottom > other.bottom) {
				bottom = other.bottom;
			}
			return !isEmpty();
		}
		public void move(bloPoint point) {
			move(point.x, point.y);
		}
		public void move(int x, int y) {
			int width = this.width;
			int height = this.height;
			left = x;
			top = y;
			resize(width, height);
		}
		public void resize(int width, int height) {
			right = (left + width);
			bottom = (top + height);
		}
		public void reform(int left, int top, int right, int bottom) {
			this.left += left;
			this.top += top;
			this.right += right;
			this.bottom += bottom;
		}
		public void normalize() {
			int a, b;
			a = left;
			b = right;
			if (a > b) {
				left = b;
				right = a;
			}
			a = top;
			b = bottom;
			if (a > b) {
				top = b;
				bottom = a;
			}
		}
		public bool isEmpty() {
			bool empty = true;
			if (left < right && top < bottom) {
				empty = false;
			}
			return empty;
		}

		public static implicit operator Rectangle(bloRectangle rectangle) {
			return new Rectangle(rectangle.left, rectangle.top, rectangle.width, rectangle.height);
		}

	}

}
