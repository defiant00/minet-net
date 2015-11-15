using Minet.Compiler.FAST;
using System.Collections.Generic;

namespace Minet.Compiler
{
	public class WalkState
	{
		public Assembly Assembly = new Assembly();
		public List<UsePackage> Uses = new List<UsePackage>();
		public string Namespace;

		public List<string> Errors = new List<string>();
		public void AddError(string error) { Errors.Add(error); }

		public void FileReset()
		{
			Uses.Clear();
			Namespace = null;
		}
	}
}
