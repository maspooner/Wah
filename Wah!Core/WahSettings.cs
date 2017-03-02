
using System;
using System.Collections.Generic;
using System.Linq;
using Wah_Interface;

namespace Wah_Core {
	internal class WahSettings : ISettings {
		private IProcessor wpro;
		private IDisk wdisk;
		private Dictionary<string, IList<Setting>> settings;

		public WahSettings(IProcessor wpro, IDisk wdisk) {
			this.wpro = wpro;
			this.wdisk = wdisk;
			this.settings = new Dictionary<string, IList<Setting>>();
		}

		public bool GetBool(AModule mod, string name) {
			throw new NotImplementedException();
		}

		public int GetInt(AModule mod, string name) {
			throw new NotImplementedException();
		}

		public string GetString(AModule mod, string name) {
			throw new NotImplementedException();
		}

		public void RegisterSetting(string name, string defValue, SettingType type) {
			throw new NotImplementedException();
		}

		public void Set(AModule mod, string name, string content) {
			Func<Setting, bool> matchesName = s => s.Name.Equals(name);
			IList<Setting> sets = settings[mod.Name];
			if (sets.Any(matchesName)) {
				SafelyModifyContent(mod, sets.First(matchesName), content);
			}
			else {
				throw new NoSuchItemException("setting " + mod.Name + "." + name + " does not exist, nya.");
			}
		}

		private void SafelyModifyContent(AModule mod, Setting s, string newValue) {
			bool aokay = false;
			switch (s.Which) {
				case SettingType.BOOL: bool temp = false; aokay = bool.TryParse(newValue, out temp); break;
				case SettingType.INT: int temp2 = 0; aokay = int.TryParse(newValue, out temp2); break;
				case SettingType.STRING: aokay = true; break;
				default: throw new NotImplementedException("SafelyModifyContent");
			}
			if (aokay) {
				s.Content = newValue;
			}
			else {
				throw new WrongDataTypeException("Attempted to set " + mod.Name + "." + s.Name + "to a data type that does not match with " + s.Which.ToString());
			}
		}

		public byte[] ToBytes() {
			throw new NotImplementedException();
		}

		public void LoadSettings(string module) {
			throw new NotImplementedException();
		}

		public void UnloadSettings(string module) {
			throw new NotImplementedException();
		}
	}
}
