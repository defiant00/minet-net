namespace Minet.Compiler.AST
{
	public partial class Class
	{
		public void GenFinal(WalkState ws)
		{
			if (!string.IsNullOrEmpty(ws.Namespace))
			{
				var cl = ws.Assembly.GetClass(ws.Namespace + "." + Name);
				foreach (var s in Statements)
				{
					var t = s.GetType();
					if (t == typeof(FunctionDef))
					{
						(s as FunctionDef).GenFinal(ws, cl);
					}
					else
					{
						ws.AddError("Unknown type '" + t.Name + "' in Class.GenFinal");
					}
				}
			}
			else
			{
				ws.AddError("No namespace specified for class " + Name);
			}
		}
	}

	public partial class File
	{
		public void GenFinal(WalkState ws)
		{
			ws.FileReset();
			foreach (var s in Statements)
			{
				var t = s.GetType();
				if (t == typeof(Class))
				{
					(s as Class).GenFinal(ws);
				}
				else if (t == typeof(Namespace))
				{
					var n = s as Namespace;
					ws.Namespace = n.Name.ToString();
				}
				else if (t == typeof(Use))
				{
					var u = s as Use;
					foreach (var p in u.Packages)
					{
						ws.Uses.Add(new FAST.UsePackage { Package = p.Pack.ToString(), Alias = p.Alias });
					}
				}
				else
				{
					ws.AddError("Unknown type '" + t.Name + "' in File.GenFinal");
				}
			}
		}
	}

	public partial class FunctionDef
	{
		public void GenFinal(WalkState ws, FAST.Class cl)
		{
			if (Params.CalcTypeList(ws, "Missing parameter types for function " + cl.Name + "." + Name))
			{
				var fn = new FAST.Function(Static, Name, Params.ToFAST(ws));

				if (!cl.AddFunction(fn))
				{
					ws.AddError("Function " + fn.FunctionSig + " already exists in class " + cl.Name);
				}

				foreach (var r in Returns) { fn.Returns.Add(r.ToFAST(ws)); }

				foreach (var s in Statements)
				{
					var t = s.GetType();
					if (t == typeof(ExprStmt))
					{

					}
					else
					{
						ws.AddError("Unknown type '" + t.Name + "' in FunctionDef.GenFinal");
					}
				}
			}
		}
	}
}
