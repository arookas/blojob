
using arookas.IO.Binary;
using System;
using System.Collections.Generic;
using System.IO;

namespace arookas {

	abstract class bloResource {

		protected bloResourceType mResourceType;
		protected string mResourcePath, mResourceAbsPath;

		public abstract void load(Stream stream);

		public void save(aBinaryWriter writer) {
			writer.Write8((byte)mResourceType);
			writer.Write8((byte)mResourcePath.Length);
			writer.WriteString(mResourcePath);
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

	}
	
	enum bloResourceType {
		None, // null reference
		Unknown, // not used by SMS
		LocalDirectory, // resource is in the specified directory of the specified archive
		LocalArchive, // resource is in specified archive
		Global, // resource may be found in any loaded global resource
	}

	class bloResourceFinder {

		string mLocalPath;
		List<string> mGlobalPaths;

		public bloResourceFinder(string localPath) {
			mLocalPath = Path.GetFullPath(localPath);
			mGlobalPaths = new List<string>(10);
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

		public T find<T>(aBinaryReader reader, string directory)
			where T : bloResource, new() {

			long start = reader.Position;
			bloResourceType type = (bloResourceType)reader.Read8();
			int length = reader.Read8();
			string name = reader.ReadString(length);

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
				resource = new T();
				resource.setResourceType(type);
				resource.setResourcePath(name);
				resource.setResourceAbsPath(path);
				using (Stream stream = File.OpenRead(path)) {
					resource.load(stream);
				}
			}
			if (resource == null && type != bloResourceType.None) {
				Console.WriteLine(">>> FAILED: could not find {0} resource '{1}' at 0x{2:X}.", type, name, start);
			}
			return resource;
		}

	}

}
