using Minet.Compiler.AST;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Minet.Compiler
{
	public class GenState
	{
		public string OutFile;
		public File CurrentFile;
		public Class CurrentClass;
		public FunctionDef CurrentFunc;
		public List<string> Errors = new List<string>();

		public AssemblyBuilder AssemblyBuilder;
		public ModuleBuilder ModuleBuilder;
		public TypeBuilder TypeBuilder;

		public void AddError(string error) { Errors.Add(error); }
	}
}
