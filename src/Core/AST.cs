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
    // Base class for all AST nodes
    internal abstract class ASTNode
    {
        public ASTNode? Parent { get; set; }
    }

    // Base for all literal-related nodes
    internal abstract class LiteralNodeBase : ASTNode { }

    // Generic base class for literal nodes that hold a typed value
    internal abstract class LiteralNode<T> : LiteralNodeBase
    {
        public abstract T Value { get; set; }
        protected LiteralNode(Token token)
        {
            if (token.Value == null)
                throw new ArgumentException("Token must have a value.");
        }
        protected LiteralNode(T val)
        {
            if (val == null) throw new ArgumentNullException("Argument is null.");
            Value = val;
        }
    }

    // Numeric node base
    internal abstract class NumberNode<T> : LiteralNode<T>
    {
        public abstract override T Value { get; set; }
        protected NumberNode(Token token) : base(token)
        {
            if (token.Type != TokenType.NumberLiteral)
                throw new ArgumentException("Expected a NumberLiteral token.");
        }
        public NumberNode(T val) : base(val) { Value = val; }
    }

    internal class IntegerNode : NumberNode<int>
    {
        public override int Value { get; set; }
        public IntegerNode(Token token) : base(token) { Value = int.Parse(token.Value); }
        public IntegerNode(int val) : base(val) { Value = val; }
    }

    internal class FloatNode : NumberNode<float>
    {
        public override float Value { get; set; }
        public FloatNode(Token token) : base(token) { Value = float.Parse(token.Value); }
        public FloatNode(float val) : base(val) { Value = val; }
    }

    internal class CharNode : LiteralNode<char>
    {
        public override char Value { get; set; }
        public CharNode(Token token) : base(token)
        {
            if (token.Type != TokenType.CharLiteral || token.Value == null)
                throw new ArgumentException("Expected a CharLiteral token with value.");
            Value = token.Value[0];
        }
        public CharNode(char value) : base(value) { Value = value; }
    }

    internal class BoolNode : LiteralNode<bool>
    {
        public override bool Value { get; set; }
        public BoolNode(Token token) : base(token)
        {
            if (token.Type != TokenType.BooleanLiteral || token.Value == null)
                throw new ArgumentException("Expected a BooleanLiteral token with value.");
            Value = token.Value.ToUpper() == "OO";
        }
        public BoolNode(bool value) : base(value) { Value = value; }
    }

    internal class StringNode : LiteralNode<string>
    {
        public override string Value { get; set; }
        public StringNode(Token token) : base(token)
        {
            if (token.Value == null)
                throw new ArgumentException("Expected a token with a value.");
            Value = token.Value;
        }
        public StringNode(string value) : base(value) { Value = value; }
    }

    // Node representing binary operations (e.g., +, -, *, /)
    internal class BinaryOpNode : LiteralNodeBase
    {
        public LiteralNodeBase Left { get; }
        public string Operator { get; }
        public LiteralNodeBase Right { get; }

        public BinaryOpNode(LiteralNodeBase left, Token opToken, LiteralNodeBase right)
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

    // Node representing variable assignments
    internal class AssignmentNode : LiteralNodeBase
    {
        public string VariableName { get; }
        public LiteralNodeBase Value { get; set; }

        public AssignmentNode(string name, LiteralNodeBase value)
        {
            if (value == null)
                throw new ArgumentException("Expected a token with a valid value.");
            Value = value;
            VariableName = name;
            Value.Parent = this;
        }
    }

    // Represents a block of multiple statements
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

    // Node for KUNG conditions (if-else logic)
    internal class IfNode : ASTNode
    {
        public LiteralNodeBase Condition { get; }
        public BlockNode ThenBranch { get; }
        public BlockNode? ElseBranch { get; }

        public IfNode(LiteralNodeBase condition, BlockNode thenBranch, BlockNode? elseBranch = null)
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

    // Node representing a loop (ALANG SA)
    internal class WhileNode : ASTNode
    {
        public LiteralNodeBase Condition { get; }
        public BlockNode Body { get; }

        public WhileNode(LiteralNodeBase condition, BlockNode body)
        {
            Condition = condition;
            Body = body;

            Condition.Parent = this;
            Body.Parent = this;
        }
    }

    // Represents built-in function calls like IPAKITA, DAWAT
    internal class FunctionCallNode : ASTNode
    {
        public string FunctionName { get; }
        public List<ASTNode> Arguments { get; }

        public FunctionCallNode(Token nameToken, List<ASTNode> arguments)
        {
            if (nameToken.Type != TokenType.Keyword || nameToken.Value == null)
                throw new ArgumentException("Expected a Keyword token with a value.");
            FunctionName = nameToken.Value;
            Arguments = arguments;

            foreach (var arg in arguments)
                arg.Parent = this;
        }
    }

    // Function definitions (not yet used)
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
