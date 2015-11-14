using System;

namespace Minet.Compiler.AST
{
	public partial class Accessor : Expression
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Array : Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class ArrayCons : Expression
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class ArrayValueList : Expression
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Assign : Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Binary : Expression
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Blank : Expression
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Bool : Expression
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Break : Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Char : Expression
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Class : Statement
	{
		public void GenFinal(WalkState state)
		{
			state.CurrentClass = state.Assembly.GetClass(Name);
			foreach (var s in Statements) { s.GenFinal(state); }
			state.CurrentClass = null;
		}
	}

	public partial class Constructor : Expression
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Defer : Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Error : Expression, Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class ExprList : Expression
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class ExprStmt : Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class File : Statement
	{
		public void GenFinal(WalkState state)
		{
			state.Reset();
			foreach (var s in Statements) { s.GenFinal(state); }
		}
	}

	public partial class For : Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class FunctionCall : Expression
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class FunctionDef : Expression, Statement
	{
		public void GenFinal(WalkState state)
		{
			Params.CalcTypeList(state, "Missing type declaration for function parameters in " + Name);
			var fn = new FAST.Function { Name = Name, Static = Static };
			state.CurrentClass.Functions.Add(fn);
			state.CurrentFunction = fn;
			foreach (var s in Statements) { s.GenFinal(state); }
			state.CurrentFunction = null;
		}
	}

	public partial class FunctionSig : Expression, Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Identifier : Expression, Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class IdentPart
	{
		public void GenIL(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class If : Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Is : Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Loop : Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Namespace : Statement
	{
		public void GenFinal(WalkState state)
		{
			state.CurrentNamespace = Name.ToString();
		}
	}

	public partial class Number : Expression
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Property
	{
		public void GenIL(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class PropertySet : Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Return : Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class String : Expression
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Unary : Expression
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Use : Statement
	{
		public void GenFinal(WalkState state)
		{
			foreach (var p in Packages) { state.CurrentUses.Add(p.Pack.ToString()); }
		}
	}

	public partial class UsePackage : Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Variable : Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class VarSet : Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}

	public partial class VarSetLine : Statement
	{
		public void GenFinal(WalkState state)
		{
			throw new NotImplementedException();
		}
	}
}
