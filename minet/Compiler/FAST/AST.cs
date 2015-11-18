using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

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
		public string Name, FileName;
		public List<Class> Classes = new List<Class>();
		public AssemblyBuilder AssemblyBuilder;
		public ModuleBuilder ModuleBuilder;

		public Assembly(string name, string fileName)
		{
			Name = name;
			FileName = fileName;
			AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Save);
			ModuleBuilder = AssemblyBuilder.DefineDynamicModule(fileName);
		}

		public Class GetClass(string ns, string name)
		{
			string fullName = ns + "." + name;
			foreach (var c in Classes)
			{
				if (c.FullName == fullName) { return c; }
			}
			var cl = new Class(this, ns, name);
			Classes.Add(cl);
			return cl;
		}

		public void CreateTypes()
		{
			foreach (var c in Classes) { c.CreateType(); }
		}

		public void Save() { AssemblyBuilder.Save(FileName); }
	}

	public class Class
	{
		public string Name, FullName;
		public List<Function> Functions = new List<Function>();
		public TypeBuilder TypeBuilder;
		public TypeAttributes Access { get; private set; }

		public Class(Assembly asm, string ns, string name)
		{
			Name = name;
			FullName = ns + "." + name;
			Access = char.IsUpper(Name[0]) ? TypeAttributes.Public : TypeAttributes.NotPublic;
            TypeBuilder = asm.ModuleBuilder.DefineType(FullName, Access);
		}

		public void CreateType() { TypeBuilder.CreateType(); }
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
