using System;
using Xunit;
using Entap.AnimaScript;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace Entap.AnimaScript.Test
{
	public class LexerTest
	{
		[Fact]
		public void OnlyMessageTest()
		{
			var s = "abcdefg";
			var lexer = new Lexer(new StringReader(s));
			var command = lexer.Read();
			Assert.Equal("message", command.Name);
			Assert.Equal("abcdefg", command.GetParameter<string>("text"));
			Assert.Null(lexer.Read());
		}

		[Fact]
		public void OnlyCommentTest()
		{
			var s = @"//single line comment
/* multi line comments
aaa**/
;single line comment";
			var lexer = new Lexer(new StringReader(s));
			var command = lexer.Read();
			Assert.Null(command);
		}

		[Fact]
		public void OnlyTagTest()
		{
			var s = @"[tag aaa=bbb ccc=""ddd"" eee='fff' ggg]";
			var lexer = new Lexer(new StringReader(s));
			var command = lexer.Read();
			Assert.Equal("tag", command.Name);
			Assert.Equal("bbb", command.GetParameter<string>("aaa"));
			Assert.Equal("ddd", command.GetParameter<string>("ccc"));
			Assert.Equal("fff", command.GetParameter<string>("eee"));
			Assert.Equal("ggg", command.GetParameter<string>("ggg"));
			Assert.Null(lexer.Read());
		}

		[Fact]
		public void OnlyLabelTest()
		{
			var s = @"*labelName";
			var lexer = new Lexer(new StringReader(s));
			var command = lexer.Read();
			Assert.Equal("label", command.Name);
			Assert.Equal("labelName", command.GetParameter<string>("name"));
			Assert.Null(lexer.Read());
		}

		[Fact]
		public void MessageAndCommentTest()
		{
			var s = @"aaa
//bbb
/ccc//ddd
eee/*fff*/ggg;aaa";
			var lexer = new Lexer(new StringReader(s));
			var command = lexer.Read();
			Assert.Equal("message", command.Name);
			Assert.Equal("aaa", command.GetParameter<string>("text"));
			command = lexer.Read();
			Assert.Equal("message", command.Name);
			Assert.Equal("/ccc", command.GetParameter<string>("text"));
			command = lexer.Read();
			Assert.Equal("message", command.Name);
			Assert.Equal("eee", command.GetParameter<string>("text"));
			command = lexer.Read();
			Assert.Equal("message", command.Name);
			Assert.Equal("ggg", command.GetParameter<string>("text"));
			Assert.Null(lexer.Read());
		}

		[Fact]
		public void TagAndCommentTest()
		{
			var s = @"[a]
//bbb
[b c='d']//ddd
[d]/*fff*/";
			var lexer = new Lexer(new StringReader(s));
			var command = lexer.Read();
			Assert.Equal("a", command.Name);
			command = lexer.Read();
			Assert.Equal("b", command.Name);
			Assert.Equal("d", command.GetParameter<string>("c"));
			command = lexer.Read();
			Assert.Equal("d", command.Name);
			Assert.Null(lexer.Read());
		}

		[Fact]
		public void LabelAndCommentTest()
		{
			var s = @"* label1
*label2//comment
*  label3/*****/
/**/* label4;comment";
			var lexer = new Lexer(new StringReader(s));
			var command = lexer.Read();
			Assert.Equal("label", command.Name);
			Assert.Equal("label1", command.GetParameter<string>("name"));
			command = lexer.Read();
			Assert.Equal("label", command.Name);
			Assert.Equal("label2", command.GetParameter<string>("name"));
			command = lexer.Read();
			Assert.Equal("label", command.Name);
			Assert.Equal("label3", command.GetParameter<string>("name"));
			command = lexer.Read();
			Assert.Equal("label", command.Name);
			Assert.Equal("label4", command.GetParameter<string>("name"));
		}

	}
}
