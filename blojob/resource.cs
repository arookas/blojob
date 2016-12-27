
using arookas.IO.Binary;
using arookas.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace arookas {

	public abstract class bloResource {

		protected bloResourceType mResourceType;
		protected string mResourcePath, mResourceAbsPath;

		protected bloResource() {
			// empty
		}

		public abstract void load(Stream stream);

		public void save(aBinaryWriter writer) {
			writer.Write8((byte)mResourceType);
			writer.Write8((byte)mResourcePath.Length);
			writer.WriteString(mResourcePath);
		}
		public void save(string name, XmlWriter writer) {
			writer.WriteStartElement(name);
			writer.WriteAttributeString("scope", mResourceType.ToString());
			writer.WriteValue(mResourcePath);
			writer.WriteEndElement();
		}

		public bloResourceType getResourceType() {
			return mResourceType;
		}
		public string getResourcePath() {
			return mResourcePath;
		}
		public string getResourceAbsPath() {
			return mResourceAbsPath;
		}

		public bloResourceType setResourceType(bloResourceType type) {
			bloResourceType old = mResourceType;
			mResourceType = type;
			return old;
		}
		public string setResourcePath(string path) {
			string old = mResourcePath;
			mResourcePath = path;
			return old;
		}
		public string setResourceAbsPath(string path) {
			string old = mResourceAbsPath;
			mResourceAbsPath = path;
			return old;
		}

		public static void save(bloResource resource, aBinaryWriter writer) {
			if (resource != null) {
				resource.save(writer);
			} else {
				writer.Write8(0);
				writer.Write8(0);
			}
		}
		public static void save(bloResource resource, string name, XmlWriter writer) {
			if (resource != null) {
				resource.save(name, writer);
			}
		}

	}

	public enum bloResourceType {
		None, // null reference
		Unknown, // not used by SMS
		LocalDirectory, // resource is in the specified directory of the specified archive
		LocalArchive, // resource is in specified archive
		Global, // resource may be found in any loaded global resource
	}

	public class bloResourceFinder : IDisposable {

		string mLocalPath;
		List<string> mGlobalPaths;
		Dictionary<string, bloResource> mCache;

		public bloResourceFinder(string localPath) {
			mLocalPath = Path.GetFullPath(localPath);
			mGlobalPaths = new List<string>(10);
			mCache = new Dictionary<string, bloResource>(100, new EqualityComparer());
		}
		public bloResourceFinder(bloResourceFinder finder) {
			if (finder == null) {
				throw new ArgumentNullException("finder");
			}
			mLocalPath = finder.mLocalPath;
			mGlobalPaths = new List<string>(finder.mGlobalPaths.Capacity);
			mGlobalPaths.AddRange(finder.mGlobalPaths);
			mCache = new Dictionary<string, bloResource>(100, new EqualityComparer());
		}

		public string setLocalPath(string localPath) {
			string old = mLocalPath;
			mLocalPath = Path.GetFullPath(localPath);
			return old;
		}

		public void addGlobalPath(string globalPath) {
			mGlobalPaths.Add(Path.GetFullPath(globalPath));
		}
		public void clearGlobalPaths() {
			mGlobalPaths.Clear();
		}

		public void clearCache() {
			mCache.Clear();
		}

		public T find<T>(aBinaryReader reader, string directory)
			where T : bloResource, new() {
			bloResourceType type;
			return find<T>(reader, directory, out type);
		}
		public T find<T>(aBinaryReader reader, string directory, out bloResourceType type)
			where T : bloResource, new() {
			type = (bloResourceType)reader.Read8();
			int length = reader.Read8();
			string name = reader.ReadString(length);
			T resource = find<T>(type, name, directory);
			if (resource == null && type != bloResourceType.None) {
				Console.WriteLine(">>> FAILED: could not find {0} resource '{1}'", type, name);
			}
			return resource;
		}

		public T find<T>(xElement element, string directory)
			where T : bloResource, new() {
			bloResourceType type;
			return find<T>(element, directory, out type);
		}
		public T find<T>(xElement element, string directory, out bloResourceType type)
			where T : bloResource, new() {
			type = bloResourceType.None;
			if (element == null) {
				return null;
			}
			var attr = element.Attribute("scope");
			if (attr == null || !Enum.TryParse<bloResourceType>(attr, true, out type)) {
				type = bloResourceType.LocalDirectory;
			}
			string name = element.Value;
			T resource = find<T>(type, name, directory);
			if (resource == null && type != bloResourceType.None) {
				Console.WriteLine(">>> FAILED: could not find {0} resource '{1}'", type, name);
			}
			return resource;
		}

		public T find<T>(bloResourceType type, string name, string directory)
			where T : bloResource, new() {
			string path = null;
			switch (type) {
				case bloResourceType.LocalDirectory: {
					path = Path.Combine(mLocalPath, directory, name);
					break;
				}
				case bloResourceType.LocalArchive: {
					path = Path.Combine(mLocalPath, name);
					break;
				}
				case bloResourceType.Global: {
					foreach (string globalPath in mGlobalPaths) {
						path = Path.Combine(globalPath, name);
						if (!File.Exists(path)) {
							path = null;
							continue;
						}
						break;
					}
					break;
				}
			}
			T resource = null;
			if (path != null && File.Exists(path)) {
				bloResource cached;
				if (mCache.TryGetValue(path, out cached)) {
					return (cached as T);
				}
				resource = new T();
				resource.setResourceType(type);
				resource.setResourcePath(name);
				resource.setResourceAbsPath(path);
				using (Stream stream = File.OpenRead(path)) {
					resource.load(stream);
				}
			}
			if (path != null && resource != null) {
				mCache[path] = resource;
			}
			return resource;
		}

		public void Dispose() {
			clearCache();
		}

		static bloResourceFinder sInstance;

		public static bloResourceFinder getFinder() {
			return sInstance;
		}
		public static bloResourceFinder setFinder(bloResourceFinder finder) {
			bloResourceFinder old = sInstance;
			sInstance = finder;
			return old;
		}

		class EqualityComparer : IEqualityComparer<string> {

			public bool Equals(string x, string y) {
				if (x == null || y == null) {
					return (x ?? "") == (y ?? "");
				}
				return x.Equals(y, StringComparison.InvariantCultureIgnoreCase);
			}
			public int GetHashCode(string obj) {
				return obj.GetHashCode();
			}

		}

	}

}
