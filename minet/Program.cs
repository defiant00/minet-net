using Minet.Compiler;
using System;
using System.Collections.Generic;

namespace Minet
{
	public enum CommandLineState
	{
		Files
	}

	public class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Minet Compiler 0.1");
			if (args.Length > 0)
			{
				var state = CommandLineState.Files;
				var config = new BuildConfig();
				foreach (string a in args)
				{
					switch (a)
					{
						case "/files":
							state = CommandLineState.Files;
							break;
						default:
							if (a[0] == '/')
							{
								string[] parts = a.Substring(1).Split(new[] { ':' });
								string val = null;
								if (parts.Length > 1) { val = parts[1]; }
								config.Flags[parts[0]] = val;
							}
							else
							{
								switch (state)
								{
									case CommandLineState.Files:
										config.Files.Add(a);
										break;
									default:
										Console.WriteLine("Unknown state: " + state);
										break;
								}
							}
							break;
					}
				}

				var errors = new List<string>();
				if (config.Files.Count == 0)
				{
					errors.Add("You must specify at least one file to build.");
				}
				if (config.IsSet("build"))
				{
					if (!config.IsSet("asm"))
					{
						errors.Add("You must specify an assembly name with /asm:assembly_name when building.");
					}
					if (!config.IsSet("out"))
					{
						errors.Add("You must specify an output file name with /out:filename when building.");
					}
				}

				if (errors.Count == 0)
				{
					Compiler.Compiler.Build(config);
					Console.WriteLine(Environment.NewLine + "Done");
				}
				else
				{
					Console.WriteLine("Errors:");
					foreach(var e in errors) { Console.WriteLine(e); }
                }

			}
			else { Console.WriteLine("Usage: minet <path> [parameters]"); }
			Console.ReadKey();
		}
	}
}
