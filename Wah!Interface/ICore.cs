using System;
using System.Drawing;

namespace Wah_Interface {
	/// <summary>
	/// Represents the core Wah! unit, has all the non-restricted Wah! interfaces and special methods
	/// </summary>
	public interface ICore {
		//allow access
		IApi Api { get; }
		IDisplay Display { get; }
		IAudio Audio { get; }
		IDisk Disk { get; }
		ISettings Settings { get; }
		void Log(string line);
		void Putln(string txt);
		void Put(string txt);
		void Putln(string txt, Color col);
		void Put(string txt, Color col);
		void PutErr(string err);
	}
	/// <summary>
	/// Represents the restricted core Wah! unit, with access to restricted Wah! interfaces
	/// </summary>
	public interface IReCore {
		IProcessor Processor { get; }
		IReDisk ReDisk { get; }
		IReSettings ReSettings { get; }
	}
	/// <summary>
	/// Represents the main Wah! command processor that handles the REPL and deligation of commands to modules
	/// </summary>
	public interface IProcessor {
		//prohibit access
		void InitializeModules();
		void InitializeMacros();
		void LoadModule(string dllName, string moduleName);
		void LoadModuleLibrary(string dllName);
		void LoadModule(string name);
		void UnloadModule(string name);
		AModule FindModule(string name);
		void ValidateModule(AModule mod);
		void BeginListening();
		void RunCommandLoop();
		void Prepare(string line);
		void InterruptJob();
		void Execute();

	}
	/// <summary>
	/// Represents an interface with the command processor that gives modules access to its features
	/// </summary>
	public interface IApi {
		//allow access
		bool ModuleLoaded(string module);
		IReturn Call(string line);
		void Execute(string line);
		string AwaitRead();
	}
	public interface IDisplay {
		void ShowExtra(IVisual extra);
		void ShowPersona(IVisual persona);
		void Print(string txt, Color col);
		void HideWindow();
		void ClearWindow();
	}

	public interface IAudio {

	}
	public interface IReDisk {
		//prohibit access
		bool AttemptFirstTimeSetup();
		void RunShutdownOperations();
		void LoadDisplayHelp(ICore wah, AModule mod, string helpName);
		System.Reflection.Assembly LoadAssembly(string name);
		System.Collections.Generic.IEnumerable<System.Reflection.Assembly> LoadAllAssemblies();
		bool EnsureFullDir(string dirPath);
		bool EnsureFullFile(string filePath);
	}
	public interface IDisk {
		//allow access
		string RelDataDir(AModule mod);
        void Save(string fileName, byte[] data);
		byte[] Load(string fileName);
		Bitmap LoadImage(AModule mod, string fileName);
		Bitmap[] LoadImageDir(AModule mod, string dirName, string ext);
		string[] LoadLines(AModule mod, string fileName);
		string[] LoadSettings(AModule mod);
		bool EnsureRelDir(string relDirPath);
		bool EnsureRelFile(string relFilePath);
	}
	public interface IReSettings {
		//prohibit access
		void IncludeSettings(IDisk wdisk, AModule mod);
		void ExcludeSettings(AModule mod);
		byte[] ToBytes();
		void RegisterSetting(AModule mod, string name, string defValue, SettingType type);
		void Set(AModule mod, string name, string content);
	}
	public interface ISettings {
		//allow access
		string GetString(AModule mod, string name);
		int GetInt(AModule mod, string name);
		bool GetBool(AModule mod, string name);

	}
	public class Setting {
		public SettingPair Pair { get; set; }
		public string Default { get; set; }
		public SettingType Which { get; set; }
		public Setting(SettingPair pair, string def, SettingType which) {
			Pair = pair;
			Default = def;
			Which = which;
		}
	}
	public class SettingPair {
		private const char ENCODING_CHAR = ':';
		public string Name { get; set; }
		public string Content { get; set; }
		public SettingPair(string enline) {
			string[] pieces = enline.Split(ENCODING_CHAR);
			if (pieces.Length != 2) {
				throw new IllformedInputException("Setting pair must have 2 parts");
			}
			Name = DecodeB64(pieces[0]);
			Content = DecodeB64(pieces[1]);
		}
		public string Encode() {
			return EncodeB64(Name) + ENCODING_CHAR + EncodeB64(Content);
		}
		private string EncodeB64(string s) {
			return Convert.ToBase64String(System.Text.Encoding.UTF32.GetBytes(s));
		}
		private string DecodeB64(string s) {
			return System.Text.Encoding.UTF32.GetString(Convert.FromBase64String(s));
		}
	}

	public enum SettingType { STRING, INT, BOOL }
}
