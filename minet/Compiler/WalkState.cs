using Minet.Compiler.FAST;
using System.Collections.Generic;

namespace Minet.Compiler
{
	public class WalkState
	{
		public Assembly Assembly = new Assembly();

		public string CurrentNamespace;
		public List<string> CurrentUses = new List<string>();
		public Class CurrentClass;
		public Function CurrentFunction;

		public List<string> Errors = new List<string>();
		public void AddError(string error) { Errors.Add(error); }

		public void Reset()
		{
			CurrentNamespace = null;
			CurrentClass = null;
			CurrentFunction = null;
			CurrentUses.Clear();
		}
	}
}
