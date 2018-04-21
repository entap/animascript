using Entap.Expr;

namespace Entap.AnimaScript
{
	internal class StandardModule
	{
		/// <summary>
		/// 指定したスクリプト・ラベルにジャンプする。
		/// </summary>
		/// <param name="context">実行状態</param>
		/// <param name="command">命令</param>
		public void Jump(Context context, Command command)
		{
			var label = command.GetParameter<string>("to", null);
			var script = command.GetParameter<string>("script", null);
			if (label == null && script == null) {
				throw new AnimaScriptException("Parameter 'to' or 'script' is required", command);
			}
			context.Jump(script, label, command);
		}

		/// <summary>
		/// サブルーチンを呼び出す。
		/// </summary>
		/// <param name="context">実行状態</param>
		/// <param name="command">命令</param>
		public void Call(Context context, Command command)
		{
			var label = command.GetParameter<string>("to", null);
			var script = command.GetParameter<string>("script", null);
			if (label == null && script == null) {
				throw new AnimaScriptException("Parameter 'to' or 'script' is required", command);
			}
			context.Call(script, label, command);
		}

		/// <summary>
		/// サブルーチンから戻る。
		/// </summary>
		/// <param name="context">実行状態</param>
		/// <param name="command">命令</param>
		public void Return(Context context, Command command)
		{
			context.Return(command);
		}

		/// <summary>
		/// If文を実行する。
		/// </summary>
		/// <param name="context">実行状態</param>
		/// <param name="command">命令</param>
		public void If(Context context, Command command)
		{
			var exp = command.GetParameter<string>("exp");
			if (!Expression.Evaluate<bool>(exp, context.Variables)) {
				context.Jump(null, command.GetParameter<string>("*next"));
			}
		}

		/// <summary>
		/// Elif文を実行する。
		/// </summary>
		/// <param name="context">実行状態</param>
		/// <param name="command">命令</param>
		public void Elif(Context context, Command command)
		{
			If(context, command);
		}

		/// <summary>
		/// Set文を実行する。
		/// </summary>
		/// <param name="context">実行状態</param>
		/// <param name="command">命令</param>
		public void Set(Context context, Command command)
		{
			var var = command.GetParameter<string>("var");
			var exp = command.GetParameter<string>("exp");
			context.Variables[var] = Expression.Evaluate<object>(exp, context.Variables);
		}
	}
}
