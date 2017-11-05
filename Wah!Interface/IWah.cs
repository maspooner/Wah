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
	public interface IWah {
		IApi Api { get; }
		IDisplay Display { get; }

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
	public interface IApi {
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
		IData Call(string line);

		/// <summary>
		/// Runs the given line of input against the wah command processor as a command and returns the result
		/// as the given type of IData.
		/// </summary>
		/// /// <typeparam name="D">the type of data to ensure is returned</typeparam>
		/// <param name="line">the command to run</param>
		/// <returns>the result of the run of the command</returns>
		D Call<D>(string line) where D : IData;

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
		IData ParseData(string line);
	}

	/// <summary>
	/// Represents an interface with the main display window of the Wah program
	/// </summary>
	public interface IDisplay {
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
	public interface IProcessor {
		/// <summary>
		/// Start the processor to accept input
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

		/// <summary>
		/// Load a specific module from the given dll file location
		/// </summary>
		/// <param name="dllName">the dll containing the module</param>
		/// <param name="moduleName">the name of the module to load</param>
		void LoadModule(string dllName, string moduleName);

		/// <summary>
		/// Load all modules from the given dll.
		/// </summary>
		/// <param name="dllName">the dll containing the modules to load</param>
		void LoadModuleLibrary(string dllName);

		/// <summary>
		/// Unloads the module with the given name from the Wah program
		/// </summary>
		/// <param name="name">the name of the module to remove</param>
		void UnloadModule(string name);

	}





	/// <summary>
	/// Models a collection of commands that share a similar function and can be easily added or removed from the main
	/// Wah program.
	/// </summary>
	public interface IModule {
		string Name { get; }
		Color Color { get; }

		/// <summary>
		/// Does this module contain a command with the given name?
		/// </summary>
		/// <param name="cmd">the name of the command</param>
		bool HasCommand(string cmd);

		/// <summary>
		/// Retrieves the command with the given name that is in this module.
		/// </summary>
		/// <param name="cmd">the string command name</param>
		/// <returns>the command</returns>
		ICommand GetCommand(string cmd);

	}

	/// <summary>
	/// Represents an interface with a specific Module's file system, including operations like opening and saving files.
	/// </summary>
	public interface IDisk {
		/// <summary>
		/// Gets the operating directory of the Wah program
		/// </summary>
		string ProgramDirectory { get; }

		/// <summary>
		/// Loads an image with the given file name from the data folder of the module
		/// </summary>
		/// <param name="fileName">the name of just the image file (name + extension)</param>
		/// <returns>a bitmap of the file</returns>
		Bitmap LoadImage(string fileName);

		/// <summary>
		/// Loads an entire directory of images in topological order
		/// </summary>
		/// <param name="dirName"></param>
		/// <param name="ext">the extension of files to look for</param>
		/// <returns>the list of images loaded</returns>
		Bitmap[] LoadImageDir(string dirName, string ext);

		/// <summary>
		/// Loads a list of lines of a text file
		/// </summary>
		/// <param name="fileName">the name of the text file</param>
		/// <returns>the lines of text</returns>
		string[] LoadLines(string fileName);

		/// <summary>
		/// Attempts to load help information about the given topic and 
		/// parse and display to the wah! core's display
		/// </summary>
		/// <param name="topic">the topic of the help file to load</param>
		void LoadDisplayHelp(IWah wah, string topic);

		/// <summary>
		/// Ensures that the given path starting from the program directory exists
		/// </summary>
		/// <param name="path">the directory to check</param>
		/// <returns>true if path had to be created</returns>
		bool EnsurePath(string path);

		/// <summary>
		/// Ensures that the given file exists
		/// </summary>
		/// <param name="file">the file to check</param>
		/// <returns>true if file had to be created</returns>
		bool EnsureFile(string file);
	}

}
