using System;
using Xunit;
using Entap.AnimaScript;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace Entap.AnimaScript.Test
{
	public class CommandTest
	{
		[Fact]
		public void CommandParamsTest()
		{
			var command = new Command();
			command.Name = "test";
			command.AddParameter("s", "ssss");
			command.AddParameter("i", "1111");
			command.AddParameter("f", "1.11");
			command.AddParameter("b", "true");
			Assert.Equal(command.GetParameter("s", "****"), "ssss");
			Assert.Equal(command.GetParameter<int>("i", 0), 1111);
			Assert.Equal(command.GetParameter<float>("f", 0), 1.11, 3);
			Assert.Equal(command.GetParameter<bool>("b", false), true);
		}

		[Fact]
		public void CommandSyntaxErrorTest()
		{
			var command = new Command();
			command.Name = "test";
			command.AddParameter("s", "ssss");
			Assert.Throws(typeof(AnimaScriptException), () => { command.GetParameter<int>("s", 0); });
			Assert.Throws(typeof(AnimaScriptException), () => { command.GetParameter<float>("s", 0); });
			Assert.Throws(typeof(AnimaScriptException), () => { command.GetParameter<bool>("s", false); });
		}
	}
}