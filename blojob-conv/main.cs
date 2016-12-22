
using System;
using System.IO;

namespace arookas {

	static class joblo {

		static int Main(string[] args) {

			var cmd = new aCommandLine(args);

			Console.Title = String.Format("joblo v{0}", blojob.getVersion());
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

			if (cmd.Count < 4) {
				Console.WriteLine("Usage: joblo.exe <input> <format> <output> <format> [<search path> [...]]");
				return 0;
			}

			var inputfile = Path.GetFullPath(cmd[0].Name);
			var inputpath = Path.GetDirectoryName(inputfile);
			var inputformat = parseFormat(cmd[1].Name);

			var outputfile = Path.GetFullPath(cmd[2].Name);
			var outputpath = Path.GetDirectoryName(outputfile);
			var outputformat = parseFormat(cmd[3].Name);

			var finder = new bloResourceFinder(inputpath);
			for (var i = 4; i < cmd.Count; ++i) {
				finder.addGlobalPath(cmd[i].Name);
			}
			bloResourceFinder.setFinder(finder);

			if (!File.Exists(inputfile)) {
				Console.WriteLine("Could not find input file '{0}'", inputfile);
				return 1;
			}

			if (inputfile == outputfile) {
				Console.WriteLine("Input and output files cannot be the same.");
				return 1;
			}

			if (inputformat == outputformat) {
				File.Copy(inputfile, outputfile);
				return 0;
			}

			bloScreen screen = null;

			using (var stream = File.OpenRead(inputfile)) {
				switch (inputformat) {
					case bloFormat.Compact: screen = bloScreen.loadCompact(stream); break;
					case bloFormat.Blo1: screen = bloScreen.loadBlo1(stream); break;
					default: {
						Console.WriteLine("Unimplemented input format {0}", inputformat);
						return 1;
					}
				}
			}

			if (screen == null) {
				Console.WriteLine("Failed to input file '{0}'", inputfile);
				return 1;
			}

			using (var stream = File.Create(outputfile)) {
				switch (outputformat) {
					case bloFormat.Blo1: screen.saveBlo1(stream); break;
					case bloFormat.Xml: screen.saveXml(stream); break;
					default: {
						Console.WriteLine("Unimplemented output format {0}", outputformat);
						return 1;
					}
				}
			}

			return 0;

		}

		static string[] sFormatNames = {
			"compact", "blo1", "xml"
		};

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
