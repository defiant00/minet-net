using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Minet.Compiler.AST
{
	public partial class Accessor
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class ArrayCons
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class ArrayValueList
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class Binary
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class Blank
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class Bool
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class Char
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class Constructor
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class Error
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class ExprList
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class Float
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class FunctionCall
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class FunctionDef
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class FunctionSig
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class Identifier
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class Integer
	{
		public void Emit(ILGenerator il, WalkState ws) {
			var num = Convert.ToInt32(Val);
			switch (num)
			{
				case -1:
					il.Emit(OpCodes.Ldc_I4_M1);
					break;
				case 0:
					il.Emit(OpCodes.Ldc_I4_0);
					break;
				case 1:
					il.Emit(OpCodes.Ldc_I4_1);
					break;
				case 2:
					il.Emit(OpCodes.Ldc_I4_2);
					break;
				case 3:
					il.Emit(OpCodes.Ldc_I4_3);
					break;
				case 4:
					il.Emit(OpCodes.Ldc_I4_4);
					break;
				case 5:
					il.Emit(OpCodes.Ldc_I4_5);
					break;
				case 6:
					il.Emit(OpCodes.Ldc_I4_6);
					break;
				case 7:
					il.Emit(OpCodes.Ldc_I4_7);
					break;
				case 8:
					il.Emit(OpCodes.Ldc_I4_8);
					break;
				default:
					il.Emit(OpCodes.Ldc_I4, num);
					break;
			}
		}
	}

	public partial class String
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}

	public partial class Unary
	{
		public void Emit(ILGenerator il, WalkState ws) { }
	}
}
