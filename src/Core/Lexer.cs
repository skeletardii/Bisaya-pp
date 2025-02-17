using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
The Lexer reads raw source code and converts it into tokens—small pieces of meaningful data.
    It scans characters one by one.
    Groups characters into tokens like numbers, identifiers (variable names), operators, keywords (if, while), etc.
    Ignores whitespace and comments.
    Reports errors for invalid characters.
Example: For x = 10 + 5;, the lexer produces:
    IDENTIFIER(x), ASSIGN(=), NUMBER(10), PLUS(+), NUMBER(5), SEMICOLON(;)
 */
namespace Bisaya__.src.Core
{
    internal class Lexer
    {
    }
}
