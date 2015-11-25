using System;

namespace Minet.Compiler.AST
{
	public static class Helpers
	{
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
				var types = (ps.Vals as ExprList).CalcTypes(ws);
				if (ps.Props.Count == types.Count)
				{
					for (int i = 0; i < ps.Props.Count; i++)
					{
						if (ps.Props[i].SystemType == null)
						{
							ps.Props[i].SystemType = types[i];
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

		public static object Cast(this Type type, object val, WalkState ws)
		{
			if (type == typeof(bool)) { return Convert.ToBoolean(val); }
			else if (type == typeof(char)) { return Convert.ToChar(val); }
			else if (type == typeof(float)) { return Convert.ToSingle(val); }
			else if (type == typeof(double)) { return Convert.ToDouble(val); }
			else if (type == typeof(sbyte)) { return Convert.ToSByte(val); }
			else if (type == typeof(short)) { return Convert.ToInt16(val); }
			else if (type == typeof(int)) { return Convert.ToInt32(val); }
			else if (type == typeof(long)) { return Convert.ToInt64(val); }
			else if (type == typeof(byte)) { return Convert.ToByte(val); }
			else if (type == typeof(ushort)) { return Convert.ToUInt16(val); }
			else if (type == typeof(uint)) { return Convert.ToUInt32(val); }
			else if (type == typeof(ulong)) { return Convert.ToUInt64(val); }
			else if (type == typeof(string)) { return Convert.ToString(val); }

			ws.AddError("Cannot convert '" + val + "' to " + type);
			return null;
		}

		public static bool IsFloatingPointType(this Type type)
		{
			return type == typeof(float) || type == typeof(double);
		}

		public static bool IsIntegerType(this Type type)
		{
			return type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long) ||
				   type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);
		}

		public static Type ToType(this IStatement type, WalkState ws)
		{
			var t = type.GetType();
			if (t == typeof(Identifier))
			{
				var id = type as Identifier;
				switch (id.ToString())
				{
					case "any":
						return typeof(object);
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
