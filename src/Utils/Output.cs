using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bisaya__.src.Utils
{
    internal class Output
    {
        private static string data = "";
        public static void Write(string s)
        {
            data += s;
            Console.Write(s);
        }
        public static void WriteLine(string s)
        {
            data += s + "\n";
            Console.WriteLine(s);
        }
    }
}
