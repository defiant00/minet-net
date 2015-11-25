using System;
using System.Collections.Generic;

namespace Minet.Compiler.AST
{
	public partial class Accessor
	{
		public List<Type> CalcTypes(WalkState ws) { return new List<Type> { }; }
	}

	public partial class ArrayCons
	{
		public List<Type> CalcTypes(WalkState ws) { return new List<Type> { }; }
	}

	public partial class ArrayValueList
	{
		public List<Type> CalcTypes(WalkState ws) { return new List<Type> { }; }
	}

	public partial class Binary
	{
		public List<Type> CalcTypes(WalkState ws)
		{
			List<Type> ltl = Left.CalcTypes(ws);
			List<Type> rtl = Right.CalcTypes(ws);

			if (ltl.Count == rtl.Count && ltl.Count == 1)
			{
				Type lt = ltl[0];
				Type rt = rtl[0];
				if (Op.IsBooleanOp()) { return new List<Type> { typeof(bool) }; }
				else if (Op == TokenType.As) { return rtl; }
				else if (lt == typeof(string) || rt == typeof(string)) { return new List<Type> { typeof(string) }; }
				else if (lt == rt) { return ltl; }
			}

			ws.AddError("Cannot apply binary operator " + Op + " with " + ltl.Count + " type(s) and " + rtl.Count + " type(s).");
			return new List<Type> { };
		}
	}

	public partial class Blank
	{
		public List<Type> CalcTypes(WalkState ws) { return new List<Type> { }; }
	}

	public partial class Bool
	{
		public List<Type> CalcTypes(WalkState ws) { return new List<Type> { typeof(bool) }; }
	}

	public partial class Char
	{
		public List<Type> CalcTypes(WalkState ws) { return new List<Type> { typeof(char) }; }
	}

	public partial class Constructor
	{
		public List<Type> CalcTypes(WalkState ws) { return new List<Type> { }; }
	}

	public partial class Error
	{
		public List<Type> CalcTypes(WalkState ws) { return new List<Type> { }; }
	}

	public partial class ExprList
	{
		public List<Type> CalcTypes(WalkState ws)
		{
			var types = new List<Type>();
			foreach (var e in Expressions) { types.AddRange(e.CalcTypes(ws)); }
			return types;
		}
	}

	public partial class Float
	{
		public List<Type> CalcTypes(WalkState ws) { return new List<Type> { typeof(double) }; }
	}

	public partial class FunctionCall
	{
		public List<Type> CalcTypes(WalkState ws) { return new List<Type> { }; }
	}

	public partial class FunctionDef
	{
		public List<Type> CalcTypes(WalkState ws) { return new List<Type> { }; }
	}

	public partial class FunctionSig
	{
		public List<Type> CalcTypes(WalkState ws) { return new List<Type> { }; }
	}

	public partial class Identifier
	{
		public List<Type> CalcTypes(WalkState ws) { return new List<Type> { this.ToType(ws) }; }
	}

	public partial class Integer
	{
		public List<Type> CalcTypes(WalkState ws) { return new List<Type> { typeof(int) }; }
	}

	public partial class String
	{
		public List<Type> CalcTypes(WalkState ws) { return new List<Type> { typeof(string) }; }
	}

	public partial class Unary
	{
		public List<Type> CalcTypes(WalkState ws) { return Expr.CalcTypes(ws); }
	}
}
