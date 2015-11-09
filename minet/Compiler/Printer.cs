using System;

namespace Minet.Compiler.AST
{
    public class Printer
    {
        private static void printIndent(int indent)
        {
            for (int i = 0; i < indent; i++)
            {
                Console.Write("|   ");
            }
        }

        public static void Print(Accessor obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(AccessorRange obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Array obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(ArrayCons obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(ArrayValueList obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Assign obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Binary obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Blank obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Bool obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Break obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Char obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Class obj, int indent)
        {
            printIndent(indent);
            Console.Write("class " + obj.Name);
            if (obj.TypeParams.Count > 0) { Console.Write("<" + string.Join(", ", obj.TypeParams) + ">"); }
            if (obj.Withs.Count > 0) { Console.Write(" with " + string.Join(", ", obj.Withs)); }
            Console.WriteLine();
            foreach (var s in obj.Statements) { Print(s as dynamic, indent + 1); }
        }

        public static void Print(Constructor obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Defer obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Error obj, int indent)
        {
            printIndent(indent);
            Console.WriteLine("ERROR: " + obj.Val);
        }

        public static void Print(ExprList obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(ExprStmt obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(File obj, int indent)
        {
            printIndent(indent);
            Console.WriteLine(obj.Name);
            foreach (var s in obj.Statements) { Print(s as dynamic, indent + 1); }
        }

        public static void Print(For obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(FunctionCall obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(FunctionDef obj, int indent)
        {
            printIndent(indent);
            if (obj.Static) { Console.Write("static "); }
            Console.Write(string.IsNullOrEmpty(obj.Name) ? "fn" : obj.Name);
            Console.Write("(");
            Console.Write(string.Join(", ", obj.Params));
            Console.Write(")");
            if (obj.Returns.Count > 0)
            {
                Console.Write(" ");
                if (obj.Returns.Count > 1) { Console.Write("("); }
                Console.Write(string.Join(", ", obj.Returns));
                if (obj.Returns.Count > 1) { Console.Write(")"); }
            }
            Console.WriteLine();
            foreach (var s in obj.Statements) { Print(s as dynamic, indent + 1); }
        }

        public static void Print(FunctionSig obj, int indent)
        {
            printIndent(indent);
            Console.WriteLine(obj);
        }

        public static void Print(Identifier obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(If obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Interface obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(IntfFuncSig obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Iota obj, int indent)
        {
            printIndent(indent);
            Console.WriteLine("iota reset");
        }

        public static void Print(Is obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(KeyVal obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Loop obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Namespace obj, int indent)
        {
            printIndent(indent);
            Console.WriteLine("namespace " + obj.Name);
        }

        public static void Print(Number obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(PropertySet obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Return obj, int indent)
        {
            printIndent(indent);
            Console.WriteLine("return");
            Print(obj.Vals as dynamic, indent + 1);
        }

        public static void Print(String obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(TypeAlias obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Unary obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(Use obj, int indent)
        {
            printIndent(indent);
            Console.WriteLine("use");
            foreach (var p in obj.Packages) { Print(p as dynamic, indent + 1); }
        }

        public static void Print(UsePackage obj, int indent)
        {
            printIndent(indent);
            Console.Write(obj.Pack);
            if (!string.IsNullOrEmpty(obj.Alias))
            {
                Console.Write(" as " + obj.Alias);
            }
            Console.WriteLine();
        }

        public static void Print(VarSet obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }

        public static void Print(VarSetLine obj, int indent)
        {
            printIndent(indent); Console.WriteLine("[missing]");
        }
    }
}
