using System.Collections.Generic;

namespace Minet.Compiler.AST
{
	public static class ExtensionMethods
	{
		public static bool CalcTypeList(this List<Variable> list, WalkState state, string error)
		{
			Statement type = null;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i].Type == null)
				{
					if (type == null)
					{
						state.AddError(error);
						return false;
					}
					else { list[i].Type = type; }
				}
				else { type = list[i].Type; }
			}
			return true;
		}

		public static List<FAST.Variable> ToFAST(this List<Variable> list, WalkState ws)
		{
			var ret = new List<FAST.Variable>();
			foreach (var v in list)
			{
				ret.Add(new FAST.Variable { Name = v.Name, Type = v.Type.ToFAST(ws) });
			}
			return ret;
		}

		public static FAST.IType ToFAST(this Statement type, WalkState ws)
		{
			var t = type.GetType();
			if (t == typeof(Array))
			{
				var ar = type as Array;
				return new FAST.ArrayType { Dimensions = ar.Dimensions, Type = ar.Type.ToFAST(ws) };
			}
			else if (t == typeof(FunctionSig))
			{
				return new FAST.FunctionType();
			}
			else if (t == typeof(Identifier))
			{
				return new FAST.GenType { Identifier = (type as Identifier).ToString() };
			}
			ws.AddError("Unknown type '" + t.Name + "' in Statement.ToFAST");
			return new FAST.GenType { Identifier = "error" };
		}
	}
}
