
using arookas.IO.Binary;
using arookas.Xml;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace arookas {

	public class bloPane : IEnumerable<bloPane> {

		protected bloPane mParent;
		protected List<bloPane> mChildren;
		protected uint mName;
		protected bool mVisible;
		protected bloRectangle mRect;
		protected gxCullMode mCullMode;
		protected bloAnchor mAnchor;
		protected double mAngle;
		protected byte mAlpha;
		protected bool mInheritAlpha;
		protected bool mConnectParent;

		protected byte mCumulativeAlpha;

		internal bloPane() {
			mChildren = new List<bloPane>(10);
			mVisible = true;
			mAlpha = 255;
			mInheritAlpha = true;
		}
		public bloPane(uint name, bloRectangle rectangle) {
			mName = name;
			mRect = rectangle;
		}
		public bloPane(bloPane parentPane, uint name, bool visible, bloRectangle rectangle) {
			mParent = parentPane;
			if (mParent != null) {
				mParent.mChildren.Add(this);
			}
			mName = name;
			mVisible = visible;
			mRect = rectangle;
		}

		internal void load(bloPane parentPane, object source, bloFormat format) {
			mParent = parentPane;
			if (mParent != null) {
				mParent.mChildren.Add(this);
			}
			mCullMode = gxCullMode.None;
			mConnectParent = false;
			switch (format) {
				case bloFormat.Compact: loadCompact(source as aBinaryReader); break;
				case bloFormat.Blo1: loadBlo1(source as aBinaryReader); break;
				case bloFormat.Xml: loadXml(source as xElement); break;
				default: throw new NotImplementedException("Format is not implemented.");
			}
		}

		protected virtual void loadCompact(aBinaryReader reader) {
			if (reader == null) {
				throw new ArgumentNullException("reader");
			}
			mVisible = (reader.Read8() != 0);
			reader.Step(1);

			mName = reader.Read32();

			int left = reader.ReadS16();
			int top = reader.ReadS16();
			int width = reader.ReadS16();
			int height = reader.ReadS16();
			mRect.set(left, top, (left + width), (top + height));

			setBasePosition(bloAnchor.TopLeft);
			mAngle = 0.0d;
			mAlpha = 255;
			mInheritAlpha = true;
		}
		protected virtual void loadBlo1(aBinaryReader reader) {
			if (reader == null) {
				throw new ArgumentNullException("reader");
			}

			int numparams = reader.Read8();
			mVisible = (reader.Read8() != 0);
			reader.Step(2);

			mName = reader.Read32();

			int left = reader.ReadS16();
			int top = reader.ReadS16();
			int width = reader.ReadS16();
			int height = reader.ReadS16();
			mRect.set(left, top, (left + width), (top + height));

			numparams -= 6;

			if (numparams > 0) {
				mAngle = reader.Read16();
				--numparams;
			} else {
				mAngle = 0.0d;
			}

			if (numparams > 0) {
				mAnchor = (bloAnchor)reader.Read8();
				--numparams;
			} else {
				mAnchor = bloAnchor.TopLeft;
			}

			if (numparams > 0) {
				mAlpha = reader.Read8();
				--numparams;
			} else {
				mAlpha = 255;
			}

			if (numparams > 0) {
				mInheritAlpha = (reader.Read8() != 0);
				--numparams;
			} else {
				mInheritAlpha = true;
			}

			reader.Skip(4);
		}
		protected virtual void loadXml(xElement element) {
			if (element == null) {
				throw new ArgumentNullException("element");
			}

			mName = convertStringToName(element.Attribute("id") | "");
			setConnectParent(element.Attribute("connect") | false);
			mVisible = (element.Attribute("visible") | true);
			mRect = bloXml.loadRectangle(element.Element("rectangle"));
			mAngle = ((element.Element("angle") | 0) % 360);
			if (!Enum.TryParse<bloAnchor>(element.Element("anchor"), true, out mAnchor)) {
				mAnchor = bloAnchor.TopLeft;
			}
			if (!Enum.TryParse<gxCullMode>(element.Element("cull-mode"), true, out mCullMode)) {
				mCullMode = gxCullMode.None;
			}
			mAlpha = (byte)bloMath.clamp((element.Element("alpha") | 255), 0, 255);
			mInheritAlpha = (element.Element("alpha").Attribute("inherit") | true);
		}

		internal virtual void saveCompact(aBinaryWriter writer) {
			if (writer == null) {
				throw new ArgumentNullException("writer");
			}

			writer.Write8((byte)(mVisible ? 1 : 0));
			writer.Write8(0); // padding
			writer.Write32(mName);
			writer.WriteS16((short)mRect.left);
			writer.WriteS16((short)mRect.top);
			writer.WriteS16((short)mRect.width);
			writer.WriteS16((short)mRect.height);
		}
		internal virtual void saveBlo1(aBinaryWriter writer) {
			if (writer == null) {
				throw new ArgumentNullException("writer");
			}

			byte numparams;

			if (!mInheritAlpha) {
				numparams = 10;
			} else if (mAlpha < 255) {
				numparams = 9;
			} else if (mAnchor != bloAnchor.TopLeft) {
				numparams = 8;
			} else if (mAngle != 0.0d) {
				numparams = 7;
			} else {
				numparams = 6;
			}

			writer.Write8(numparams);
			writer.Write8((byte)(mVisible ? 1 : 0));
			writer.Step(2);
			writer.Write32(mName);
			writer.WriteS16((short)mRect.left);
			writer.WriteS16((short)mRect.top);
			writer.WriteS16((short)mRect.width);
			writer.WriteS16((short)mRect.height);

			numparams -= 6;

			if (numparams > 0) {
				writer.Write16((ushort)mAngle);
				--numparams;
			}

			if (numparams > 0) {
				writer.Write8((byte)mAnchor);
				--numparams;
			}

			if (numparams > 0) {
				writer.Write8(mAlpha);
				--numparams;
			}

			if (numparams > 0) {
				writer.Write8((byte)(mInheritAlpha ? 1 : 0));
				--numparams;
			}

			writer.WritePadding(4, 0);
		}
		internal virtual void saveXml(XmlWriter writer) {
			if (writer == null) {
				throw new ArgumentNullException("writer");
			}

			if (mName != 0u) {
				writer.WriteAttributeString("id", convertNameToString(mName));
			}
			
			if (mConnectParent) {
				writer.WriteAttributeString("connect", mConnectParent.ToString());
			}

			if (!mVisible) {
				writer.WriteAttributeString("visible", mVisible.ToString());
			}

			bloXml.saveRectangle(writer, mRect, "rectangle");

			if (mAngle != 0.0d) {
				writer.WriteElementString("angle", ((ushort)mAngle).ToString());
			}

			if (mAnchor != bloAnchor.TopLeft) {
				writer.WriteElementString("anchor", mAnchor.ToString());
			}

			if (mCullMode != gxCullMode.None) {
				writer.WriteElementString("cull-mode", mCullMode.ToString());
			}

			if (mAlpha != 255 || !mInheritAlpha) {
				writer.WriteStartElement("alpha");
				writer.WriteAttributeString("inherit", mInheritAlpha.ToString());
				writer.WriteValue(mAlpha);
				writer.WriteEndElement();
			}
		}
		
		public void move(bloPoint point) {
			move(point.x, point.y);
		}
		public virtual void move(int x, int y) {
			mRect.move(x, y);
		}
		public void add(bloPoint point) {
			add(point.x, point.y);
		}
		public virtual void add(int x, int y) {
			mRect.add(x, y);
		}
		public virtual void resize(int width, int height) {
			mRect.resize(width, height);
		}
		public virtual void reform(int left, int top, int right, int bottom) {
			mRect.reform(left, top, right, bottom);
		}

		public virtual bloPane search(uint name) {
			if (mName == name) {
				return this;
			}
			foreach (var child in mChildren) {
				var found = child.search(name);
				if (found != null) {
					return found;
				}
			}
			return null;
		}
		public TPane search<TPane>(uint name)
			where TPane : bloPane {
			return (search(name) as TPane);
		}

		Vector2d getBasePosition() {
			var basePosition = new Vector2d();
			switch ((int)mAnchor % 3) {
				case 0: basePosition.X = 0; break;
				case 1: basePosition.X = ((mRect.width) / 2); break;
				case 2: basePosition.X = (mRect.width); break;
			}
			switch ((int)mAnchor / 3) {
				case 0: basePosition.Y = 0; break;
				case 1: basePosition.Y = ((mRect.height) / 2); break;
				case 2: basePosition.Y = (mRect.height); break;
			}
			return basePosition;
		}

		void setMatrix() {
			GL.Translate(mRect.left, mRect.top, 0.0d);
			if (mAngle != 0.0d) {
				var basePosition = getBasePosition();
				GL.Translate(basePosition.X, basePosition.Y, 0.0d);
				GL.Rotate(-mAngle, Vector3d.UnitZ);
				GL.Translate(-basePosition.X, -basePosition.Y, 0.0d);
			}
		}
		void setAlpha() {
			mCumulativeAlpha = mAlpha;
			if (mParent != null && mInheritAlpha) {
				mCumulativeAlpha = (byte)((mAlpha * mParent.mCumulativeAlpha) / 256);
			}
		}

		public void loadGL() {
			loadGLSelf();
			foreach (var child in mChildren) {
				child.loadGL();
			}
		}
		protected virtual void loadGLSelf() {
			// empty
		}

		public void draw() {
			var context = bloContext.getContext();

			if ((!mVisible && !context.hasRenderFlags(bloRenderFlags.ShowInvisible)) || mRect.isEmpty()) {
				return;
			}

			GL.PushMatrix();
			setMatrix();
			setAlpha();
			gl.setCullMode(gxCullMode.None);
			drawSelf();

			foreach (var child in mChildren) {
				child.draw();
			}

			GL.PopMatrix();
		}
		protected virtual void drawSelf() {
			var context = bloContext.getContext();

			if (!context.hasRenderFlags(bloRenderFlags.PaneWireframe)) {
				return;
			}

			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

			GL.Begin(PrimitiveType.Quads);
			GL.Color4(Color4.White);
			GL.Vertex3(mRect.left, mRect.top, 0.0d);
			GL.Color4(Color4.White);
			GL.Vertex3(mRect.right, mRect.top, 0.0d);
			GL.Color4(Color4.White);
			GL.Vertex3(mRect.right, mRect.bottom, 0.0d);
			GL.Color4(Color4.White);
			GL.Vertex3(mRect.left, mRect.bottom, 0.0d);
			GL.End();

			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
		}

		public bloPane getParentPane() {
			return mParent;
		}
		public int getChildPane() {
			return mChildren.Count;
		}
		public bloPane getChildPane(int index) {
			return mChildren[index];
		}
		public TPane getChildPane<TPane>(int index) where TPane : bloPane {
			return (mChildren[index] as TPane);
		}

		public uint getName() {
			return mName;
		}
		public bool getVisible() {
			return mVisible;
		}
		public bloRectangle getRectangle() {
			return mRect;
		}
		public gxCullMode getCullMode() {
			return mCullMode;
		}
		public bloAnchor getAnchor() {
			return mAnchor;
		}
		public double getAngle() {
			return mAngle;
		}
		public byte getAlpha() {
			return mAlpha;
		}
		public bool getInheritAlpha() {
			return mInheritAlpha;
		}
		public bool getConnectParent() {
			return mConnectParent;
		}

		public uint setName(uint name) {
			uint old = mName;
			mName = name;
			return old;
		}
		public bool setVisible(bool visible) {
			bool old = mVisible;
			mVisible = visible;
			return old;
		}
		public bloRectangle setRectangle(bloRectangle rectangle) {
			bloRectangle old = mRect;
			mRect = rectangle;
			return old;
		}
		public void setCullMode(gxCullMode cull) {
			mCullMode = cull;
			foreach (var child in mChildren) {
				child.setCullMode(cull);
			}
		}
		public void setBasePosition(bloAnchor anchor) {
			mAnchor = anchor;
		}
		public double setAngle(double angle) {
			double old = mAngle;
			mAngle = angle;
			return old;
		}
		public byte setAlpha(byte alpha) {
			byte old = mAlpha;
			mAlpha = alpha;
			return old;
		}
		public bool setInheritAlpha(bool set) {
			bool old = mInheritAlpha;
			mInheritAlpha = set;
			return old;
		}
		public virtual bool setConnectParent(bool set) {
			mConnectParent = false;
			return false;
		}

		public IEnumerator<bloPane> GetEnumerator() {
			return mChildren.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public static uint convertStringToName(string str) {
			var name = 0u;
			for (int i = 0; i < str.Length; ++i) {
				name <<= 8;
				name |= (uint)(str[i] & 255);
			}
			return name;
		}
		public static string convertNameToString(uint name) {
			var builder = new StringBuilder(4);
			while ((name & 255) != 0) {
				builder.Insert(0, (char)(name & 255));
				name >>= 8;
			}
			return builder.ToString();
		}

	}

}
