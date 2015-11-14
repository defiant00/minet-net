using System.Collections.Generic;

namespace Minet.Compiler.AST
{
	public static class ExtensionMethods
	{
		public static void CalcTypeList(this List<Variable> list, WalkState state, string error)
		{
			Statement type = null;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i].Type == null)
				{
					if (type == null) { state.AddError(error); }
					else { list[i].Type = type; }
				}
				else { type = list[i].Type; }
			}
		}
	}
}
