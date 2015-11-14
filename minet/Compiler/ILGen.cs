using System;
using System.Reflection;

namespace Minet.Compiler.AST
{
	public partial class Accessor : Expression
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Array : Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class ArrayCons : Expression
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class ArrayValueList : Expression
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Assign : Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Binary : Expression
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Blank : Expression
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Bool : Expression
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Break : Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Char : Expression
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Class : Statement
	{
		public void GenIL(GenState state)
		{
			if (string.IsNullOrEmpty(state.CurrentFile.Namespace))
			{
				state.AddError("No namespace specified in " + state.CurrentFile.Name);
			}
			else
			{
				state.CurrentClass = this;

				var attr = TypeAttributes.Class | TypeAttributes.Public;
				state.TypeBuilder = state.ModuleBuilder.DefineType(state.CurrentFile.Namespace + "." + Name, attr);

				foreach (var s in Statements) { s.GenIL(state); }
			}
		}
	}

	public partial class Defer : Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Error : Expression, Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class ExprList : Expression
	{
		public void GenIL(GenState state)
		{
			foreach(var e in Expressions) { e.GenIL(state); }
		}
	}

	public partial class ExprStmt : Statement
	{
		public void GenIL(GenState state)
		{
			Expr.GenIL(state);
		}
	}

	public partial class File : Statement
	{
		public void GenIL(GenState state)
		{
			state.CurrentFile = this;

			foreach (var s in Statements) { s.GenIL(state); }
		}
	}

	public partial class For : Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class FunctionCall : Expression
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class FunctionDef : Expression, Statement
	{
		public void GenIL(GenState state)
		{
			state.CurrentFunc = this;
			Params.CalcTypeList(state, "Missing type in parameter for " + Name);
			foreach(var s in Statements) { s.GenIL(state); }
		}
	}

	public partial class FunctionSig : Expression, Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Identifier : Expression, Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class IdentPart
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class If : Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Is : Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Loop : Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Namespace : Statement
	{
		public void GenIL(GenState state)
		{
			state.CurrentFile.Namespace = Name.ToString();
		}
	}

	public partial class Number : Expression
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Property
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class PropertySet : Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Return : Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class String : Expression
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Unary : Expression
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Use : Statement
	{
		public void GenIL(GenState state)
		{
			state.CurrentFile.Uses.Add(this);
		}
	}

	public partial class UsePackage : Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Variable : Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class VarSet : Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class VarSetLine : Statement
	{
		public void GenIL(GenState state)
		{
			throw new NotImplementedException();
		}
	}
}
