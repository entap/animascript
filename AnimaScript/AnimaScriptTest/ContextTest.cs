﻿using System;
using System.IO;
using NUnit.Framework;
using Entap.AnimaScript;

namespace Entap.AnimaScript.Test
{
	public class ContextTest
	{
		[Test]
		public void ExecuteTest()
		{
			int param = 0;
			var ctx = new Context();
			ctx.DefineFunction("testSet", (context, command) => {
				param = command.GetParameter<int>("param");
			});
			ctx.DefineFunction("testAdd", (context, command) => {
				param += command.GetParameter<int>("param");
			});
			ctx.SetScript("[testSet param=1234][testAdd param=1][testAdd param=2]", null);
			ctx.Execute();
			Assert.AreEqual(1237, param);
		}

		[Test]
		public void JumpTest()
		{
			int param = 0;
			var ctx = new Context();
			ctx.DefineFunction("testSet", (context, command) => {
				param = command.GetParameter<int>("param");
			});

			// ジャンプしたら[testSet param=2]は実行されないはず。
			ctx.SetScript("[testSet param=1][jump to=a][testSet param=2]*a");
			ctx.Execute();
			Assert.AreEqual(1, param);

			// パラメータがないとエラー
			ctx.SetScript("[jump]");
			Assert.Throws(typeof(AnimaScriptException), () => {
				ctx.Execute();
			});

			// 外部スクリプトの呼び出し
			param = 0;
			ctx.Loader = (name) => {
				return new StringReader("[testSet param=1234]");
			};
			ctx.SetScript("[jump script=abc]");
			ctx.Execute();
			Assert.AreEqual(1234, param);
		}

		[Test]
		public void CallTest()
		{
			int param = 0;
			var ctx = new Context();
			ctx.DefineFunction("testSet", (context, command) => {
				param = command.GetParameter<int>("param");
			});

			// コールして、[testSet param=1]を呼び出す。
			ctx.SetScript("[jump to=a]*b[testSet param=1][return]*a[testSet param=2][call to=b]");
			ctx.Execute();
			Assert.AreEqual(1, param);

			// スタックオーバーフロー
			ctx.SetScript("*a[call to=a]");
			Assert.Throws(typeof(AnimaScriptException), () => {
				ctx.Execute();
			});

			// 外部スクリプトの呼び出し
			param = 0;
			ctx.Clear();
			ctx.Loader = (name) => {
				return new StringReader("*set1234[testSet param=1234][return]");
			};
			ctx.SetScript("[call to=set1234 script=abc]");
			ctx.Execute();
			Assert.AreEqual(1234, param);
		}
	}
}
