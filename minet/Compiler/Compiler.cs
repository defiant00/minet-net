using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

// https://neildanson.wordpress.com/2014/02/11/building-a-c-compiler-in-f/
// http://www.trelford.com/blog/post/compiler.aspx

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
					fAST.Result.Print(1);
				}
				if (build)
				{
					Console.WriteLine("Analyzing AST...");
					var gs = new WalkState();
					fAST.Result.Analyze(gs);
					if (gs.Errors.Count == 0)
					{
						Console.WriteLine("Generating IL...");
						gs = new WalkState();
						gs.AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("MT"), AssemblyBuilderAccess.Save);
						gs.ModuleBuilder = gs.AssemblyBuilder.DefineDynamicModule("minetTest.exe");

						fAST.Result.GenIL(gs);

						gs.AssemblyBuilder.Save("minetTest.exe");
                    }
					else
					{
						Console.WriteLine("Errors:");
						foreach (var e in gs.Errors)
						{
							Console.WriteLine(e);
						}
					}
				}
			}
		}
	}
}
