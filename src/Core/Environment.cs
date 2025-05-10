using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
The Environment is a symbol table where variables and functions are stored.

Example Use Cases:
    When x = 10; is executed, the environment stores x → 10.
    When print(x); is executed, the environment retrieves 10.

The environment supports:
    Global scope (variables available everywhere).
    Local scope (variables within functions).
    Function storage (storing user-defined functions).
 */
namespace Bisaya__.src.Core
{
    internal class Environment
    {

        public static Dictionary<string, dynamic> variables = new Dictionary<string, dynamic>();
        public static dynamic? Get(string varName)
        {
            if (Program.verbose) Console.WriteLine($"Get: {varName}");
            if (variables.ContainsKey(varName))
            {
                if (Program.verbose) Console.WriteLine($"Found: {varName} = {variables[varName]}");
                return variables[varName];
            }
            if (Program.verbose) Console.WriteLine($"Not Found: {varName}");
            return null;
        }
        public static bool Set(string varName, dynamic value)
        {
            if (value == null) value = "null";
            if (Program.verbose) Console.WriteLine($"Set: {varName} = {value} ({value.GetType()})");
            //if (value == null) return false;
            if (!variables.ContainsKey(varName))
            {
                variables.Add(varName, value);
                return true;
            }
            if (variables[varName].GetType() == value.GetType())
            {
                variables[varName]=value;
                return true;
            }
            return false;
        }
        public static bool Free(string varName)
        {
            return variables.Remove(varName);
        }
    }
}
