using System;
using System.Collections.Generic;
using System.Text;

namespace Minet.Compiler.AST
{
	public interface General
	{
		void Print(int indent);
	}

	public interface Expression : General { }
	public interface Statement : General { }

	public static class Helper
	{
		public static void PrintIndent(int indent)
		{
			for (int i = 0; i < indent; i++)
			{
				Console.Write("|   ");
			}
		}
	}

	public class Accessor : Expression
	{
		public Expression Object, Index;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("accessor");
			Object.Print(indent + 2);
			Helper.PrintIndent(indent + 1);
			Console.WriteLine("index");
			Index.Print(indent + 2);
		}
	}

	public class Array : Statement
	{
		public Statement Type;
		public int Dimensions;
		public override string ToString() { return "[" + Dimensions + "]" + Type; }

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(this);
		}
	}

	public class ArrayCons : Expression
	{
		public Statement Type;
		public Expression Size;
		public override string ToString() { return "cons []" + Type; }

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(this);
			Size.Print(indent + 1);
		}
	}

	public class ArrayValueList : Expression
	{
		public Expression Vals;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("array value list");
			Vals.Print(indent + 1);
		}
	}

	public class Assign : Statement
	{
		public Expression Left, Right;
		public TokenType Op;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(Op);
			Left.Print(indent + 1);
			Right.Print(indent + 1);
		}
	}

	public class Binary : Expression
	{
		public Expression Left, Right;
		public TokenType Op;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(Op);
			Left.Print(indent + 1);
			Right.Print(indent + 1);
		}
	}

	public class Blank : Expression
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("_");
		}
	}

	public class Bool : Expression
	{
		public bool Val;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("bool " + Val);
		}
	}

	public class Break : Statement
	{
		public string Label;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(string.IsNullOrEmpty(Label) ? "break" : "break " + Label);
		}
	}

	public class Char : Expression
	{
		public string Val;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("char '" + Val + "'");
		}
	}

	public class Class : Statement
	{
		public string Name;
		public List<string> TypeParams = new List<string>();
		public List<Statement> Withs = new List<Statement>();
		public List<Statement> Statements = new List<Statement>();

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.Write("class " + Name);
			if (TypeParams.Count > 0) { Console.Write("<" + string.Join(", ", TypeParams) + ">"); }
			if (Withs.Count > 0) { Console.Write(" with " + string.Join(", ", Withs)); }
			Console.WriteLine();
			foreach (var s in Statements) { s.Print(indent + 1); }
		}
	}

	public class Defer : Statement
	{
		public Expression Expr;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("defer");
			Expr.Print(indent + 1);
		}
	}

	public class Error : Expression, Statement
	{
		public string Val;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("ERROR: " + Val);
		}
	}

	public class ExprList : Expression
	{
		public List<Expression> Expressions = new List<Expression>();

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("expression list");
			foreach (var e in Expressions) { e.Print(indent + 1); }
		}
	}

	public class ExprStmt : Statement
	{
		public Expression Expr;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("expression statement");
			Expr.Print(indent + 1);
		}
	}

	public class File : Statement
	{
		public string Name;
		public List<Statement> Statements = new List<Statement>();

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(Name);
			foreach (var s in Statements) { s.Print(indent + 1); }
		}
	}

	public class For : Statement
	{
		public string Label;
		public List<Variable> Vars = new List<Variable>();
		public Expression In;
		public List<Statement> Statements = new List<Statement>();

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			if (!string.IsNullOrEmpty(Label)) { Console.Write(Label + ": "); }
			Console.WriteLine("for " + string.Join(", ", Vars), " in");
			In.Print(indent + 2);
			foreach (var s in Statements) { s.Print(indent + 1); }
		}
	}

	public class FunctionCall : Expression
	{
		public Expression Function, Params;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("func");
			Function.Print(indent + 2);
			if (Params != null)
			{
				Helper.PrintIndent(indent + 1);
				Console.WriteLine("params");
				Params.Print(indent + 2);
			}
		}
	}

	public class FunctionDef : Expression, Statement
	{
		public bool Static;
		public string Name;
		public List<Variable> Params = new List<Variable>();
		public List<Statement> Returns = new List<Statement>();
		public List<Statement> Statements = new List<Statement>();

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			if (Static) { Console.Write("static "); }
			Console.Write(string.IsNullOrEmpty(Name) ? "fn" : Name);
			Console.Write("(");
			Console.Write(string.Join(", ", Params));
			Console.Write(")");
			if (Returns.Count > 0)
			{
				Console.Write(" ");
				if (Returns.Count > 1) { Console.Write("("); }
				Console.Write(string.Join(", ", Returns));
				if (Returns.Count > 1) { Console.Write(")"); }
			}
			Console.WriteLine();
			foreach (var s in Statements) { s.Print(indent + 1); }
		}
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

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(this);
		}
	}

	public class Identifier : Expression, Statement
	{
		public List<IdentPart> Idents = new List<IdentPart>();
		public override string ToString() { return string.Join(".", Idents); }

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(this);
		}
	}

	public class IdentPart
	{
		public string Name;
		public List<Statement> TypeParams = new List<Statement>();

		public override string ToString()
		{
			return TypeParams.Count > 0 ? Name + "<" + string.Join(", ", TypeParams) + ">" : Name;
		}

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(this);
		}
	}

	public class If : Statement
	{
		public Expression Condition;
		public Statement With;
		public List<Statement> Statements = new List<Statement>();

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("if");
			if (Condition != null) { Condition.Print(indent + 1); }
			else
			{
				Helper.PrintIndent(indent + 1);
				Console.WriteLine("(implicit true)");
			}
			if (With != null)
			{
				Helper.PrintIndent(indent + 1);
				Console.WriteLine("with");
				With.Print(indent + 2);
			}
			Helper.PrintIndent(indent + 1);
			Console.WriteLine("then");
			foreach (var s in Statements) { s.Print(indent + 2); }
		}
	}

	public class Interface : Statement
	{
		public string Name;
		public List<Statement> Withs = new List<Statement>();
		public List<Statement> FuncSigs = new List<Statement>();

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.Write("interface " + Name);
			if (Withs.Count > 0)
			{
				Console.Write(" with ");
				Console.Write(string.Join(", ", Withs));
			}
			Console.WriteLine();
			foreach (var f in FuncSigs) { f.Print(indent + 1); }
		}
	}

	public class IntfFuncSig : Statement
	{
		public string Name;
		public List<Statement> Params = new List<Statement>();
		public List<Statement> Returns = new List<Statement>();

		public override string ToString()
		{
			var sb = new StringBuilder(Name);
			sb.Append("(");
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

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(this);
		}
	}

	public class Is : Statement
	{
		public Expression Condition;
		public List<Statement> Statements = new List<Statement>();

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("is");
			Condition.Print(indent + 1);
			Helper.PrintIndent(indent + 1);
			Console.WriteLine("then");
			foreach (var s in Statements) { s.Print(indent + 2); }
		}
	}

	public class Loop : Statement
	{
		public string Label;
		public List<Statement> Statements = new List<Statement>();

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(string.IsNullOrEmpty(Label) ? "loop" : Label + ": loop");
			foreach (var s in Statements) { s.Print(indent + 1); }
		}
	}

	public class Namespace : Statement
	{
		public Identifier Name;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("namespace " + Name);
		}
	}

	public class Number : Expression
	{
		public string Val;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("number " + Val);
		}
	}

	public class Property
	{
		public bool Static;
		public string Name;
		public Statement Type;
		public override string ToString()
		{
			string ret = Static ? "static " + Name : Name;
			if (Type != null) { ret += " " + Type; }
			return ret;
		}

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(this);
		}
	}

	public class PropertySet : Statement
	{
		public List<Property> Props = new List<Property>();
		public Expression Vals;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("prop set: " + string.Join(", ", Props));
			if (Vals != null) { Vals.Print(indent + 1); }
		}
	}

	public class Return : Statement
	{
		public Expression Vals;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("return");
			Vals.Print(indent + 1);
		}
	}

	public class String : Expression
	{
		public string Val;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("string '" + Val + "'");
		}
	}

	public class Unary : Expression
	{
		public Expression Expr;
		public TokenType Op;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(Op);
			Expr.Print(indent + 1);
		}
	}

	public class Use : Statement
	{
		public List<UsePackage> Packages = new List<UsePackage>();

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("use");
			foreach (var p in Packages) { p.Print(indent + 1); }
		}
	}

	public class UsePackage : Statement
	{
		public Identifier Pack;
		public string Alias;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(string.IsNullOrEmpty(Alias) ? Pack.ToString() : Pack + " as " + Alias);
		}
	}

	public class Variable : Statement
	{
		public string Name;
		public Statement Type;
		public override string ToString() { return Type != null ? Name + " " + Type : Name; }

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(this);
		}
	}

	public class VarSet : Statement
	{
		public List<VarSetLine> Lines = new List<VarSetLine>();

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("var set");
			foreach (var l in Lines) { l.Print(indent + 1); }
		}
	}

	public class VarSetLine : Statement
	{
		public List<Variable> Vars = new List<Variable>();
		public Expression Vals;

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(string.Join(", ", Vars));
			if (Vals != null) { Vals.Print(indent + 1); }
		}
	}
}
