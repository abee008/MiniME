﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using MiniME;

namespace MiniMETestCases
{
	[TestFixture]
	public class Test
	{
		public static string LoadTextResource(string name)
		{
			// get a reference to the current assembly
			var a = System.Reflection.Assembly.GetExecutingAssembly();
			System.IO.StreamReader r = new System.IO.StreamReader(a.GetManifestResourceStream(name));
			string str = r.ReadToEnd();
			r.Close();

			return str;
		}

		public static IEnumerable<TestCaseData> GetTests()
		{
			var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();

			return from name in Assembly.GetExecutingAssembly().GetManifestResourceNames()
				   where name.StartsWith("MiniMETestCases.TestScripts.") && name.EndsWith(".txt")
				   orderby name
				   select new TestCaseData(name);
		}

		[Test, TestCaseSource("GetTests")]
		public void RunTest(string resourceName)
		{
			var t = new TestFile();
			t.LoadFromString(LoadTextResource(resourceName));

			// Compile the input script
			var c = new Compiler();
			c.Formatted = t.Input.IndexOf("[Formatted]") >= 0;
			c.NoObfuscate = t.Input.IndexOf("[NoObfuscate]") >= 0;
			c.SymbolInfo = t.Input.IndexOf("[SymbolInfo]") >= 0;
			c.MinifyKind = t.Input.IndexOf("[CSS]") >= 0 ? MinifyKind.CSS : MinifyKind.JS;
			c.NoCredit = true;
			c.MaxLineLength = 0;
			c.AddScript(resourceName, t.Input, false);

			// Render it
			string strActual = c.CompileToString().Trim();
		
			string sep = new string('-', 15);
			Console.WriteLine(sep + " input " + sep + "\n" + t.Input);
			Console.WriteLine(sep + " actual " + sep + "\n" + strActual);
			Console.WriteLine(sep + " expected" + sep + "\n" + t.Output);

			Assert.That(strActual, Is.EqualTo(t.Output));

		}
	}
}
