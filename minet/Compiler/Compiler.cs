using System;
using System.Collections.Generic;

namespace Minet.Compiler
{
	public class Compiler
	{
		public static void Build(BuildConfig config)
		{
			bool printAST = config.IsSet("printAST");
			var asts = new List<AST.File>();
			var errors = new List<string>();

			foreach (string file in config.Files)
			{
				var p = new Parser(file, config);
				var ast = p.Parse();
				if (!ast.Error) { asts.Add(ast.Result as AST.File); }
				if (p.Errors.Count == 0)
				{
					if (printAST)
					{
						Console.WriteLine(Environment.NewLine + Environment.NewLine + "AST");
						ast.Result.Print(1);
					}
				}
				else { foreach (var e in p.Errors) { errors.Add(e); } }
			}

			if (config.IsSet("build") && errors.Count == 0)
			{
				var ws = new WalkState(config["asm"], config["out"]);
				errors = ws.Errors;

				if (errors.Count == 0) { foreach (var a in asts) { a.GenTypes(ws); } }
				if (errors.Count == 0) { foreach (var a in asts) { a.GenClassItems(ws); } }
				if (errors.Count == 0) { ws.Assembly.GenFunctions(ws); }
				if (errors.Count == 0) { ws.Assembly.CreateTypes(); }
				if (errors.Count == 0) { ws.Save(); }
			}

			if (errors.Count > 0)
			{
				Console.WriteLine(Environment.NewLine + Environment.NewLine + "Errors:");
				foreach (var e in errors) { Console.WriteLine(e); }
			}
		}
	}
}