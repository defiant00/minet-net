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
                int tokPrec = peekCombo.Precedence();

                // If this is a binary operator that binds at least as tightly as the
                // current operator then consume it, otherwise we're done.
                if (tokPrec < exprPrec)
                {
                    return new ParseResult<Expression>(lhs, false);
                }

                var op = nextCombo();

                var rhs = parsePrimaryExpr();
                if (rhs.Error) { return rhs; }

                // If binop binds less tightly with RHS than the op after RHS, let the
                // pending op take RHS as its LHS.
                int nextPrec = peekCombo.Precedence();
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

            if (accept(TokenType.With).Success)
            {
                while (true)
                {
                    if (peek.Type != TokenType.Identifier)
                    {
                        return error<Statement>(true, "Invalid token in class " + c.Name + " with declaration: " + res.LastToken);
                    }
                    var st = parseIdentifier<Statement>();
                    c.Withs.Add(st.Result);
                    if (st.Error) { break; }
                    if (!accept(TokenType.Comma).Success) { break; }
                }
            }

            res = accept(TokenType.EOL, TokenType.Indent);
            if (!res.Success)
            {
                return error<Statement>(true, "Invalid token in class " + c.Name + " declaration: " + res.LastToken);
            }

            while (peek.Type != TokenType.Dedent && peek.Type != TokenType.EOF)
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
                case TokenType.Iota:
                    return parseIotaStmt();
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
                else if (peek.Type.IsType()) { type = parseType().Result; }

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

        private ParseResult<Statement> parseExprStmt(bool inWith)
        {
            var ex = parseExprList();
            Statement assign = null;
            if (peek.Type.IsAssign()) { assign = parseAssign(ex.Result).Result; }
            if (!inWith)
            {
                var res = accept(TokenType.EOL);
                if (!res.Success)
                {
                    return error<Statement>(true, "Invalid token in expression statement: " + res.LastToken);
                }
            }
            if (assign != null)
            {
                return new ParseResult<Statement>(assign, false);
            }
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
                    //case TokenType.Function:
                    //	break;
                    case TokenType.Identifier:
                        f.Statements.Add(parseClass().Result);
                        break;
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

        private ParseResult<Statement> parseFunctionDef(bool dotted, string name)
        {
            var res = accept(TokenType.LeftParen);
            if (!res.Success)
            {
                return error<Statement>(true, "Invalid token in function definition: " + res.LastToken);
            }
            var fn = new FunctionDef { Static = !dotted, Name = name };
            while (peek.Type != TokenType.RightParen)
            {
                res = accept(TokenType.Identifier);
                if (!res.Success)
                {
                    return error<Statement>(true, "Invalid token in function definition: " + res.LastToken);
                }
                string pName = res[0].Val;
                Statement type = peek.Type.IsType() ? parseType().Result : null;
                fn.Params.Add(new Parameter { Name = pName, Type = type });
                switch (peek.Type)
                {
                    case TokenType.Comma:
                        next(); // eat ,
                        break;
                    case TokenType.RightParen: break;
                    default:
                        return error<Statement>(true, "Invalid token in function definition: " + peek);
                }
            }
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

            while (peek.Type != TokenType.Dedent && peek.Type != TokenType.EOF)
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
                    return parseVar(false);
                default:
                    var res = accept(TokenType.Identifier, TokenType.Colon);
                    if (res.Success) { return parseForOrLoop(res[0].Val); }
                    return parseExprStmt(false);
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

        private ParseResult<Expression> parseIotaExpr()
        {
            next(); // eat iota
            return new ParseResult<Expression>(new Iota(), false);
        }

        private ParseResult<Statement> parseIotaStmt()
        {
            var res = accept(TokenType.Iota, TokenType.EOL);
            if (!res.Success)
            {
                return error<Statement>(true, "Invalid token in iota: " + res.LastToken);
            }
            return new ParseResult<Statement>(new Iota(), false);
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
                case TokenType.Iota:
                    lhs = parseIotaExpr().Result;
                    break;
                case TokenType.LeftBracket:
                    lhs = parseArrayCons().Result;
                    break;
                case TokenType.LeftCurly:
                    lhs = parseCurlyExpr().Result;
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
                bool loop = true;
                while (loop)
                {
                    switch (peek.Type)
                    {
                        case TokenType.LeftBracket:
                            lhs = parseAccessor(lhs).Result;
                            break;
                        case TokenType.LeftCurly:
                            lhs = parseConstructor(lhs).Result;
                            break;
                        case TokenType.LeftParen:
                            lhs = parseFunctionCall(lhs).Result;
                            break;
                        default:
                            loop = false;
                            break;
                    }
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

        private ParseResult<Expression> parseStringExpr()
        {
            var s = new AST.String { Val = next().Val };
            return new ParseResult<Expression>(s, false);
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
