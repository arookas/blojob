
using arookas.IO.Binary;
using System;
using System.Collections.Generic;
using System.IO;

namespace arookas {

	abstract class bloResource {

		public abstract void load(Stream stream);

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
			ReferenceType type = (ReferenceType)reader.Read8();
			int length = reader.Read8();
			string name = reader.ReadString(length);

			string path = null;
			switch (type) {
				case ReferenceType.LocalDirectory: {
					path = Path.Combine(mLocalPath, directory, name);
					break;
				}
				case ReferenceType.LocalArchive: {
					path = Path.Combine(mLocalPath, name);
					break;
				}
				case ReferenceType.Global: {
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
				using (Stream stream = File.OpenRead(path)) {
					resource.load(stream);
				}
			}
			if (resource == null && type != ReferenceType.None) {
				Console.WriteLine(">>> FAILED: could not find {0} resource '{1}' at 0x{2:X}.", type, name, start);
			}
			return resource;
		}

		enum ReferenceType {
			None, // null reference
			Unknown, // not used by SMS
			LocalDirectory, // resource is in the specified directory of the specified archive
			LocalArchive, // resource is in specified archive
			Global, // resource may be found in any loaded global resource
		}

	}

}
