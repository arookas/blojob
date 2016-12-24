
using System;
using System.IO;

namespace arookas {

	static class pablo {

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
				if (formatname.Equals("compact", StringComparison.InvariantCultureIgnoreCase)) {
					format = bloFormat.Compact;
				} else if (formatname.Equals("blo1", StringComparison.InvariantCultureIgnoreCase)) {
					format = bloFormat.Blo1;
				} else if (formatname.Equals("xml", StringComparison.InvariantCultureIgnoreCase)) {
					format = bloFormat.Xml;
				}
			}

			var finder = new bloResourceFinder(path);
			for (var i = 2; i < cmd.Count; ++i) {
				finder.addGlobalPath(cmd[i].Name);
			}
			bloResourceFinder.setFinder(finder);

			var viewer = new bloViewer(input, format);
			viewer.run();
		}

	}

}
