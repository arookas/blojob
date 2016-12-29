
using System;
using System.Collections.Generic;
using System.IO;

namespace arookas {

	static class joblo {

		static string[] sFormatNames = {
			"compact", "blo1", "xml"
		};

		static int Main(string[] args) {
			var cmd = new aCommandLine(args);

			Console.Title = String.Format("joblo v{0}", blojob.getVersion());
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

			switch (args.Length) {
				case 0: return doUsage();
				default: return doCommandLine(cmd);
			}
		}

		static int doUsage() {
			Console.WriteLine("Usage: joblo.exe -input <file> [<format>] -output <file> [<format>] [options]");
			Console.WriteLine();
			Console.WriteLine("Options:");
			Console.WriteLine("  -search-paths [<path> [...]]");
			return 0;
		}
		static int doCommandLine(aCommandLine cmd) {
			bool inputSet = false;
			string input = null;
			bloFormat inputFormat = bloFormat.Blo1;

			bool outputSet = false;
			string output = null;
			bloFormat outputFormat = bloFormat.Blo1;

			var searchPaths = new List<string>(5);

			foreach (var param in cmd) {
				switch (param.Name.ToLowerInvariant()) {
					case "-input": {
						if (param.Count < 1) {
							break;
						}
						inputSet = true;
						input = param[0];
						inputFormat = (param.Count >= 2 ? parseFormat(param[1]) : bloFormat.Blo1);
						break;
					}
					case "-output": {
						if (param.Count < 1) {
							break;
						}
						outputSet = true;
						output = param[0];
						outputFormat = (param.Count >= 2 ? parseFormat(param[1]) : bloFormat.Blo1);
						break;
					}
					case "-search-paths": {
						foreach (var arg in param) {
							searchPaths.Add(arg);
						}
						break;
					}
				}
			}

			if (!inputSet || !outputSet) {
				return doUsage();
			}

			var inputFile = Path.GetFullPath(input);
			var outputFile = Path.GetFullPath(output);

			var inputPath = Path.GetDirectoryName(inputFile);
			var outputPath = Path.GetDirectoryName(outputFile);

			var finder = new bloResourceFinder(inputPath);
			foreach (var searchPath in searchPaths) {
				finder.addGlobalPath(searchPath);
			}
			bloResourceFinder.setFinder(finder);

			if (!File.Exists(inputFile)) {
				Console.WriteLine("Could not find input file '{0}'", inputFile);
				return 1;
			}

			if (inputFile == outputFile) {
				Console.WriteLine("Input and output files cannot be the same.");
				return 1;
			}

			if (inputFormat == outputFormat) {
				File.Copy(inputFile, outputFile);
				return 0;
			}

			bloScreen screen = null;

			using (var stream = File.OpenRead(inputFile)) {
				switch (inputFormat) {
					case bloFormat.Compact: screen = bloScreen.loadCompact(stream); break;
					case bloFormat.Blo1: screen = bloScreen.loadBlo1(stream); break;
					case bloFormat.Xml: screen = bloScreen.loadXml(stream); break;
					default: {
						Console.WriteLine("Unimplemented input format {0}", inputFormat);
						return 1;
					}
				}
			}

			if (screen == null) {
				Console.WriteLine("Failed to input file '{0}'", inputFile);
				return 1;
			}

			using (var stream = File.Create(outputFile)) {
				switch (outputFormat) {
					case bloFormat.Compact: screen.saveCompact(stream); break;
					case bloFormat.Blo1: screen.saveBlo1(stream); break;
					case bloFormat.Xml: screen.saveXml(stream); break;
					default: {
						Console.WriteLine("Unimplemented output format {0}", outputFormat);
						return 1;
					}
				}
			}

			return 0;
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
