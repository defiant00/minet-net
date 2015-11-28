using Minet.Compiler.FAST;

namespace Minet.Compiler.AST
{
	public partial class Class
	{
		public void GenClassItems(WalkState ws, ITypeContainer parent)
		{
			var c = parent.GetClass(ws.Assembly, ws.Namespace, Name, Public);
			foreach (var s in Statements)
			{
				var t = s.GetType();
				if (t == typeof(Class))
				{
					(s as Class).GenClassItems(ws, c);
				}
				else if (t == typeof(FunctionDef))
				{
					c.Functions.Add(new Function(c, s as FunctionDef, ws));
				}
				else if (t == typeof(PropertySet))
				{
					var ps = s as PropertySet;
					if (ps.CalcTypeList(ws))
					{
						ExprList el = null;
						if (ps.Vals != null)
						{
							el = ps.Vals as ExprList;
							if (ps.Props.Count != el.Expressions.Count)
							{
								ws.AddError("Mismatched property/value counts: " + ps.Props.Count + " != " + el.Expressions.Count);
								el = null;
							}
						}

						for (int i = 0; i < ps.Props.Count; i++)
						{
							var p = ps.Props[i];
							var prop = p.Static ? new Constant(p, ws) : new Field(p, ws);
							prop.CreateField(c);
							if (el != null)
							{
								object val = el.Expressions[i].Calculate(ws);
								if (val != null)
								{
									prop.SetDefault(p.SystemType.Cast(val, ws));
								}
								// TODO - Do something with default values for fields.
							}
							c.Fields.Add(prop);
						}
					}
				}
				else
				{
					ws.AddError("Unknown class statement: " + t);
				}
			}
		}
	}

	public partial class File
	{
		public void GenClassItems(WalkState ws)
		{
			ws.FileReset();
			foreach (var s in Statements)
			{
				var t = s.GetType();
				if (t == typeof(Class))
				{
					(s as Class).GenClassItems(ws, ws.Assembly);
				}
				else if (t == typeof(Namespace))
				{
					var n = s as Namespace;
					ws.Namespace = n.Name.ToString();
				}
			}
		}
	}
}
