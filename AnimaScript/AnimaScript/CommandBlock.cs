using System;
using System.Collections.Generic;
using System.IO;

namespace Entap.AnimaScript
{
	using CommandList = List<Command>;
	using LabelDictionary = Dictionary<string, int>;

	/// <summary>
	/// 命令ブロック
	/// </summary>
	public class CommandBlock
	{
		CommandList _commands;
		LabelDictionary _labels;

		/// <summary>
		/// 空の命令ブロック
		/// </summary>
		public static CommandBlock Empty = new CommandBlock();

		/// <summary>
		/// <see cref="T:Entap.AnimaScript.CommandBlock"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="script">スクリプト</param>
		public CommandBlock(string script = "")
			: this(new StringReader(script))
		{
		}

		/// <summary>
		/// <see cref="T:Entap.AnimaScript.ScriptBlock"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="reader">文字の入力元</param>
		public CommandBlock(TextReader reader)
		{
			SetScript(reader);
		}

		/// <summary>
		/// スクリプト文を設定する。
		/// </summary>
		/// <param name="reader">入力</param>
		public void SetScript(TextReader reader)
		{
			_commands = Lexer.ReadAll(reader);
			TranslateIfStatement(_commands, out _commands);
			InitLabels(_commands, out _commands, out _labels);
		}

		/// <summary>
		/// if文にjump文とラベルを加えた形式に変換する。
		/// ・if文・elif文に、条件が成立しなかった際の移動先を'*next'引数として加える。
		/// ・else文・elif文の前に、endif文へのjump文を加える。
		/// </summary>
		/// <param name="src">トークン配列の入力変数</param>
		/// <param name="dest">トークン配列の出力変数</param>
		static void TranslateIfStatement(CommandList src, out CommandList dest)
		{
			var unique = 0;
			var uStack = new Stack<int>();
			var eStack = new Stack<int>();
			dest = new CommandList();
			foreach (var command in src) {
				int u, e;
				switch (command.Name) {
					case "if":
						// if文
						uStack.Push(unique);
						eStack.Push(0);
						command.AddParameter("*next", ElifLabel(unique, 0));
						dest.Add(command);
						unique++;
						break;
					case "elif":
					case "else":
						// elif文かelse文
						if (uStack.Count == 0) {
							throw new AnimaScriptException("if文が足りません", command.LineNumber);
						}
						u = uStack.Peek();
						e = eStack.Pop();
						if (e == -1) {
							throw new AnimaScriptException("Endif文が足りません", command.LineNumber);
						}
						dest.Add(new Command(command.LineNumber, "jump", "target", EndIfLabel(u)));
						dest.Add(new Command(command.LineNumber, "label", "name", ElifLabel(u, e)));
						if (command.Name == "elif") {
							command.AddParameter("*next", ElifLabel(u, e + 1));
							dest.Add(command);
							eStack.Push(e + 1);
						} else {
							eStack.Push(-1);
						}
						break;
					case "endif":
						// endif文
						if (uStack.Count == 0) {
							throw new AnimaScriptException("if文が足りません", command.LineNumber);
						}
						u = uStack.Peek();
						e = eStack.Peek();
						if (e != -1) {
							dest.Add(new Command(command.LineNumber, "label", "name", ElifLabel(u, e)));
						}
						dest.Add(new Command(command.LineNumber, "label", "name", EndIfLabel(u)));
						uStack.Pop();
						eStack.Pop();
						break;
					default:
						dest.Add(command);
						break;
				}
			}
			if (uStack.Count > 0) {
				throw new AnimaScriptException("endif文が足りません");
			}
		}

		/// <summary>
		/// endifのラベルを生成する。
		/// </summary>
		/// <returns>ラベルの文字列</returns>
		/// <param name="u">ifに固有の値</param>
		static string EndIfLabel(int u)
		{
			return "=if" + u + "=end";
		}

		/// <summary>
		/// elifのラベルを生成する。
		/// </summary>
		/// <returns>ラベルの文字列</returns>
		/// <param name="u">ifに固有の値</param>
		/// <param name="e">elifに固有の値</param>
		static string ElifLabel(int u, int e)
		{
			return "=if" + u + "=" + e;
		}

		/// <summary>
		/// ラベルの辞書を生成する。その際、ラベル命令を除去する。
		/// </summary>
		/// <param name="src">トークン配列の入力変数</param>
		/// <param name="dest">トークン配列の出力変数</param>
		/// <param name="labels">ラベル辞書の出力変数</param>
		static void InitLabels(CommandList src, out CommandList dest, out LabelDictionary labels)
		{
			labels = new LabelDictionary();
			dest = new CommandList();
			foreach (var command in src) {
				if (command.Name == "label") {
					var labelName = command.GetParameter<string>("name");
					if (labels.ContainsKey(labelName)) {
						throw new AnimaScriptException("ラベル " + labelName + " が重複しています", command.LineNumber);
					}
					labels.Add(labelName, dest.Count);
				} else {
					dest.Add(command);
				}
			}
		}

		/// <summary>
		/// ラベルのアドレスを取得する。
		/// </summary>
		/// <returns>ラベルのアドレス。ラベルがない場合には-1</returns>
		/// <param name="labelName">ラベルの名前</param>
		public int LookUpLabel(string labelName)
		{
			return _labels.GetValueOrDefault(labelName, -1);
		}

		/// <summary>
		/// 指定したアドレスの命令を取得する。
		/// </summary>
		/// <returns>命令</returns>
		/// <param name="address">アドレス</param>
		public Command GetCommand(int address)
		{
			return 0 <= address && address < _commands.Count ? _commands[address] : null;
		}
	}
}
