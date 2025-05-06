using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
The Parser takes tokens from the lexer and builds a tree-like structure called the Abstract Syntax Tree (AST).
The parser follows the language’s grammar rules:
    It checks if the syntax is correct.
    It handles precedence (e.g., multiplication before addition).
    It structures expressions and statements logically.
Example: For 5 + 10, the parser produces an AST:

    (+)
   /   \
 (5)   (10)

 */
namespace Bisaya__.src.Core
{
    internal class Parser
    {
        private List<Token> _tokens;
        private int _pos;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _pos = 0;
        }

        // Example: parsing method
        public ASTNode ParseExpression()
        {
            // implementation here...
            return null;
        }
    }
}
