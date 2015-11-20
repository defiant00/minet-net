using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Minet.Compiler.FAST
{
	public interface IType { }

	public interface ITypeContainer
	{
		Class GetClass(Assembly asm, string ns, string name, bool pub);
	}

	public class ArrayType : IType
	{
		public IType Type;
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

	public class Assembly : ITypeContainer
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

		public Class GetClass(Assembly asm, string ns, string name, bool pub)
		{
			string fullName = ns + "." + name;
			foreach (var c in Classes)
			{
				if (c.FullName == fullName) { return c; }
			}
			var cl = new Class(asm, ns, name, pub, null);
			Classes.Add(cl);
			return cl;
		}

		public void CreateTypes()
		{
			foreach (var c in Classes) { c.CreateType(); }
		}

		public void Save() { AssemblyBuilder.Save(FileName); }
	}

	public class Class : ITypeContainer
	{
		public string Name, FullName;
		public List<Function> Functions = new List<Function>();
		public List<Class> Classes = new List<Class>();
		public TypeBuilder TypeBuilder;
		public TypeAttributes Access { get; private set; }

		public Class(Assembly asm, string ns, string name, bool pub, TypeBuilder parent)
		{
			Name = name;
			if (parent != null)
			{
				FullName = name;
				Access = pub ? TypeAttributes.NestedPublic : TypeAttributes.NestedPrivate;
				TypeBuilder = parent.DefineNestedType(FullName, Access);
			}
			else
			{
				FullName = ns + "." + name;
				Access = pub ? TypeAttributes.Public : TypeAttributes.NotPublic;
				TypeBuilder = asm.ModuleBuilder.DefineType(FullName, Access);
			}
		}

		public Class GetClass(Assembly asm, string ns, string name, bool pub)
		{
			foreach (var c in Classes)
			{
				if (c.FullName == name) { return c; }
			}
			var cl = new Class(asm, ns, name, pub, TypeBuilder);
			Classes.Add(cl);
			return cl;
		}

		public void CreateType()
		{
			TypeBuilder.CreateType();
			foreach (var c in Classes) { c.CreateType(); }
		}
	}

	public class Function
	{
		public bool Static;
		public string Name;
		public List<Variable> Parameters;
		public List<IType> Returns = new List<IType>();

		public Function(bool stat, string name, List<Variable> pars)
		{
			Static = stat;
			Name = name;
			Parameters = pars;
		}
	}

	public class FunctionType : IType
	{
		public override string ToString() { return "fn()"; }
	}

	public class GenType : IType
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
		public IType Type;
	}
}
