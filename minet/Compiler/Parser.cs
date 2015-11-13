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

		// Returns whether the next token(s) are a type, as either an identifier, an anonymous function,
		// or an array.
		private bool nextIsType
		{
			get
			{
				if (peek.Type == TokenType.Identifier || peek.Type == TokenType.Function) { return true; }
				if (peek.Type == TokenType.LeftBracket)
				{
					int p = pos + 1;
					while (tokens[p].Type == TokenType.Comma) { p++; }
					if (tokens[p].Type == TokenType.RightBracket) { return true; }
				}
				return false;
			}
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

		private ParseResult<Expression> parseAccessor(Expression lhs)
		{
			next(); // eat [
			var expr = parseExprList();
			if (expr.Error) { return expr; }
			var res = accept(TokenType.RightBracket);
			if (!res.Success)
			{
				return error<Expression>(true, "Invalid token in accessor: " + res.LastToken);
			}
			var acc = new Accessor { Object = lhs, Index = expr.Result };
			return new ParseResult<Expression>(acc, false);
		}

		private ParseResult<Expression> parseAnonFuncExpr()
		{
			next(); // eat fn
			var st = parseFunctionDef(true, "");
			return new ParseResult<Expression>(st.Result as Expression, st.Error);
		}

		private ParseResult<Statement> parseArrayType()
		{
			next(); // eat [
			int dim = 1;
			while (peek.Type == TokenType.Comma)
			{
				dim++;
				next();
			}
			var res = accept(TokenType.RightBracket);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in array type: " + res.LastToken);
			}
			var type = parseType();
			if (type.Error) { return type; }
			return new ParseResult<Statement>(new AST.Array { Type = type.Result, Dimensions = dim }, false);
		}

		private ParseResult<Statement> parseAssign(Expression lhs)
		{
			var op = next().Type;
			var rhs = parseExprList();
			var a = new Assign { Op = op, Left = lhs, Right = rhs.Result };
			return new ParseResult<Statement>(a, false);
		}

		private ParseResult<Expression> parseBinopRHS(int exprPrec, Expression lhs)
		{
			while (true)
			{
				int tokPrec = peek.Precedence();

				// If this is a binary operator that binds at least as tightly as the
				// current operator then consume it, otherwise we're done.
				if (tokPrec < exprPrec)
				{
					return new ParseResult<Expression>(lhs, false);
				}

				var op = next();

				var rhs = parsePrimaryExpr();
				if (rhs.Error) { return rhs; }

				// If binop binds less tightly with RHS than the op after RHS, let the
				// pending op take RHS as its LHS.
				int nextPrec = peek.Precedence();
				if (tokPrec < nextPrec)
				{
					rhs = parseBinopRHS(tokPrec + 1, rhs.Result);
					if (rhs.Error) { return rhs; }
				}

				// Merge LHS/RHS.
				lhs = new Binary { Op = op.Type, Left = lhs, Right = rhs.Result };
			}
		}

		private ParseResult<Expression> parseBlankExpr()
		{
			next(); // eat _
			return new ParseResult<Expression>(new Blank(), false);
		}

		private ParseResult<Expression> parseBoolExpr()
		{
			var b = new Bool { Val = (next().Type == TokenType.True) };
			return new ParseResult<Expression>(b, false);
		}

		private ParseResult<Expression> parseBracketExpr()
		{
			var ex = parseMLExprList(TokenType.LeftBracket, TokenType.RightBracket);
			if (ex.Error) { return ex; }
			if (nextIsType)
			{
				var type = parseType();
				if (type.Error)
				{
					return new ParseResult<Expression>(type.Result as Expression, true);
				}
				var ac = new ArrayCons { Type = type.Result, Size = ex.Result };
				return new ParseResult<Expression>(ac, false);
			}
			return new ParseResult<Expression>(new ArrayValueList { Vals = ex.Result }, false);
		}

		private ParseResult<Statement> parseBreak()
		{
			next(); // eat break
			var b = new Break();
			var res = accept(TokenType.Identifier);
			if (res.Success) { b.Label = res[0].Val; }
			res = accept(TokenType.EOL);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in break: " + res.LastToken);
			}
			return new ParseResult<Statement>(b, false);
		}

		private ParseResult<Expression> parseCharExpr()
		{
			var c = new AST.Char { Val = next().Val };
			return new ParseResult<Expression>(c, false);
		}

		private ParseResult<Statement> parseClass()
		{
			var res = accept(TokenType.Identifier);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in class declaration: " + res.LastToken);
			}
			var c = new Class { Name = res[0].Val };

			if (accept(TokenType.LeftCaret).Success)
			{
				while (true)
				{
					res = accept(TokenType.Identifier);
					if (!res.Success)
					{
						return error<Statement>(true, "Invalid token in class " + c.Name + " type declaration: " + res.LastToken);
					}
					c.TypeParams.Add(res[0].Val);
					if (!accept(TokenType.Comma).Success) { break; }
				}
				res = accept(TokenType.RightCaret);
				if (!res.Success)
				{
					return error<Statement>(true, "Invalid token in class " + c.Name + " type declaration: " + res.LastToken);
				}
			}

			res = accept(TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in class " + c.Name + " declaration: " + res.LastToken);
			}

			while (!peek.Type.IsDedentStop())
			{
				c.Statements.Add(parseClassStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				c.Statements.Add(error<Statement>(true, "Invalid token in class " + c.Name + " declaration: " + res.LastToken).Result);
			}

			return new ParseResult<Statement>(c, false);
		}

		private ParseResult<Statement> parseClassStmt()
		{
			switch (peek.Type)
			{
				case TokenType.Dot:
				case TokenType.Identifier:
					return parseClassStmtIdent();
				default:
					return error<Statement>(true, "Invalid token in class statement: " + peek);
			}
		}

		private ParseResult<Statement> parseClassStmtIdent()
		{
			var ps = new PropertySet();

			while (true)
			{
				bool dotted = accept(TokenType.Dot).Success;
				var r = accept(TokenType.Identifier);
				if (!r.Success)
				{
					return error<Statement>(true, "Invalid token in class statement: " + r.LastToken);
				}
				string name = r[0].Val;

				Statement type = null;
				if (peek.Type == TokenType.LeftParen)
				{
					return parseFunctionDef(dotted, name);
				}
				else if (nextIsType) { type = parseType().Result; }

				ps.Props.Add(new Property { Static = !dotted, Name = name, Type = type });
				if (!accept(TokenType.Comma).Success) { break; }
			}

			if (accept(TokenType.Assign).Success) { ps.Vals = parseExprList().Result; }

			var res = accept(TokenType.EOL);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in class statement: " + res.LastToken);
			}

			return new ParseResult<Statement>(ps, false);
		}

		private ParseResult<Statement> parseDefer()
		{
			next(); // eat defer
			var d = new Defer { Expr = parseExpr().Result };
			var res = accept(TokenType.EOL);
			if (!res.Success)
			{
				d.Expr = error<Expression>(true, "Invalid token in defer: " + res.LastToken).Result;
			}
			return new ParseResult<Statement>(d, false);
		}

		private ParseResult<Expression> parseExpr()
		{
			var lhs = parsePrimaryExpr();
			if (lhs.Error) { return lhs; }
			return parseBinopRHS(0, lhs.Result);
		}

		private ParseResult<Expression> parseExprList()
		{
			var el = new ExprList();
			while (true)
			{
				var ex = parseExpr();
				el.Expressions.Add(ex.Result);
				if (ex.Error) { break; }
				if (!accept(TokenType.Comma).Success) { break; }
			}
			return new ParseResult<Expression>(el, false);
		}

		private ParseResult<Statement> parseExprStmt()
		{
			var ex = parseExprList();
			Statement assign = null;
			if (peek.Type.IsAssign()) { assign = parseAssign(ex.Result).Result; }
			var res = accept(TokenType.EOL);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in expression statement: " + res.LastToken);
			}
			if (assign != null) { return new ParseResult<Statement>(assign, false); }
			var es = new ExprStmt { Expr = ex.Result };
			return new ParseResult<Statement>(es, false);
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
					case TokenType.Identifier:
						f.Statements.Add(parseClass().Result);
						break;
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

		private ParseResult<Statement> parseFor(string label)
		{
			next(); // eat for
			AcceptResult res;

			var f = new For { Label = label };

			var par = parseVarTypeDeclaration();
			if (par.Result.Count == 0)
			{
				return error<Statement>(true, "No variable specified for for loop.");
			}
			else if (par.Error)
			{
				return new ParseResult<Statement>(par.Result[par.Result.Count - 1], true);
			}

			foreach (Variable v in par.Result) { f.Vars.Add(v); }

			res = accept(TokenType.In);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in for: " + res.LastToken);
			}

			var inExpr = parseExpr();
			if (inExpr.Error)
			{
				return new ParseResult<Statement>(inExpr.Result as Statement, true);
			}
			f.In = inExpr.Result;

			res = accept(TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in for: " + res.LastToken);
			}

			while (!peek.Type.IsDedentStop())
			{
				f.Statements.Add(parseFunctionStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				f.Statements.Add(error<Statement>(true, "Invalid token in for: " + res.LastToken).Result);
			}

			return new ParseResult<Statement>(f, false);
		}

		private ParseResult<Statement> parseForOrLoop(string label)
		{
			switch (peek.Type)
			{
				case TokenType.For:
					return parseFor(label);
				case TokenType.Loop:
					return parseLoop(label);
				default:
					return error<Statement>(true, "Invalid token in for or loop:" + peek);
			}
		}

		private ParseResult<Expression> parseFunctionCall(Expression lhs)
		{
			var fc = new FunctionCall { Function = lhs };
			fc.Params = parseMLExprList(TokenType.LeftParen, TokenType.RightParen).Result;
			return new ParseResult<Expression>(fc, false);
		}

		private ParseResult<Statement> parseFunctionDef(bool dotted, string name)
		{
			var res = accept(TokenType.LeftParen);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in function definition: " + res.LastToken);
			}
			var fn = new FunctionDef { Static = !dotted, Name = name };

			var par = parseVarTypeDeclaration();
			if (par.Error)
			{
				return new ParseResult<Statement>(par.Result[par.Result.Count - 1], true);
			}
			foreach (Variable v in par.Result) { fn.Params.Add(v); }

			res = accept(TokenType.RightParen);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in function definition: " + res.LastToken);
			}

			// return value(s)
			fn.Returns = parseReturnValues().Result;

			res = accept(TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in function definition: " + res.LastToken);
			}

			while (!peek.Type.IsDedentStop())
			{
				fn.Statements.Add(parseFunctionStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				fn.Statements.Add(error<Statement>(true, "Invalid token in function definition: " + res.LastToken).Result);
			}

			// If it's an anonymous function and we're not in the middle of a block
			// (followed by either ',' or ')' ) then put the EOL back.
			if (string.IsNullOrEmpty(name) && !peek.Type.IsInBlock()) { backup(1); }

			return new ParseResult<Statement>(fn, false);
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

		private ParseResult<Statement> parseFunctionStmt()
		{
			switch (peek.Type)
			{
				case TokenType.Break:
					return parseBreak();
				case TokenType.Defer:
					return parseDefer();
				case TokenType.For:
				case TokenType.Loop:
					return parseForOrLoop("");
				case TokenType.If:
					return parseIf();
				case TokenType.Return:
					return parseReturn();
				case TokenType.Var:
					return parseVar();
				default:
					var res = accept(TokenType.Identifier, TokenType.Colon);
					if (res.Success) { return parseForOrLoop(res[0].Val); }
					return parseExprStmt();
			}
		}

		private ParseResult<T> parseIdentifier<T>() where T : class, General
		{
			var id = new Identifier();
			while (true)
			{
				var res = accept(TokenType.Identifier);
				if (!res.Success) { return error<T>(true, "Invalid token in identifier: " + res.LastToken); }
				var ip = new IdentPart { Name = res[0].Val };
				if (accept(TokenType.LeftCaret).Success)
				{
					int resetPos = pos - 1; // store the position in case it isn't a generic
					while (nextIsType)
					{
						var st = parseType();
						ip.TypeParams.Add(st.Result);
						if (st.Error) { break; }
						if (!accept(TokenType.Comma).Success) { break; }
					}
					if (!accept(TokenType.RightCaret).Success)
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

		private ParseResult<Statement> parseIf()
		{
			next(); // eat if
			Expression cond = null;
			if (peek.Type != TokenType.EOL && peek.Type != TokenType.With)
			{
				var condRes = parseExpr();
				if (condRes.Error)
				{
					return new ParseResult<Statement>(condRes.Result as Statement, true);
				}
				cond = condRes.Result;
			}

			Statement with = null;
			if (accept(TokenType.With).Success) { with = parseVarLine(false).Result; }

			var res = accept(TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in if: " + res.LastToken);
			}

			var ifs = new If { Condition = cond, With = with };
			while (!peek.Type.IsDedentStop())
			{
				ifs.Statements.Add(parseIfInnerStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				ifs.Statements.Add(error<Statement>(true, "Invalid token in if: " + res.LastToken).Result);
			}

			return new ParseResult<Statement>(ifs, false);
		}

		private ParseResult<Statement> parseIfInnerStmt()
		{
			if (peek.Type == TokenType.Is) { return parseIs(); }
			return parseFunctionStmt();
		}

		private ParseResult<Statement> parseIs()
		{
			next(); // eat is
			var cond = parseExprList();
			var res = accept(TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in is: " + res.LastToken);
			}

			var iss = new Is { Condition = cond.Result };
			while (!peek.Type.IsDedentStop())
			{
				iss.Statements.Add(parseFunctionStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				iss.Statements.Add(error<Statement>(true, "Invalid token in is: " + res.LastToken).Result);
			}

			return new ParseResult<Statement>(iss, false);
		}

		private ParseResult<Statement> parseLoop(string label)
		{
			var res = accept(TokenType.Loop, TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in loop: " + res.LastToken);
			}

			var l = new Loop { Label = label };

			while (!peek.Type.IsDedentStop())
			{
				l.Statements.Add(parseFunctionStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				l.Statements.Add(error<Statement>(true, "Invalid token in loop: " + res.LastToken).Result);
			}

			return new ParseResult<Statement>(l, false);
		}

		private ParseResult<Expression> parseMLExprList(TokenType start, TokenType end)
		{
			var el = new ExprList();
			var res = accept(start);
			if (!res.Success)
			{
				el.Expressions.Add(error<Expression>(true, "Invalid token in expression list: " + res.LastToken).Result);
				return new ParseResult<Expression>(el, true);
			}
			if (peek.Type != end)
			{
				if (accept(TokenType.EOL, TokenType.Indent).Success)
				{
					while (true)
					{
						var e = parseExpr();
						el.Expressions.Add(e.Result);
						if (e.Error) { break; }
						if (accept(TokenType.EOL, TokenType.Dedent, TokenType.EOL).Success) { break; }
						res = accept(TokenType.Comma);
						if (!res.Success)
						{
							el.Expressions.Add(error<Expression>(true, "Invalid token in expression list: " + res.LastToken).Result);
							break;
						}
						accept(TokenType.EOL); // eat EOL if it's there
					}
				}
				else
				{
					var ex = parseExprList();
					if (ex.Error)
					{
						el.Expressions.Add(ex.Result);
						return new ParseResult<Expression>(el, true);
					}
					el = ex.Result as ExprList;
				}
			}
			res = accept(end);
			if (!res.Success)
			{
				el.Expressions.Add(error<Expression>(true, "Invalid token in expression list: " + res.LastToken).Result);
			}
			return new ParseResult<Expression>(el, false);
		}

		private ParseResult<Statement> parseNamespace()
		{
			var res = accept(TokenType.Namespace);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in namespace: " + res.LastToken);
			}
			var id = parseIdentifier<Statement>();
			var ns = new Namespace { Name = id.Result as Identifier };
			res = accept(TokenType.EOL);
			if (!res.Success)
			{
				return error<Statement>(true, "Invalid token in namespace: " + res.LastToken);
			}
			return new ParseResult<Statement>(ns, false);
		}

		private ParseResult<Expression> parseNumberExpr()
		{
			var n = new Number { Val = next().Val };
			return new ParseResult<Expression>(n, false);
		}

		private ParseResult<Expression> parseParenExpr()
		{
			next(); // eat (
			var expr = parseExpr();
			var res = accept(TokenType.RightParen);
			if (!res.Success)
			{
				return error<Expression>(true, "Invalid token in (): " + res.LastToken);
			}
			return expr;
		}

		private ParseResult<Expression> parsePrimaryExpr()
		{
			Expression lhs = null;
			switch (peek.Type)
			{
				case TokenType.Blank:
					lhs = parseBlankExpr().Result;
					break;
				case TokenType.Char:
					lhs = parseCharExpr().Result;
					break;
				case TokenType.Function:
					lhs = parseAnonFuncExpr().Result;
					break;
				case TokenType.Identifier:
					lhs = parseIdentifier<Expression>().Result;
					break;
				case TokenType.LeftBracket:
					lhs = parseBracketExpr().Result;
					break;
				case TokenType.LeftParen:
					lhs = parseParenExpr().Result;
					break;
				case TokenType.Number:
					lhs = parseNumberExpr().Result;
					break;
				case TokenType.String:
					lhs = parseStringExpr().Result;
					break;
				case TokenType.True:
				case TokenType.False:
					lhs = parseBoolExpr().Result;
					break;
				default:
					if (peek.Type.IsUnaryOp()) { lhs = parseUnaryExpr().Result; }
					break;
			}

			if (lhs != null)
			{
				while (peek.Type == TokenType.LeftBracket || peek.Type == TokenType.LeftParen)
				{
					if (peek.Type == TokenType.LeftBracket) { lhs = parseAccessor(lhs).Result; }
					else { lhs = parseFunctionCall(lhs).Result; }
				}
				return new ParseResult<Expression>(lhs, false);
			}
			return error<Expression>(true, "Token is not an expression: " + peek);
		}

		private ParseResult<Statement> parseReturn()
		{
			next(); // eat ret
			var r = new Return();
			if (peek.Type != TokenType.EOL) { r.Vals = parseExprList().Result; }
			var res = accept(TokenType.EOL);
			if (!res.Success)
			{
				r.Vals = error<Expression>(true, "Invalid token in return: " + res.LastToken).Result;
			}
			return new ParseResult<Statement>(r, false);
		}

		private ParseResult<List<Statement>> parseReturnValues()
		{
			var rvs = new List<Statement>();

			if (nextIsType) { rvs.Add(parseType().Result); }
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

		private ParseResult<Expression> parseStringExpr()
		{
			var s = new AST.String { Val = next().Val };
			return new ParseResult<Expression>(s, false);
		}

		private ParseResult<Statement> parseType()
		{
			switch (peek.Type)
			{
				case TokenType.LeftBracket:
					return parseArrayType();
				case TokenType.Function:
					return parseFunctionSig();
				case TokenType.Identifier:
					return parseIdentifier<Statement>();
				default:
					return error<Statement>(true, "Invalid token in type: " + peek);
			}
		}

		private ParseResult<Expression> parseUnaryExpr()
		{
			var op = next().Type;
			var ex = parsePrimaryExpr();
			var un = new Unary { Expr = ex.Result, Op = op };
			return new ParseResult<Expression>(un, false);
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

		private ParseResult<Statement> parseVar()
		{
			next(); // eat var
			var vs = new VarSet();

			var vsl = parseVarLine(true);
			if (vsl.Error) { return vsl; }
			vs.Lines.Add(vsl.Result as VarSetLine);

			if (accept(TokenType.Indent).Success)
			{
				while (!peek.Type.IsDedentStop())
				{
					vsl = parseVarLine(true);
					if (vsl.Error) { return vsl; }
					vs.Lines.Add(vsl.Result as VarSetLine);
				}

				var res = accept(TokenType.Dedent, TokenType.EOL);
				if (!res.Success)
				{
					return error<Statement>(true, "Invalid token in var statement: " + res.LastToken);
				}
			}

			return new ParseResult<Statement>(vs, false);
		}

		private ParseResult<Statement> parseVarLine(bool eatEOL)
		{
			var v = new VarSetLine();

			var vars = parseVarTypeDeclaration();
			if (vars.Result.Count == 0)
			{
				return error<Statement>(true, "No variables specified after var.");
			}
			else if (vars.Error)
			{
				return new ParseResult<Statement>(vars.Result[vars.Result.Count - 1], true);
			}
			foreach(Variable va in vars.Result) { v.Vars.Add(va); }

			if (accept(TokenType.Assign).Success) { v.Vals = parseExprList().Result; }

			if (eatEOL)
			{
				var res = accept(TokenType.EOL);
				if (!res.Success)
				{
					return error<Statement>(true, "Invalid token in var statement: " + res.LastToken);
				}
			}
			return new ParseResult<Statement>(v, false);
		}

		private ParseResult<List<Statement>> parseVarTypeDeclaration()
		{
			var parameters = new List<Statement>();
			while (peek.Type == TokenType.Identifier || peek.Type == TokenType.Blank)
			{
				string pName = next().Val;
				Statement type = nextIsType ? parseType().Result : null;
				parameters.Add(new Variable { Name = pName, Type = type });
				if (!accept(TokenType.Comma).Success) { break; }
			}
			return new ParseResult<List<Statement>>(parameters, false);
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
