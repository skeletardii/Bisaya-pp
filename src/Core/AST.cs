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
    // Base node type
    internal abstract class ASTNode
    {
        public ASTNode Parent { get; set; }
    }

    // 1. Literals
    internal class NumberNode : ASTNode
    {
        public int Value { get; }
        public NumberNode(int value) => Value = value;
    }

    // 2. Variable reference
    internal class VariableNode : ASTNode
    {
        public string Name { get; }
        public VariableNode(string name) => Name = name;
    }

    // 3. Binary operation (e.g., a + b)
    internal class BinaryOpNode : ASTNode
    {
        public ASTNode Left { get; }
        public string Operator { get; }
        public ASTNode Right { get; }

        public BinaryOpNode(ASTNode left, string op, ASTNode right)
        {
            Left = left;
            Right = right;
            Operator = op;

            Left.Parent = this;
            Right.Parent = this;
        }
    }

    // 4. Assignment (e.g., x = 5)
    internal class AssignmentNode : ASTNode
    {
        public string VariableName { get; }
        public ASTNode Value { get; }

        public AssignmentNode(string variableName, ASTNode value)
        {
            VariableName = variableName;
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

    // 6. If Statement
    internal class IfNode : ASTNode
    {
        public ASTNode Condition { get; }
        public ASTNode ThenBranch { get; }
        public ASTNode ElseBranch { get; }

        public IfNode(ASTNode condition, ASTNode thenBranch, ASTNode elseBranch = null)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;

            Condition.Parent = this;
            ThenBranch.Parent = this;
            if (ElseBranch != null) ElseBranch.Parent = this;
        }
    }

    // 7. While Loop
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

    // 8. Function Call
    internal class FunctionCallNode : ASTNode
    {
        public string Name { get; }
        public List<ASTNode> Arguments { get; }

        public FunctionCallNode(string name, List<ASTNode> arguments)
        {
            Name = name;
            Arguments = arguments;

            foreach (var arg in arguments)
                arg.Parent = this;
        }
    }

    // 9. Function Definition
    internal class FunctionDefNode : ASTNode
    {
        public string Name { get; }
        public List<string> Parameters { get; }
        public ASTNode Body { get; }

        public FunctionDefNode(string name, List<string> parameters, ASTNode body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;

            Body.Parent = this;
        }
    }
}
