using System;
using NUnit.Framework;
using Entap.AnimaScript;
using System.IO;

namespace Entap.AnimaScript.Test
{
	public class CommandBlockTest
	{
		[Test]
		public void LabelTest()
		{
			var s = "*aaa[test1][test2]*bbb[test3][test4]";
			var cb = new CommandBlock(new StringReader(s));
			Assert.AreEqual(0, cb.LookUpLabel("aaa"));
			Assert.AreEqual(2, cb.LookUpLabel("bbb"));
		}

		[Test]
		public void IfTest()
		{
			var s = "[if][else][endif]";
			var cb = new CommandBlock(new StringReader(s));
			Assert.AreEqual("if", cb.GetCommand(0).Name);
			Assert.AreEqual("jump", cb.GetCommand(1).Name);
		}

		[Test]
		public void IfTest2()
		{
			var s = "[if][elif][else][endif]";
			var cb = new CommandBlock(new StringReader(s));
			Assert.AreEqual("if", cb.GetCommand(0).Name);
			Assert.AreEqual("jump", cb.GetCommand(1).Name);
			Assert.AreEqual("elif", cb.GetCommand(2).Name);
			Assert.AreEqual("jump", cb.GetCommand(3).Name);
		}

		[Test]
		public void IfTest3()
		{
			var s = "[if][if][else][endif][else][endif]";
			var cb = new CommandBlock(new StringReader(s));
			Assert.AreEqual("if", cb.GetCommand(0).Name);
			Assert.AreEqual("if", cb.GetCommand(1).Name);
			Assert.AreEqual("jump", cb.GetCommand(2).Name);
			Assert.AreEqual("jump", cb.GetCommand(3).Name);
		}
	}
}
