using Minet.Compiler.FAST;

namespace Minet.Compiler.AST
{
	public partial class Class
	{
		public void GenTypes(WalkState ws, ITypeContainer parent)
		{
			if (!string.IsNullOrEmpty(ws.Namespace))
			{
				var c = parent.GetClass(ws.Assembly, ws.Namespace, Name, Public);
				foreach(var s in Statements)
				{
					var t = s.GetType();
					if (t == typeof(Class))
					{
						(s as Class).GenTypes(ws, c);
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
		public void GenTypes(WalkState ws)
		{
			ws.FileReset();
			foreach (var s in Statements)
			{
				var t = s.GetType();
				if (t == typeof(Class))
				{
					(s as Class).GenTypes(ws, ws.Assembly);
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
