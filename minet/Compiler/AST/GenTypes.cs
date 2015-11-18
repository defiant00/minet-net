namespace Minet.Compiler.AST
{
	public partial class Class
	{
		public void GenTypes(WalkState ws)
		{
			if (!string.IsNullOrEmpty(ws.Namespace))
			{
				ws.Assembly.GetClass(ws.Namespace, Name);
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
					(s as Class).GenTypes(ws);
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
