using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
A Token represents a categorized chunk of text.
Each token consists of:
    Type (e.g., NUMBER, IDENTIFIER, PLUS).
    Value (e.g., "10" for a number, "x" for an identifier).
Tokens make parsing easier by structuring the input into logical parts.
 */
namespace Bisaya__.src.Core
{
    internal class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int LineNumber { get; } // Line number in the source code
        public int ColumnNumber { get; } // Column number in the source code

        public Token(TokenType type, string value, int lineNumber, int columnNumber)
        {
            if (!Enum.IsDefined(typeof(TokenType), type))
                throw new ArgumentException($"Invalid token type: {type}");
            Type = type;
            Value = value;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }
    }
    public enum TokenType
    {
        Keyword,
        DataType,
        Identifier, // variable names

        // Literals (values)
        NumberLiteral,
        StringLiteral,
        CharLiteral,
        BooleanLiteral,

        // Operators 
        ArithmeticOperator,
        LogicalOperator,
        RelationalOperator,
        AssignmentOperator,
        Concatenator,
        CarriageReturn,

        // Delimiters
        LeftParen,
        RightParen,
        LeftBrace,
        RightBrace,
        Comma,
        Colon
    }
}