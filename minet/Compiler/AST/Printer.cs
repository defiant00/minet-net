using System;
using System.Text;

namespace Minet.Compiler.AST
{
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

	public partial class Accessor
	{
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

	public partial class Array
	{
		public override string ToString() { return "[" + Dimensions + "]" + Type; }

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(this);
		}
	}

	public partial class ArrayCons
	{
		public override string ToString() { return "cons []" + Type; }

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(this);
			Size.Print(indent + 1);
		}
	}

	public partial class ArrayValueList
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("array value list");
			Vals.Print(indent + 1);
		}
	}

	public partial class Assign
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(Op);
			Left.Print(indent + 1);
			Right.Print(indent + 1);
		}
	}

	public partial class Binary
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(Op);
			Left.Print(indent + 1);
			Right.Print(indent + 1);
		}
	}

	public partial class Blank
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("_");
		}
	}

	public partial class Bool
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("bool " + Val);
		}
	}

	public partial class Break
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(string.IsNullOrEmpty(Label) ? "break" : "break " + Label);
		}
	}

	public partial class Char
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("char '" + Val + "'");
		}
	}

	public partial class Class
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.Write("class " + Name);
			if (TypeParams.Count > 0) { Console.Write("<" + string.Join(", ", TypeParams) + ">"); }
			Console.WriteLine();
			foreach (var s in Statements) { s.Print(indent + 1); }
		}
	}

	public partial class Constructor
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("cons " + Type);
			if (Params != null)
			{
				Helper.PrintIndent(indent + 1);
				Console.WriteLine("params");
				Params.Print(indent + 2);
			}
		}
	}

		public partial class Defer
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("defer");
			Expr.Print(indent + 1);
		}
	}

	public partial class Error
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("ERROR: " + Val);
		}
	}

	public partial class ExprList
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("expression list");
			foreach (var e in Expressions) { e.Print(indent + 1); }
		}
	}

	public partial class ExprStmt
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("expression statement");
			Expr.Print(indent + 1);
		}
	}

	public partial class File
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(Name);
			foreach (var s in Statements) { s.Print(indent + 1); }
		}
	}

	public partial class For
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			if (!string.IsNullOrEmpty(Label)) { Console.Write(Label + ": "); }
			Console.WriteLine("for " + string.Join(", ", Vars), " in");
			In.Print(indent + 2);
			foreach (var s in Statements) { s.Print(indent + 1); }
		}
	}

	public partial class FunctionCall
	{
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

	public partial class FunctionDef
	{
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

	public partial class FunctionSig
	{
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

	public partial class Identifier
	{
		public override string ToString() { return string.Join(".", Idents); }

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(this);
		}
	}

	public partial class IdentPart
	{
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

	public partial class If
	{
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

	public partial class Is
	{
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

	public partial class Loop
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(string.IsNullOrEmpty(Label) ? "loop" : Label + ": loop");
			foreach (var s in Statements) { s.Print(indent + 1); }
		}
	}

	public partial class Namespace
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("namespace " + Name);
		}
	}

	public partial class Number
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("number " + Val);
		}
	}

	public partial class Property
	{
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

	public partial class PropertySet
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("prop set: " + string.Join(", ", Props));
			if (Vals != null) { Vals.Print(indent + 1); }
		}
	}

	public partial class Return
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("return");
			Vals.Print(indent + 1);
		}
	}

	public partial class String
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("string '" + Val + "'");
		}
	}

	public partial class Unary
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(Op);
			Expr.Print(indent + 1);
		}
	}

	public partial class Use
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("use");
			foreach (var p in Packages) { p.Print(indent + 1); }
		}
	}

	public partial class UsePackage
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(string.IsNullOrEmpty(Alias) ? Pack.ToString() : Pack + " as " + Alias);
		}
	}

	public partial class Variable
	{
		public override string ToString() { return Type != null ? Name + " " + Type : Name; }

		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(this);
		}
	}

	public partial class VarSet
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine("var set");
			foreach (var l in Lines) { l.Print(indent + 1); }
		}
	}

	public partial class VarSetLine
	{
		public void Print(int indent)
		{
			Helper.PrintIndent(indent);
			Console.WriteLine(string.Join(", ", Vars));
			if (Vals != null) { Vals.Print(indent + 1); }
		}
	}
}
