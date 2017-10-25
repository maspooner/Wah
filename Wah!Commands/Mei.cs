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
		public Mei() : base(MEI_NO_NAMAE, MEI_NO_HAN) { }


		protected override NewICommand[] CreateCommands() {
			return new NewICommand[] {
				new Cmd_Mkfile()
			};
		}

		/************************************************
		***  Commands
		*************************************************/
		private OldIData Cmd_Scale(OldICore wah, OldCommandBundle bun) {
			//Usage: mei scale -i=[img-name|dir-name] -o=[out-file|out-dir] -h=300 -w=200
			//TODO ImageReturn, display automatically image in picture box
			bun.AssertNoArgs();
			if (bun.HasFlag("-i")) {
				string inPath = bun.Flags["-i"];
				bool inFile = File.Exists(inPath);
				bool inDir = Directory.Exists(inPath);
				//does not exist
				if (!(inFile || inDir)) {
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
							IList<OldIData> bits = new List<OldIData>();
							foreach (string f in Directory.EnumerateFiles(outPath)) {
								bits.Add(Scale_ScaleOne(f));
							}
							return new OldListData(bits);
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

		private OldIData Scale_ScaleOne(string filePath) {
			return new OldNoData();
		}

		private OldIData Cmd_Chop(OldICore wah, OldCommandBundle bun) {
			//Usage: mei chop -i=[img-name|dir-name] -o=[out-file|out-dir] -l=50 -r=200
			return new OldNoData();
		}
	}

	internal class Cmd_Mkfile : CheckedCommand<NoData>, IDataVisitor<IData> {
		private string filePath;
		public Cmd_Mkfile() : base("mkfile",
			//Rules
			new RequireRule('w'),
			new RequireRule('f')) {
			filePath = null;
		}

		public override NoData Apply(IWah wah, IBundle bun) {
			filePath = bun.Argument<StringData>('f').Data;
			if (bun.HasFlag("force") || !File.Exists(filePath)) {
				// visit with this visitor to find out how to make a file
				bun.Argument('w').Accept(this);
			}
			else {
				wah.Putln("File cannot be found or file already exists. To force overriding an existing file,"
					+ " use the -force flag", Color.Crimson);

			}
			return new NoData();
		}

		public IData Visit(IData data) {
			return data.Accept(this);
		}

		public IData VisitImage(ImageData data) {
			data.Data.Save(filePath);
			return data;
		}

		public IData VisitInt(IntData data) {
			File.WriteAllText(filePath, data.Data.ToString());
			return data;
		}

		public IData VisitNone(NoData data) {
			File.WriteAllText(filePath, "");
			return data;
		}

		public IData VisitString(StringData data) {
			File.WriteAllText(filePath, data.Data);
			return data;
		}
	}
}
