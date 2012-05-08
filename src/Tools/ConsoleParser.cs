using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia.Tools
{
	public class ConsoleParser
	{
		#region Delegates
		public delegate void Command(Commands.CommandEvent e);
		#endregion Delegates

		#region Nested Types
		public interface ICommand
		{
			void OnInvoke(Commands.CommandEvent e);
			string OnTab(Commands.CommandEvent e);
		}
		#endregion Nested Types

		#region Fields
		private int _tabCounter = 0;
		private string _tabStr = null;
		/// <summary>
		/// Is insert mode active?
		/// </summary>
		private bool _ins = false;
		/// <summary>
		/// Current input line.
		/// </summary>
		private StringBuilder _line = new StringBuilder("");
		/// <summary>
		/// Container of older input lines.
		/// </summary>
		private LinkedList<string> _lines = new LinkedList<string>();
		/// <summary>
		/// Iterator thought command history.
		/// </summary>
		private LinkedListNode<string> _node = null;
		/// <summary>
		/// Container for console commands.
		/// </summary>
		private Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();
		/// <summary>
		/// Instance of this class (singleton).
		/// </summary>
		private static ConsoleParser _instance = null;
		/// <summary>
		/// Array of command line arguments separators.
		/// </summary>
		private readonly char[] _seps = { ' ', '\t', '\n', '\0' };
		/// <summary>
		/// Color of printed arguments.
		/// </summary>
		private readonly ConsoleColor _argClr = ConsoleColor.Green;
		/// <summary>
		/// Color of printed error messages.
		/// </summary>
		private readonly ConsoleColor _errClr = ConsoleColor.Red;
		/// <summary>
		/// Color of printed header.
		/// </summary>
		private readonly ConsoleColor _headClr = ConsoleColor.Blue;
		/// <summary>
		/// Color of printed input text.
		/// </summary>
		private readonly ConsoleColor _inpClr = ConsoleColor.White;
		/// <summary>
		/// Color of printed ordinary messages.
		/// </summary>
		private readonly ConsoleColor _txtClr = ConsoleColor.Cyan;
		#endregion Fields

		#region Properties
		private ConsoleParser Instance
		{
			get { return _instance; }
		}
		#endregion Properties

		#region Methods
		public static void Init()
		{
			if (_instance != null)
				return;
			_instance = new ConsoleParser();
			_instance.RegisterCommands();
		}
		public void RegisterCommand(string commandName, ICommand command)
		{
			if (command == null)
				throw new ArgumentNullException("Command is null.");
			_commands.Add(commandName, command);
		}
		public bool TryRegisterCommand(string commandName, ICommand command)
		{
			try
			{
				RegisterCommand(commandName, command);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				return false;
			}
			return true;
		}

		private ConsoleParser()
		{
			Mogre.Root.Singleton.FrameStarted += new Mogre.FrameListener.FrameStartedHandler(this.OnFrame);
		}
		private void FillRow(char c)
		{
			Console.CursorLeft = 0;
			for (int i = 1; i < Console.WindowWidth; i++)
				Console.Write(c);
			Console.CursorLeft = 0;
		}
		private void OnBackspace()
		{
			if (Console.CursorLeft > 0)
			{
				int pos = --Console.CursorLeft;
				_line.Remove(pos, 1);
				Console.Write(_line.ToString().Substring(pos) + " ");
				Console.CursorLeft = pos;
			}
		}
		private void OnDownArrow()
		{
			if (_node != _lines.Last && _node != null)
			{
				_node = _node.Next;
				_line.Remove(0, _line.Length);
				_line.Append(_node.Value);
				FillRow(' ');
				Console.Write(_line.ToString());
			}
		}
		private void OnEnter()
		{
			Console.WriteLine();
			if (_line.Length > 0)
			{
				_lines.AddLast(_line.ToString());
				ParseLine();
			}
			_line.Remove(0, _line.Length);
			_node = null;
			Core.Singleton.ShowFPS = true;
			Console.ResetColor();
		}
		private void OnEscape()
		{
			FillRow(' ');
			if (_tabCounter == 0)
			{
				_line.Remove(0, _line.Length);
				_node = null;
				Core.Singleton.ShowFPS = true;
				Console.ResetColor();
			}
			else
			{
				Console.Write(_line);
				_tabCounter = 0;
				_tabStr = null;
			}
		}
		private bool OnFrame(Mogre.FrameEvent e)
		{
			while (Console.KeyAvailable)
				ParseInputKey();
			return true;
		}
		private void OnInsert()
		{
			_ins = !_ins;
			Console.CursorSize = _ins ? 100 : 25;
		}
		private void OnKey(char key)
		{
			if (Console.CursorLeft == _line.Length)
			{
				_line.Append(key);
				Console.Write(key);
			}
			else
			{
				if (_ins)
				{
					_line.Replace(_line[Console.CursorLeft], key, Console.CursorLeft, 1);
					Console.Write(key);
				}
				else
				{
					int pos = Console.CursorLeft;
					_line.Insert(pos, key);
					Console.Write(_line.ToString().Substring(pos));
					Console.CursorLeft = pos + 1;
				}
			}
		}
		private void OnLeftArrow()
		{
			if (Console.CursorLeft > 0)
				--Console.CursorLeft;
		}
		private void OnRightArrow()
		{
			if (Console.CursorLeft < _line.Length)
				++Console.CursorLeft;
		}
		private void OnTab()
		{
			_tabStr = null;
			string[] args = _line.ToString().Split(_seps, StringSplitOptions.RemoveEmptyEntries);
			if (args.Length == 0)
			{
				_tabCounter %= _commands.Count;
				Dictionary<string, ICommand>.Enumerator it = _commands.GetEnumerator();
				for (int i = 0; i <= _tabCounter; i++)
					it.MoveNext();
				_tabStr = it.Current.Key;
			}
			else if (args.Length == 1)
			{
				List<string> list = new List<string>();
				foreach (KeyValuePair<string, ICommand> pair in _commands)
					if (pair.Key.StartsWith(args[0]))
						list.Add(pair.Key);
				if (list.Count != 0)
				{
					_tabCounter %= list.Count;
					_tabStr = list[_tabCounter];
				}
			}
			else if (_commands.ContainsKey(args[0]))
				_tabStr = _commands[args[0]].OnTab(new Commands.CommandEvent(args, _tabCounter));
			if (_tabStr != null)
			{
				FillRow(' ');
				if (args.Length == 1)
					Console.Write(_tabStr);
				else
				{
					for (int i = 0; i < args.Length - 1; i++)
						Console.Write(args[i] + " ");
					Console.Write(_tabStr);
				}
				++_tabCounter;
			}
		}
		private void OnUpArrow()
		{
			if (_node != _lines.First)
			{
				if (_node == null)
					_node = _lines.Last;
				else
					_node = _node.Previous;
				if (_node != null)
				{
					_line.Remove(0, _line.Length);
					_line.Append(_node.Value);
					FillRow(' ');
					Console.Write(_line.ToString());
				}
			}
		}
		private void ParseInputKey()
		{
			ConsoleKeyInfo info = Console.ReadKey(true);
			Core.Singleton.ShowFPS = false;
			Console.ForegroundColor = _inpClr;
			if (_tabCounter > 0 && info.Key != ConsoleKey.Escape && info.Key != ConsoleKey.Tab)
			{
				string[] str = _line.ToString().Split(_seps, StringSplitOptions.RemoveEmptyEntries);
				_line.Remove(0, _line.Length);
				for (int i = 0; i < str.Length - 1; i++)
					_line.Append(str[i] + " ");
				_line.Append(_tabStr);
				_tabCounter = 0;
				_tabStr = null;
			}
			switch (info.Key)
			{
				case ConsoleKey.Backspace:
					OnBackspace();
					break;
				case ConsoleKey.DownArrow:
					OnDownArrow();
					break;
				case ConsoleKey.Enter:
					OnEnter();
					break;
				case ConsoleKey.Escape:
					OnEscape();
					break;
				case ConsoleKey.Insert:
					OnInsert();
					break;
				case ConsoleKey.LeftArrow:
					OnLeftArrow();
					break;
				case ConsoleKey.RightArrow:
					OnRightArrow();
					break;
				case ConsoleKey.Tab:
					OnTab();
					break;
				case ConsoleKey.UpArrow:
					OnUpArrow();
					break;
				default:
					OnKey(info.KeyChar);
					break;
			}
		}
		private void ParseLine()
		{
			string[] args = _line.ToString().Split(_seps, StringSplitOptions.RemoveEmptyEntries);
			if (_commands.ContainsKey(args[0]))
				_commands[args[0]].OnInvoke(new Commands.CommandEvent(args));
		}
		private void RegisterCommands()
		{
			RegisterCommand("skyx", new Commands.SkyX());
		}
		#endregion Methods
	}
}
