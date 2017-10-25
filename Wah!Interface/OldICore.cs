using System;
using System.Drawing;

namespace Wah_Interface {
	/// <summary>
	/// Represents the core Wah! unit, has all the non-restricted Wah! interfaces and special methods
	/// </summary>
	public interface OldICore {
		//allow access
		IApi Api { get; }
		IDisk Disk { get; }
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
	}
	
	public interface IProcessor {
		//prohibit access
		void InitializeMacros();
	}

	public interface IApi {
		//allow access
		bool ModuleLoaded(string module);
		OldIData Call(string line);
		string AwaitRead();
	}
	public interface IDisk {
		//allow access
		string[] LoadLines(OldAModule mod, string fileName);
		string[] LoadSettings(OldAModule mod);
		void LoadDisplayHelp(OldICore wah, OldAModule mod, string helpName);
		System.Collections.Generic.IEnumerable<System.Reflection.Assembly> LoadAllAssemblies();
	}

	public interface ISettings {
		//allow access
		void RegisterSetting(OldAModule mod, string name, string defValue, SettingType type);
		void Set(OldAModule mod, string name, string content);
		string GetString(OldAModule mod, string name);
		int GetInt(OldAModule mod, string name);
		bool GetBool(OldAModule mod, string name);

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
