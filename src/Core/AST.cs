using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
The AST is an internal representation of the code. It consists of nodes, each representing an operation or value.
Types of AST nodes:
    Literal nodes (numbers, strings).
    Binary operations (+, -, *, /).
    Control flow (if-statements, loops).
    Function calls.
The AST simplifies execution and optimization.
 */
using System.Collections.Generic;

namespace Bisaya__.src.Core
{
    // Base AST node
    internal abstract class ASTNode
    {
        public ASTNode? Parent { get; set; }
    }

    // 1. Number literal
    internal class NumberNode : ASTNode
    {
        public Token Token { get; }

        public NumberNode(Token token)
        {
            if (token.Type != TokenType.NumberLiteral)
                throw new ArgumentException("Expected a NumberLiteral token.");
            Token = token;
        }
    }

    // 2. Variable reference
    internal class VariableNode : ASTNode
    {
        public Token Token { get; }

        public VariableNode(Token token)
        {
            if (token.Type != TokenType.Identifier)
                throw new ArgumentException("Expected an Identifier token.");
            Token = token;
        }
    }

    // 3. Binary operation
    internal class BinaryOpNode : ASTNode
    {
        public ASTNode Left { get; }
        public Token OperatorToken { get; }
        public ASTNode Right { get; }

        public BinaryOpNode(ASTNode left, Token op, ASTNode right)
        {
            Left = left;
            OperatorToken = op;
            Right = right;

            Left.Parent = this;
            Right.Parent = this;
        }
    }

    // 4. Assignment
    internal class AssignmentNode : ASTNode
    {
        public Token Identifier { get; }
        public ASTNode Value { get; }

        public AssignmentNode(Token identifier, ASTNode value)
        {
            if (identifier.Type != TokenType.Identifier)
                throw new ArgumentException("Expected an Identifier token.");
            Identifier = identifier;
            Value = value;
            Value.Parent = this;
        }
    }

    // 5. Block (sequence of statements)
    internal class BlockNode : ASTNode
    {
        public List<ASTNode> Statements { get; }

        public BlockNode(List<ASTNode> statements)
        {
            Statements = statements;
            foreach (var stmt in statements)
                stmt.Parent = this;
        }
    }

    // 6. If statement
    internal class IfNode : ASTNode
    {
        public ASTNode Condition { get; }
        public ASTNode ThenBranch { get; }
        public ASTNode? ElseBranch { get; }

        public IfNode(ASTNode condition, ASTNode thenBranch, ASTNode? elseBranch = null)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;

            Condition.Parent = this;
            ThenBranch.Parent = this;
            if (ElseBranch != null)
                ElseBranch.Parent = this;
        }
    }

    // 7. While loop
    internal class WhileNode : ASTNode
    {
        public ASTNode Condition { get; }
        public ASTNode Body { get; }

        public WhileNode(ASTNode condition, ASTNode body)
        {
            Condition = condition;
            Body = body;

            Condition.Parent = this;
            Body.Parent = this;
        }
    }

    // 8. Function call
    internal class FunctionCallNode : ASTNode
    {
        public Token NameToken { get; }
        public List<ASTNode> Arguments { get; }

        public FunctionCallNode(Token nameToken, List<ASTNode> arguments)
        {
            if (nameToken.Type != TokenType.Identifier)
                throw new ArgumentException("Expected an Identifier token for function name.");
            NameToken = nameToken;
            Arguments = arguments;

            foreach (var arg in arguments)
                arg.Parent = this;
        }
    }

    // 9. Function definition
    internal class FunctionDefNode : ASTNode
    {
        public Token NameToken { get; }
        public List<Token> Parameters { get; }
        public ASTNode Body { get; }

        public FunctionDefNode(Token nameToken, List<Token> parameters, ASTNode body)
        {
            if (nameToken.Type != TokenType.Identifier)
                throw new ArgumentException("Expected an Identifier token for function name.");
            NameToken = nameToken;
            Parameters = parameters;
            Body = body;

            Body.Parent = this;
        }
    }
}
