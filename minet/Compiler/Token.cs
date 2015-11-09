using System;
using System.Collections.Generic;

namespace Minet.Compiler
{
    public enum TokenType
    {
        Error,              // an error, val contains the error text
        Indent,             // an increase in indentation
        Dedent,             // a decrease in indentation
        EOL,                // the end of a line of code
        EOF,                // the end of the file
        Comment,            // comment
        String,             // a literal string
        Char,               // a literal character
        Number,             // a literal number
        Identifier,         // an identifier
        keyword_start,
        Namespace,          // 'ns'
        Use,                // 'use'
        As,                 // 'as'
        If,                 // 'if'
        Is,                 // 'is'
        In,                 // 'in'
        With,               // 'with'
        Function,           // 'fn'
        Interface,          // 'intf'
        Var,                // 'var'
        Return,             // 'ret'
        Defer,              // 'defer'
        Blank,              // '_'
        For,                // 'for'
        Loop,               // 'loop'
        Break,              // 'break'
        Iota,               // 'iota'
        True,               // 'true'
        False,              // 'false'
        Equal,              // '=='
        NotEqual,           // '!='
        LeftCaret,          // '<'
        RightCaret,         // '>'
        LtEqual,            // '<='
        GtEqual,            // '>='
        And,                // 'and'
        Or,                 // 'or'
        Dot,                // '.'
        Comma,              // ','
        Colon,              // ':'
        LeftParen,          // '('
        RightParen,         // ')'
        LeftBracket,        // '['
        RightBracket,       // ']'
        Array,              // '[]'
        LeftCurly,          // '{'
        RightCurly,         // '}'
        assign_start,
        Assign,             // '='
        AddAssign,          // '+='
        SubAssign,          // '-='
        MulAssign,          // '*='
        DivAssign,          // '/='
        ModAssign,          // '%='
        BAndAssign,         // '&='
        BOrAssign,          // '|='
        BXOrAssign,         // '^='
        LShiftAssign,       // '<<='
        RShiftAssign,       // '>>='
        assign_end,
        Add,                // '+'
        Mul,                // '*'
        Div,                // '/'
        Mod,                // '%'
        LShift,             // '<<'
        RShift,             // '>>'
        BAnd,               // '&'
        BOr,                // '|'
        BXOr,               // '^'
        unary_op_start,
        Sub,                // '-'
        Not,                // '!'
        unary_op_end,
        keyword_end
    }

    public static class TokenHelper
    {
        public static bool IsKeyword(this TokenType type)
        {
            return type > TokenType.keyword_start && type < TokenType.keyword_end;
        }

        public static bool IsUnaryOp(this TokenType type)
        {
            return type > TokenType.unary_op_start && type < TokenType.unary_op_end;
        }

        public static bool IsAssign(this TokenType type)
        {
            return type > TokenType.assign_start && type < TokenType.assign_end;
        }

        public static bool IsType(this TokenType type)
        {
            return type == TokenType.Identifier || type == TokenType.Function || type == TokenType.Array;
        }

        public static bool IsInBlock(this TokenType type)
        {
            return type == TokenType.Comma || type == TokenType.RightParen;
        }
    }

    public class Token
    {
        public TokenType Type;
        public Position Pos;
        public string Val;

        public readonly static Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            {"ns",      TokenType.Namespace},
            {"use",     TokenType.Use},
            {"as",      TokenType.As},
            {"if",      TokenType.If},
            {"is",      TokenType.Is},
            {"in",      TokenType.In},
            {"with",    TokenType.With},
            {"fn",      TokenType.Function},
            {"intf",    TokenType.Interface},
            {"var",     TokenType.Var},
            {"ret",     TokenType.Return},
            {"defer",   TokenType.Defer},
            {"_",       TokenType.Blank},
            {"for",     TokenType.For},
            {"loop",    TokenType.Loop},
            {"break",   TokenType.Break},
            {"iota",    TokenType.Iota},
            {"true",    TokenType.True},
            {"false",   TokenType.False},
            {"==",      TokenType.Equal},
            {"!=",      TokenType.NotEqual},
            {"<",       TokenType.LeftCaret},
            {">",       TokenType.RightCaret},
            {"<=",      TokenType.LtEqual},
            {">=",      TokenType.GtEqual},
            {"and",     TokenType.And},
            {"or",      TokenType.Or},
            {".",       TokenType.Dot},
            {",",       TokenType.Comma},
            {":",       TokenType.Colon},
            {"(",       TokenType.LeftParen},
            {")",       TokenType.RightParen},
            {"[",       TokenType.LeftBracket},
            {"]",       TokenType.RightBracket},
            {"[]",      TokenType.Array},
            {"{",       TokenType.LeftCurly},
            {"}",       TokenType.RightCurly},
            {"=",       TokenType.Assign},
            {"+=",      TokenType.AddAssign},
            {"-=",      TokenType.SubAssign},
            {"*=",      TokenType.MulAssign},
            {"/=",      TokenType.DivAssign},
            {"%=",      TokenType.ModAssign},
            {"&=",      TokenType.BAndAssign},
            {"|=",      TokenType.BOrAssign},
            {"^=",      TokenType.BXOrAssign},
            {"<<=",     TokenType.LShiftAssign},
            {">>=",     TokenType.RShiftAssign},
            {"+",       TokenType.Add},
            {"-",       TokenType.Sub},
            {"*",       TokenType.Mul},
            {"/",       TokenType.Div},
            {"%",       TokenType.Mod},
            {"<<",      TokenType.LShift},		// RShift purposefully excluded since it clashes
			{"&",       TokenType.BAnd},		// with generics. See parser.peekCombo and
			{"|",       TokenType.BOr},			// parser.nextCombo for where this is handled.
			{"^",       TokenType.BXOr},
            {"!",       TokenType.Not}
        };

        public override string ToString()
        {
            switch (Type)
            {
                case TokenType.EOL:
                    return Pos + " " + Type + Environment.NewLine;
                case TokenType.Comment:
                case TokenType.String:
                case TokenType.Char:
                case TokenType.Number:
                case TokenType.Identifier:
                case TokenType.Error:
                    return Pos + " " + Type + " : '" + Val + "'";
                default:
                    return Pos + " " + Type;
            }
        }

        public int Precedence()
        {
            switch (Type)
            {
                case TokenType.Dot:
                    return 7;
                case TokenType.As:
                    return 6;
                case TokenType.Mul:
                case TokenType.Div:
                case TokenType.Mod:
                case TokenType.LShift:
                case TokenType.RShift:
                case TokenType.BAnd:
                    return 5;
                case TokenType.Add:
                case TokenType.Sub:
                case TokenType.BOr:
                case TokenType.BXOr:
                    return 4;
                case TokenType.Equal:
                case TokenType.NotEqual:
                case TokenType.LeftCaret:
                case TokenType.LtEqual:
                case TokenType.RightCaret:
                case TokenType.GtEqual:
                    return 3;
                case TokenType.And:
                    return 2;
                case TokenType.Or:
                    return 1;
                default:
                    return -1;
            }
        }
    }

    public class Position
    {
        public int Line, Char;
        public override string ToString() { return Line + ":" + Char; }
    }
}
