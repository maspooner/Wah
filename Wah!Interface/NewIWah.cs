using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	/// <summary>
	/// Models the interface between the coders and the core functions of the Wah program. Allows a coder to do tasks
	/// such as write to the screen, access the hard drive, call native functions, and more.
	/// </summary>
	public interface NewIWah {
		NewIApi Api { get; }
		NewIDisplay Display { get; }

		void Log(string line);
		void Putln(string txt);
		void Put(string txt);
		void Putln(string txt, Color col);
		void Put(string txt, Color col);
		void PutErr(string err);
	}

	/// <summary>
	/// Represents an interface for the coder with the core command processor to execute certain native commands.
	/// </summary>
	public interface NewIApi {
		/// <summary>
		/// Is the module with the given name loaded?
		/// </summary>
		/// <param name="module">the module name</param>
		bool ModuleLoaded(string module);

		/// <summary>
		/// Runs the given line of input against the wah command processor as a command and returns the result.
		/// </summary>
		/// <param name="line">the command to run</param>
		/// <returns>the result of the run of the command</returns>
		NewIData Call(string line);

		/// <summary>
		/// Runs the given line of input against the wah command processor as a command and returns the result
		/// as the given type of IData.
		/// </summary>
		/// /// <typeparam name="D">the type of data to ensure is returned</typeparam>
		/// <param name="line">the command to run</param>
		/// <returns>the result of the run of the command</returns>
		D Call<D>(string line) where D : NewIData;

		/// <summary>
		/// Waits for the user to enter some input and returns the result.
		/// </summary>
		/// <returns>what string the user input</returns>
		string AwaitRead();

		/// <summary>
		/// Parses the given line into a piece of IData, making it StringData by default.
		/// </summary>
		/// <param name="line">the input to parse</param>
		/// <returns>the input parsed as IData</returns>
		NewIData ParseData(string line);
	}

	/// <summary>
	/// Represents an interface with the main display window of the Wah program
	/// </summary>
	public interface NewIDisplay {
		/// <summary>
		/// Shows the given text in the console with the given color
		/// </summary>
		/// <param name="txt">the text to display</param>
		/// <param name="col">the color to display the text in</param>
		void Print(string txt, Color col);

		/// <summary>
		/// Hides the main Wah window.
		/// </summary>
		void HideWindow();

		/// <summary>
		/// Clears all text from the main Wah window.
		/// </summary>
		void ClearWindow();

		/// <summary>
		/// Shows the given visual on the panel with the corresponding identifier
		/// </summary>
		/// <param name="visual">the visual to show</param>
		/// <param name="id">the id of where to show the visual</param>
		void ShowVisual(IVisual visual, int id);

		/// <summary>
		/// Clears all visuals from the window
		/// </summary>
		void ClearVisuals();
	}

	/// <summary>
	/// Models the interface between other components of Wah with the main command processor that
	/// should not be exposed to the coder.
	/// </summary>
	public interface NewIProcessor {
		/// <summary>
		/// 
		/// </summary>
		void BeginListening();

		/// <summary>
		/// Prepares the next line of input to be processed by Wah.
		/// </summary>
		/// <param name="line">the line of user input</param>
		void Prepare(string line);

		/// <summary>
		/// Kills the current running process.
		/// </summary>
		void InterruptJob();

	}

}
