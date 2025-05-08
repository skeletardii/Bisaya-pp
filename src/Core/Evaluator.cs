using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Env = Bisaya__.src.Core.Environment;

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
            else if (node.GetType() == typeof(FloatNode))
                res = (FloatNode)node;
            else if (node.GetType() == typeof(BoolNode))
                res = (BoolNode)node;
            else if (node.GetType() == typeof(CharNode))
                res = (CharNode)node;
            else if (node.GetType() == typeof(VariableNode))
                res = handleVariable((VariableNode)node);
            else if (node.GetType() == typeof(BinaryOpNode))
                res = handleBinaryOp((BinaryOpNode)node);
            else if (node.GetType() == typeof(AssignmentNode))
                res = handleAssignment((AssignmentNode)node);
            else if (node.GetType() == typeof(BlockNode))
                res = handleBlock((BlockNode)node);
            else if (node.GetType() == typeof(IfNode))
                res = handleIf((IfNode)node);
            else if (node.GetType() == typeof(WhileNode))
                res = handleWhile((WhileNode)node);
            else if (node.GetType() == typeof(FunctionCallNode))
                res = handleFunctionCall((FunctionCallNode)node);
            //else if (node.GetType() == typeof(PrintNode))
            //    res = handlePrint((PrintNode)node);

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

        private static LiteralNodeBase handleBinaryOp(BinaryOpNode curr)
        {
            LiteralNodeBase left = (LiteralNodeBase)autoExec(curr.Left);
            LiteralNodeBase right = (LiteralNodeBase)autoExec(curr.Right);
            LiteralNodeBase resNode = null;
            dynamic leftval = getLiteralValue(left);
            dynamic rightval = getLiteralValue(right);
            string op = curr.Operator;
            dynamic res = null;

            switch (op)
            {
                case "+":
                    res = leftval + rightval;
                    break;
                case "-":
                    res = leftval - rightval;
                    break;
                case "*":
                    res = leftval * rightval;
                    break;
                case "/":
                    res = leftval / rightval;
                    break;
                case "%":
                    res = leftval % rightval;
                    break;
                case "==":
                    res = leftval == rightval;
                    break;
                case ">":
                    res = leftval > rightval;
                    break;
                case "<":
                    res = leftval < rightval;
                    break;
                case "<=":
                    res = leftval <= rightval;
                    break;
                case ">=":
                    res = leftval >= rightval;
                    break;
                case "<>":
                    res = leftval < rightval && leftval > rightval;
                    break;

            }

            Type type = res?.GetType();
            if (type == typeof(float))
                resNode = new FloatNode((float)res);
            else if (type == typeof(int))
                resNode = new IntegerNode((int)res);
            else if (type == typeof(bool))
                resNode = new BoolNode((bool)res);

            resNode.Parent = curr.Parent;
            return resNode;
        }

        private static LiteralNodeBase handleAssignment(AssignmentNode curr)
        {
            string varName = curr.VariableName;
            dynamic value = getLiteralValue(curr.Value);
            Env.Set(varName, value);
            return valToLiteral(value);
        }

        private static ASTNode handleVariable(VariableNode node)
        {
            dynamic value = Env.Get(node.VariableName);
            ASTNode par = node.Parent;
            ASTNode res;
            Type type = value.GetType();

            if (type == typeof(float))
                res = new FloatNode((float)value);
            else if (type == typeof(int))
                res = new IntegerNode((int)value);
            else if (type == typeof(char))
                res = new CharNode((char)value);
            else if (type == typeof(bool))
                res = new BoolNode((bool)value);
            else
                throw new Exception("Unsupported type.");

            res.Parent = par;
            return res;
        }

        private static dynamic getLiteralValue(LiteralNodeBase node)
        {
            if (node.GetType() == typeof(IntegerNode))
                return ((IntegerNode)node).Value;
            if (node.GetType() == typeof(FloatNode))
                return ((FloatNode)node).Value;
            if (node.GetType() == typeof(BoolNode))
                return ((BoolNode)node).Value;
            if (node.GetType() == typeof(CharNode))
                return ((CharNode)node).Value;
            return null;
        }

        private static LiteralNodeBase valToLiteral(dynamic value)
        {
            LiteralNodeBase res = null;
            Type type = value.GetType();

            if (type == typeof(float))
                res = new FloatNode((float)value);
            else if (type == typeof(int))
                res = new IntegerNode((int)value);
            else if (type == typeof(char))
                res = new CharNode((char)value);
            else if (type == typeof(bool))
                res = new BoolNode((bool)value);
            else if (type == typeof(string))
                res = new StringNode((string)value);

            return res;
        }

        // Handle If statement
        private static ASTNode handleIf(IfNode node)
        {
            dynamic conditionValue = getLiteralValue(node.Condition);
            if (conditionValue)
            {
                autoExec(node.ThenBranch);
            }
            else
            {
                if (node.ElseBranch != null)
                    autoExec(node.ElseBranch);
            }
            return null;
        }

        // Handle While loop
        private static ASTNode handleWhile(WhileNode node)
        {
            while (true)
            {
                dynamic conditionValue = getLiteralValue(node.Condition);
                if (!conditionValue)
                    break;
                autoExec(node.Body);
            }
            return null;
        }

        // Handle Function calls (e.g., Print)
        private static ASTNode handleFunctionCall(FunctionCallNode node)
        {
            if (node.FunctionName == "print")
            {
                foreach (var arg in node.Arguments)
                {
                    dynamic value = getLiteralValue((LiteralNodeBase)autoExec(arg));
                    Console.Write(value);
                }
            }
            return null;
        }

        // Handle Print statement
        //private static ASTNode handlePrint(PrintNode node)
        //{
        //    dynamic value = getLiteralValue((LiteralNodeBase)autoExec(node.Value));
        //    Console.WriteLine(value);
        //    return null;
        //}
    }
}
