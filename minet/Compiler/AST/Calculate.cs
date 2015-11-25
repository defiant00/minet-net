using System;

namespace Minet.Compiler.AST
{
	public partial class Accessor
	{
		public object Calculate(WalkState ws) { return null; }
	}

	public partial class ArrayCons
	{
		public object Calculate(WalkState ws) { return null; }
	}

	public partial class ArrayValueList
	{
		public object Calculate(WalkState ws) { return null; }
	}

	public partial class Binary
	{
		public object Calculate(WalkState ws)
		{
			Type lt, rt;
			object left = Left.Calculate(ws);
			object right = Right.Calculate(ws);
			switch (Op)
			{
				case TokenType.As:
					rt = Right.CalcTypes(ws)[0];
					return rt.Cast(left, ws);
				case TokenType.Is:
					var lts = Left.CalcTypes(ws);
					rt = Right.CalcTypes(ws)[0];
					return (lts.Count == 1 && lts[0] == rt);
				case TokenType.Add:
					lt = CalcTypes(ws)[0];
					if (lt.IsFloatingPointType())
					{
						return lt.Cast(Convert.ToDouble(left) + Convert.ToDouble(right), ws);
					}
					else if (lt.IsIntegerType())
					{
						return lt.Cast(Convert.ToInt64(left) + Convert.ToInt64(right), ws);
					}
					else if (lt == typeof(string))
					{
						return left.ToString() + right.ToString();
					}
					break;
				case TokenType.Sub:
					lt = CalcTypes(ws)[0];
					if (lt.IsFloatingPointType())
					{
						return lt.Cast(Convert.ToDouble(left) - Convert.ToDouble(right), ws);
					}
					else if (lt.IsIntegerType())
					{
						return lt.Cast(Convert.ToInt64(left) - Convert.ToInt64(right), ws);
					}
					break;
				case TokenType.Mul:
					lt = CalcTypes(ws)[0];
					if (lt.IsFloatingPointType())
					{
						return lt.Cast(Convert.ToDouble(left) * Convert.ToDouble(right), ws);
					}
					else if (lt.IsIntegerType())
					{
						return lt.Cast(Convert.ToInt64(left) * Convert.ToInt64(right), ws);
					}
					break;
				case TokenType.Div:
					lt = CalcTypes(ws)[0];
					if (lt.IsFloatingPointType())
					{
						return lt.Cast(Convert.ToDouble(left) / Convert.ToDouble(right), ws);
					}
					else if (lt.IsIntegerType())
					{
						return lt.Cast(Convert.ToInt64(left) / Convert.ToInt64(right), ws);
					}
					break;
				case TokenType.Mod:
					lt = CalcTypes(ws)[0];
					if (lt.IsIntegerType())
					{
						return lt.Cast(Convert.ToInt64(left) % Convert.ToInt64(right), ws);
					}
					break;
				case TokenType.BAnd:
				case TokenType.BOr:
				case TokenType.BXOr:
					break;
				case TokenType.Equal:
					return (left as IComparable).CompareTo(right) == 0;
				case TokenType.NotEqual:
					return (left as IComparable).CompareTo(right) != 0;
				case TokenType.LeftCaret:
					return (left as IComparable).CompareTo(right) < 0;
				case TokenType.LtEqual:
					return (left as IComparable).CompareTo(right) < 1;
				case TokenType.RightCaret:
					return (left as IComparable).CompareTo(right) > 0;
				case TokenType.GtEqual:
					return (left as IComparable).CompareTo(right) > -1;
				case TokenType.And:
					return ((bool)left) && ((bool)right);
				case TokenType.Or:
					return ((bool)left) || ((bool)right);
			}

			ws.AddError("Could not calculate binary operator: " + Op);
			return null;
		}
	}

	public partial class Blank
	{
		public object Calculate(WalkState ws) { return null; }
	}

	public partial class Bool
	{
		public object Calculate(WalkState ws) { return Val; }
	}

	public partial class Char
	{
		public object Calculate(WalkState ws) { return Val[0]; }
	}

	public partial class Constructor
	{
		public object Calculate(WalkState ws) { return null; }
	}

	public partial class Error
	{
		public object Calculate(WalkState ws) { return null; }
	}

	public partial class ExprList
	{
		public object Calculate(WalkState ws) { return null; }
	}

	public partial class Float
	{
		public object Calculate(WalkState ws) { return Convert.ToDouble(Val); }
	}

	public partial class FunctionCall
	{
		public object Calculate(WalkState ws) { return null; }
	}

	public partial class FunctionDef
	{
		public object Calculate(WalkState ws) { return null; }
	}

	public partial class FunctionSig
	{
		public object Calculate(WalkState ws) { return null; }
	}

	public partial class Identifier
	{
		public object Calculate(WalkState ws) { return null; }
	}

	public partial class Integer
	{
		public object Calculate(WalkState ws) { return Convert.ToInt64(Val); }
	}

	public partial class String
	{
		public object Calculate(WalkState ws) { return Val; }
	}

	public partial class Unary
	{
		public object Calculate(WalkState ws)
		{
			var ts = Expr.CalcTypes(ws);
			if (ts.Count != 1)
			{
				ws.AddError("Cannot use a unary operator on a type with " + ts.Count + " return type(s).");
				return null;
			}
			Type type = ts[0];
			var val = Expr.Calculate(ws);
			switch (Op)
			{
				case TokenType.Sub:
					if (type.IsFloatingPointType())
					{
						return type.Cast(-Convert.ToDouble(val), ws);
					}
					else if (type.IsIntegerType())
					{
						return type.Cast(-Convert.ToInt64(val), ws);
					}
					ws.AddError("Cannot negate a field of type " + type);
					return null;
				case TokenType.Not:
					if (type == typeof(bool)) { return !(bool)val; }
					ws.AddError("Cannot use not on a field of type " + type);
					return null;
			}

			ws.AddError("Unknown unary op encountered when calculating value: " + Op);
			return null;
		}
	}
}
