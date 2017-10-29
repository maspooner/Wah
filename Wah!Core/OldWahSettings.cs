
using System;
using System.Collections.Generic;
using System.Linq;
using Wah_Interface;

namespace Wah_Core {
	internal class OldWahSettings : ISettings {
		private Dictionary<string, IList<Setting>> settings;

		public OldWahSettings(IDisk wdisk) {
			this.settings = new Dictionary<string, IList<Setting>>();
		}

		public void RegisterSetting(string name, string defValue) {
			
		}

		public void Set(string name, string content) {
			Func<Setting, bool> matchesName = s => s.Pair.Name.Equals(name);
			IList<Setting> sets = settings[mod.Name];
			if (sets.Any(matchesName)) {
				ModifyContent(sets.First(matchesName), content);
			}
			else {
				throw new Exception("setting " + mod.Name + "." + name + " does not exist, nya.");
			}
		}

		public void IncludeSettings(IDisk disk) {
			if (settings.ContainsKey(mod.Name)) {
				throw new Exception("Settings for module " + mod.Name + " are already loaded");
			}
			
			string[] lines = disk.LoadSettings();
			IEnumerable<SettingPair> pairs = lines.Select(l => new SettingPair(l));
			//initialize settings of module
			mod.SetDefaultSettings(this);
			//set all the ones from file
			foreach(SettingPair pair in pairs) {
				Set(pair.Name, pair.Content);
			}
		}
	}
}
