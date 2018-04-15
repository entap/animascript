using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Entap.AnimaScript
{
	/// <summary>
	/// AnimaScriptの命令
	/// </summary>
	public class Command
	{
		/// <summary>
		/// ソースコード内の行番号
		/// </summary>
		public int LineNumber;

		/// <summary>
		/// 命令の名前
		/// </summary>
		public string Name;

		/// <summary>
		/// 命令のパラメータ
		/// </summary>
		public Dictionary<string, string> Parameters;

		/// <summary>
		/// <see cref="T:Entap.AnimaScript.Command"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="lineNumber">行番号</param>
		/// <param name="name">命令の名前</param>
		/// <param name="params_">命令のパラメータ</param>
		public Command(int lineNumber = 0, string name = "", Dictionary<string, string> params_ = null)
		{
			LineNumber = lineNumber;
			Name = name;
			Parameters = params_ ?? new Dictionary<string, string>();
		}

		/// <summary>
		/// <see cref="T:Entap.AnimaScript.ScriptTag"/> クラスのインスタンスを初期化する。
		/// </summary>
		/// <param name="lineNumber">行番号</param>
		/// <param name="name">命令の名前</param>
		/// <param name="parameters">命令のパラメータ</param>
		public Command(int lineNumber = 0, string name = "", params string[] parameters)
		{
			LineNumber = lineNumber;
			Name = name;
			Parameters = new Dictionary<string, string>();
			for (int i = 0; i < parameters.Length - 1; i++) {
				Parameters.Add(parameters[i], parameters[i + 1]);
			}
		}

		/// <summary>
		/// パラメータを追加する。
		/// </summary>
		/// <param name="name">名前</param>
		/// <param name="value">値</param>
		public void AddParameter(string name, string value)
		{
			Parameters.Add(name, value);
		}

		/// <summary>
		/// 入力必須のパラメータを取得する。
		/// </summary>
		/// <returns>パラメータ</returns>
		/// <param name="name">名前</param>
		public T GetParameter<T>(string name)
		{
			if (!Parameters.ContainsKey(name)) {
				throw new AnimaScriptException("パラメータ " + name + " は必須です", LineNumber);
			}
			try {
				return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(Parameters[name]);
			} catch (Exception) {
				throw new AnimaScriptException("パラメータ " + name + " の形式が不正です", LineNumber);
			}
		}

		/// <summary>
		/// パラメータを取得する。指定されていない場合にはデフォルト値を返す。
		/// </summary>
		/// <returns>パラメータ</returns>
		/// <param name="name">名前</param>
		/// <param name="defaultValue">デフォルト値</param>
		public T GetParameter<T>(string name, T defaultValue)
		{
			return Parameters.ContainsKey(name) ? GetParameter<T>(name) : defaultValue;
		}
	}
}
