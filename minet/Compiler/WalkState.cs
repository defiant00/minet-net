using Minet.Compiler.FAST;
using System.Collections.Generic;

namespace Minet.Compiler
{
	public class WalkState
	{
		public Assembly Assembly;
		public string Namespace;

		public List<string> Errors = new List<string>();
		public void AddError(string error) { Errors.Add(error); }

		public WalkState(string asmName, string fileName)
		{
			Assembly = new Assembly(asmName, fileName);
		}

		public void Save() { Assembly.Save(); }

		public void FileReset()
		{
			Namespace = null;
		}
	}
}
