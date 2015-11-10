using System;
using System.Collections.Generic;
using System.Text;

namespace Minet.Compiler.AST
{
	public interface General { }
	public interface Expression : General { }
	public interface Statement : General { }

	public class Accessor : Expression
	{
		public Expression Object, Index;
	}

	public class AccessorRange : Expression
	{
		public Expression Object, Low, High;
	}

	public class Array : Statement
	{
		public Statement Type;
		public override string ToString() { return "[]" + Type; }
	}

	public class ArrayCons : Expression
	{
		public Statement Type;
		public Expression Size;
	}

	public class ArrayValueList : Expression
	{
		public Expression Vals;
	}

	public class Assign : Statement
	{
		public Expression Left, Right;
		public TokenType Op;
	}

	public class Binary : Expression
	{
		public Expression Left, Right;
		public TokenType Op;
	}

	public class Blank : Expression { }

	public class Bool : Expression
	{
		public bool Val;
	}

	public class Break : Statement
	{
		public string Label;
	}

	public class Char : Expression
	{
		public string Val;
	}

	public class Class : Statement
	{
		public string Name;
		public List<string> TypeParams = new List<string>();
		public List<Statement> Withs = new List<Statement>();
		public List<Statement> Statements = new List<Statement>();
	}

	public class Constructor : Expression
	{
		public Expression Type;
		public List<Statement> Params = new List<Statement>();
	}

	public class Defer : Statement
	{
		public Expression Expr;
	}

	public class Error : Expression, Statement
	{
		public string Val;
	}

	public class ExprList : Expression
	{
		public List<Expression> Expressions = new List<Expression>();
	}

	public class ExprStmt : Statement
	{
		public Expression Expr;
	}

	public class File : Statement
	{
		public string Name;
		public List<Statement> Statements = new List<Statement>();
	}

	public class For : Statement
	{
		public string Label;
		public List<string> Vars = new List<string>();
		public Expression In;
		public List<Statement> Statements = new List<Statement>();
	}

	public class FunctionCall : Expression
	{
		public Expression Function, Params;
	}

	public class FunctionDef : Expression, Statement
	{
		public bool Static;
		public string Name;
		public List<Parameter> Params = new List<Parameter>();
		public List<Statement> Returns = new List<Statement>();
		public List<Statement> Statements = new List<Statement>();
	}

	public class FunctionSig : Expression, Statement
	{
		public List<Statement> Params = new List<Statement>();
		public List<Statement> Returns = new List<Statement>();
		public override string ToString()
		{
			var sb = new StringBuilder("fn(");
			sb.Append(string.Join(", ", Params));
			sb.Append(")");
			if (Returns.Count > 0)
			{
				sb.Append(" ");
				if (Returns.Count > 1) { sb.Append("("); }
				sb.Append(string.Join(", ", Returns));
				if (Returns.Count > 1) { sb.Append(")"); }
			}
			return sb.ToString();
		}
	}

	public class Identifier : Expression, Statement
	{
		public List<IdentPart> Idents = new List<IdentPart>();
		public override string ToString() { return string.Join(".", Idents); }
	}

	public class IdentPart
	{
		public string Name;
		public List<Statement> TypeParams = new List<Statement>();
		public override string ToString()
		{
			return TypeParams.Count > 0 ? Name + "<" + string.Join(", ", TypeParams) + ">" : Name;
		}
	}

	public class If : Statement
	{
		public Expression Condition;
		public Statement With;
		public List<Statement> Statements = new List<Statement>();
	}

	public class Interface : Statement
	{
		public string Name;
		public List<Statement> Withs = new List<Statement>();
		public List<Statement> FuncSigs = new List<Statement>();
	}

	public class IntfFuncSig : Statement
	{
		public string Name;
		public List<Statement> Params = new List<Statement>();
		public List<Statement> Returns = new List<Statement>();
	}

	public class Iota : Expression, Statement { }

	public class Is : Statement
	{
		public Expression Condition;
		public List<Statement> Statements = new List<Statement>();
	}

	public class KeyVal : Statement
	{
		public string Key;
		public Expression Val;
	}

	public class Loop : Statement
	{
		public string Label;
		public List<Statement> Statements = new List<Statement>();
	}

	public class Namespace : Statement
	{
		public Identifier Name;
	}

	public class Number : Expression
	{
		public string Val;
	}

	public class Parameter
	{
		public string Name;
		public Statement Type;
		public override string ToString() { return Type != null ? Name + " " + Type : Name; }
	}

	public class Property
	{
		public bool Static;
		public string Name;
		public Statement Type;
	}

	public class PropertySet : Statement
	{
		public List<Property> Props = new List<Property>();
		public Expression Vals;
	}

	public class Return : Statement
	{
		public Expression Vals;
	}

	public class String : Expression
	{
		public string Val;
	}

	public class TypeAlias : Statement
	{
		public Statement Type;
		public string Alias;
	}

	public class Unary : Expression
	{
		public Expression Expr;
		public TokenType Op;
	}

	public class Use : Statement
	{
		public List<UsePackage> Packages = new List<UsePackage>();
	}

	public class UsePackage : Statement
	{
		public Identifier Pack;
		public string Alias;
	}

	public class Variable
	{
		public string Name;
		public Statement Type;
	}

	public class VarSet : Statement
	{
		public List<VarSetLine> Lines = new List<VarSetLine>();
	}

	public class VarSetLine : Statement
	{
		public List<Variable> Vars = new List<Variable>();
		public Expression Vals;
	}
}
