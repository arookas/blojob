
using System;
using System.IO;
using System.Threading;

namespace arookas {

	static class blojob {

		public static readonly Version sVersion = new Version(0, 1, 0);

		public static bloViewer sViewer;
		public static bloResourceFinder sResourceFinder;

		static void Main(string[] args) {
			Console.Title = "";
			var cmd = new aCommandLine(args);
			runViewer(cmd);
		}

		static void runViewer(aCommandLine cmd) {
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
				}
			}

			sResourceFinder = new bloResourceFinder(path);
			for (var i = 2; i < cmd.Count; ++i) {
				sResourceFinder.addGlobalPath(cmd[i].Name);
			}

			sViewer = new bloViewer(input, format);
			
#if DEBUGCONSOLE
			var thread = new Thread(runConsole);
			thread.Start();
#endif

			sViewer.run();

#if DEBUGCONSOLE
			thread.Abort();
#endif
		}

		static void runConsole() {
			while (true) {
				var cmd = Console.ReadLine().Split(' ');
				switch (cmd[0]) {
					case "pane": {
						bloPane screen = sViewer.getScreen();
						bloPane pane = screen;
						if (cmd.Length > 1) {
							pane = screen.search(convertStringToName(cmd[1]));
						}
						if (pane == null) {
							Console.WriteLine("could not fine pane");
							break;
						}
						dumpPane(0, pane);
						break;
					}
					case "info": {
						bloPane screen = sViewer.getScreen();
						bloPane pane = screen;
						if (cmd.Length > 1) {
							pane = screen.search(convertStringToName(cmd[1]));
						}
						if (pane == null) {
							Console.WriteLine("could not fine pane");
							break;
						}
						pane.info();
						break;
					}
					case "show": {
						bloPane screen = sViewer.getScreen();
						bloPane pane = screen;
						if (cmd.Length > 1) {
							pane = screen.search(convertStringToName(cmd[1]));
						}
						if (pane == null) {
							Console.WriteLine("could not fine pane");
							break;
						}
						pane.setVisible(true);
						break;
					}
					case "hide": {
						bloPane screen = sViewer.getScreen();
						bloPane pane = screen;
						if (cmd.Length > 1) {
							pane = screen.search(convertStringToName(cmd[1]));
						}
						if (pane == null) {
							Console.WriteLine("could not fine pane");
							break;
						}
						pane.setVisible(false);
						break;
					}
					case "text": {
						bloPane screen = sViewer.getScreen();
						bloPane pane = screen;
						if (cmd.Length > 1) {
							pane = screen.search(convertStringToName(cmd[1]));
						}
						if (pane == null) {
							Console.WriteLine("could not fine pane");
							break;
						}
						bloTextbox textbox = (pane as bloTextbox);
						if (textbox == null) {
							Console.WriteLine("pane is not a textbox");
							break;
						}
						bloFont font = textbox.getFont();
						ushort[] buffer = font.createStringBuffer(Console.ReadLine());
						textbox.setString(buffer);
						break;
					}
				}
			}
		}

		static void dumpPane(int indent, bloPane pane) {
			for (int i = 0; i < indent; ++i) {
				Console.Write("  ");
			}
			if (pane is bloTextbox) {
				Console.Write("TBX1 ");
			} else if (pane is bloWindow) {
				Console.Write("WIN1 ");
			} else if (pane is bloPicture) {
				Console.Write("PIC1 ");
			} else if (pane is bloScreen) {
				Console.Write("SCRN ");
			} else {
				Console.Write("PAN1 ");
			}
			Console.Write("'{0,4}' ", convertNameToString(pane.getName()));
			var rectangle = pane.getRectangle();
			Console.Write("({0}, {1} : {2}x{3}) ", rectangle.left, rectangle.top, rectangle.width, rectangle.height);
			Console.WriteLine();
			foreach (var childpane in pane) {
				dumpPane(indent + 1, childpane);
			}
		}

		static uint convertStringToName(string str) {
			var name = 0u;
			for (int i = 0; i < str.Length; ++i) {
				name <<= 8;
				name |= (uint)(str[i] & 255);
			}
			return name;
		}
		static string convertNameToString(uint name) {
			if (name > 0xFFFFFFu) {
				var chars = new char[4];
				chars[0] = (char)((name >> 24) & 255);
				chars[1] = (char)((name >> 16) & 255);
				chars[2] = (char)((name >> 8) & 255);
				chars[3] = (char)((name >> 0) & 255);
				return new String(chars);
			} else if (name > 0xFFFFu) {
				var chars = new char[3];
				chars[0] = (char)((name >> 16) & 255);
				chars[1] = (char)((name >> 8) & 255);
				chars[2] = (char)((name >> 0) & 255);
				return new String(chars);
			} else if (name > 0xFFu) {
				var chars = new char[2];
				chars[0] = (char)((name >> 8) & 255);
				chars[1] = (char)((name >> 0) & 255);
				return new String(chars);
			} else if (name > 0u) {
				var chars = new char[1];
				chars[0] = (char)((name >> 0) & 255);
				return new String(chars);
			}
			return "";
		}

	}

}
