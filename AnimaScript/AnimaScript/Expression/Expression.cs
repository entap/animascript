using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Entap.AnimaScript.Expression
{
	public class Expression
	{
		List<object> _tokens;

		/// <summary>
		/// <see cref="T:Entap.AnimaScript.Expression.Expression"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="expression">数式</param>
		public Expression(string expression)
		{
			SetExpression(expression);
		}

		/// <summary>
		/// 数式を設定する。
		/// </summary>
		/// <param name="expression">数式</param>
		public void SetExpression(string expression)
		{
			_tokens = Lexer.ReadAll(expression);
		}
	}
}
