using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Entap.AnimaScript.Expression
{
	/// <summary>
	/// 数式の字句解析
	/// </summary>
	public class Lexer
	{
		StringReader _reader;

		/// <summary>
		/// <see cref="T:Entap.AnimaScript.Expression.Lexer"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="reader">入力</param>
		public Lexer(StringReader reader)
		{
			_reader = reader;
		}

		/// <summary>
		/// 全てのトークンを読み込む。
		/// </summary>
		/// <param name="s">文字列</param>
		/// <returns>トークンの配列</returns>
		public static List<object> ReadAll(string s)
		{
			return (new Lexer(new StringReader(s))).ReadAll();
		}

		/// <summary>
		/// 全てのトークンを読み込む。
		/// </summary>
		/// <returns>トークンの配列</returns>
		public List<object> ReadAll()
		{
			var tokens = new List<object>();
			for (var token = ReadToken(); token != null; token = ReadToken()) {
				tokens.Add(token);
			}
			return tokens;
		}

		/// <summary>
		/// 条件が成立する間、文字を読み込む。
		/// </summary>
		/// <returns>読み込んだ文字列</returns>
		/// <param name="predicate">条件</param>
		string ReadWhile(Predicate<char> predicate)
		{
			var s = new StringBuilder();
			var c = _reader.Peek();
			while (c != -1 && predicate((char)c)) {
				s.Append((char)c);
				_reader.Read();
				c = _reader.Peek();
			}
			return s.ToString();
		}

		/// <summary>
		/// 文字列から字句を読み込む。
		/// </summary>
		/// <returns>トークン</returns>
		public object ReadToken()
		{
			ReadWhile(char.IsWhiteSpace);
			var c = _reader.Peek();
			if (c == -1) {
				return null;
			}
			if (c == '\'' || c == '"') {
				return ReadString();
			}
			if (char.IsDigit((char)c)) {
				return ReadNumber();
			}
			if (Identifier.IsHeadChar((char)c)) {
				return ReadIdentifier();
			}
			return null;
		}

		/// <summary>
		/// 文字列リテラルを読み込む。
		/// </summary>
		/// <returns>文字列</returns>
		string ReadString()
		{
			var q = (char)_reader.Peek();
			var s = ReadWhile(c => c != q && c != '\n');
			if (_reader.Read() != q) {
				throw new AnimaScriptException("引用符が閉じていません");
			}
			return s;
		}

		/// <summary>
		/// 数値リテラルを読み込む。
		/// </summary>
		/// <returns>数値</returns>
		object ReadNumber()
		{
			var s = ReadWhile(c => char.IsDigit(c) || c == '.');

			// 整数
			int intValue;
			if (int.TryParse(s, out intValue)) {
				return intValue;
			}

			// 実数
			float floatValue;
			if (float.TryParse(s, out floatValue)) {
				return floatValue;
			}

			// 整数でも実数でも認識できない
			throw new AnimaScriptException("数値の形式が不正です");
		}

		/// <summary>
		/// 識別子を読み込む。
		/// </summary>
		/// <returns>識別子</returns>
		Identifier ReadIdentifier()
		{
			var s = ReadWhile(Identifier.IsPartChar);
			return new Identifier(s);
		}
	}
}
