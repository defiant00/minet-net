using Minet.Compiler.AST;
using System;
using System.Collections.Generic;

namespace Minet.Compiler
{
	public class Parser
	{
		private string filename;
		private int pos = 0;
		private List<Token> tokens = new List<Token>();

		public static ParseResult Parse(string filename, bool build, bool printTokens)
		{
			Console.WriteLine("Parsing file " + filename);

			string data = System.IO.File.ReadAllText(filename);
			Console.WriteLine("Data loaded...");

			var lexer = new Lexer(data);
			var parser = new Parser { filename = filename };

			if (printTokens)
			{
				Console.WriteLine(Environment.NewLine + "Tokens");
			}

			foreach (var t in lexer.Tokens)
			{
				if (build && t.Type != TokenType.Comment) { parser.tokens.Add(t); }
				if (printTokens) { Console.Write(" " + t); }
			}
			if (parser.tokens.Count > 0)
			{
				var t = parser.tokens[parser.tokens.Count - 1];
				if (t.Type == TokenType.Error)
				{
					return parser.error(false, "Error: " + t);
				}
			}
			return new ParseResult { Error = false };
		}

		public ParseResult error(bool toNextLine, string error)
		{
			return new ParseResult { Result = new Error { Val = error }, Error = true };
		}

		public void toNextLine(bool toNextLine)
		{
			if (!toNextLine) { return; }

			// TODO
		}
	}

	public class ParseResult
	{
		public General Result;
		public bool Error;
	}
}
