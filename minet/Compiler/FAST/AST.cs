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

		public void GenFunctions(WalkState ws) { foreach (var c in Classes) { c.GenFunctions(ws); } }

		public void CreateTypes() { foreach (var c in Classes) { c.CreateType(); } }

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

		public void GenFunctions(WalkState ws)
		{
			foreach (var c in Classes) { c.GenFunctions(ws); }
			foreach (var f in Functions) { f.GenFunctionBody(ws); }
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
		private List<IStatement> Statements;
		public MethodBuilder MethodBuilder;

		public MethodAttributes Accessibility
		{
			get { return char.IsUpper(Name[0]) ? MethodAttributes.Public : MethodAttributes.Private; }
		}

		public Function(Class cl, FunctionDef func, WalkState ws)
		{
			Name = func.Name;
			Static = func.Static;
			Statements = func.Statements;
			var attrs = Accessibility | MethodAttributes.HideBySig;
			if (Static) { attrs |= MethodAttributes.Static; }
			func.Params.CalcTypeList(ws);

			var pTypes = new List<Type>();
			foreach (var p in func.Params) { pTypes.Add(p.SystemType); }
			Type rType = null;
			if (func.Returns.Count > 0)
			{
				rType = func.Returns[0].ToType(ws);
				for (int i = 1; i < func.Returns.Count; i++)
				{
					pTypes.Add(func.Returns[i].ToType(ws).MakeByRefType());
				}
			}

			MethodBuilder = cl.TypeBuilder.DefineMethod(Name, attrs, rType, pTypes.ToArray());

			int counter = 1;
			for (int i = 0; i < func.Params.Count; i++)
			{
				MethodBuilder.DefineParameter(counter++, ParameterAttributes.None, func.Params[i].Name);
			}
			for (int i = 1; i < func.Returns.Count; i++)
			{
				MethodBuilder.DefineParameter(counter++, ParameterAttributes.Out, "_ret" + i);
			}
		}

		public void GenFunctionBody(WalkState ws)
		{
			var il = MethodBuilder.GetILGenerator();
			foreach (var s in Statements)
			{
				var t = s.GetType();
				if (t == typeof(ExprStmt))
				{
					var es = s as ExprStmt;
					var el = es.Expr as ExprList;
					if (el.Expressions.Count > 1)
					{
						ws.AddError("Cannot have more than a single expression as a statement.");
					}
					else
					{
						var et = el.Expressions[0].GetType();
						if (et == typeof(FunctionCall))
						{
							var fn = el.Expressions[0] as FunctionCall;
							// TODO - Function call
						}
						else
						{
							ws.AddError(et + " is not valid for a statement.");
						}
					}
				}
				else if (t == typeof(Return))
				{
					var r = s as Return;
					if (r.Vals != null)
					{
						var el = r.Vals as ExprList;
						for (int i = 1; i < el.Expressions.Count; i++)
						{
							// TODO - multiple return values
						}
						el.Expressions[0].Emit(il, ws);
					}
					il.Emit(OpCodes.Ret);
				}
				else
				{
					ws.AddError("Invalid method instruction " + t);
				}
			}

			// If no commands have been emitted, emit a single return.
			if (il.ILOffset == 0) { il.Emit(OpCodes.Ret); }
		}
	}
}
