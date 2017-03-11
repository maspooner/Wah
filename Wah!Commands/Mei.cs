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
		private IReturn Cmd_Scale(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			//Usage: mei scale -i=[img-name|dir-name] -o=[out-file|out-dir] -h=300 -w=200
			//TODO ImageReturn, display automatically image in picture box
			if(args.Count == 0) {
				if (flags.ContainsKey("-i")) {
					string inPath = flags["-i"];
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
								string outPath = flags.ContainsKey("-o") ? flags["-o"] :
									Path.Combine(Path.GetDirectoryName(inPath),
									Path.GetFileNameWithoutExtension(inPath) + "_scaled.png");
								return Scale_ScaleOne(outPath);
							}
							else {
								string outPath = flags.ContainsKey("-o") ? flags["-o"] :
									Path.Combine(inPath, "scaled");
								IList<IReturn> bits = new List<IReturn>();
								foreach(string f in Directory.EnumerateFiles(outPath)) {
									bits.Add(Scale_ScaleOne(f));
								}
								return new ListReturn(bits);
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
		private BitmapReturn Scale_ScaleOne(string filePath) {

		}

		private IReturn Cmd_Chop(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			//Usage: mei chop -i=[img-name|dir-name] -o=[out-file|out-dir] -l=50 -r=200
			return new NoReturn();
		}
	}
}
