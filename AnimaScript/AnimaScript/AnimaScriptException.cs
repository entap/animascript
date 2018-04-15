using System;
namespace Entap.AnimaScript
{
	[System.Serializable]
	public class AnimaScriptException : Exception
	{
		/// <summary>
		/// エラーが発生した行番号
		/// </summary>
		public int LineNumber;

		/// <summary>
		/// <see cref="T:SyntaxErrorException"/> クラスを初期化する
		/// </summary>
		/// <param name="message">エラー内容を説明する文字列</param>
		/// <param name="lineNumber">行番号</param>
		public AnimaScriptException(string message, int lineNumber = 0) : base(message)
		{
			LineNumber = lineNumber;
		}

		/// <summary>
		/// <see cref="T:SyntaxErrorException"/> クラスを初期化する
		/// </summary>
		/// <param name="message">エラー内容を説明する文字列</param>
		/// <param name="command">エラーの原因となった命令</param>
		public AnimaScriptException(string message, Command command) : base(message)
		{
			LineNumber = command.LineNumber;
		}
	}
}
