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

		public static ParseResult<Statement> Parse(string filename, bool build, bool printTokens)
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
					return parser.error<Statement>(false, "Error: " + t);
				}
			}
			return parser.parseFile();
		}

		private Token peek { get { return tokens[pos]; } }
		private Token next() { return tokens[pos++]; }
		private void backup(int count) { pos -= count; }
		private int tokensAvailable { get { return tokens.Count - pos; } }

		// Returns the next token, or combines >> into rshift.
		private Token peekCombo
		{
			get
			{
				var t = next();
				if (tokensAvailable > 0)
				{
					var t2 = peek;
					if (t.Type == TokenType.RightCaret && t2.Type == TokenType.RightCaret)
					{
						backup(1);
						return new Token { Type = TokenType.RShift, Pos = t.Pos };
					}
				}
				backup(1);
				return t;
			}
		}

		// Consumes and returns the next token, or combines >> into rshift.
		private Token nextCombo()
		{
			var t = next();
			var t2 = next();
			if (t.Type == TokenType.RightCaret && t2.Type == TokenType.RightCaret)
			{
				return new Token { Type = TokenType.RShift, Pos = t.Pos };
			}
			backup(1);
			return t;
		}

		private ParseResult<T> error<T>(bool toNextLine, string error) where T : class, General
		{
			this.toNextLine(toNextLine);
			return new ParseResult<T>(new Error { Val = error } as T, true);
		}

		private void toNextLine(bool toNextLine)
		{
			if (!toNextLine) { return; }

			var t = peek.Type;
			for (; t != TokenType.EOL && t != TokenType.EOF; t = peek.Type) { next(); }
			if (t == TokenType.EOL)
			{
				next();
				while (true)
				{
					var res = accept(TokenType.Dedent, TokenType.EOL);
					if (!res.Success) { return; }
				}
			}
		}

		private class AcceptResult
		{
			public bool Success = true;
			public List<Token> Tokens = new List<Token>();

			public Token LastToken
			{
				get { return Tokens.Count > 0 ? Tokens[Tokens.Count - 1] : null; }
			}

			public Token this[int index] { get { return Tokens[index]; } }
		}

		private AcceptResult accept(params TokenType[] args)
		{
			int start = pos;
			var res = new AcceptResult();
			foreach (var a in args)
			{
				var cur = next();
				res.Tokens.Add(cur);
				if (cur.Type != a)
				{
					pos = start;
					res.Success = false;
					return res;
				}
			}
			return res;
		}

		private ParseResult<Statement> parseArrayType()
		{
			next(); // eat []
			var type = parseType();
			if (type.Error) { return type; }
			return new ParseResult<Statement>(new AST.Array { Type = type.Result }, false);
		}

		private ParseResult<Statement> parseFile()
		{
			var f = new File { Name = filename };
			bool error = false;
			while (pos < tokens.Count)
			{
				switch (peek.Type)
				{
					case TokenType.EOF:
						next();
						break;
					//case TokenType.Function:
					//	break;
					//case TokenType.Identifier:
					//	break;
					//case TokenType.Interface:
					//	break;
					case TokenType.Namespace:
						f.Statements.Add(parseNamespace().Result);
						break;
					case TokenType.Use:
						f.Statements.Add(parseUse().Result);
						break;
					default:
						f.Statements.Add(error<Statement>(true, "Invalid token " + peek).Result);
						error = true;
						break;
				}
			}
			return new ParseResult<Statement>(f, error);
		}

		private ParseResult<Statement> parseFunctionSig()
		{
			var func = new FunctionSig();

			// fn(types)
			var res = accept(TokenType.Function, TokenType.LeftParen);
			if (!res.Success) { return error<Statement>(true, "Invalid token in function type: " + res.LastToken); }
			while (peek.Type != TokenType.RightParen)
			{
				var type = parseType();
				if (type.Error) { return type; }
				func.Params.Add(type.Result);
				switch (peek.Type)
				{
					case TokenType.Comma:
						next(); // eat ,
						break;
					case TokenType.RightParen: break;
					default:
						return error<Statement>(true, "Invalid token in function type: " + peek);
				}
			}
			next(); // eat )

			// return value(s)
			func.Returns = parseReturnValues().Result;

			return new ParseResult<Statement>(func, false);
		}

		private ParseResult<T> parseIdentifier<T>() where T : class, General
		{
			var id = new Identifier();
			while (true)
			{
				var res = accept(TokenType.Identifier);
				if (!res.Success) { return error<T>(true, "Invalid token in identifier: " + res.LastToken); }
				var ip = new IdentPart { Name = res[0].Val };
				res = accept(TokenType.LeftCaret);
				if (res.Success)
				{
					int resetPos = pos - 1; // store the position in case it isn't a generic
					while (peek.Type.IsType())
					{
						var st = parseType();
						ip.TypeParams.Add(st.Result);
						if (st.Error) { break; }
						res = accept(TokenType.Comma);
						if (!res.Success) { break; }
					}
					res = accept(TokenType.RightCaret);
					if (!res.Success)
					{
						pos = resetPos;
						ip.TypeParams.Clear();
					}
				}
				id.Idents.Add(ip);
				res = accept(TokenType.Dot);
				if (!res.Success) { break; }
			}
			return new ParseResult<T>(id as T, false);
		}

		private ParseResult<Statement> parseNamespace()
		{
			var res = accept(TokenType.Namespace);
			if (!res.Success) { return error<Statement>(true, "Invalid token in namespace: " + res.LastToken); }
			var id = parseIdentifier<Statement>();
			var ns = new Namespace { Name = id.Result as Identifier };
			res = accept(TokenType.EOL);
			if (!res.Success) { return error<Statement>(true, "Invalid token in namespace: " + res.LastToken); }
			return new ParseResult<Statement>(ns, false);
		}

		private ParseResult<List<Statement>> parseReturnValues()
		{
			var rvs = new List<Statement>();

			if (peek.Type.IsType()) { rvs.Add(parseType().Result); }
			else if (peek.Type == TokenType.LeftParen)
			{
				next(); // eat (
				while (peek.Type != TokenType.RightParen)
				{
					rvs.Add(parseType().Result);
					switch (peek.Type)
					{
						case TokenType.Comma:
							next(); // eat ,
							break;
						case TokenType.RightParen: break;
						default:
							rvs.Add(error<Statement>(true, "Invalid token in return types: " + peek).Result);
							return new ParseResult<List<Statement>>(rvs, true);
					}
				}
				next(); // eat )
			}

			return new ParseResult<List<Statement>>(rvs, false);
		}

		private ParseResult<Statement> parseType()
		{
			switch (peek.Type)
			{
				case TokenType.Array:
					return parseArrayType();
				case TokenType.Function:
					return parseFunctionSig();
				case TokenType.Identifier:
					return parseIdentifier<Statement>();
				default:
					return error<Statement>(true, "Invalid token in type: " + peek);
			}
		}

		private ParseResult<Statement> parseUse()
		{
			next(); // eat use
			var pack = parseUsePackage();
			if (pack.Error) { return pack; }
			var use = new Use();
			use.Packages.Add(pack.Result as UsePackage);

			if (accept(TokenType.Indent).Success)
			{
				while (peek.Type == TokenType.Identifier)
				{
					pack = parseUsePackage();
					if (pack.Error) { return pack; }
					use.Packages.Add(pack.Result as UsePackage);
				}
				if (accept(TokenType.Dedent, TokenType.EOL).Success)
				{
					return new ParseResult<Statement>(use, false);
				}
				return error<Statement>(true, "Invalid token in use: " + peek);
			}

			return new ParseResult<Statement>(use, false);
		}

		private ParseResult<Statement> parseUsePackage()
		{
			var ident = parseIdentifier<Statement>();
			if (ident.Error) { return ident; }
			var asRes = accept(TokenType.As, TokenType.Identifier);
			var res = accept(TokenType.EOL);
			if (!res.Success) { return error<Statement>(true, "Invalid token in use: " + res.LastToken); }
			var pack = new UsePackage { Pack = ident.Result as Identifier };
			if (asRes.Success) { pack.Alias = asRes[1].Val; }
			return new ParseResult<Statement>(pack, false);
		}
	}

	public class ParseResult<T>
	{
		public T Result;
		public bool Error;

		public ParseResult(T r, bool error)
		{
			Result = r;
			Error = error;
		}
	}
}
