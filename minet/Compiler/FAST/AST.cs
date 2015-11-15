using System.Collections.Generic;

namespace Minet.Compiler.FAST
{
	public interface Type { }

	public class ArrayType : Type
	{
		public Type Type;
		public int Dimensions;

		public override string ToString()
		{
			string ret = "[";
			for (int i = 1; i < Dimensions; i++)
			{
				ret += ",";
			}
			ret += "]" + Type;
			return ret;
		}
	}

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
		public bool Static;
		public string Name;
		public List<Variable> Parameters;
		public List<Type> Returns = new List<Type>();

		public Function(bool stat, string name, List<Variable> pars)
		{
			Static = stat;
			Name = name;
			Parameters = pars;
		}
	}

	public class FunctionType : Type
	{
		public override string ToString() { return "fn()"; }
	}

	public class GenType : Type
	{
		public string Identifier;

		public override string ToString() { return Identifier; }
	}

	public class UsePackage
	{
		public string Package, Alias;
	}

	public class Variable
	{
		public string Name;
		public Type Type;
	}
}
