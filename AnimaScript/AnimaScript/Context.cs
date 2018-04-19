using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Entap.AnimaScript
{
	/// <summary>
	/// 外部スクリプトを読み込むデリゲート
	/// </summary>
	public delegate TextReader LoaderDelegate(string name);

	/// <summary>
	/// コマンドを実行するデリゲート
	/// </summary>
	public delegate void CommandFuncDelegate(Context context, Command command);

	/// <summary>
	/// AnimaScriptの実行コンテキスト
	/// </summary>
	public class Context
	{
		Dictionary<string, CommandBlock> _scripts;
		Dictionary<string, object> _vars;
		Dictionary<string, CommandFuncDelegate> _funcs;
		Stack<CommandBlock> _blockStack;
		Stack<int> _addressStack;
		bool _yield;

		/// <summary>
		/// コールスタックの最大値
		/// </summary>
		const int CallStackMax = 4096;

		/// <summary>
		/// 外部スクリプトを読み込むデリゲート
		/// </summary>
		public LoaderDelegate Loader;

		/// <summary>
		/// 変数の辞書を取得する。
		/// </summary>
		/// <value>変数の辞書</value>
		public Dictionary<string, object> Variables {
			get => _vars;
		}

		/// <summary>
		/// <see cref="T:Entap.AnimaScript.Context"/> クラスのインスタンスを初期化する。
		/// </summary>
		public Context()
		{
			_scripts = new Dictionary<string, CommandBlock>();
			_vars = new Dictionary<string, object>();
			_funcs = new Dictionary<string, CommandFuncDelegate>();
			Clear();
			LoadModule(new StandardModule());
		}

		/// <summary>
		/// スタックを初期化する。
		/// </summary>
		public void Clear()
		{
			_blockStack = new Stack<CommandBlock>();
			_addressStack = new Stack<int>();
			_blockStack.Push(CommandBlock.Empty);
			_addressStack.Push(0);
		}

		/// <summary>
		/// 実行するスクリプトを文字列で設定する。
		/// </summary>
		/// <param name="script">スクリプトの文字列</param>
		/// <param name="label">ラベル。先頭から実行するならnull</param>
		public void SetScript(string script, string label = null)
		{
			_scripts[""] = new CommandBlock(script);
			Jump("", label, null);
		}

		/// <summary>
		/// 指定したスクリプト・実行アドレスにジャンプする。
		/// </summary>
		/// <param name="scriptName">スクリプト名。実行中のスクリプトならnull</param>
		/// <param name="label">ラベル。先頭ならnull</param>
		/// <param name="command">このジャンプを呼び出した命令。指定しないならnull</param>
		public void Jump(string scriptName, string label, Command command = null)
		{
			// スクリプトの指定がある場合
			if (scriptName != null) {
				// 命令ブロックを取得する。
				CommandBlock block;
				if (_scripts.ContainsKey(scriptName)) {
					block = _scripts[scriptName];
				} else {
					block = new CommandBlock(Loader(scriptName));
					_scripts[scriptName] = block;
				}

				// 実行中の命令ブロックとアドレスを変更する。
				_blockStack.Pop();
				_blockStack.Push(block);
				_addressStack.Pop();
				_addressStack.Push(0);
			}

			// ラベルの指定がある場合
			if (label != null) {
				// 実行中のアドレスを変更する。
				var address = _blockStack.Peek().LookUpLabel(label);
				if (address != -1) {
					_addressStack.Pop();
					_addressStack.Push(address);
				} else {
					int lineNumber = command != null ? command.LineNumber : 0;
					throw new AnimaScriptException("Label not found: " + label, lineNumber);
				}
			}
		}

		/// <summary>
		/// サブルーチンを呼び出す。
		/// </summary>
		/// <param name="scriptName">サブルーチンのスクリプト名。実行中のスクリプトならnull</param>
		/// <param name="label">サブルーチンのラベル。先頭ならnull</param>
		/// <param name="command">このジャンプを呼び出した命令。指定しないならnull</param>
		public void Call(string scriptName, string label, Command command = null)
		{
			if (_blockStack.Count > CallStackMax) {
				throw new AnimaScriptException("Stack overflow", command);
			}
			_blockStack.Push(_blockStack.Peek());
			_addressStack.Push(0);
			Jump(scriptName, label, command);
		}

		/// <summary>
		/// Call文から元の実行位置に戻る。
		/// </summary>
		/// <param name="command">このジャンプを呼び出した命令(例外のヒントのために使用)。指定しないならnull</param>
		public void Return(Command command)
		{
			if (_blockStack.Count == 0) {
				throw new AnimaScriptException("Can't return outside a subroutine", command);
			}
			_blockStack.Pop();
			_addressStack.Pop();
		}

		/// <summary>
		/// 命令を実行する関数を登録する。
		/// </summary>
		/// <param name="name">命令の名前</param>
		/// <param name="func">命令の実行する関数</param>
		public void DefineFunction(string name, CommandFuncDelegate func)
		{
			_funcs[name.ToLower()] = func;
		}

		/// <summary>
		/// 命令を実行する関数を定義したモジュールを登録する。
		/// </summary>
		/// <param name="module">モジュール</param>
		public void LoadModule(object module)
		{
			var type = module.GetType();
			foreach (var mi in type.GetMethods()) {
				if (IsAnimaScriptMethod(mi)) {
					DefineFunction(mi.Name, (context, command) => {
						try {
							mi.Invoke(module, new object[] { context, command });
						} catch (TargetInvocationException e) {
							throw e.InnerException;
						}
					});
				}
			}
		}

		/// <summary>
		/// 指定されたメソッド情報がAnimaScriptのコマンド実行をできるか判定する
		/// </summary>
		/// <returns>AnimaScriptのコマンド実行をできるなら<c>true</c>、そうでないなら<c>false</c></returns>
		/// <param name="mi">Mi.</param>
		static bool IsAnimaScriptMethod(MethodInfo mi)
		{
			var parameters = mi.GetParameters();
			return
				parameters.Length == 2 &&
				parameters[0].ParameterType == typeof(Context) &&
				parameters[1].ParameterType == typeof(Command);

		}

		/// <summary>
		/// 次に実行する命令を取得し、実行位置を進める。
		/// </summary>
		/// <returns>次に実行する命令</returns>
		Command FetchCommand()
		{
			var address = _addressStack.Pop();
			var command = _blockStack.Peek().GetCommand(address);
			if (command != null) {
				address++;
			}
			_addressStack.Push(address);
			return command;
		}

		/// <summary>
		/// 命令を実行する。
		/// </summary>
		/// <param name="command">命令</param>
		void ExecuteCommand(Command command)
		{
			var commandName = command.Name.ToLower();
			if (_funcs.ContainsKey(commandName)) {
				_funcs[commandName].Invoke(this, command);
			} else {
				throw new AnimaScriptException("Command not found: " + command.Name, command.LineNumber);
			}
		}

		/// <summary>
		/// 命令の実行を一時停止して、Execute関数から抜け出す。
		/// </summary>
		public void Yield()
		{
			_yield = true;
		}

		/// <summary>
		/// スクリプトを実行する。
		/// </summary>
		public void Execute()
		{
			_yield = false;
			var command = FetchCommand();
			while (command != null && !_yield) {
				ExecuteCommand(command);
				command = FetchCommand();
			}
		}
	}
}
