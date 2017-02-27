using System;
using System.Drawing;

namespace Wah_Interface {
	/// <summary>
	/// Represents the core Wah! unit, has all the Wah! interfaces and special methods
	/// </summary>
	public interface ICore {
		//prohibit access
		IApi Api { get; }
		IDisplay Display { get; }
		IAudio Audio { get; }
		IDisk Disk { get; }
		ISettings Settings { get; }
		void Log(string line);
		void Put(string txt);
		void PutErr(string err);
		void Put(string txt, Color col);
	}
	/// <summary>
	/// Represents the main Wah! command processor that handles the REPL and deligation of commands to modules
	/// </summary>
	public interface IProcessor {
		//prohibit access
		AModule ActiveModule { get; set; }
		void InitializeModules();
		void LoadModule(string dllName, string moduleName);
		void LoadModuleLibrary(string dllName);
		void LoadModule(string name);
		void UnloadModule(string name);
		AModule FindModule(string name);
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
		void ShowPersona(Bitmap persona);
		void ShowTitle(string title);
		void Print(string txt, Color col);
		void HideWindow();
	}
	public interface IAudio {

	}
	public interface IDisk {
		void Save(string fileName, byte[] data);
		byte[] Load(string fileName);
		void RunShutdownOperations();
		System.Reflection.Assembly LoadAssembly(string name);
	}
	public interface ISettings {
		//allow access
		void RegisterSetting(string name, string defValue, SettingType type);
		
		string GetString(string name);
		int GetInt(string name);
		bool GetBool(string name);
		//prohibit access
		void LoadSettings(string module);
		void UnloadSettings(string module);
		byte[] ToBytes();
		void Set(string name, string content);
	}
	public class Setting {
		public string Name { get; set; }
		public string Content { get; set; }
		public string Default { get; set; }
		public SettingType Which { get; set; }
	}

	public enum SettingType { STRING, INT, BOOL }
}
