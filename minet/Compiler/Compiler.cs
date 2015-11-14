using System;
using System.IO;

// https://neildanson.wordpress.com/2014/02/11/building-a-c-compiler-in-f/
// http://www.trelford.com/blog/post/compiler.aspx

namespace Minet.Compiler
{
	public class Compiler
	{
		public static void Build(string path, bool build, bool printTokens, bool printAST)
		{
			Console.WriteLine("Building " + path);

			var ws = new WalkState();

			var files = Directory.GetFiles(path, "*.mn");
			foreach (string file in files)
			{
				var p = new Parser(file, build, printTokens);
				var ast = p.Parse();
				if (p.Errors.Count == 0)
				{
					if (printAST)
					{
						Console.WriteLine(Environment.NewLine + Environment.NewLine + "AST");
						ast.Result.Print(1);
					}
					if (build)
					{
						ast.Result.GenFinal(ws);
						if (ws.Errors.Count > 0) { break; }
					}
				}
				else { foreach (var e in p.Errors) { ws.AddError(e); } }
			}

			if (build)
			{
				if (ws.Errors.Count == 0)
				{

				}
				else
				{
					Console.WriteLine(Environment.NewLine + Environment.NewLine + "Errors:");
					foreach (var e in ws.Errors) { Console.WriteLine(e); }
				}
			}
		}
	}
}