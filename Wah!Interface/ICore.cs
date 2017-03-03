using System;
using System.Drawing;

namespace Wah_Interface {
	/// <summary>
	/// Represents the core Wah! unit, has all the Wah! interfaces and special methods
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
	}
	public interface IDisk {
		//allow access
		bool EnsureDir(string dirName);
		void Save(string fileName, byte[] data);
		byte[] Load(string fileName);
		Bitmap LoadImage(AModule mod, string fileName);
		Bitmap[] LoadImageDir(AModule mod, string dirName, string ext);
	}
	public interface IReSettings {
		//prohibit access
		void LoadSettings(string module);
		void UnloadSettings(string module);
		byte[] ToBytes();
		void Set(AModule mod, string name, string content);
	}
	public interface ISettings {
		//allow access
		void RegisterSetting(string name, string defValue, SettingType type);
		
		string GetString(AModule mod, string name);
		int GetInt(AModule mod, string name);
		bool GetBool(AModule mod, string name);
		
	}
	public class Setting {
		public string Name { get; set; }
		public string Content { get; set; }
		public string Default { get; set; }
		public SettingType Which { get; set; }
	}

	public enum SettingType { STRING, INT, BOOL }
}
