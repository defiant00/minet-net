using Minet.Compiler.AST;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Minet.Compiler.FAST
{
	public interface ITypeContainer
	{
		Class GetClass(Assembly asm, string ns, string name, bool pub);
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
		public List<Class> Classes = new List<Class>();
		public List<Field> Fields = new List<Field>();
		public List<Function> Functions = new List<Function>();
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

	public class Constant : Field
	{
		private FieldBuilder FieldBuilder;

		public Constant(Property p, WalkState ws) : base(p, ws) { }

		public override void CreateField(Class c)
		{
			FieldBuilder = c.TypeBuilder.DefineField(Name, Type, Accessibility | FieldAttributes.Literal | FieldAttributes.Static);
		}

		public override void SetDefault(object val) { FieldBuilder.SetConstant(val); }
	}

	public class Field
	{
		public string Name;
		public Type Type;
		public object Value;

		public FieldAttributes Accessibility
		{
			get { return char.IsUpper(Name[0]) ? FieldAttributes.Public : FieldAttributes.Private; }
		}

		public Field(Property p, WalkState ws)
		{
			Name = p.Name;
			Type = p.SystemType;
		}

		public virtual void CreateField(Class c)
		{
			c.TypeBuilder.DefineField(Name, Type, Accessibility);
		}

		public virtual void SetDefault(object val) { Value = val; }
	}

	public class Function
	{
		public string Name;
		public bool Static;
		public MethodBuilder MethodBuilder;

		public MethodAttributes Accessibility
		{
			get { return char.IsUpper(Name[0]) ? MethodAttributes.Public : MethodAttributes.Private; }
		}

		public Function(Class cl, FunctionDef func)
		{
			Name = func.Name;
			Static = func.Static;
			var attrs = Accessibility;
			if (Static) { attrs |= MethodAttributes.Static; }
			MethodBuilder = cl.TypeBuilder.DefineMethod(Name, attrs);

			// methods must have a body, so this generates a single return statement
			// so it doesn't crash
			var il = MethodBuilder.GetILGenerator();
			il.Emit(OpCodes.Ret);
		}
	}
}
