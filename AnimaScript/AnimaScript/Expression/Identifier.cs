using System;
namespace Entap.AnimaScript.Expression
{
	/// <summary>
	/// 数式の中の識別子
	/// </summary>
	public class Identifier
	{
		public string Name;

		/// <summary>
		/// <see cref="T:Entap.AnimaScript.Expression.Identifier"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="name">識別子の名前</param>
		public Identifier(string name)
		{
			Name = name;
		}

		/// <summary>
		/// 識別子の先頭の文字か？
		/// </summary>
		/// <returns>識別子の先頭の文字なら<c>true</c>、そうでなければ<c>false</c></returns>
		/// <param name="c">文字</param>
		public static bool IsHeadChar(char c)
		{
			return ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || c == '_' || c == '$';
		}

		/// <summary>
		/// 識別子の構成文字か？
		/// </summary>
		/// <returns>識別子の構成文字なら<c>true</c>、そうでなければ<c>false</c></returns>
		/// <param name="c">文字</param>
		public static bool IsPartChar(char c)
		{
			return IsHeadChar(c) || ('0' <= c && c <= '9');
		}
	}
}
