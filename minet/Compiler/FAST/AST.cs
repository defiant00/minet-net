using System.Collections.Generic;

namespace Minet.Compiler.FAST
{
	public class Assembly
	{
		public List<Class> Classes = new List<Class>();

		public Class GetClass(string name)
		{
			foreach (var c in Classes)
			{
				if (c.Name == name) { return c; }
			}
			var cl = new Class { Name = name };
            Classes.Add(cl);
			return cl;
		}
	}

	public class Class
	{
		public string Name;
		public List<Function> Functions = new List<Function>();
	}

	public class Function
	{
		public string Name;
		public bool Static;
	}
}
