using System;
using System.Drawing;

namespace Wah_Interface {
	public interface IDisk {
		//allow access
		string[] LoadLines(string fileName);
		string[] LoadSettings();
		System.Collections.Generic.IEnumerable<System.Reflection.Assembly> LoadAllAssemblies();
	}

	public interface ISettings {
		//allow access
		void RegisterSetting(string name, string defValue);
		void Set(string name, string content);
	}
	public class Setting {
		public SettingPair Pair { get; set; }
		public string Default { get; set; }
		public Setting(SettingPair pair, string def) {
			Pair = pair;
			Default = def;
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
}
