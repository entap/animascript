using System;
using NUnit.Framework;
using Entap.AnimaScript;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace Entap.AnimaScript.Test
{
	public class LexerTest
	{
		[Test]
		public void OnlyMessageTest()
		{
			var s = "abcdefg";
			var lexer = new Lexer(new StringReader(s));
			var command = lexer.Read();
			Assert.AreEqual("message", command.Name);
			Assert.AreEqual("abcdefg", command.GetParameter<string>("text"));
			Assert.Null(lexer.Read());
		}

		[Test]
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

		[Test]
		public void OnlyTagTest()
		{
			var s = @"[tag aaa=bbb ccc=""ddd"" eee='fff' ggg]";
			var lexer = new Lexer(new StringReader(s));
			var command = lexer.Read();
			Assert.AreEqual("tag", command.Name);
			Assert.AreEqual("bbb", command.GetParameter<string>("aaa"));
			Assert.AreEqual("ddd", command.GetParameter<string>("ccc"));
			Assert.AreEqual("fff", command.GetParameter<string>("eee"));
			Assert.AreEqual("ggg", command.GetParameter<string>("ggg"));
			Assert.Null(lexer.Read());
		}

		[Test]
		public void OnlyLabelTest()
		{
			var s = @"*labelName";
			var lexer = new Lexer(new StringReader(s));
			var command = lexer.Read();
			Assert.AreEqual("label", command.Name);
			Assert.AreEqual("labelName", command.GetParameter<string>("name"));
			Assert.Null(lexer.Read());
		}

		[Test]
		public void MessageAndCommentTest()
		{
			var s = @"aaa
//bbb
/ccc//ddd
eee/*fff*/ggg;aaa";
			var lexer = new Lexer(new StringReader(s));
			var command = lexer.Read();
			Assert.AreEqual("message", command.Name);
			Assert.AreEqual("aaa", command.GetParameter<string>("text"));
			command = lexer.Read();
			Assert.AreEqual("message", command.Name);
			Assert.AreEqual("/ccc", command.GetParameter<string>("text"));
			command = lexer.Read();
			Assert.AreEqual("message", command.Name);
			Assert.AreEqual("eee", command.GetParameter<string>("text"));
			command = lexer.Read();
			Assert.AreEqual("message", command.Name);
			Assert.AreEqual("ggg", command.GetParameter<string>("text"));
			Assert.Null(lexer.Read());
		}

		[Test]
		public void TagAndCommentTest()
		{
			var s = @"[a]
//bbb
[b c='d']//ddd
[d]/*fff*/";
			var lexer = new Lexer(new StringReader(s));
			var command = lexer.Read();
			Assert.AreEqual("a", command.Name);
			command = lexer.Read();
			Assert.AreEqual("b", command.Name);
			Assert.AreEqual("d", command.GetParameter<string>("c"));
			command = lexer.Read();
			Assert.AreEqual("d", command.Name);
			Assert.Null(lexer.Read());
		}

		[Test]
		public void LabelAndCommentTest()
		{
			var s = @"* label1
*label2//comment
*  label3/*****/
/**/* label4;comment";
			var lexer = new Lexer(new StringReader(s));
			var command = lexer.Read();
			Assert.AreEqual("label", command.Name);
			Assert.AreEqual("label1", command.GetParameter<string>("name"));
			command = lexer.Read();
			Assert.AreEqual("label", command.Name);
			Assert.AreEqual("label2", command.GetParameter<string>("name"));
			command = lexer.Read();
			Assert.AreEqual("label", command.Name);
			Assert.AreEqual("label3", command.GetParameter<string>("name"));
			command = lexer.Read();
			Assert.AreEqual("label", command.Name);
			Assert.AreEqual("label4", command.GetParameter<string>("name"));
		}

	}
}
