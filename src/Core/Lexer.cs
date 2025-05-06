using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        // References to reserved words/operators
        static string[] keywords = {
            "SUGOD", "KATAPUSAN", "KUNG", "KUNG-DILI", "PUNDOK", "ALANG SA", "IPAKITA", "DAWAT", "MUGNA"
        };
        static string[] relationalOperators =
        {
            ">", "<", ">=", "<=", "==", "<>"
        };
        static string[] dataTypes =
        {
            "NUMERO", "TIPIK", "LETRA", "TINUOD"
        };
        static string[] logicOperators =
        {
            "UG", "O", "DILI"
        };
        static string[] booleanValues =
        {
            "OO", "DILI"
        };

        public static List<List<Token>> tokenize(string s)
        {
            List<List<Token>> tokens = new List<List<Token>>();
            string[] split = s.Split("\n");
            
            // Tokenize the lines between start and end
            for (int i = 0; i < split.Length; i++)
            {
                string line = split[i].Trim();
                int indent = countLeadingWhiteSpaces(split[i]);

                // Ignore comments
                if (line.StartsWith("--"))
                    continue;

                // Otherwise tokenize the line
                List<Token> lineTokens = tokenizeLine(line, i + 1, indent);
                tokens.Add(lineTokens);    
            }

            return tokens;
        }

        // This method tokenizes a single line of code.
        private static List<Token> tokenizeLine(string line, int lineNumber, int indent)
        {
            List<Token> lineTokens = new List<Token>();

            for (int i = 0; i < line.Length; i++)
            {
                char currentChar = line[i];
                int currentColumn = indent + i + 1;

                // Skip whitespace
                if (char.IsWhiteSpace(currentChar))
                    continue;

                // Skip comments
                if (i + 1 < line.Length && currentChar.ToString() + line[i + 1] == "--")
                    break;

                // Check for letters (keywords, data types, logic operators, identifiers)
                if (char.IsLetter(currentChar))
                {
                    StringBuilder sb = new StringBuilder();
                    while (i < line.Length && (char.IsLetter(line[i]) || char.IsDigit(line[i]) || line[i] == '_'))
                    {
                        sb.Append(line[i]);
                        i++;
                    }

                    string tokenValue = sb.ToString();
                    TokenType tokenType;

                    if (keywords.Contains(tokenValue))
                        tokenType = TokenType.Keyword; 
                    else if (dataTypes.Contains(tokenValue))
                        tokenType = TokenType.DataType;
                    else if (logicOperators.Contains(tokenValue))
                        tokenType = TokenType.LogicalOperator;
                    else
                        tokenType = TokenType.Identifier;

                    lineTokens.Add(new Token(tokenType, tokenValue, lineNumber, currentColumn));
                    i--;
                    continue;
                }

                // Check for digits (number literals)
                else if (char.IsDigit(currentChar))
                {
                    StringBuilder sb = new StringBuilder();
                    bool isDotFound = false;
                    while (i < line.Length && (char.IsDigit(line[i]) || (!isDotFound && line[i] == '.') ))
                    {
                        if (line[i] == '.')
                            isDotFound = true;
                        sb.Append(line[i]);
                        i++;
                    }
                    string tokenValue = sb.ToString();
                    lineTokens.Add(new Token(TokenType.NumberLiteral, tokenValue, lineNumber, currentColumn));
                    i--;
                    continue;
                }

                // Check for string/char literals
                else if (currentChar == '"' || currentChar == '\'')
                {
                    char quoteChar = currentChar;
                    StringBuilder sb = new StringBuilder();
                    i++; // Skip the opening quote

                    while (i < line.Length && line[i] != quoteChar)
                    {
                        sb.Append(line[i]);
                        i++;
                    }

                    // Handle unterminated string/char
                    if (i >= line.Length)
                        throw new InvalidExpressionException($"Unterminated string or char literal: {sb}");

                    if (i < line.Length) // Skip the closing quote
                        i++;

                    string tokenValue = sb.ToString();
                    if (quoteChar == '\'' && tokenValue.Length != 1)
                        throw new InvalidExpressionException($"Invalid char literal: '{tokenValue}'"); // Char literals should be a single character

                    TokenType tokenType;
                    if (quoteChar == '"')
                    {
                        if (booleanValues.Contains(tokenValue))
                            tokenType = TokenType.BooleanLiteral;
                        else
                            tokenType = TokenType.StringLiteral;
                    }
                    else
                        tokenType = TokenType.CharLiteral;

                    lineTokens.Add(new Token(tokenType, tokenValue, lineNumber, currentColumn));
                    continue;
                }

                // Check for escape characters (treated as string literal)
                else if (currentChar == '[')
                {
                    StringBuilder sb = new StringBuilder();
                    i++; // Skip the opening bracket
                    while (i < line.Length && line[i] != ']')
                    {
                        sb.Append(line[i]);
                        i++;
                    }
                    if (i < line.Length) // Skip the closing bracket
                        i++;
                    string tokenValue = sb.ToString();
                    lineTokens.Add(new Token(TokenType.StringLiteral, tokenValue, lineNumber, currentColumn));
                    continue;
                }

                // Check for symbols/operators
                else
                {
                    string symbol = currentChar.ToString();
                    TokenType tokenType;

                    // Handle two-character operators
                    if (relationalOperators.Contains(symbol))
                    {
                        tokenType = TokenType.RelationalOperator;

                        // Check for 2-character relational operators first
                        if (i + 1 < line.Length)
                        {
                            symbol = line.Substring(i, 2);
                            if (relationalOperators.Contains(symbol))
                            {
                                lineTokens.Add(new Token(TokenType.RelationalOperator, symbol, lineNumber, currentColumn));
                                i++; // Skip an extra char
                                continue;
                            }
                        }

                        // Check for 1-character relational operators
                        if (relationalOperators.Contains(symbol))
                        {
                            lineTokens.Add(new Token(TokenType.RelationalOperator, symbol, lineNumber, currentColumn));
                            continue;
                        }

                    }
                    else
                    {
                        switch (currentChar)
                        {
                            case '+':
                            case '-':
                            case '*':
                            case '/':
                            case '%':
                                tokenType = TokenType.ArithmeticOperator;
                                break;
                            case ',':
                                tokenType = TokenType.Comma;
                                break;
                            case '=':
                                tokenType = TokenType.AssignmentOperator;
                                break;
                            case '(':
                                tokenType = TokenType.LeftParen;
                                break;
                            case ')':
                                tokenType = TokenType.RightParen;
                                break;
                            case '{':
                                tokenType = TokenType.LeftCurly;
                                break;
                            case '}':
                                tokenType = TokenType.RightCurly;
                                break;
                            case ':':
                                tokenType = TokenType.Colon;
                                break;
                            case '&':
                                tokenType = TokenType.Concatenator;
                                break;
                            case '$':
                                tokenType = TokenType.CarriageReturn;
                                break;
                            default:
                                throw new InvalidExpressionException($"Invalid symbol: '{currentChar}'");
                        }
                    }

                    lineTokens.Add(new Token(tokenType, symbol, lineNumber, currentColumn));
                    continue;
                }
            }

            return lineTokens;
        }

        private static int countLeadingWhiteSpaces(string line)
        {
            int count = 0;
            int tabSize = 4;
            foreach (char c in line)
            {
                if (c == ' ')
                    count++;
                else if (c == '\t')
                    count += tabSize;
                else
                    break;
            }
            return count;
        }
    }
}
