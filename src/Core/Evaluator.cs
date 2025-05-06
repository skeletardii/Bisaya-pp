using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Env = Bisaya__.src.Core.Environment;
/*
The Evaluator executes the AST by walking through it in post order.

For example, given the AST for 5 + 10, the evaluator:
    1. Visits the left node (5).
    2. Visits the right node (10).
    3. Applies the operator (+).
    4. Returns 15.

The evaluator can also:
    1. Handle variables (e.g., storing values in memory).
    2. Execute functions (e.g., calling built-in or user-defined functions).
    3. Manage scope (e.g., function-level vs. global variables).
 */
namespace Bisaya__.src.Core
{
    internal class Evaluator
    {
        public static void evaluate(BlockNode bn)
        {
            handleBlock(bn);
        }
        private static ASTNode autoExec(ASTNode node)
        {
            ASTNode res = node;
            if (node.GetType() == typeof(IntegerNode))
                res = (IntegerNode)node;
            if (node.GetType() == typeof(FloatNode))
                res = (FloatNode)node;
            if (node.GetType() == typeof(BoolNode))
                res = (BoolNode)node;
            if (node.GetType() == typeof(FloatNode))
                res = (FloatNode)node;
            if (node.GetType() == typeof(VariableNode))
                res = handleVariable((VariableNode)node);
            if (node.GetType() == typeof(BinaryOpNode))
                res = handleBinaryOp((BinaryOpNode)node);
            if (node.GetType() == typeof(AssignmentNode))
                return (AssignmentNode)node;
            if (node.GetType() == typeof(BlockNode))
                return (BlockNode)node;
            return res;
        }

        
        private static ASTNode handleBlock(BlockNode block)
        {
            List<ASTNode> queue = block.Statements;
            foreach (ASTNode node in queue)
            {
                autoExec(node);
            }
            return null;
        }
        private static NumberNode handleBinaryOp(BinaryOpNode curr)
        {
            ASTNode left = curr.Left;
            ASTNode right = curr.Right;
            string op = curr.OperatorToken.Value;
            float leftval;
            float rightval;
            string res;
            if (op == "+")
                res = "" + (leftval + rightval);
            else if (op == "-")
                res = "" + (leftval - rightval);
            else if (op == "*")
                res = "" + (leftval * rightval);
            else if (op == "/")
                res = "" + (leftval / rightval);
            return new NumberNode(new Token(TokenType.NumberLiteral,res));
        }
        private static NumberNode handleAssignment(AssignmentNode curr)
        {
            string varName = curr.Identifier.Value;
            dynamic value = curr.Value.Value;
            return new NumberNode(new Token(TokenType.));
        }
        private static ASTNode handleVariable(VariableNode node)
        {
            dynamic value = Env.Get(node.Name);
            ASTNode par = node.Parent;
            ASTNode res;
            Type type = value.GetType();
            if (type == typeof(float))
                res = new FloatNode((float)value);
            else if (type == typeof(int))
                res = new IntNode((int)value);
            else if (type == typeof(char))
                res = new CharNode((char)value);
            else if (type == typeof(bool))
                res = new BoolNode((bool)value);
            else
                throw new Exception("gay");
            res.Parent = par;
            return res;
        }
        private static dynamic autoCast(ASTNode node)
        {
            if (node.GetType() == typeof(NumberNode))
                return (NumberNode)node;
            if (node.GetType() == typeof(VariableNode))
                return (VariableNode)node;
            if (node.GetType() == typeof(BinaryOpNode))
                return (BinaryOpNode)node;
            if (node.GetType() == typeof(AssignmentNode))
                return (AssignmentNode)node;
            if (node.GetType() == typeof(BlockNode))
                return (BlockNode)node;
            return node;
        }
    }
}
