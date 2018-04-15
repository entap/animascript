using System;
using Xunit;
using Entap.AnimaScript;
using System.IO;

namespace Entap.AnimaScript.Test
{
	public class CommandBlockTest
	{
		[Fact]
		public void LabelTest()
		{
			var s = "*aaa[test1][test2]*bbb[test3][test4]";
			var cb = new CommandBlock(new StringReader(s));
			Assert.Equal(0, cb.LookUpLabel("aaa"));
			Assert.Equal(2, cb.LookUpLabel("bbb"));
		}

		[Fact]
		public void IfTest()
		{
			var s = "[if][else][endif]";
			var cb = new CommandBlock(new StringReader(s));
			Assert.Equal("if", cb.GetCommand(0).Name);
			Assert.Equal("jump", cb.GetCommand(1).Name);
		}

		[Fact]
		public void IfTest2()
		{
			var s = "[if][elif][else][endif]";
			var cb = new CommandBlock(new StringReader(s));
			Assert.Equal("if", cb.GetCommand(0).Name);
			Assert.Equal("jump", cb.GetCommand(1).Name);
			Assert.Equal("elif", cb.GetCommand(2).Name);
			Assert.Equal("jump", cb.GetCommand(3).Name);
		}

		[Fact]
		public void IfTest3()
		{
			var s = "[if][if][else][endif][else][endif]";
			var cb = new CommandBlock(new StringReader(s));
			Assert.Equal("if", cb.GetCommand(0).Name);
			Assert.Equal("if", cb.GetCommand(1).Name);
			Assert.Equal("jump", cb.GetCommand(2).Name);
			Assert.Equal("jump", cb.GetCommand(3).Name);
		}
	}
}
