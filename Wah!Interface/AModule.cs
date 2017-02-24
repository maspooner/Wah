using System;

namespace Wah_Interface {
	public abstract class AModule {
		public string Name { get; set; }
		public AModule(string name) {
			Name = name;
		}
		public abstract void InitializeSettings(ISettings sets);
		public abstract IReturn Handle(ICore wah, string line);
		public void Execute(ICore wah, string line) {
			try {
				Handle(wah, line);
			}
			catch (WahException we) {
				
			}
			catch (Exception e) {

			}
		}
		public string Call(ICore wah, string line) {
			return Handle(wah, line).Accept(new ReturnToString());
		}

	}
}
