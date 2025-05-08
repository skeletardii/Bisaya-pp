using System;
using System.Collections.Generic;

namespace Bisaya__.src.Core
{
    internal class Parser
    {
        private readonly List<List<Token>> _tokens;  // A 2D list of tokens
        private int _linePosition;  // Current line index
        private int _tokenPosition; // Current token index in the line

        public Parser(List<List<Token>> tokens)
        {
            _tokens = tokens;
            _linePosition = 0;
            _tokenPosition = 0;
        }

        private Token Current
        {
            get
            {
                if (_linePosition < _tokens.Count)
                {
                    if (_tokenPosition < _tokens[_linePosition].Count)
                    {
                        return _tokens[_linePosition][_tokenPosition];
                    }
                }

                return null;
            }
        }


        private Token Advance()
        {
            while (_linePosition < _tokens.Count)
            {
                if (_tokenPosition < _tokens[_linePosition].Count)
                {
                    var token = _tokens[_linePosition][_tokenPosition++];
                    Console.WriteLine($"Advancing to: {token.Value} ({token.Type})"); // Debugging line
                    return token;
                }
                else
                {
                    // Debugging: Print out when we're moving to the next line
                    Console.WriteLine($"Moving to next line: {_linePosition + 1}");
                    _linePosition++;
                    _tokenPosition = 0;
                }
            }
            return null;
        }


        private bool Match(TokenType type)
        {
            if (Current != null && Current.Type == type)
            {
                Advance();
                return true;
            }
            return false;
        }

        private void Expect(TokenType type, string errorMessage)
        {
            if (!Match(type))
                throw new Exception(errorMessage);
        }

        public ASTNode ParseProgram()
        {
            var statements = new List<ASTNode>();
            while (Current != null)
            {
                statements.Add(ParseStatement());
            }
            return new BlockNode(statements);
        }

        private ASTNode ParseStatement()
        {
            if (Current.Type == TokenType.Keyword && Current.Value == "SUGOD") return ParseStartBlock();
            if (Current.Type == TokenType.Keyword && Current.Value == "KUNG") return ParseIfStatement();
            if (Current.Type == TokenType.Keyword && Current.Value == "SAMTANG") return ParseWhileStatement();
            if (Current.Type == TokenType.Keyword && Current.Value == "ALANG SA") return ParseForLoop();

            // Check for the 'MUGNA' keyword here to handle variable declarations
            if (Current.Type == TokenType.Keyword && Current.Value == "MUGNA")
            {
                return ParseDeclaration();
            }
            if (Current.Type == TokenType.DataType)
            {
                var dataTypeToken = Advance();  // e.g., NUMERO, TINUOD, TIPIK
                return ParseDeclarationWithDataType(dataTypeToken);
            }

            return ParseAssignmentOrExpression();
        }



        private ASTNode ParseStartBlock()
        {
            // Expect the 'SUGOD' keyword and advance.
            Expect(TokenType.Keyword, "Expected 'SUGOD' keyword to start block.");
            Advance(); // consume 'SUGOD'

            var statements = new List<ASTNode>();

            // Continue parsing statements until 'KATAPUSAN' is encountered.
            while (Current != null && !(Current.Type == TokenType.Keyword && Current.Value == "KATAPUSAN"))
            {
                // This will now process the statement, checking for declarations, expressions, etc.
                statements.Add(ParseStatement());
            }

            // At this point, we should have encountered 'KATAPUSAN' to close the block
            if (Current == null || Current.Value != "KATAPUSAN")
            {
                throw new Exception("Expected 'KATAPUSAN' keyword to close the block.");
            }

            // Consume 'KATAPUSAN' token
            Advance(); // consume 'KATAPUSAN'

            // Return the block node with the collected statements
            return new BlockNode(statements);
        }

        private ASTNode ParseIfStatement()
        {
            Advance(); // 'KUNG'
            Expect(TokenType.LeftParen, "Expected '(' after 'KUNG'.");
            var condition = ParseExpression();
            Expect(TokenType.RightParen, "Expected ')' after condition.");
            var thenBranch = ParseBlock();
            BlockNode elseBranch = null;
            if (Current.Type == TokenType.Keyword && Current.Value == "KUNGDILI")
            {
                Advance();
                elseBranch = ParseBlock();
            }
            return new IfNode((LiteralNodeBase)condition, thenBranch, elseBranch);
        }

        private ASTNode ParseWhileStatement()
        {
            Advance(); // 'Samtang'
            Expect(TokenType.LeftParen, "Expected '(' after 'Samtang'.");
            var condition = ParseExpression();
            Expect(TokenType.RightParen, "Expected ')' after condition.");
            var body = ParseBlock();
            return new WhileNode((LiteralNodeBase)condition, body);
        }

        private ASTNode ParseForLoop()
        {
            Advance(); // 'ALANG'
            Expect(TokenType.Identifier, "Expected 'sa' keyword after 'ALANG'.");
            Advance(); // consume 'sa'
            Expect(TokenType.LeftParen, "Expected '(' after 'ALANG SA'.");

            var initialization = (AssignmentNode)ParseExpression();
            Expect(TokenType.Comma, "Expected ',' after initialization.");

            var condition = (BinaryOpNode)ParseExpression();
            Expect(TokenType.Comma, "Expected ',' after condition.");

            var increment = (AssignmentNode)ParseExpression();
            Expect(TokenType.RightParen, "Expected ')' after increment.");

            var body = ParseBlock();

            return new ForLoopNode(initialization, condition, increment, body);
        }

        private ASTNode ParseDeclaration()
        {
            Advance(); // consume 'MUGNA'

            if (Current?.Type != TokenType.DataType)
                throw new Exception($"Expected data type after 'MUGNA', found: {Current?.Value}");

            var dataTypeToken = Advance(); // consume data type
            var declarations = new List<DeclarationNode>();

            while (true)
            {
                Expect(TokenType.Identifier, "Expected variable name.");
                var nameToken = Advance(); // consume identifier

                LiteralNodeBase initValue = null;
                if (Current?.Type == TokenType.AssignmentOperator)
                {
                    Advance(); // consume '='
                    initValue = (LiteralNodeBase)ParseExpression();
                }

                declarations.Add(new DeclarationNode(nameToken.Value, GetDataType(dataTypeToken.Value), initValue));

                // Stop if there's no comma for more declarations
                if (Current == null || Current.Type != TokenType.Comma)
                    break;

                Advance(); // consume comma and continue
            }

            return new BlockNode(declarations.Cast<ASTNode>().ToList());
        }


        private ASTNode ParseDeclarationWithDataType(Token dataTypeToken)
        {
            var declarations = new List<DeclarationNode>();

            while (true)
            {
                Expect(TokenType.Identifier, "Expected variable name after data type.");
                var nameToken = Advance();

                LiteralNodeBase initValue = null;

                if (Current?.Type == TokenType.AssignmentOperator)
                {
                    Advance(); // consume '='
                    initValue = (LiteralNodeBase)ParseExpression();
                }

                declarations.Add(new DeclarationNode(nameToken.Value, GetDataType(dataTypeToken.Value), initValue));

                // Stop if next token is not a comma
                if (Current?.Type != TokenType.Comma)
                    break;

                Advance(); // consume comma, continue loop
            }

            return new BlockNode(declarations.Cast<ASTNode>().ToList());
        }


        private ASTNode ParseAssignmentOrExpression()
        {
            var expr = ParseExpression();
            if (expr is VariableNode variable && Match(TokenType.AssignmentOperator))
            {
                var value = ParseExpression();
                return new AssignmentNode(variable.VariableName, (LiteralNodeBase)value);
            }
            return expr;
        }

        private ASTNode ParseExpression(params TokenType[] stopAt)
        {
            return ParseBinaryOperation(0, stopAt);
        }

        private ASTNode ParseBinaryOperation(int parentPrecedence, params TokenType[] stopAt)
        {
            var left = ParsePrimary();

            while (true)
            {
                if (Current == null || stopAt.Contains(Current.Type))
                    break;

                var precedence = GetPrecedence(Current);
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;

                var opToken = Advance();
                var right = ParseBinaryOperation(precedence, stopAt);
                left = new BinaryOpNode((LiteralNodeBase)left, opToken, (LiteralNodeBase)right);
            }

            return left;
        }

        private ASTNode ParsePrimary()
        {
            var token = Advance();
            switch (token.Type)
            {
                case TokenType.NumberLiteral:
                    return new IntegerNode(token);  // Handle numbers correctly
                case TokenType.StringLiteral:
                    return new StringNode(token);  // Handle string literals
                case TokenType.BooleanLiteral:
                    return new BoolNode(token);  // Handle boolean literals
                case TokenType.Identifier:
                    if (Current != null && Current.Type == TokenType.LeftParen)
                    {
                        var args = new List<ASTNode>();
                        Advance();
                        if (Current.Type != TokenType.RightParen)
                        {
                            do
                            {
                                args.Add(ParseExpression());
                            } while (Match(TokenType.Comma));
                        }
                        Expect(TokenType.RightParen, "Expected ')' after arguments.");
                        return new FunctionCallNode(token, args);
                    }
                    return ParseAssignmentOrExpression();  // Handle variable assignments or expressions
                case TokenType.LeftParen:
                    var expr = ParseExpression();
                    Expect(TokenType.RightParen, "Expected ')' after expression.");
                    return expr;
                case TokenType.DataType:  // If the token is a data type (like NUMERO, TINUOD, TIPIK)
                                          // Handle data type tokens specifically for declarations (e.g., MUGNA NUMERO x = 5;)
                    return ParseDeclarationWithDataType(token);  // Pass data type token to handle the declaration
                default:
                    throw new Exception($"Unexpected token: {token.Value}");
            }
        }

        private Type GetDataType(string typeName)
        {
            return typeName switch
            {
                "NUMERO" => typeof(int),
                "LETRA" => typeof(string),
                "TINUOD" => typeof(bool),
                "TIPIK" => typeof(double),
                _ => throw new Exception($"Unknown type: {typeName}")
            };
        }

        private int GetPrecedence(Token token)
        {
            if (token == null) return 0;
            return token.Type switch
            {
                TokenType.ArithmeticOperator => token.Value == "+" || token.Value == "-" ? 1 : 2,
                TokenType.RelationalOperator => 3,
                TokenType.LogicalOperator => 4,
                _ => 0
            };
        }

        private BlockNode ParseBlock()
        {
            Expect(TokenType.LeftBrace, "Expected '{' to start block.");
            var statements = new List<ASTNode>();
            while (Current != null && Current.Type != TokenType.RightBrace)
            {
                statements.Add(ParseStatement());
            }
            Expect(TokenType.RightBrace, "Expected '}' to end block.");
            return new BlockNode(statements);
        }
    }
}
