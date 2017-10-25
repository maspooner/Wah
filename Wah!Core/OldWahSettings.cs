
using System;
using System.Collections.Generic;
using System.Linq;
using Wah_Interface;

namespace Wah_Core {
	internal class OldWahSettings : ISettings {
		private IProcessor wpro;
		private IDisk wdisk;
		private Dictionary<string, IList<Setting>> settings;

		public OldWahSettings(IProcessor wpro, IDisk wdisk) {
			this.wpro = wpro;
			this.wdisk = wdisk;
			this.settings = new Dictionary<string, IList<Setting>>();
		}

		public bool GetBool(OldAModule mod, string name) {
			throw new NotImplementedException();
		}

		public int GetInt(OldAModule mod, string name) {
			throw new NotImplementedException();
		}

		public string GetString(OldAModule mod, string name) {
			throw new NotImplementedException();
		}

		public void RegisterSetting(OldAModule mod, string name, string defValue, SettingType type) {
			
		}

		public void Set(OldAModule mod, string name, string content) {
			Func<Setting, bool> matchesName = s => s.Pair.Name.Equals(name);
			IList<Setting> sets = settings[mod.Name];
			if (sets.Any(matchesName)) {
				SafelyModifyContent(mod, sets.First(matchesName), content);
			}
			else {
				throw new NoSuchItemException("setting " + mod.Name + "." + name + " does not exist, nya.");
			}
		}

		private void SafelyModifyContent(OldAModule mod, Setting s, string newValue) {
			bool aokay = false;
			switch (s.Which) {
				case SettingType.BOOL: bool temp = false; aokay = bool.TryParse(newValue, out temp); break;
				case SettingType.INT: int temp2 = 0; aokay = int.TryParse(newValue, out temp2); break;
				case SettingType.STRING: aokay = true; break;
				default: throw new NotImplementedException("SafelyModifyContent");
			}
			if (aokay) {
				s.Pair.Content = newValue;
			}
			else {
				throw new WrongDataTypeException("Attempted to set " + mod.Name + "." + s.Pair.Name + "to a data type that does not match with " + s.Which.ToString());
			}
		}

		public void IncludeSettings(IDisk disk, OldAModule mod) {
			if (settings.ContainsKey(mod.Name)) {
				throw new InvalidStateException("Settings for module " + mod.Name + " are already loaded");
			}
			
			string[] lines = disk.LoadSettings(mod);
			IEnumerable<SettingPair> pairs = lines.Select(l => new SettingPair(l));
			//initialize settings of module
			mod.SetDefaultSettings(this);
			//set all the ones from file
			foreach(SettingPair pair in pairs) {
				Set(mod, pair.Name, pair.Content);
			}
		}
	}
}
