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
        public static List<Token> tokenize(string s)
        {
            List<Token> tokens = new List<Token>();
            string[] split = s.Split("\n");
            byte sugodIndex = 0;
            byte humanIndex = (byte)(split.Length - 1);
            for(int i = 0; i < split.Length; i++)
                if (split[i].Trim().Equals("SUGOD")) {
                    sugodIndex = (byte)i;
                    break; }
            for (int i = split.Length - 1; i > sugodIndex; i--)
                if (split[i].Trim().Equals("KATAPUSAN")){
                    humanIndex = (byte)i;
                    break;}
            for (int i = sugodIndex + 1; i < humanIndex; i++) {
                string line = split[i].Trim();
                if (line.StartsWith("--"))
                    continue;
                Console.WriteLine(line);
            }
            return tokens; 
        }   
        private static List<Token> tokenizeLine(string s)
        {
            List<Token> tokens = new List<Token> ();

            return tokens;
        }

        private static Dictionary<string, Token> tokenTable = new Dictionary<string, Token>
        {
            {"+", new Token(TokenType.Plus)},
            {"-", new Token(TokenType.Minus)},
            {"*", new Token(TokenType.Multiply)},
            {"/", new Token(TokenType.Divide)},
            {"=", new Token(TokenType.Assign)},
            {"MUGNA", new Token(TokenType.Mugna)},
            {"&", new Token(TokenType.Concatenate)},
        };
    }
}
