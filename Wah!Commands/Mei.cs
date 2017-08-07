using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wah_Interface;

namespace Wah_Commands {
	/// <summary>
	/// Mei handles utility commands
	/// </summary>
	public class Mei : AModule {
		private const string MEI_NO_NAMAE = "mei";
		private const string MEI_NO_HAN = "Onii alpha 0";
		public Mei() : base(MEI_NO_NAMAE, MEI_NO_HAN) {

		}

		public override Dictionary<string, CommandDelegate> InitializeCommands() {
			Dictionary<string, CommandDelegate> cmds = new Dictionary<string, CommandDelegate>();
			return cmds;
		}

		public override void SetDefaultSettings(IReSettings sets) {
			sets.RegisterSetting(this, "image.quality", "0", SettingType.INT);
		}

		/************************************************
		***  Commands
		*************************************************/
		private IData Cmd_Scale(ICore wah, CommandBundle bun) {
			//Usage: mei scale -i=[img-name|dir-name] -o=[out-file|out-dir] -h=300 -w=200
			//TODO ImageReturn, display automatically image in picture box
			if(bun.ArgCount(0)) {
				if (bun.HasFlag("-i")) {
					string inPath = bun.Flags["-i"];
					bool inFile = File.Exists(inPath);
					bool inDir = Directory.Exists(inPath);
					//does not exist
					if(!(inFile || inDir)) {
						throw new IllformedInputException(
							"Could not find the input bitmap path specified: " + inPath);
					}
					else {
						try {
							if (inFile) {
								string outPath = bun.HasFlag("-o") ? bun.Flags["-o"] :
									Path.Combine(Path.GetDirectoryName(inPath),
									Path.GetFileNameWithoutExtension(inPath) + "_scaled.png");
								return Scale_ScaleOne(outPath);
							}
							else {
								string outPath = bun.HasFlag("-o") ? bun.Flags["-o"] :
									Path.Combine(inPath, "scaled");
								IList<IData> bits = new List<IData>();
								foreach(string f in Directory.EnumerateFiles(outPath)) {
									bits.Add(Scale_ScaleOne(f));
								}
								return new ListData(bits);
							}
						}
						catch {
							throw new IOLoadException("Could not find specified bitmap image");
						}
					}
				}
				else {
					throw new IllformedInputException("scale must take a flag -i for the input path");
				}
			}
			else {
				throw new IllformedInputException("Too many arguments");
			}
		}
		private IData Scale_ScaleOne(string filePath) {
			return new NoData();
		}

		private IData Cmd_Chop(ICore wah, CommandBundle bun) {
			//Usage: mei chop -i=[img-name|dir-name] -o=[out-file|out-dir] -l=50 -r=200
			return new NoData();
		}
	}
}
