using System;
using System.IO;

namespace Entap.AnimaScript
{
	/// <summary>
	/// AnimaScriptのスクリプトを字句解析する。
	/// </summary>
	public class ScriptParser
	{
		/// <summary>
		/// スクリプトを解析する。
		/// </summary>
		/// <returns>The parse.</returns>
		/// <param name="reader">Reader.</param>
		public void Parse(TextReader reader)
		{
			var tokens = ScriptTokenizer.Tokenize(reader);
		}
	}
}
