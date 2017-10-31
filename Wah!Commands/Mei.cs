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
		public Mei() : base(MEI_NO_NAMAE, Color.SlateBlue, MEI_NO_HAN) { }


		protected override ICommand[] CreateCommands() {
			return new ICommand[] {
				new Cmd_Mkfile()
			};
		}

		/************************************************
		***  Commands
		*************************************************/
		private IData Cmd_Scale(IWah wah, IBundle bun) {
			//Usage: mei scale -i=[img-name|dir-name] -o=[out-file|out-dir] -h=300 -w=200
			//TODO ImageReturn, display automatically image in picture box
			if (bun.HasArgument('i')) {
				string inPath = bun.Argument<StringData>('i').String;
				bool inFile = File.Exists(inPath);
				bool inDir = Directory.Exists(inPath);
				//does not exist
				if (!(inFile || inDir)) {
					throw new WahBadFormatException(
						"Could not find the input bitmap path specified: " + inPath);
				}
				else {
					try {
						if (inFile) {
							string outPath = bun.HasArgument('o') ? bun.Argument('o').ToString() :
								Path.Combine(Path.GetDirectoryName(inPath),
								Path.GetFileNameWithoutExtension(inPath) + "_scaled.png");
							return Scale_ScaleOne(outPath);
						}
						else {
							string outPath = bun.HasArgument('o') ? bun.Argument('o').ToString() :
								Path.Combine(inPath, "scaled");
							IList<IData> bits = new List<IData>();
							foreach (string f in Directory.EnumerateFiles(outPath)) {
								bits.Add(Scale_ScaleOne(f));
							}
							return new ListData(bits);
						}
					}
					catch {
						throw new WahIOLoadException("Could not find specified bitmap image");
					}
				}
			}
			else {
				throw new WahBadFormatException("scale must take a flag -i for the input path");
			}
		}

		private ImageData Scale_ScaleOne(string filePath) {
			return new ImageData(new Bitmap(300, 300));
		}
	}

	//TODO tuneup
	internal class Cmd_Mkfile : CheckedCommand<NoData>, IDataVisitor<IData> {
		private string filePath;
		private IWah wah;
		private IBundle bun;
		public Cmd_Mkfile() : base("mkfile",
			//Rules
			new RequireRule('w'),
			new RequireRule('f'),
			new TypeRule<StringData>('f')) {
			filePath = null;
		}

		private IData WriteFile(string toPath, IData data, IWah wah, IBundle bun) {
			this.wah = wah;
			this.bun = bun;
			filePath = toPath;
			if (bun.HasFlag("force") || !File.Exists(toPath)) {
				// visit with this visitor to find out how to make a file
				data.Accept(this);
			}
			else {
				wah.Putln("File cannot be found or file already exists. To force overriding an existing file,"
					+ " use the -force flag", Color.Crimson);

			}
			return data;
		}

		public override NoData Apply(IWah wah, IBundle bun) {
			WriteFile(bun.Argument<StringData>('f').String, bun.Argument('w'), wah, bun);
			return new NoData();
		}

		public IData Visit(IData data) {
			return data.Accept(this);
		}

		public IData VisitImage(ImageData data) {
			data.Image.Save(filePath + ".png");
			return data;
		}

		public IData VisitInt(IntData data) {
			File.WriteAllText(filePath + ".txt", data.Int.ToString());
			return data;
		}

		public IData VisitNone(NoData data) {
			File.WriteAllText(filePath + ".txt", "");
			return data;
		}

		public IData VisitString(StringData data) {
			File.WriteAllText(filePath + ".txt", data.String);
			return data;
		}

		public IData VisitList(ListData data) {
			//don't know what to call the items, just give generic names
			foreach(IData d in data.List) {
				WriteFile(Path.Combine(filePath, d.ToString()), d, wah, bun);
			}
			return data;
		}
	}
}
