using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wah_Core {
	static class GlobalHotKeys {
		//constants
		internal const int MOD_ALT = 1;
		internal const int MOD_CONTROL = 2;
		internal const int MOD_SHIFT = 4;
		internal const int MOD_WIN = 8;
		internal const int WM_HOTKEY = 0x312;
		//P/Invokes
		[DllImport("user32", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool RegisterHotKey(IntPtr hwnd, int id, uint fsModifiers, uint vk);
		[DllImport("user32", SetLastError = true)]
		private static extern int UnregisterHotKey(IntPtr hwnd, int id);
		[DllImport("kernel32", SetLastError = true)]
		private static extern short GlobalAddAtom(string lpString);
		[DllImport("kernel32", SetLastError = true)]
		private static extern short GlobalDeleteAtom(short nAtom);
		//members
		internal static short HotkeyID { get; private set; }
		//methods
		internal static void RegisterGlobalHotKey(int hotkey, int modifiers, IntPtr handle) {
			UnregisterGlobalHotKey(handle);

			try {
				// use the GlobalAddAtom API to get a unique ID (as suggested by MSDN)
				string atomName = Thread.CurrentThread.ManagedThreadId.ToString("X8") + "GlobalHotKeyer";
				HotkeyID = GlobalAddAtom(atomName);
				if (HotkeyID == 0)
					throw new Exception("Unable to generate unique hotkey ID. Error: " + Marshal.GetLastWin32Error().ToString());
				// register the hotkey, throw if any error
				if (!RegisterHotKey(handle, HotkeyID, (uint)modifiers, (uint)hotkey))
					throw new Exception("Unable to register hotkey. Error: " + Marshal.GetLastWin32Error().ToString());
			}
			catch (Exception ex) {
				// clean up if hotkey registration failed
				UnregisterGlobalHotKey(handle);
				Console.WriteLine(ex);
			}
		}
		internal static void UnregisterGlobalHotKey(IntPtr handle) {
			if (HotkeyID != 0) {
				UnregisterHotKey(handle, HotkeyID);
				// clean up the atom list
				GlobalDeleteAtom(HotkeyID);
				HotkeyID = 0;
			}
		}
	}
}
