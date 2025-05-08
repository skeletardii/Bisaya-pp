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
                while (_linePosition < _tokens.Count)
                {
                    if (_tokenPosition < _tokens[_linePosition].Count)
                        return _tokens[_linePosition][_tokenPosition];

                    _linePosition++;
                    _tokenPosition = 0;
                }
                return null;
            }
        }
            
        private Token Advance()
        {
            var token = Current;
            _tokenPosition++;
            return token;
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
            if (Current == null)
                throw new Exception("Program is empty.");

            var root = ParseStatement();

            if (Current != null)
                throw new Exception("Unexpected tokens after program block.");

            return root;
        }


        private ASTNode ParseStatement()
        {
            if (Current.Type == TokenType.Keyword && Current.Value == "SUGOD") return ParseStartBlock();
            if (Current.Type == TokenType.Keyword && Current.Value == "KUNG") return ParseIfStatement();
            if (Current.Type == TokenType.Keyword && Current.Value == "SAMTANG") return ParseWhileStatement();
            if (Current.Type == TokenType.Keyword && Current.Value == "ALANG SA") return ParseForLoop();
            if ((Current.Type == TokenType.Keyword && Current.Value == "DAWAT")) return ParseInputStatement();

            if (Current.Type == TokenType.Keyword && Current.Value == "IPAKITA")
            {
                return ParseOutputStatement();  // Handle 'IPAKITA' print statement
            }

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

        private ASTNode ParseOutputStatement()
        {
            Expect(TokenType.Keyword, "Expected 'IPAKITA'.");
            Expect(TokenType.Colon, "Expected ':' after 'IPAKITA'.");
            if(Current.Type == TokenType.Identifier)
            {
                return new OutputNode(new VariableNode(Advance()));
            }
            var expression = ParseExpression();
            return new OutputNode(expression);
        }


        private ASTNode ParseInputStatement()
        {
            Expect(TokenType.Keyword, "Expected 'DAWAT' keyword.");
            Expect(TokenType.Colon, "Expected ':' after 'DAWAT'");

            if (Current.Type != TokenType.Identifier)
                throw new Exception($"Expected identifier after ':'. Found {Current.Type}.");

            var identifier = Advance().Value;  
            return new InputNode(identifier);
        }
        public ASTNode ParseIfStatement()
        {
            // Parse the 'KUNG' keyword
            Expect(TokenType.Keyword, "KUNG");

            // Parse the condition (e.g., 'a > 0')
            var condition = ParseExpression(new TokenType[] { TokenType.Keyword });

            // Expect the 'PUNDOK' keyword to start the block
            Expect(TokenType.Keyword, "PUNDOK");

            // Parse the block (statements inside the 'if')
            var block = ParseBlock();

            // Check if there's an 'else if' or 'else' clause
            if (Current.Type == TokenType.Keyword && Current.Value == "DILI")
            {
                // Parse the 'DILI' keyword for 'else if'
                Advance();

                // Parse the 'KUNG' keyword for 'else if'
                Expect(TokenType.Keyword, "KUNG");

                // Parse the 'else if' condition
                var elseIfCondition = ParseExpression(new TokenType[] { TokenType.Keyword });

                // Expect 'PUNDOK' for the 'else if' block
                Expect(TokenType.Keyword, "PUNDOK");

                // Parse the 'else if' block
                var elseIfBlock = ParseIfStatement();

                // Return the 'if' node with the 'else if' block
                return new IfNode((LiteralNodeBase)condition, block, new IfNode((LiteralNodeBase)elseIfCondition, elseIfBlock));
            }

            // Check for 'else' clause (i.e., 'WALA')
            if (Current.Type == TokenType.Keyword && Current.Value == "WALA")
            {
                // Parse the 'WALA' keyword for 'else'
                Advance();

                // Parse the 'else' block
                var elseBlock = ParseBlock();

                // Return the 'if' node with the 'else' block
                return new IfNode((LiteralNodeBase)condition, block, elseBlock);
            }

            return new IfNode((LiteralNodeBase)condition, block); // Return the 'if' node without the 'else if' or 'else' block
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
            Expect(TokenType.Keyword, "Expected 'ALANG' keyword.");
            if (Current.Value != "ALANG") throw new Exception("Expected 'ALANG' keyword.");
            Advance(); // Consume 'ALANG'

            Expect(TokenType.Keyword, "Expected 'SA' keyword.");
            if (Current.Value != "SA") throw new Exception("Expected 'SA' keyword.");
            Advance(); // Consume 'SA'

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
                if (Current?.Type != TokenType.Identifier)
                    throw new Exception("Expected valid variable name.");

                var nameToken = Current;
                Advance();

                LiteralNodeBase initValue = null;
                if (Current?.Type == TokenType.AssignmentOperator)
                {
                    Advance();
                    initValue = (LiteralNodeBase)ParseExpression();
                }

                declarations.Add(new DeclarationNode(nameToken.Value, GetDataType(dataTypeToken.Value), initValue));

                if (Current == null || Current.Type != TokenType.Comma)
                    break;

                Advance(); // consume comma
            }

            if (declarations.Count == 1)
                return declarations[0];

            return new BlockNode(declarations.Cast<ASTNode>().ToList());
        }



        private ASTNode ParseDeclarationWithDataType(Token dataTypeToken)
        {
            var declarations = new List<DeclarationNode>();

            while (true)
            {
                var nameToken = Current;
                Expect(TokenType.Identifier, $"Error at line {Current.LineNumber} column {Current.ColumnNumber}: Expected valid variable name but got {Current.Type}:{Current.Value}");

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

            if (declarations.Count == 1)
                return declarations[0];
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
                case TokenType.CharLiteral:
                    return new CharNode(token);  // Handle character literals
                case TokenType.RelationalOperator: // Handle relational operators like '>'
                    return ParseBinaryOperation(0); // Use the ParseBinaryOperation to handle the relational operation

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
                case TokenType.DataType:
                    throw new Exception("Unexpected data type in expression.");
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
                TokenType.RelationalOperator => 3,  // Handle relational operators like '>', '<', etc.
                TokenType.LogicalOperator => 4,
                _ => 0
            };
        }

        private BlockNode ParseBlock()
        {
            // Ensure we have '{' to start the block
            Expect(TokenType.LeftCurly, "Expected '{' to start block.");

            var statements = new List<ASTNode>();

            // Parse statements until we encounter '}'
            while (Current != null && Current.Type != TokenType.RightCurly)
            {
                // Parse each statement
                statements.Add(ParseStatement());
            }

            // Expect '}' to close the block
            Expect(TokenType.RightCurly, "Expected '}' to end block.");

            return new BlockNode(statements);
        }

    }
}
