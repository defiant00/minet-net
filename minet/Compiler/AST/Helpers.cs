using Minet.Compiler.FAST;
using System;
using System.Collections.Generic;

namespace Minet.Compiler.AST
{
	public static class Helpers
	{
		public static bool CalcTypeList(this List<Variable> list, WalkState ws, string error)
		{
			IStatement type = null;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i].Type == null)
				{
					if (type == null)
					{
						ws.AddError(error);
						return false;
					}
					else { list[i].Type = type; }
				}
				else { type = list[i].Type; }
			}
			return true;
		}

		public static bool CalcTypeList(this List<Property> list, WalkState ws, string error)
		{
			IStatement type = null;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i].Type == null)
				{
					if (type == null)
					{
						ws.AddError(error);
						return false;
					}
					else { list[i].Type = type; }
				}
				else { type = list[i].Type; }
			}
			return true;
		}

		public static Type ToFASTType(this IStatement type, WalkState ws)
		{
			var t = type.GetType();
			if (t == typeof(Identifier))
			{
				var id = type as Identifier;
				switch (id.ToString())
				{
					case "bool":
						return typeof(bool);
					case "char":
						return typeof(char);
					case "f32":
						return typeof(float);
					case "f64":
						return typeof(double);
					case "f128":
						return typeof(decimal);
					case "i8":
						return typeof(sbyte);
					case "i16":
						return typeof(short);
					case "i32":
						return typeof(int);
					case "i64":
						return typeof(long);
					case "string":
						return typeof(string);
					case "u8":
						return typeof(byte);
					case "u16":
						return typeof(ushort);
					case "u32":
						return typeof(uint);
					case "u64":
						return typeof(ulong);
					default:
						ws.AddError("Unknown type string '" + id.ToString() + "'");
						return null;
				}
			}
			ws.AddError("Unknown type '" + t + "' encountered.");
			return null;
		}
	}
}
