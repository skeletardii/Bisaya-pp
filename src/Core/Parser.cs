using System;
using System.Collections.Generic;
using System.Threading.Tasks.Sources;
using Bisaya__.src.Core.Bisaya__.src.Core;
using static Bisaya__.src.Core.BinaryOpNode;

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
            Console.WriteLine($"Returning: {token?.Value}");
            _tokenPosition++;
            return token;
        }

        private bool Match(TokenType type)
        {
            if (Current != null && Current.Type == type)
            {
                Advance();
                Console.WriteLine("Matched successfully");
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
            if (Current.Type == TokenType.Identifier && Current.Value == "ALANG") return ParseForLoop();
            if (Current.Type == TokenType.Keyword && Current.Value == "BUHAT") return ParseDoWhileStatement();
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
            Expect(TokenType.Keyword, "Expected 'SUGOD' keyword to start block.");
            Advance(); // consume 'SUGOD'

            var statements = new List<ASTNode>();

            while (Current != null && !(Current.Type == TokenType.Keyword && Current.Value == "KATAPUSAN"))
            {
                statements.Add(ParseStatement());
            }
            if (Current == null || Current.Value != "KATAPUSAN")
            {
                throw new Exception("Expected 'KATAPUSAN' keyword to close the block.");
            }

            Advance(); // consume 'KATAPUSAN'
            Console.WriteLine("Terminating Start Block");
            return new BlockNode(statements);
        }

        private ASTNode ParseOutputStatement()
        { 
            Expect(TokenType.Keyword, "Expected 'IPAKITA'.");
            Expect(TokenType.Colon, "Expected ':' after 'IPAKITA'.");
            var expression = ParseExpression();
            return new OutputNode(expression);
        }



        private ASTNode ParseInputStatement()
        {
            List<string> vars = new List<string>();
            Expect(TokenType.Keyword, "Expected 'DAWAT' keyword.");
            Expect(TokenType.Colon, "Expected ':' after 'DAWAT'");

            if(Current.Type != TokenType.Identifier)
                throw new Exception($"Expected identifier after ':'. Found {Current.Type}.");
            vars.Add(Advance().Value); // Consume identifier
            while (Match(TokenType.Comma))
            {
                Console.WriteLine("Continuing DAWAT clause");
                vars.Add(Advance().Value); // Consume identifier
            }
            return new InputNode(vars);
        }
        public ASTNode ParseIfStatement()
        {
            // Parse initial condition
            Expect(TokenType.Keyword, "Expected 'KUNG' keyword.");

            // KUNG WALA (ELSE)
            if (Current?.Value == "WALA")
            {
                Advance(); // Consume WALA
                Expect(TokenType.Keyword, "Expected 'PUNDOK' to start a code block");
                var elseBlock = ParseBlock();
                return new IfNode(null, null, elseBlock);
            }

            // KUNG DILI (ELSE IF)
            bool isElseIf = false;
            if (Current?.Value == "DILI")
            {
                isElseIf = true;
                Advance(); // Consume DILI
            }

            Expect(TokenType.LeftParen, $"Expected '(' after 'KUNG{(isElseIf ? " DILI" : "")}'");
            var condition = ParseExpression(new TokenType[] { TokenType.RightParen });
            Expect(TokenType.RightParen, "Expected ')' to close condition. instead, got"+Current.Type);
            Expect(TokenType.Keyword, "Expected 'PUNDOK' to start a code block");
            var thenBlock = ParseBlock();

            // Check for next branch
            IfNode nextBranch = null;
            if (Current?.Type == TokenType.Keyword && Current.Value == "KUNG")
            {
                nextBranch = (IfNode)ParseIfStatement(); 
            }

            return new IfNode((LiteralNodeBase)condition, thenBlock, nextBranch);
        }

        private ASTNode ParseWhileStatement()
        {
            Advance(); // 'Samtang'
            Expect(TokenType.LeftParen, "Expected '(' after 'SAMTANG'.");
            var condition = ParseExpression(TokenType.RightParen);
            Expect(TokenType.RightParen, "Expected ')' after condition.");
            Expect(TokenType.Keyword, "Expected 'PUNDOK' before body.");
            var body = ParseBlock();
            return new WhileNode((LiteralNodeBase)condition, body);
        }

        private ASTNode ParseDoWhileStatement()
        {
            Advance(); // 'BUHAT'   
            var body = ParseBlock(); // BUHAT is followed by a block in curly braces

            Expect(TokenType.Keyword, "Expected 'SAMTANG' after 'BUHAT' block.");
            Expect(TokenType.LeftParen, "Expected '(' after 'SAMTANG'.");
            var condition = (LiteralNodeBase)ParseExpression(TokenType.RightParen);
            Expect(TokenType.RightParen, "Expected ')' after condition.");

            return new DoWhileNode(body, condition);
        }

        private ASTNode ParseForLoop()
        {
            Expect(TokenType.Identifier, "Expected 'ALANG' keyword.");
            Expect(TokenType.Identifier, "Expected 'SA' keyword.");
            Expect(TokenType.LeftParen, $"Expected '(' after ALANG SA at line {_linePosition} column {_tokenPosition}");
            if (Current.Type != TokenType.Identifier)
                throw new Exception("Expected variable assignment in initialization.");

            var variable = Advance();

            if (Current.Type != TokenType.AssignmentOperator)
                throw new Exception("Expected '=' in initialization.");

            Advance(); // consume '='

            var initValue = (LiteralNodeBase)ParseExpression(TokenType.Comma);
            var initialization = new AssignmentNode(variable.Value, initValue);
            Expect(TokenType.Comma, "Expected ',' after initialization");
            var condition = (BinaryOpNode)ParseExpression(TokenType.Comma);
            Expect(TokenType.Comma, "Expected ',' after condition");
            var increment = (AssignmentNode)ParseIncrementDecrement();
            Expect(TokenType.RightParen, "Expected ')' before after condition block");
            Expect(TokenType.Keyword, "Expected 'PUNDOK' before body");
            var body = ParseBlock();
            return new ForLoopNode(initialization, condition, increment, body);
        }

        private ASTNode ParseIncrementDecrement()
        {
            // Case: a++ or a--
            if (Current?.Type == TokenType.Identifier)
            {
                var identifierToken = Advance(); // consume identifier

                if (Current?.Type == TokenType.ArithmeticOperator && (Current.Value == "+" || Current.Value == "-"))
                {
                    var op = Advance().Value; // consume first '+/-'

                    if (Current?.Type == TokenType.ArithmeticOperator && Current.Value == op)
                    {
                        Advance(); // consume second '+/-'
                        var variable = new VariableNode(identifierToken);
                        var operation = new VariableNode(op+op);
                        return new AssignmentNode(identifierToken.Value, operation);
                    }
                }
            }

            // Case: ++a or --a
            if (Current?.Type == TokenType.ArithmeticOperator && (Current.Value == "+" || Current.Value == "-"))
            {
                var op = Current.Value;
                Advance(); // consume first '+/-'

                if (Current?.Type == TokenType.ArithmeticOperator && Current.Value == op)
                {
                    Advance(); // consume second '+/-'

                    if (Current?.Type == TokenType.Identifier)
                    {
                        var variable = new VariableNode(Advance());
                        var operation = new VariableNode(op + op);
                        return new AssignmentNode(operation.VariableName, variable);
                    }
                }
            }

            throw new Exception("Invalid increment or decrement syntax.");
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
            ASTNode left = ParseExpression();

            while (Current?.Type == TokenType.AssignmentOperator)
            {
                Advance(); // consume '='
                ASTNode right = ParseAssignmentOrExpression(); // recursively parse the right side

                if (left is VariableNode leftVar && right is LiteralNodeBase rightLit)
                {
                    return new AssignmentNode(leftVar.VariableName, rightLit);
                }
                else if (left is VariableNode leftVar2 && right is AssignmentNode rightAssign)
                {
                    // nest assignment result
                    return new AssignmentNode(leftVar2.VariableName, (LiteralNodeBase)rightAssign);
                }
                else
                {
                    throw new Exception("Invalid assignment structure.");
                }
            }

            return left;
        }


        private ASTNode ParseExpression(params TokenType[] stopAt)
        {
            return ParseBinaryOperation(0, stopAt);
        }
        
        private ASTNode ParseBinaryOperation(int parentPrecedence, params TokenType[] stopAt)
        {
            Console.WriteLine("Parsing left si " + Current.Value);
            var left = ParsePrimary(); // Start with the primary expression (could be literals, variables, etc.)
            Console.WriteLine($"Starting new Binary Op where Left: {left.GetType().Name} with value {left}");

            while (true)
            {
                if (Current == null || stopAt.Contains(Current.Type))
                {
                    Console.WriteLine("Exiting binary operation loop due to reaching stopAt condition");
                    break;
                }

                var precedence = GetPrecedence(Current);
                if (precedence <= parentPrecedence)
                {
                    Console.WriteLine($"Exiting binary operation loop due to precedence check. self = {precedence} parent = {parentPrecedence}");
                    break;
                }

                var opToken = Advance();
                if(opToken.Type == TokenType.Keyword || opToken.Type == TokenType.Identifier)
                {
                    return left;
                }
                // Handle concatenation operator & (with appropriate precedence)
                else if (opToken.Type == TokenType.Concatenator)
                {

                    var right = ParseExpression(); // Parse the right-hand side of the concatenation operation
                    left = new BinaryOpNode((LiteralNodeBase)left, opToken, (LiteralNodeBase)right);
                }
                else if(opToken.Type == TokenType.LeftParen)
                {
                    Advance(); // consume '('
                    Console.WriteLine("Consumed (");
                    var right = ParseExpression([TokenType.RightParen]);
                    Expect(TokenType.RightParen, "Expected ')' after expression in declaration.");
                    left = new BinaryOpNode((LiteralNodeBase)left, opToken, (LiteralNodeBase)right); // Create a binary operation node
                }
                else
                {
                    // Handle other operators (e.g., +, -, etc.)
                    var right = ParsePrimary();
                    left = new BinaryOpNode((LiteralNodeBase)left, opToken, (LiteralNodeBase)right);
                }
            }
            Console.WriteLine($"Returning Left: {left.GetType().Name} with value {left}");
            return left;
        }

        private ASTNode ParsePrimary()
        {
            // Handle unary DILI
            if (Current?.Type == TokenType.LogicalOperator && Current.Value == "DILI")
            {
                var opToken = Advance(); // consume 'DILI'
                var operand = ParsePrimary();

                // You can define a UnaryOpNode or just use a BinaryOpNode with a null left
                return new UnaryOpNode(opToken, (LiteralNodeBase)operand);
            }
            // Handle boolean literals directly (OO and DILI are valid)
            if (Current.Value == "OO" || Current.Value == "DILI")
            {
                if (Current?.Type == TokenType.BooleanLiteral)
                {
                    var booleanValue = new BoolNode(Current);
                    Advance(); // consume the boolean literal
                    return booleanValue;
                }
                else
                {
                    throw new Exception($"Invalid boolean literal: {Current.Value}. Expected 'OO' or 'DILI'.");
                }
            }
            // Handle unary + or -
            else if (Current?.Type == TokenType.ArithmeticOperator && (Current.Value == "-" || Current.Value == "+"))
            {
                var opToken = Advance(); // consume unary operator
                var operand = ParsePrimary(); // recursively get the next primary

                if (operand is IntegerNode intNode)
                {
                    return new IntegerNode(opToken.Value == "-" ? -intNode.Value : intNode.Value);
                }
                else if (operand is FloatNode floatNode)
                {
                    return new FloatNode(opToken.Value == "-" ? -floatNode.Value : floatNode.Value);
                }
                else
                {
                    throw new Exception($"Unary operator '{opToken.Value}' is not valid for type {operand.GetType().Name}");
                }
            }

            var token = Advance();

            switch (token.Type)
            {
                case TokenType.NumberLiteral:
                    // Determine if it's an integer or float
                    if (token.Value.Contains("."))
                        return new FloatNode(token);
                    else
                        return new IntegerNode(token);

                case TokenType.StringLiteral:
                    return new StringNode(token);

                case TokenType.BooleanLiteral:
                    // This part is now handled at the start of the method
                    throw new Exception("Boolean literal should have been handled already.");

                case TokenType.CharLiteral:
                    return new CharNode(token);

                case TokenType.CarriageReturn:
                    return new StringNode("\n");

                case TokenType.Identifier:
                    if (Current != null && Current.Type == TokenType.LeftParen)
                    {
                        return ParseStatement(); // function call
                    }
                    return new VariableNode(token);

                case TokenType.LeftParen:
                    var expr = ParseExpression();
                    Expect(TokenType.RightParen, "Expected ')' after expression.");
                    return expr;

                case TokenType.Concatenator:
                    var left = ParsePrimary();
                    var right = ParsePrimary();
                    Console.WriteLine($"Concat {left} and {right}");
                    return new BinaryOpNode((LiteralNodeBase)left, token, (LiteralNodeBase)right);
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
                TokenType.AssignmentOperator => 4,
                TokenType.RelationalOperator => 3,
                TokenType.Concatenator => 4, // ✅ Handle & here with appropriate precedence
                TokenType.LogicalOperator => token.Value switch
                {
                    "UG" => 1,
                    "O" => 1,
                    _ => 0
                },
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
