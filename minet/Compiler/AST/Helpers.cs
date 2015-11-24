using System;

namespace Minet.Compiler.AST
{
	public static class Helpers
	{
		public static Type CalcType(this IExpression expr, WalkState ws)
		{
			var t = expr.GetType();
			if (t == typeof(Bool)) { return typeof(bool); }
			else if (t == typeof(Char)) { return typeof(char); }
			else if (t == typeof(Float)) { return typeof(float); }
			else if (t == typeof(Integer)) { return typeof(int); }
			else if (t == typeof(String)) { return typeof(string); }
			else
			{
				ws.AddError("Unknown type '" + t + "' when calculating type.");
				return null;
			}
		}

		public static bool CalcTypeList(this PropertySet ps, WalkState ws)
		{
			Type type = null;
			for (int i = ps.Props.Count - 1; i >= 0; i--)
			{
				if (ps.Props[i].Type == null)
				{
					ps.Props[i].SystemType = type;
				}
				else
				{
					type = ps.Props[i].Type.ToType(ws);
					ps.Props[i].SystemType = type;
				}
			}

			if (ps.Vals != null)
			{
				var el = ps.Vals as ExprList;
				if (ps.Props.Count == el.Expressions.Count)
				{
					for (int i = 0; i < ps.Props.Count; i++)
					{
						if (ps.Props[i].SystemType == null)
						{
							ps.Props[i].SystemType = el.Expressions[i].CalcType(ws);
						}
					}
				}
			}

			bool retVal = true;
			foreach (var p in ps.Props)
			{
				if (p.SystemType == null)
				{
					ws.AddError("Type could not be determined for " + p.Name);
					retVal = false;
				}
			}
			return retVal;
		}

		public static object GetLiteralVal(this IExpression expr, Type type, WalkState ws)
		{
			var t = expr.GetType();
			if (t == typeof(Bool) && type == typeof(bool))
			{
				return (expr as Bool).Val;
			}
			else if (t == typeof(Char) && type == typeof(char))
			{
				return (expr as Char).Val[0];
			}
			else if (t == typeof(Float) && type == typeof(float))
			{
				return Convert.ToSingle((expr as Float).Val);
			}
			else if (t == typeof(Float) && type == typeof(double))
			{
				return Convert.ToDouble((expr as Float).Val);
			}
			else if (t == typeof(Integer) && type == typeof(sbyte))
			{
				return Convert.ToSByte((expr as Integer).Val);
			}
			else if (t == typeof(Integer) && type == typeof(short))
			{
				return Convert.ToInt16((expr as Integer).Val);
			}
			else if (t == typeof(Integer) && type == typeof(int))
			{
				return Convert.ToInt32((expr as Integer).Val);
			}
			else if (t == typeof(Integer) && type == typeof(long))
			{
				return Convert.ToInt64((expr as Integer).Val);
			}
			else if (t == typeof(Integer) && type == typeof(byte))
			{
				return Convert.ToByte((expr as Integer).Val);
			}
			else if (t == typeof(Integer) && type == typeof(ushort))
			{
				return Convert.ToUInt16((expr as Integer).Val);
			}
			else if (t == typeof(Integer) && type == typeof(uint))
			{
				return Convert.ToUInt32((expr as Integer).Val);
			}
			else if (t == typeof(Integer) && type == typeof(ulong))
			{
				return Convert.ToUInt64((expr as Integer).Val);
			}
			else if (t == typeof(String))
			{
				return (expr as String).Val;
			}
			else
			{
				ws.AddError("Unknown expression '" + t + "' for type '" + type + "' encountered.");
				return null;
			}
		}

		public static Type ToType(this IStatement type, WalkState ws)
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
