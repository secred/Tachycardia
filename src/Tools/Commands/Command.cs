using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia.Tools.Commands
{
		public struct CommandEvent
		{
			/// <summary>
			/// Array of command arguments.
			/// </summary>
			public readonly string[] args;
			public int TabCounter;

			public CommandEvent(string[] _args)
			{
				args = _args;
				TabCounter = 0;
			}
			public CommandEvent(string[] _args, int tabCounter)
			{
				args = _args;
				TabCounter = tabCounter;
			}
		}
	/// <summary>
	/// Implements command engine.
	/// </summary>
	/// <remarks>
	/// In child classes:
	/// 1. Write argumets callbacks;
	/// 2. Implement RegisterArgs method:
	/// 2.1. Register every callback with RegisterArg method. Argument name should be long name. Shorter names should be avaible thought aliases;
	/// 2.2. Register every subargument with RegisterSubArg method. Not registered subargs will not be passed to callback;
	/// 2.3. Register aliases to callback to shorter typing time;
	/// </remarks>
	abstract public class Command : ConsoleParser.ICommand
	{
		#region Delegates
		protected delegate void ArgCall(ArgEvent e);
		#endregion Delegates

		#region Nested Types
		public struct ArgEvent
		{
			/// <summary>
			/// Called argument name.
			/// </summary>
			public string name;
			/// <summary>
			/// Collection of subarguments and its parameters. Key - subargument name; Value - list of subargument parameters to parse;
			/// </summary>
			public Dictionary<string, List<string>> subArgs;
			/// <summary>
			/// List of parameters to parse by argument.
			/// </summary>
			public List<string> parameters;
		}
		internal struct CommandColors
		{
			/// <summary>
			/// Color of printed arguments.
			/// </summary>
			public readonly ConsoleColor Argument;
			/// <summary>
			/// Color of printed error messages.
			/// </summary>
			public readonly ConsoleColor Error;
			/// <summary>
			/// Color of printed header.
			/// </summary>
			public readonly ConsoleColor Header;
			/// <summary>
			/// Color of printed ordinary messages.
			/// </summary>
			public readonly ConsoleColor Text;

			public CommandColors(ConsoleColor _arg, ConsoleColor _err, ConsoleColor _head, ConsoleColor _txt)
			{
				Argument = _arg;
				Error = _err;
				Header = _head;
				Text = _txt;
			}
		}
		internal class CommandPrinter
		{
			#region Fields
			private string _cmd;
			private static CommandColors _clr = new CommandColors(ConsoleColor.Green, ConsoleColor.Red, ConsoleColor.Blue, ConsoleColor.Cyan);
			#endregion Fields

			#region Methods
			public CommandPrinter(string command)
			{
				_cmd = command;
			}
			private void Error(string msg)
			{
				Header();
				Console.ForegroundColor = _clr.Error;
				Console.WriteLine(msg);
			}
			private void Header()
			{
				Console.ForegroundColor = _clr.Header;
				Console.Write("[{0}]", _cmd);
			}
			public void BadSyntax(int pos)
			{
				Error("Command syntax corrupted at position " + pos + ".");
			}
			public void EmptyArg(int pos)
			{
				Error("Argument at position " + pos + "is empty.");
			}
			public void EmptyParamList(string argName)
			{
				Error("Empty parameter list for arg " + argName + " is not permitted.");
			}
			public void EmptyParamList(string argName, string subArgName)
			{
				Error("Empty parameter list for arg " + argName + " " + subArgName + " is not permitted.");
			}
			public void ParamChanged(string param, string oldValue, string newValue)
			{
				Header();
				Console.ForegroundColor = _clr.Text;
				Console.Write(param + ": ");
				Console.ForegroundColor = _clr.Argument;
				Console.Write(oldValue);
				Console.ForegroundColor = _clr.Text;
				Console.Write(" => ");
				Console.ForegroundColor = _clr.Argument;
				Console.Write(newValue);
				Console.ForegroundColor = _clr.Text;
				Console.WriteLine(" ;");
			}
			public void UnrecognisedArg(string argName, int pos)
			{
				Error("Unrecognized argument '" + argName + "' at position " + pos + ".");
			}
			public void UnrecognisedSubArg(string argName, string subArgName)
			{
				Error("Argument '" + argName + "' does not have subarg with name '" + subArgName + "'. Type 'help " + _cmd + " " + argName + "' for correct syntax.");
			}
			public void UnrecognisedParam(string argName)
			{
				Error("Param for arg '" + argName + "' is not valid in current context.");
			}
			public void UnrecognisedParam(string argName, string subArgName)
			{
				Error("Param for arg '" + argName + " " + subArgName + "' is not valid in current context.");
			}
			#endregion Methods
		}
		#endregion Nested Types

		#region Fields
		internal CommandPrinter p;
		/// <summary>
		/// Container for main args methods. Key - argument name; Value - Argument callback;
		/// </summary>
		private Dictionary<string, ArgCall> _main = new Dictionary<string, ArgCall>();
		/// <summary>
		/// Container for arguments aliases. Key - alias; Value - argument name;
		/// </summary>
		private Dictionary<string, string> _aliases = new Dictionary<string, string>();
		/// <summary>
		/// Container for help strings. Key - argument name; Value - help string;
		/// </summary>
		private Dictionary<string, string> _help = new Dictionary<string, string>();
		Wintellect.PowerCollections.MultiDictionary<string, string> _subs = new Wintellect.PowerCollections.MultiDictionary<string,string>(false);
		#endregion Fields

		#region ICommand
		virtual public void OnInvoke(CommandEvent e)
		{
			if (e.args.Length <= 1)
				OnHelp();
			else
			{
				for (int i = 1; i < e.args.Length; )
					OnInvokeParseArg(ref e, ref i);
			}
		}
		virtual public string OnTab(CommandEvent e)
		{
			string last = e.args[e.args.Length - 1];
			if (e.args.Length < 1 || last[0] != '-' || _main.Count == 0)
				return null;
			if (last == "-")
			{
				e.TabCounter %= _main.Count;
				Dictionary<string, ArgCall>.KeyCollection.Enumerator it = _main.Keys.GetEnumerator();
				for (int i = 0; i <= e.TabCounter; i++)
					it.MoveNext();
				return it.Current;
			}
			string arg = OnTabGetLastArg(ref e);
			if (arg != null)
			{
				List<string> list = new List<string>();
				if (last == arg)
				{
					foreach (KeyValuePair<string, ArgCall> pair in _main)
						if (pair.Key.StartsWith(last))
							list.Add(pair.Key);
				}
				else
				{
					if (_aliases.ContainsKey(arg))
						arg = _aliases[arg];
					if (_main.ContainsKey(arg))
					{
						foreach (string str in _subs[arg])
							if (str.StartsWith(last))
								list.Add(str);
					}
					else
						return null;
				}
				if (list.Count == 0)
					return null;
				e.TabCounter %= list.Count;
				return list[e.TabCounter];
			}
			else
				return null;
			throw new NotImplementedException();
		}
		#endregion ICommand

		#region Methods
		public Command(string name)
		{
			p = new CommandPrinter(name);
			RegisterArgs();
		}
		protected void Dummy(ArgEvent e)
		{
		}
		/// <summary>
		/// Parse str as bool value, update param and return previous param value.
		/// </summary>
		/// <param name="str">String to parse.</param>
		/// <param name="param">Reference to bool value to update with str content. If parsing fails, this param is not changed.</param>
		/// <returns>Null in case of parse fail, otherwise value of param before update.</returns>
		/// <remarks>Can parse 'on' and 'off' strings as (respectively) true and false.</remarks>
		protected bool? ParseBool(string str, ref bool param)
		{
			bool? prev = param;
			if (str == "on" || str == bool.TrueString)
				param = true;
			else if (str == "off" || str == bool.FalseString)
				param = false;
			else
				prev = null;
			return prev;
		}
		/// <summary>
		/// Parse str as float value, update param and return previous param value.
		/// </summary>
		/// <param name="str">String to parse.</param>
		/// <param name="value">Reference to float value to update with str content. If parsing fails, this param is not changed.</param>
		/// <returns>Null in case of parse fail, otherwise value of param before update.</returns>
		/// <remarks>If the first character of str is '+' or '-', the value of str will be added/substracted to/from param, otherwise value of param will be exactly as in string.</remarks>
		protected float? ParseFloat(string str, ref float param)
		{
			float? prev = param;
			if (!float.TryParse(str, out param))
			{
				param = prev.Value;
				prev = null;
			}
			return prev;
		}
		/// <summary>
		/// Parse str as int value, update param and return previous param value.
		/// </summary>
		/// <param name="str">String to parse.</param>
		/// <param name="param">Reference to int value to update with str content. If parsing fails, this param is not changed.</param>
		/// <returns>Null in case of parse fail, otherwise value of param before update.</returns>
		/// <remarks>If the first character of str is '+' or '-', the value of str will be added/substracted to/from param, otherwise value of param will be exactly as in string.</remarks>
		protected int? ParseInt(string str, ref int param)
		{
			int? prev = param;
			if (!int.TryParse(str, out param))
			{
				param = prev.Value;
				prev = null;
			}
			return prev;
		}
		/// <summary>
		/// Parse list as Mogre::Vector2 value, update param and return previous param value.
		/// </summary>
		/// <param name="list">List of strings with float values to parse. If list lenght is less than 2 parse fails. If list lenght is greater than 2, additional strings will be ignored.</param>
		/// <param name="param">Reference to Mogre::Vector2 value to update with list content. If parsing fails, this param is not changed.</param>
		/// <returns>Null in case of parse fail, otherwise value of param before update.</returns>
		/// <remarks>If the first character of string is '+' or '-', the value of this field will be added/substracted to/from param, otherwise value of param field will be exactly as in string.</remarks>
		protected Mogre.Vector2? ParseVector2(List<string> list, ref Mogre.Vector2 param)
		{
			if (list.Count < 2)
				return null;
			Mogre.Vector2 prev = param;
			if (ParseFloat(list[0], ref param.x) != null)
			{
				if (ParseFloat(list[1], ref param.y) != null)
					return prev;
			}
			param = prev;
			return null;
		}
		/// <summary>
		/// Parse list as Mogre::Vector3 value, update param and return previous param value.
		/// </summary>
		/// <param name="list">List of strings with float values to parse. If list lenght is less than 3 parse fails. If list lenght is greater than 3, additional strings will be ignored.</param>
		/// <param name="param">Reference to Mogre::Vector3 value to update with list content. If parsing fails, this param is not changed.</param>
		/// <returns>Null in case of parse fail, otherwise value of param before update.</returns>
		/// <remarks>If the first character of string is '+' or '-', the value of this field will be added/substracted to/from param, otherwise value of param field will be exactly as in string.</remarks>
		protected Mogre.Vector3? ParseVector3(List<string> list, ref Mogre.Vector3 value)
		{
			if (list.Count < 3)
				return null;
			Mogre.Vector3 prev = value;
			if (ParseFloat(list[0], ref value.x) != null)
			{
				if (ParseFloat(list[1], ref value.y) != null)
				{
					if (ParseFloat(list[2], ref value.z) != null)
						return prev;
				}
			}
			value = prev;
			return null;
		}
		/// <summary>
		/// Parse list as Mogre::Vector4 value, update param and return previous param value.
		/// </summary>
		/// <param name="list">List of strings with float values to parse. If list lenght is less than 4 parse fails. If list lenght is greater than 4, additional strings will be ignored.</param>
		/// <param name="param">Reference to Mogre::Vector4 value to update with list content. If parsing fails, this param is not changed.</param>
		/// <returns>Null in case of parse fail, otherwise value of param before update.</returns>
		/// <remarks>If the first character of string is '+' or '-', the value of this field will be added/substracted to/from param, otherwise value of param field will be exactly as in string.</remarks>
		protected Mogre.Vector4? ParseVector4(List<string> list, ref Mogre.Vector4 value)
		{
			Mogre.Vector4 prev = value;
			if (ParseFloat(list[0], ref value.x) != null)
			{
				if (ParseFloat(list[1], ref value.y) != null)
				{
					if (ParseFloat(list[2], ref value.z) != null)
					{
						if (ParseFloat(list[3], ref value.w) != null)
							return prev;
					}
				}
			}
			value = prev;
			return null;
		}
		protected void RegisterAlias(string argName, string aliasName)
		{
			if (!_main.ContainsKey("-" + argName))
				throw new ArgumentException("Argument '-" + argName + "' does not exist.", "argName");
			_aliases.Add("-" + aliasName, "-" + argName);
		}
		protected void RegisterArg(string argName, ArgCall call)
		{
			RegisterArg(argName, call, null);
		}
		protected void RegisterArg(string argName, ArgCall call, string helpString)
		{
			if (call == null)
				throw new ArgumentNullException("Argument callback can not be null.");
			if (argName.StartsWith("-"))
				throw new ArgumentException("Argument name can not starts with '-'.", "argName");
			_main.Add("-" + argName, call);
			_help.Add("-" + argName, helpString);
		}
		abstract protected void RegisterArgs();
		protected void RegisterSubArg(string argName, string subArgName)
		{
			if (subArgName == null)
				throw new ArgumentNullException("Can not register subarg  without a name.");
			if (subArgName.StartsWith("-"))
				throw new ArgumentException("Subargument name can not starts with '-'.", "subArgName");
			argName = "-" + argName;
			if (_aliases.ContainsKey(argName))
				argName = _aliases[argName];
			if (_main.ContainsKey(argName))
				_subs.Add(argName, "--" + subArgName);
			else
				throw new ArgumentException("Argument with name " + argName + "does not exist.", "argName");
		}
		private void OnHelp()
		{
		}
		private void OnInvokeParseArg(ref CommandEvent e, ref int i)
		{
			if (i < e.args.Length)
			{
				if (e.args[i].Length >= 2 && e.args[i][0] == '-' && e.args[i][1] != '-')
				{
					string arg = e.args[i];
					if (_aliases.ContainsKey(arg))
						arg = _aliases[arg];
					if (_main.ContainsKey(arg))
					{
						ArgEvent evt = new ArgEvent();
						evt.subArgs = new Dictionary<string, List<string>>();
						evt.parameters = null;
						evt.name = arg.Substring(1);
						++i;
						if (i < e.args.Length && e.args[i][0] != '-')
						{
							evt.parameters = new List<string>();
							do
							{
								evt.parameters.Add(e.args[i]);
								++i;
							} while (i < e.args.Length && e.args[i][0] != '-');
						}
						while (i < e.args.Length && e.args[i].StartsWith("--"))
							OnInvokeParseSubArg(ref e, ref i, ref evt, arg);
						_main[arg](evt);
					}
					else
					{
						p.UnrecognisedArg(arg, i);
						++i;
					}
				}
				else
				{
					p.BadSyntax(i);
					++i;
				}
			}
		}
		private void OnInvokeParseSubArg(ref CommandEvent e, ref int i, ref ArgEvent evt, string argName)
		{
			if (i < e.args.Length)
			{
				if (e.args[i].StartsWith("--"))
				{
					if (e.args[i].Length >= 3)
					{
						if (_subs[argName].Contains(e.args[i]))
						{
							string sub = e.args[i];
							List<string> list = new List<string>();
							++i;
							while (i < e.args.Length && e.args[i][0] != '-')
							{
								list.Add(e.args[i]);
								++i;
							}
							evt.subArgs.Add(sub.Substring(2), list);
						}
						else
						{
							p.UnrecognisedSubArg(argName, e.args[i]);
							++i;
						}
					}
					else
					{
						p.EmptyArg(i);
						++i;
					}
				}
				else
				{
					p.BadSyntax(i);
					++i;
				}
			}
		}
		private string OnTabGetLastArg(ref CommandEvent e)
		{
			for (int i = e.args.Length - 1; i > 0; i--)
				if (e.args[i] == "-")
					return null;
				else if (e.args[i].Length >= 2 && e.args[i][0] == '-' && e.args[i][1] != '-')
					return e.args[i];
			return null;
		}
		#endregion Methods
	}
}
