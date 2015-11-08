using System;
using System.IO;

namespace Minet.Compiler
{
	public class Compiler
	{
		public static void Build(string path, bool build, bool printTokens, bool printAST)
		{
			Console.WriteLine("Building " + path);

			var files = Directory.GetFiles(path, "*.mn");
			foreach (string file in files)
			{
				var fAST = Parser.Parse(file, build, printTokens);
				if (printAST)
				{
					Console.WriteLine(Environment.NewLine + Environment.NewLine + "AST");
					AST.Printer.Print(fAST.Result as dynamic, 1);
				}
			}
		}
	}
}
