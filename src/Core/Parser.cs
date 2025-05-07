using System;
using System.Collections.Generic;

namespace Bisaya__.src.Core
{
    internal class Parser
    {
        private readonly List<List<Token>> _tokens;
        private int _xpos, _ypos;

        public Parser(List<List<Token>> tokens)
        {
            _tokens = tokens;
            _xpos = 0;
            _ypos = 0;
        }

        private Token Peek(int offset = 0)
        {
            int line = _ypos;
            int col = _xpos;
            while (offset > 0)
            {
                col++;
                if (line >= _tokens.Count) break;
                if (col >= _tokens[line].Count)
                {
                    line++;
                    col = 0;
                }
                offset--;
            }

            if (line >= _tokens.Count || col >= _tokens[line].Count)
                return _tokens[^1][^1];

            return _tokens[line][col];
        }

        private Token Next()
        {
            var current = _tokens[_ypos][_xpos];
            _xpos++;
            if (_xpos >= _tokens[_ypos].Count)
            {
                _ypos++;
                _xpos = 0;
            }
            return current;
        }

        private bool Match(TokenType type)
        {
            if (Peek().Type == type)
            {
                Next();
                return true;
            }
            return false;
        }

        private Token Expect(TokenType type, string errorMessage)
        {
            if (Peek().Type != type)
                throw new Exception(errorMessage);
            return Next();
        }

        private bool HasMoreTokens()
        {
            return _ypos < _tokens.Count && (_xpos < _tokens[_ypos].Count || _ypos + 1 < _tokens.Count);
        }

        public ASTNode Parse()
        {
            return ParseBlock();
        }

        private BlockNode ParseBlock()
        {
            var statements = new List<ASTNode>();
            while (HasMoreTokens())
            {
                statements.Add(ParseStatement());
            }
            return new BlockNode(statements);
        }

        private ASTNode ParseStatement()
        {
            var current = Peek();

            if (current.Type == TokenType.Keyword && current.Value == "KUNG")
                return ParseIf();
            if (current.Type == TokenType.Keyword && current.Value == "ALANG SA")
                return ParseWhile();
            if (current.Type == TokenType.Keyword && (current.Value == "IPAKITA" || current.Value == "DAWAT"))
                return ParseFunctionCall();
            if (current.Type == TokenType.Identifier && Peek(1).Type == TokenType.AssignmentOperator)
                return ParseAssignment();

            return ParseExpression();
        }

        private AssignmentNode ParseAssignment()
        {
            var name = Expect(TokenType.Identifier, "Expected variable name.");
            Expect(TokenType.AssignmentOperator, "Expected '='.");
            var expr = (LiteralNodeBase)ParseExpression();
            return new AssignmentNode(name.Value, expr);
        }

        private ASTNode ParseExpression()
        {
            return ParseBinaryOp();
        }

        private ASTNode ParseBinaryOp(int parentPrecedence = 0)
        {
            ASTNode left = ParsePrimary();

            while (true)
            {
                var opToken = Peek();
                int precedence = GetPrecedence(opToken.Value);
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;

                Next(); // consume operator
                ASTNode right = ParseBinaryOp(precedence);
                left = new BinaryOpNode((LiteralNodeBase)left, opToken, (LiteralNodeBase)right);
            }

            return left;
        }

        private int GetPrecedence(string op)
        {
            return op switch
            {
                "*" or "/" => 2,
                "+" or "-" => 1,
                _ => 0,
            };
        }

        private ASTNode ParsePrimary()
        {
            var token = Next();
            return token.Type switch
            {
                TokenType.NumberLiteral when token.Value.Contains('.') => new FloatNode(token),
                TokenType.NumberLiteral => new IntegerNode(token),
                TokenType.StringLiteral => new StringNode(token),
                TokenType.BooleanLiteral => new BoolNode(token),
                TokenType.CharLiteral => new CharNode(token),
                TokenType.Identifier when Peek().Type == TokenType.LeftParen => ParseFunctionCall(token),
                _ => throw new Exception($"Unexpected token {token.Type}")
            };
        }

        private IfNode ParseIf()
        {
            Next(); // consume 'KUNG'
            var condition = (LiteralNodeBase)ParseExpression();
            var thenBranch = ParseBlock();

            BlockNode? elseBranch = null;
            if (Peek().Type == TokenType.Keyword && Peek().Value == "KUNG-DILI")
            {
                Next(); // consume 'KUNG-DILI'
                elseBranch = ParseBlock();
            }

            return new IfNode(condition, thenBranch, elseBranch);
        }

        private WhileNode ParseWhile()
        {
            Next(); // consume 'ALANG SA'
            var condition = (LiteralNodeBase)ParseExpression();
            var body = ParseBlock();
            return new WhileNode(condition, body);
        }

        private FunctionCallNode ParseFunctionCall(Token? overrideToken = null)
        {
            var nameToken = overrideToken ?? Next();
            Expect(TokenType.LeftParen, "Expected '(' after function name.");

            var args = new List<ASTNode>();
            if (Peek().Type != TokenType.RightParen)
            {
                do
                {
                    args.Add(ParseExpression());
                } while (Match(TokenType.Comma));
            }

            Expect(TokenType.RightParen, "Expected ')' after arguments.");
            return new FunctionCallNode(nameToken, args);
        }
    }
}
