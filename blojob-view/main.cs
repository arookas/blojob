
using System;
using System.IO;

namespace arookas {

	static class pablo {

		static string[] sFormatNames = {
			"compact", "blo1", "xml"
		};

		static void Main(string[] args) {
			var cmd = new aCommandLine(args);

			Console.Title = String.Format("pablo v{0}", blojob.getVersion());
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

			if (cmd.Count == 0) {
				Console.WriteLine("Usage: pablo.exe <input> [<format> [<search path> [...]]]");
				return;
			}
			
			string input = Path.GetFullPath(cmd[0].Name);
			string path = Path.GetDirectoryName(Path.GetFullPath(input));
			string file = Path.GetFileName(input);

			if (!File.Exists(input)) {
				Console.WriteLine("Couldn't not find input file '{0}'", input);
				return;
			}

			var format = bloFormat.Blo1;

			if (cmd.Count > 1) {
				var formatname = cmd[1].Name;
				for (int i = 0; i < sFormatNames.Length; ++i) {
					if (formatname.Equals(sFormatNames[i], StringComparison.InvariantCultureIgnoreCase)) {
						format = (bloFormat)i;
						break;
					}
				}
			}

			var finder = new bloResourceFinder(path);
			for (var i = 2; i < cmd.Count; ++i) {
				finder.addGlobalPath(cmd[i].Name);
			}
			bloResourceFinder.setFinder(finder);

			bloScreen screen = null;
			using (Stream stream = File.OpenRead(input)) {
				switch (format) {
					case bloFormat.Compact: screen = bloScreen.loadCompact(stream); break;
					case bloFormat.Blo1: screen = bloScreen.loadBlo1(stream); break;
					case bloFormat.Xml: screen = bloScreen.loadXml(stream); break;
				}
			}

			if (screen == null) {
				Console.WriteLine("Failed to load input file '{0}'", input);
				return;
			}

			var viewer = new bloViewer(screen);
			viewer.setTitle(Path.GetFileName(input));
			viewer.setSize(600, 448);
			viewer.run();
		}

	}

}
