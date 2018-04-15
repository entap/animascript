using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Entap.AnimaScript
{
	/// <summary>
	/// AnimaScriptのスクリプトを字句解析する。
	/// </summary>
	public class Lexer
	{
		TextReader _reader;
		Stack<char> _buffer;
		int _lineNumber;

		/// <summary>
		/// 読み込み中の行番号を取得する。
		/// </summary>
		/// <value>読み込み中の行番号</value>
		public int LineNumber {
			get {
				return _lineNumber;
			}
		}

		/// <summary>
		/// <see cref="T:Entap.AnimaScript.Lexer"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="reader">入力</param>
		public Lexer(TextReader reader)
		{
			_reader = reader;
			_buffer = new Stack<char>();
			_lineNumber = 0;
		}

		/// <summary>
		/// AnimaScriptのスクリプトを字句解析する。
		/// </summary>
		/// <returns>命令の配列</returns>
		/// <param name="reader">入力</param>
		public static List<Command> ReadAll(TextReader reader)
		{
			return (new Lexer(reader)).ReadAll();
		}

		/// <summary>
		/// 命令を全て読み込む。
		/// </summary>
		/// <returns>命令の配列</returns>
		public List<Command> ReadAll()
		{
			var commands = new List<Command>();
			for (var command = Read(); command != null; command = Read()) {
				commands.Add(command);
			}
			return commands;
		}

		/// <summary>
		/// 命令を読み込む。
		/// </summary>
		/// <returns>タグ</returns>
		public Command Read()
		{
			SkipSeparator();
			var c = PeekChar();
			switch (c) {
				case -1:
					return null; // 終端
				case '[':
					return ReadTag();
				case '*':
					return ReadLabel();
				default:
					return ReadMessage();
			}
		}

		/// <summary>
		/// 現在位置から文字を読み込む。
		/// </summary>
		/// <returns>文字</returns>
		int PeekChar()
		{
			return _buffer.Count > 0 ? _buffer.Peek() : _reader.Peek();
		}

		/// <summary>
		/// 読み込み位置を進める。
		/// </summary>
		void NextChar()
		{
			var c = _buffer.Count > 0 ? _buffer.Pop() : _reader.Read();
			if (c == '\n') {
				_lineNumber++;
			}
		}

		/// <summary>
		/// 読み込み位置を戻す。
		/// </summary>
		void BackChar(int c)
		{
			_buffer.Push((char)c);
			if (c == '\n') {
				_lineNumber--;
			}
		}

		/// <summary>
		/// 条件が成立する間、文字を読み込む。
		/// </summary>
		/// <returns>読み込んだ文字列</returns>
		/// <param name="predicate">条件</param>
		StringBuilder ReadWhile(Predicate<char> predicate)
		{
			var s = new StringBuilder();
			var c = PeekChar();
			while (c != -1 && predicate((char)c)) {
				s.Append((char)c);
				NextChar();
				c = PeekChar();
			}
			return s;
		}

		/// <summary>
		/// 現在の読み込み位置から、命令の区切り（空白・コメント）を読み飛ばす。
		/// </summary>
		void SkipSeparator()
		{
			while (SkipWhiteSpace() || SkipComment()) { }
		}

		/// <summary>
		/// 空白を読み飛ばす。
		/// </summary>
		/// <returns>文字を読み飛ばしたら<c>true</c>、そうでない場合<c>false</c></returns>
		bool SkipWhiteSpace()
		{
			return ReadWhile(char.IsWhiteSpace).Length > 0;
		}

		/// <summary>
		/// コメントを読み飛ばす。
		/// </summary>
		/// <returns>文字を読み飛ばしたら<c>true</c>、そうでない場合<c>false</c></returns>
		bool SkipComment()
		{
			var c1 = PeekChar();
			if (c1 == '/') {
				NextChar();
				var c2 = PeekChar();
				if (c2 == '/') {
					// 単一行コメント
					ReadWhile(c => c != '\n');
					return true;
				}
				if (c2 == '*') {
					// 複数行コメント
					NextChar();
					while (true) {
						c1 = PeekChar();
						NextChar();
						if (c1 == -1) {
							throw new AnimaScriptException("コメントが閉じていません", LineNumber);
						}
						if (c1 == '*') {
							if (PeekChar() == '/') {
								NextChar();
								return true; // コメント閉じ
							}
						}
					}
				}
				// スラッシュ文字'/'
				BackChar(c1);
			} else if (c1 == ';') {
				// 単一行コメント
				ReadWhile(c => c != '\n');
			}
			return false;
		}

		/// <summary>
		/// 単語の構成文字として有効か？
		/// </summary>
		/// <returns>単語の構成文字として有効なら<c>true</c>, そうでないなら<c>false</c></returns>
		/// <param name="c">文字</param>
		static bool isWordChar(char c)
		{
			return !(c == '[' || c == ']' || c == '/' || c == ';' || c == '=' || char.IsWhiteSpace(c));
		}

		/// <summary>
		/// 単語を読み込む。
		/// </summary>
		/// <returns>読み込んだ文字列</returns>
		StringBuilder ReadWord()
		{
			var q = PeekChar();
			if (q == '\'' || q == '\"') {
				NextChar();
				var s = ReadWhile(c => c != q && c != '\n');
				if (PeekChar() == -1) {
					throw new AnimaScriptException("引用符が足りません", LineNumber);
				}
				NextChar();
				return s;
			}
			return ReadWhile(isWordChar);
		}

		/// <summary>
		/// タグを読み込む。
		/// </summary>
		/// <returns>読み込んだタグ</returns>
		Command ReadTag()
		{
			var command = new Command(LineNumber);
			NextChar(); // skip [
			SkipWhiteSpace();
			command.Name = ReadWord().ToString();
			while (true) {
				SkipWhiteSpace();
				var c = PeekChar();
				if (c == '=') {
					throw new AnimaScriptException("期待しない文字 '='", LineNumber);
				}
				if (c == -1) {
					throw new AnimaScriptException("タグが閉じていません", LineNumber);
				}
				if (c == ']') {
					NextChar();
					return command;
				}
				var paramName = ReadWord().ToString();
				string paramValue;
				if (PeekChar() == '=') {
					NextChar();
					SkipWhiteSpace();
					paramValue = ReadWord().ToString();
				} else {
					paramValue = paramName;
				}
				command.AddParameter(paramName, paramValue);
			}
		}

		/// <summary>
		/// ラベルを読み込む。
		/// </summary>
		/// <returns>読み込んだタグ</returns>
		Command ReadLabel()
		{
			var command = new Command(LineNumber);
			NextChar();
			SkipWhiteSpace();
			var labelName = ReadWord().ToString();
			if (labelName.Length == 0) {
				throw new AnimaScriptException("ラベルが不正", LineNumber);
			}
			command.Name = "label";
			command.AddParameter("name", labelName);
			return command;
		}

		/// <summary>
		/// メッセージを読み込む。
		/// </summary>
		/// <returns>読み込んだタグ</returns>
		Command ReadMessage()
		{
			var command = new Command(LineNumber);
			var messageText = new StringBuilder();
			while (true) {
				var c = PeekChar();
				if (c == -1 || c == '[' || c == ';' || c == '*' || c == '\n') {
					break; // 区切り文字・改行・ファイル終了のいずれか
				}
				if (c == ']') {
					throw new AnimaScriptException("メッセージ中に']'は直接記述できません", LineNumber);
				}
				if (c == '/') {
					NextChar();
					var c2 = PeekChar();
					if (c2 == '/' || c2 == '*') {
						BackChar(c);
						break;
					}
					messageText.Append((char)c);
				} else {
					if (!char.IsWhiteSpace((char)c)) {
						messageText.Append((char)c);
					}
					NextChar();
				}
			}
			command.Name = "message";
			command.AddParameter("text", messageText.ToString());
			return command;
		}
	}
}
