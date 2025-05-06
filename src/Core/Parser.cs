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

Note: The lexer outputs a List<List<Token>>, with each inner list representing tokens from a single line of code.
 */
namespace Bisaya__.src.Core
{
    internal class Parser
    {
        private List<List<Token>> _tokenLines;
        private int lineIndex = 0;
        private int tokenIndex = 0;

        public Parser(List<List<Token>> tokenLines)
        {
            _tokenLines = tokenLines;
        }

        // Get the current token in the current line
        private Token curr => lineIndex < _tokenLines.Count && tokenIndex < _tokenLines[lineIndex].Count ? _tokenLines[lineIndex][tokenIndex] : null;

        // Move to the next token in the current line, or the first token of the next line
        private Token Advance()
        {
            if (lineIndex >= _tokenLines.Count) return null;
            tokenIndex++;
            if (tokenIndex >= _tokenLines[lineIndex].Count)
            {
                lineIndex++;
                tokenIndex = 0;
            }
            return curr;
        }

        // Check if the current token matches a specific type without consuming it
        private bool Check(TokenType type)
        {
            return curr != null && curr.Type == type;
        }

        // Check and advance if the current token matches the type
        private bool Match(TokenType type)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
            return false;
        }

        // Ensure the next token is of the expected type, or throw an error
        private Token Consume(TokenType type, string errorMessage)
        {
            if (Check(type)) return Advance();
            throw new Exception($"Parse error at line {curr?.LineNumber}, column {curr?.ColumnNumber}: {errorMessage}");
        }

        // Parse the entire program by walking through each line's tokens
        public BlockNode ParseProgram()
        {
            var statements = new List<ASTNode>();

            // Skip any structural delimiters like SUGOD/KATAPUSAN
            while (lineIndex < _tokenLines.Count &&
                   _tokenLines[lineIndex].Count == 1 &&
                   (_tokenLines[lineIndex][0].Value.ToUpper() == "SUGOD" ||
                    _tokenLines[lineIndex][0].Value.ToUpper() == "KATAPUSAN"))
            {
                lineIndex++;
            }

            while (lineIndex < _tokenLines.Count)
            {
                if (_tokenLines[lineIndex].Count == 0)
                {
                    lineIndex++;
                    continue;
                }
                tokenIndex = 0;

                // Skip empty or structural lines again
                if (_tokenLines[lineIndex].Count == 1 &&
                    (_tokenLines[lineIndex][0].Value.ToUpper() == "SUGOD" ||
                     _tokenLines[lineIndex][0].Value.ToUpper() == "KATAPUSAN"))
                {
                    lineIndex++;
                    continue;
                }

                statements.Add(ParseStatement());
                lineIndex++;
            }
            return new BlockNode(statements);
        }

        // Parse a single statement based on its starting keyword or identifier
        private ASTNode ParseStatement()
        {
            if (Check(TokenType.Keyword))
            {
                string keyword = curr.Value.ToUpper();
                if (keyword == "MUGNA") return ParseDeclaration();
                if (keyword == "IPAKITA") return ParseOutput();
                if (keyword == "DAWAT") return ParseInput();
                if (keyword == "KUNG") return ParseIf();
                if (keyword == "ALANG") return ParseLoop();
            }
            if (Check(TokenType.Identifier)) return ParseAssignment();

            throw new Exception($"Unrecognized statement at line {curr?.LineNumber}, column {curr?.ColumnNumber}: '{curr?.Value}'");
        }

        // Parse variable declarations: MUGNA NUMERO x = 5
        private ASTNode ParseDeclaration()
        {
            Advance(); // Consume MUGNA
            var typeToken = Consume(TokenType.DataType, "Expected data type after MUGNA");

            var declarations = new List<AssignmentNode>();
            while (true)
            {
                var identifier = Consume(TokenType.Identifier, "Expected variable name");
                LiteralNodeBase value = null;
                if (Match(TokenType.AssignmentOperator))
                {
                    value = ParseExpression();
                }
                declarations.Add(new AssignmentNode(identifier.Value, value));

                if (!Match(TokenType.Comma)) break;
            }

            return new BlockNode(declarations.Cast<ASTNode>().ToList());
        }

        // Parse variable assignment: x = 5
        private ASTNode ParseAssignment()
        {
            var identifier = Advance();
            Consume(TokenType.AssignmentOperator, "Expected '=' after identifier");
            var value = ParseExpression();
            return new AssignmentNode(identifier.Value, value);
        }

        // Parse output: IPAKITA: expression
        private ASTNode ParseOutput()
        {
            var token = Advance(); // Consume IPAKITA
            Consume(TokenType.Colon, "Expected ':' after IPAKITA");
            var expr = ParseExpression();
            return new FunctionCallNode(new Token(TokenType.Keyword, "IPAKITA", token.LineNumber, token.ColumnNumber), new List<ASTNode> { expr });
        }

        // Parse input: DAWAT: x, y
        private ASTNode ParseInput()
        {
            var token = Advance(); // Consume DAWAT
            Consume(TokenType.Colon, "Expected ':' after DAWAT");
            var vars = new List<ASTNode>();
            do
            {
                var varToken = Consume(TokenType.Identifier, "Expected variable name");
                vars.Add(new StringNode(new Token(TokenType.StringLiteral, varToken.Value, varToken.LineNumber, varToken.ColumnNumber)));
            } while (Match(TokenType.Comma));

            return new FunctionCallNode(new Token(TokenType.Keyword, "DAWAT", token.LineNumber, token.ColumnNumber), vars);
        }

        // Parse an if-statement block
        private ASTNode ParseIf()
        {
            Advance(); // Consume KUNG
            Consume(TokenType.LeftParen, "Expected '(' after KUNG");
            var condition = ParseExpression();
            Consume(TokenType.RightParen, "Expected ')' after condition");

            var thenBranch = ParseBlock();
            BlockNode elseBranch = null;

            if (Check(TokenType.Keyword) && curr.Value.ToUpper() == "KUNG WALA")
            {
                Advance();
                elseBranch = ParseBlock();
            }

            return new IfNode(condition, thenBranch, elseBranch);
        }

        // Parse a block of statements enclosed by PUNDOK { }
        private BlockNode ParseBlock()
        {
            Consume(TokenType.Keyword, "Expected PUNDOK block start");
            Consume(TokenType.LeftBrace, "Expected '{' after PUNDOK");
            var statements = new List<ASTNode>();
            while (!Check(TokenType.RightBrace))
            {
                statements.Add(ParseStatement());
            }
            Consume(TokenType.RightBrace, "Expected '}' to close PUNDOK block");
            return new BlockNode(statements);
        }

        // Parse a for-loop (simplified as a while loop for now)
        private ASTNode ParseLoop()
        {
            Advance(); // Consume ALANG
            Consume(TokenType.Keyword, "Expected SA after ALANG");
            Consume(TokenType.LeftParen, "Expected '(' after ALANG SA");

            var init = ParseAssignment();
            var condition = ParseExpression();
            Consume(TokenType.Comma, "Expected ',' after condition");
            var update = ParseAssignment();
            Consume(TokenType.RightParen, "Expected ')' after for header");

            var body = ParseBlock();
            return new WhileNode(condition, body); // Simplified loop representation
        }

        // Parse an expression (entry point for arithmetic and logical operations)
        public LiteralNodeBase ParseExpression()
        {
            return ParseTerm();
        }

        // Parse '+' and '-' binary expressions
        private LiteralNodeBase ParseTerm()
        {
            var left = ParseFactor();
            while (Check(TokenType.ArithmeticOperator) && (curr.Value == "+" || curr.Value == "-"))
            {
                var op = Advance();
                var right = ParseFactor();
                left = new BinaryOpNode(left, op, right);
            }
            return left;
        }

        // Parse '*', '/', '%' binary expressions
        private LiteralNodeBase ParseFactor()
        {
            var left = ParsePrimary();
            while (Check(TokenType.ArithmeticOperator) && (curr.Value == "*" || curr.Value == "/" || curr.Value == "%"))
            {
                var op = Advance();
                var right = ParsePrimary();
                left = new BinaryOpNode(left, op, right);
            }
            return left;
        }

        // Parse literal values (number, string, bool, char)
        private LiteralNodeBase ParsePrimary()
        {
            if (Check(TokenType.NumberLiteral)) return new IntegerNode(Advance());
            if (Check(TokenType.StringLiteral)) return new StringNode(Advance());
            if (Check(TokenType.BooleanLiteral)) return new BoolNode(Advance());
            if (Check(TokenType.CharLiteral)) return new CharNode(Advance());

            throw new Exception($"Unexpected token at line {curr?.LineNumber}, column {curr?.ColumnNumber}: {curr?.Value}");
        }
    }
}
