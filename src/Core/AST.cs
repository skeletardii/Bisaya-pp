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

namespace Bisaya__.src.Core
{
    internal abstract class ASTNode
    {
        public ASTNode? Parent { get; set; }
    }

    // Base for number nodes
    internal abstract class NumberNode<T> : ASTNode
    {
        public abstract T Value { get; }

        protected NumberNode(Token token)
        {
            if (token.Value == null)
                throw new ArgumentException("Token must have a value.");
            //Value = (int.Parse(token.Value));
        }
    }

    internal class IntegerNode : NumberNode<int>
    {
        public override int Value { get; }
        public IntegerNode(Token token) : base (token)
        {
            if (token.Type != TokenType.NumberLiteral)
                throw new ArgumentException("Expected an NumberLiteral token.");
        }
    }

    internal class FloatNode : NumberNode<float>
    {
        public override float Value { get; }
        public FloatNode(Token token) : base (token)
        {
            if (token.Type != TokenType.NumberLiteral)
                throw new ArgumentException("Expected a NumberLiteral token.");
        }
    }

    internal class CharNode : ASTNode
    {
        public char Value { get; } 
  
        public CharNode(Token token)
        {
            if (token.Type != TokenType.CharLiteral || token.Value == null)
                throw new ArgumentException("Expected a CharLiteral token.");
            Value = token.Value[0];
        }
    }

    internal class BoolNode : ASTNode
    {
        public bool Value { get; }

        public BoolNode(Token token)
        {
            if (token.Type != TokenType.CharLiteral || token.Value == null)
                throw new ArgumentException("Expected a CharLiteral token.");
            if (token.Value == "oo")
                Value = true;
            Value = false;
        } 
    }

    internal class StringNode : ASTNode
    {
        public string Value { get; }

        public StringNode(Token token)
        {
            if (token.Type != TokenType.StringLiteral || token.Value == null)
                throw new ArgumentException("Expected a CharLiteral token.");
            Value = token.Value;
        }
    }

    // Binary operation
    internal class BinaryOpNode : ASTNode
    {
        public ASTNode Left { get; }
        public string Operator { get; }
        public ASTNode Right { get; }

        public BinaryOpNode(ASTNode left, Token opToken, ASTNode right)
        {
            if (opToken.Value == null)
                throw new ArgumentException("Operator token must have a value.");

            Left = left;
            Operator = opToken.Value;
            Right = right;

            Left.Parent = this;
            Right.Parent = this;
        }
    }

    // Assignment
    internal class AssignmentNode : ASTNode
    {
        public string VariableName { get; }
        public ASTNode Value { get; }


        public AssignmentNode(string name, ASTNode value)
        {
            if (value==null)
                throw new ArgumentException("Expected an token with a valid value.");
            Value = value;
            VariableName = name;

            Value.Parent = this;
        }
    }

    // Block (list of statements)
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

    // If statement
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

    // While loop
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

    // Function call
    internal class FunctionCallNode : ASTNode
    {
        public string FunctionName { get; }
        public List<ASTNode> Arguments { get; }

        public FunctionCallNode(Token nameToken, List<ASTNode> arguments)
        {
            if (nameToken.Type != TokenType.Keyword || nameToken.Value == null)
                throw new ArgumentException("Expected an Identifier token with a value.");
            FunctionName = nameToken.Value;
            Arguments = arguments;

            foreach (var arg in arguments)
                arg.Parent = this;
        }
    }

    // Function definition
    internal class FunctionDefNode : ASTNode
    {
        public string Name { get; }
        public List<string> Parameters { get; }
        public ASTNode Body { get; }

        public FunctionDefNode(Token nameToken, List<Token> parameterTokens, ASTNode body)
        {
            if (nameToken.Type != TokenType.Identifier || nameToken.Value == null)
                throw new ArgumentException("Expected an Identifier token with a value.");
            Name = nameToken.Value;

            Parameters = new List<string>();
            foreach (var param in parameterTokens)
            {
                if (param.Type != TokenType.Identifier || param.Value == null)
                    throw new ArgumentException("Function parameter must be an Identifier token with a value.");
                Parameters.Add(param.Value);
            }

            Body = body;
            Body.Parent = this;
        }
    }
}
