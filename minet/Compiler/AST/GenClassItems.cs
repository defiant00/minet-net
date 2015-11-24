using Minet.Compiler.FAST;
using System.Reflection;

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
				else if (t == typeof(PropertySet))
				{
					var ps = s as PropertySet;
					ps.Props.CalcTypeList(ws, "Missing type qualifier in class " + c.Name);
					if (ws.Errors.Count == 0)
					{
						foreach (var p in ps.Props)
						{
							var prop = p.Static ? new Constant(p, ws) : new FAST.Field(p, ws);
							prop.CreateField(c);
							c.Fields.Add(prop);
						}

						if (ps.Vals != null)
						{
							var el = ps.Vals as ExprList;
							if (ps.Props.Count != el.Expressions.Count)
							{
								ws.AddError("Mismatched property/value counts: " + ps.Props.Count + " != " + el.Expressions.Count);
							}
							else
							{

							}
						}
					}
				}
				else
				{
					//ws.AddError("Unknown class statement: " + t);
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
