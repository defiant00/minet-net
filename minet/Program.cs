using System;

namespace Minet
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Minet Compiler 0.1");
			if (args.Length > 0)
			{
				bool build = false, printTokens = false, printAST = false;
				for (int i = 1; i < args.Length; i++)
				{
					switch (args[i])
					{
						case "-build":
							build = true;
							break;
						case "-printTokens":
							printTokens = true;
							break;
						case "-printAST":
							printAST = true;
							break;
						default:
							Console.WriteLine("Unknown parameter: " + args[i]);
							break;
					}
				}
				Compiler.Compiler.Build(args[0], build, printTokens, printAST);
				Console.WriteLine(Environment.NewLine + "Done");
			}
			else { Console.WriteLine("Usage: minet <path> [parameters]"); }
			Console.ReadKey();
		}
	}
}
