﻿using System.Collections.Generic;

namespace Minet.Compiler
{
	public class Lexer
	{
		const char eof = '\0';
		const string operatorChars = "()[]<>!=+-*/%,.:&|^";
		const string commentStart = "/;";
		const string commentEnd = ";/";

		delegate stateFn stateFn();

		private string input;
		private stateFn state;
		private Stack<int> indentLevels;
		private int start = 0, pos = 0;
		public List<Token> Tokens;
		private Stack<int> widths;
		private bool inStmt = false;

		private Position curPos
		{
			get
			{
				int l = 1, c = 1;
				bool cl = true;
				for (var i = start - 1; i >= 0; i--)
				{
					if (input[i] == '\n')
					{
						l++;
						cl = false;
					}
					else if (cl) { c++; }
				}
				return new Position { Char = c, Line = l };
			}
		}

		private string current { get { return input.Substring(start, (pos - start)); } }

		private bool currentPosStartsWith(string str)
		{
			if (pos + str.Length > input.Length) { return false; }
			for (int i = 0; i < str.Length; i++)
			{
				if (input[i + pos] != str[i]) { return false; }
			}
			return true;
		}

		public Lexer(string input)
		{
			this.input = input;
			state = lexIndent;
			indentLevels = new Stack<int>();
			indentLevels.Push(0);
			Tokens = new List<Token>();
			widths = new Stack<int>();
			while (state != null) { state = state(); }
		}

		private stateFn error(string error)
		{
			Tokens.Add(new Token { Type = TokenType.Error, Pos = curPos, Val = error });
			return null;
		}

		private char next()
		{
			if (pos >= input.Length)
			{
				widths.Push(0);
				return eof;
			}
			widths.Push(1);
			return input[pos++];
		}

		private char peek
		{
			get
			{
				char c = next();
				backup();
				return c;
			}
		}

		private void backup() { pos -= widths.Pop(); }
		private void discard() { start = pos; }

		private bool accept(string valid)
		{
			if (valid.IndexOf(next()) >= 0) { return true; }
			backup();
			return false;
		}

		private void acceptRun(string valid)
		{
			while (valid.IndexOf(next()) >= 0) { }
			backup();
		}

		private void emit(TokenType t)
		{
			Tokens.Add(new Token { Type = t, Pos = curPos, Val = current });
			start = pos;
			widths = new Stack<int>();
		}

		private void emitIndent(int indent)
		{
			int i = indentLevels.Peek();
			if (indent > i)
			{
				emit(TokenType.Indent);
				indentLevels.Push(indent);
			}
			else
			{
				while (indentLevels.Count > 0 && indent < i)
				{
					emit(TokenType.Dedent);
					emit(TokenType.EOL);
					indentLevels.Pop();
					i = indentLevels.Peek();
				}
				if (indentLevels.Count == 0 || i != indent)
				{
					error("Mismatched indentation level encountered.");
				}
			}
		}

		private stateFn lexIndent()
		{
			inStmt = false;
			int indent = 0;
			while (true)
			{
				if (currentPosStartsWith(commentStart))
				{
					discard();
					return lexMLComment;
				}
				switch (next())
				{
					case eof:
						discard();
						emitIndent(0);
						emit(TokenType.EOF);
						return null;
					case '\r':
					case '\n':
						indent = 0;
						discard();
						break;
					case ' ':
						indent++;
						break;
					case '\t':
						indent += 4;
						break;
					case ';':
						backup();
						discard();
						return lexComment;
					default:
						backup();
						discard();
						emitIndent(indent);
						return lexStatement;
				}
			}
		}

		private stateFn lexStatement()
		{
			while (true)
			{
				char c = peek;
				if (currentPosStartsWith(commentStart)) { return lexMLComment; }
				else if (c == eof)
				{
					if (inStmt) { emit(TokenType.EOL); }
					emitIndent(0);
					emit(TokenType.EOF);
					return null;
				}
				else if (c == ' ' || c == '\t' || c == '\r')
				{
					next();
					discard();
				}
				else if (c == '\n')
				{
					next();
					if (inStmt) { emit(TokenType.EOL); }
					return lexIndent;
				}
				else if (c == ';') { return lexComment; }
				else if (c == '"')
				{
					inStmt = true;
					return lexString;
				}
				else if (c == '\'')
				{
					inStmt = true;
					return lexChar;
				}
				else if (char.IsLetter(c) || c == '_')
				{
					inStmt = true;
					return lexIdentifier;
				}
				else if (char.IsDigit(c))
				{
					inStmt = true;
					return lexNumber;
				}
				else if (accept(operatorChars))
				{
					inStmt = true;
					return lexOperator;
				}
				else { return error("Invalid character '" + c + "' encountered."); }
			}
		}

		private stateFn lexComment()
		{
			next(); // eat the ;
			discard();
			for (var c = peek; c != eof && c != '\r' && c != '\n'; c = peek) { next(); }
			emit(TokenType.Comment);
			return lexStatement;
		}

		private stateFn lexMLComment()
		{
			next();
			next();
			discard(); // eat /;
			while (peek != eof)
			{
				if (currentPosStartsWith(commentEnd))
				{
					emit(TokenType.Comment);
					next();
					next();
					discard(); // eat ;/
					return lexStatement;
				}
				next();
			}
			return error("Unclosed /;");
		}

		private stateFn lexIdentifier()
		{
			for (var c = peek; char.IsLetterOrDigit(c) || c == '_'; c = peek) { next(); }
			TokenType t;
			bool found = Token.Keywords.TryGetValue(current, out t);
			if (!found) { t = TokenType.Identifier; }
			emit(t);
			return lexStatement;
		}

		private stateFn lexNumber()
		{
			for (var c = peek; char.IsDigit(c); c = peek) { next(); }
			accept(".");
			for (var c = peek; char.IsDigit(c); c = peek) { next(); }
			emit(TokenType.Number);
			return lexStatement;
		}

		private stateFn lexOperator()
		{
			acceptRun(operatorChars);
			int p = pos;
			TokenType t;
			bool found = Token.Keywords.TryGetValue(current, out t);
			while (!found && pos > start)
			{
				backup();
				found = Token.Keywords.TryGetValue(current, out t);
			}
			if (pos > start)
			{
				emit(t);
				return lexStatement;
			}
			return error("Invalid operator '" + input.Substring(start, p - start) + "'");
		}

		private stateFn lexString()
		{
			next(); // eat the "
			discard();
			bool inEsc = false;
			while (true)
			{
				char c = next();
				if (c == eof || c == '\r' || c == '\n')
				{
					return error("Unclosed \"");
				}
				else if (!inEsc && c == '\\') { inEsc = true; }
				else if (!inEsc && c == '"')
				{
					backup();
					emit(TokenType.String);
					next();
					discard();
					return lexStatement;
				}
				else if (inEsc) { inEsc = false; }
			}
		}

		private stateFn lexChar()
		{
			next();
			discard();
			switch (next())
			{
				case eof:
				case '\r':
				case '\n':
					return error("Unclosed '");
				case '\\':
					switch (next())
					{
						case eof:
						case '\r':
						case '\n':
							return error("Unclosed '");
						default:
							if (accept("'"))
							{
								backup();
								emit(TokenType.Char);
								next();
								discard();
								return lexStatement;
							}
							return error("Unclosed '");
					}
				default:
					if (accept("'"))
					{
						backup();
						emit(TokenType.Char);
						next();
						discard();
						return lexStatement;
					}
					return error("Unclosed '");
			}
		}
	}
}
