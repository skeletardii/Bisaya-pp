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
                if (_linePosition < _tokens.Count && _tokenPosition < _tokens[_linePosition].Count)
                    return _tokens[_linePosition][_tokenPosition];
                return null;
            }
        }

        private Token Advance()
        {
            while (_linePosition < _tokens.Count)
            {
                if (_tokenPosition < _tokens[_linePosition].Count)
                {
                    return _tokens[_linePosition][_tokenPosition++];
                }
                else
                {
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
            if (Current.Type == TokenType.Keyword && Current.Value == "KATAPUSAN") return ParseEndBlock();
            if (Current.Type == TokenType.Keyword && Current.Value == "KUNG") return ParseIfStatement();
            if (Current.Type == TokenType.Keyword && Current.Value == "SAMTANG") return ParseWhileStatement();
            if (Current.Type == TokenType.Keyword && Current.Value == "ALANG SA") return ParseForLoop();

            // Check for the 'MUGNA' keyword here to handle variable declarations
            if (Current.Type == TokenType.Keyword && Current.Value == "MUGNA")
            {
                return ParseDeclaration();
            }

            return ParseAssignmentOrExpression();
        }



        private ASTNode ParseStartBlock()
        {
            Expect(TokenType.Keyword, "Expected 'SUGOD' keyword to start block.");
            Advance(); // consume 'SUGOD'

            var statements = new List<ASTNode>();
            while (Current != null && !(Current.Type == TokenType.Keyword && Current.Value == "KATAPUSAN"))
            {
                statements.Add(ParseStatement());
            }

            Expect(TokenType.Keyword, "Expected 'KATAPUSAN' keyword to close the block.");
            Advance(); // consume 'KATAPUSAN'

            return new BlockNode(statements);
        }

        private ASTNode ParseEndBlock()
        {
            Expect(TokenType.Keyword, "Expected 'KATAPUSAN' keyword.");
            Advance(); // consume 'KATAPUSAN'
            return null;
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
            // Ensure 'MUGNA' keyword is found first
            Expect(TokenType.Keyword, "Expected 'MUGNA' to declare variables.");
            Advance(); // Consume 'MUGNA'

            // Debugging: Check the current token immediately after 'MUGNA'
            Console.WriteLine($"Next Token After MUGNA: {Current?.Value}");

            // Expect the data type (NUMERO, LETRA, etc.)
            if (Current?.Type != TokenType.DataType)
            {
                throw new Exception($"Expected data type after 'MUGNA', found: {Current?.Value}");
            }

            var dataTypeToken = Advance(); // Consume the data type (e.g., NUMERO)
            Console.WriteLine($"Data Type: {dataTypeToken?.Value}");

            var declarations = new List<DeclarationNode>();

            // Parse each variable declaration
            do
            {
                // Expect an identifier (variable name)
                Expect(TokenType.Identifier, "Expected variable name.");

                var nameToken = Advance(); // Consume the identifier (variable name)
                LiteralNodeBase initValue = null;

                // If there's an assignment, process it
                if (Current?.Type == TokenType.AssignmentOperator)
                {
                    Advance(); // Consume '='
                    initValue = (LiteralNodeBase)ParseExpression();
                }

                declarations.Add(new DeclarationNode(nameToken.Value, GetDataType(dataTypeToken.Value), initValue));

                // Handle additional variables separated by commas
            } while (Current?.Type == TokenType.Comma && Advance() != null);

            // Return a block node for the declaration statements
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

        private ASTNode ParseExpression() => ParseBinaryOperation();

        private ASTNode ParseBinaryOperation(int parentPrecedence = 0)
        {
            var left = ParsePrimary();
            while (true)
            {
                var precedence = GetPrecedence(Current);
                if (precedence == 0 || precedence <= parentPrecedence) break;

                var opToken = Advance();
                var right = ParseBinaryOperation(precedence);
                left = new BinaryOpNode((LiteralNodeBase)left, opToken, (LiteralNodeBase)right);
            }
            return left;
        }

        private ASTNode ParseDeclarationWithDataType(Token dataTypeToken)
        {
            // This will handle the declaration when a data type is encountered (e.g., NUMERO, TINUOD, TIPIK)

            // Start by consuming the data type token
            var dataType = dataTypeToken.Value;

            // Ensure 'MUGNA' keyword was encountered before the data type (already checked in ParseDeclaration)
            Expect(TokenType.Keyword, "Expected 'MUGNA' keyword to declare variables.");
            Advance();  // Consume 'MUGNA' keyword

            // Expect an identifier (variable name)
            Expect(TokenType.Identifier, "Expected variable name after data type.");

            var nameToken = Advance();  // Consume identifier (variable name)
            LiteralNodeBase initValue = null;

            // If there's an assignment, process it
            if (Current?.Type == TokenType.AssignmentOperator)
            {
                Advance();  // Consume '='
                initValue = (LiteralNodeBase)ParseExpression();  // Parse expression for initial value
            }

            // Determine the appropriate .NET type based on the data type token (NUMERO, TINUOD, TIPIK)
            var type = GetDataType(dataType);

            // Return the declaration node
            return new DeclarationNode(nameToken.Value, type, initValue);
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
