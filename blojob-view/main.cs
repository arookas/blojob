
using System;
using System.Collections.Generic;
using System.IO;

namespace arookas {

	static class pablo {

		static string[] sFormatNames = {
			"compact", "blo1", "xml"
		};

		static void Main(string[] args) {
			var cmd = new aCommandLine(args, '-');

			Console.Title = String.Format("pablo v{0}", blojob.getVersion());
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

			// do not use cmd for this check, as both d&d and -input will both return 1
			switch (args.Length) {
				case 0: doUsage(); break;
				case 1: doDragAndDrop(cmd[0].Name); break;
				default: doCommandLine(cmd); break;
			}
		}

		static void doUsage() {
			Console.WriteLine("Usage: pablo.exe -input <file> [<format>] [options]");
			Console.WriteLine();
			Console.WriteLine("Options: ");
			Console.WriteLine("  -search-paths [<path> [...]]");
			Console.WriteLine("  -display-size <width> <height>");
		}
		static void doDragAndDrop(string input) {
			if (!File.Exists(input)) {
				Console.WriteLine("Could not find input file '{0}'", input);
				return;
			}

			var path = Path.GetDirectoryName(Path.GetFullPath(input));
			var file = Path.GetFileName(input);

			createFinder(path, null);
			var screen = loadScreen(input, bloFormat.Blo1);

			if (screen == null) {
				Console.WriteLine("Failed to load input file '{0}'", input);
				return;
			}

			openViewer(screen, file);
		}
		static void doCommandLine(aCommandLine cmd) {
			bool inputSet = false;
			string input = null;
			bloFormat format = bloFormat.Blo1;

			var searchPaths = new List<string>(5);

			bool sizeSet = false;
			int width = 0, height = 0;

			foreach (var param in cmd) {
				switch (param.Name.ToLowerInvariant()) {
					case "-input": {
						if (param.Count < 1) {
							break;
						}
						inputSet = true;
						input = param[0];
						format = (param.Count >= 2 ? parseFormat(param[1]) : bloFormat.Blo1);
						break;
					}
					case "-search-paths": {
						foreach (var arg in param) {
							searchPaths.Add(arg);
						}
						break;
					}
					case "-display-size": {
						if (param.Count != 2) {
							break;
						}
						if (Int32.TryParse(param[0], out width) && Int32.TryParse(param[1], out height)) {
							sizeSet = true;
						}
						break;
					}
				}
			}

			if (!inputSet) {
				doUsage();
				return;
			}

			if (!File.Exists(input)) {
				Console.WriteLine("Could not find input file '{0}'", input);
				return;
			}

			var path = Path.GetDirectoryName(Path.GetFullPath(input));
			var file = Path.GetFileName(input);

			createFinder(path, searchPaths);
			var screen = loadScreen(input, format);

			if (screen == null) {
				Console.WriteLine("Failed to load input file '{0}'", input);
				return;
			}

			if (sizeSet) {
				openViewer(screen, file, width, height);
			} else {
				openViewer(screen, file);
			}
		}

		static bloResourceFinder createFinder(string localPath, IEnumerable<string> globalPaths) {
			var finder = new bloResourceFinder(localPath);
			if (globalPaths != null) {
				foreach (var globalPath in globalPaths) {
					finder.addGlobalPath(globalPath);
				}
			}
			bloResourceFinder.setFinder(finder);
			return finder;
		}
		static bloScreen loadScreen(string input, bloFormat format) {
			bloScreen screen = null;
			using (Stream stream = File.OpenRead(input)) {
				switch (format) {
					case bloFormat.Compact: screen = bloScreen.loadCompact(stream); break;
					case bloFormat.Blo1: screen = bloScreen.loadBlo1(stream); break;
					case bloFormat.Xml: screen = bloScreen.loadXml(stream); break;
				}
			}
			return screen;
		}

		static void openViewer(bloScreen screen, string title) {
			var rectangle = screen.getRectangle();
			openViewer(screen, title, rectangle.width, rectangle.height);
		}
		static void openViewer(bloScreen screen, string title, int width, int height) {
			var viewer = new bloViewer(screen);
			viewer.setTitle(title);
			viewer.setSize(width, height);
			viewer.run();
		}

		static bloFormat parseFormat(string text) {
			var format = bloFormat.Blo1;

			for (int i = 0; i < sFormatNames.Length; ++i) {
				if (text.Equals(sFormatNames[i], StringComparison.InvariantCultureIgnoreCase)) {
					format = (bloFormat)i;
					break;
				}
			}

			return format;
		}

	}

}
