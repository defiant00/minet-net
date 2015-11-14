using System.Collections.Generic;

namespace Minet.Compiler.AST
{
	public interface General
	{
		void Print(int indent);
		void GenFinal(WalkState state);
	}

	public interface Expression : General { }
	public interface Statement : General { }

	public partial class Accessor : Expression
	{
		public Expression Object, Index;
	}

	public partial class Array : Statement
	{
		public Statement Type;
		public int Dimensions;
	}

	public partial class ArrayCons : Expression
	{
		public Statement Type;
		public Expression Size;
	}

	public partial class ArrayValueList : Expression
	{
		public Expression Vals;
	}

	public partial class Assign : Statement
	{
		public Expression Left, Right;
		public TokenType Op;
	}

	public partial class Binary : Expression
	{
		public Expression Left, Right;
		public TokenType Op;
	}

	public partial class Blank : Expression { }

	public partial class Bool : Expression
	{
		public bool Val;
	}

	public partial class Break : Statement
	{
		public string Label;
	}

	public partial class Char : Expression
	{
		public string Val;
	}

	public partial class Class : Statement
	{
		public string Name;
		public List<string> TypeParams = new List<string>();
		public List<Statement> Statements = new List<Statement>();
	}

	public partial class Constructor : Expression
	{
		public Expression Type, Params;
	}

	public partial class Defer : Statement
	{
		public Expression Expr;
	}

	public partial class Error : Expression, Statement
	{
		public string Val;
	}

	public partial class ExprList : Expression
	{
		public List<Expression> Expressions = new List<Expression>();
	}

	public partial class ExprStmt : Statement
	{
		public Expression Expr;
	}

	public partial class File : Statement
	{
		public string Name;
		public List<Statement> Statements = new List<Statement>();
	}

	public partial class For : Statement
	{
		public string Label;
		public List<Variable> Vars = new List<Variable>();
		public Expression In;
		public List<Statement> Statements = new List<Statement>();
	}

	public partial class FunctionCall : Expression
	{
		public Expression Function, Params;
	}

	public partial class FunctionDef : Expression, Statement
	{
		public bool Static;
		public string Name;
		public List<Variable> Params = new List<Variable>();
		public List<Statement> Returns = new List<Statement>();
		public List<Statement> Statements = new List<Statement>();
	}

	public partial class FunctionSig : Expression, Statement
	{
		public List<Statement> Params = new List<Statement>();
		public List<Statement> Returns = new List<Statement>();
	}

	public partial class Identifier : Expression, Statement
	{
		public List<IdentPart> Idents = new List<IdentPart>();
	}

	public partial class IdentPart
	{
		public string Name;
		public List<Statement> TypeParams = new List<Statement>();
	}

	public partial class If : Statement
	{
		public Expression Condition;
		public Statement With;
		public List<Statement> Statements = new List<Statement>();
	}

	public partial class Is : Statement
	{
		public Expression Condition;
		public List<Statement> Statements = new List<Statement>();
	}

	public partial class Loop : Statement
	{
		public string Label;
		public List<Statement> Statements = new List<Statement>();
	}

	public partial class Namespace : Statement
	{
		public Identifier Name;
	}

	public partial class Number : Expression
	{
		public string Val;
	}

	public partial class Property
	{
		public bool Static;
		public string Name;
		public Statement Type;
	}

	public partial class PropertySet : Statement
	{
		public List<Property> Props = new List<Property>();
		public Expression Vals;
	}

	public partial class Return : Statement
	{
		public Expression Vals;
	}

	public partial class String : Expression
	{
		public string Val;
	}

	public partial class Unary : Expression
	{
		public Expression Expr;
		public TokenType Op;
	}

	public partial class Use : Statement
	{
		public List<UsePackage> Packages = new List<UsePackage>();
	}

	public partial class UsePackage : Statement
	{
		public Identifier Pack;
		public string Alias;
	}

	public partial class Variable : Statement
	{
		public string Name;
		public Statement Type;
	}

	public partial class VarSet : Statement
	{
		public List<VarSetLine> Lines = new List<VarSetLine>();
	}

	public partial class VarSetLine : Statement
	{
		public List<Variable> Vars = new List<Variable>();
		public Expression Vals;
	}
}
