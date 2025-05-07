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
        private readonly List<Token> tokens;
        private int position = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        private Token Current => position < tokens.Count ? tokens[position] : tokens[^1];
        private Token Advance() => position < tokens.Count ? tokens[position++] : tokens[^1];
        private bool Check(TokenType type)
        {
            if (Current.Type == type)
            {
                Advance();
                return true;
            }
            return false;
        }

        private Token Expect(TokenType type)
        {
            if (Current.Type != type)
                throw new Exception($"Expected {type}, found {Current.Type}");
            return Advance();
        }

        public ASTNode Parse()
        {
            return ParseStatement();
        }

        private ASTNode ParseStatement()
        {
            if (Check(TokenType.Keyword) && Current.Value == "kung") 
                return ParseIf();

            return ParseAssignment(); 
        }

        private ASTNode ParseAssignment()
        {
            var identifier = Expect(TokenType.Identifier);
            Expect(TokenType.AssignmentOperator);
            var expr = ParseExpression();
            return new AssignmentNode(identifier.Value!, expr);
        }

        private ASTNode ParseExpression()
        {
            var left = ParsePrimary();
            while (IsBinaryOperator(Current.Type))
            {
                var op = Advance();
                var right = ParsePrimary();
                left = new BinaryOpNode(left, op, right);
            }
            return left;
        }

        private ASTNode ParsePrimary()
        {
            var token = Advance();
            return token.Type switch
            {
                TokenType.NumberLiteral => new IntegerNode(token),
                TokenType.StringLiteral => new StringNode(token),
                TokenType.CharLiteral => new CharNode(token),
                TokenType.BooleanLiteral => new BoolNode(token),
                TokenType.Identifier => new VariableNode(token),
                _ => throw new Exception($"Unexpected token: {token.Type}")
            };
        }

        private ASTNode ParseIf()
        {
            Expect(TokenType.LeftParen);
            var condition = ParseExpression();
            Expect(TokenType.RightParen);

            var thenBranch = ParseStatement();
            ASTNode? elseBranch = null;

            if (Match(TokenType.Keyword) && Current.Value == "kungdili")
                elseBranch = ParseStatement();

            return new IfNode(condition, thenBranch, elseBranch);
        }

        private bool IsBinaryOperator(TokenType type)
        {
            return type is TokenType.ArithmeticOperator
                        or TokenType.RelationalOperator
                        or TokenType.LogicalOperator
                        or TokenType.Concatenator;
        }
    }
}
