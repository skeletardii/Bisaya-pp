using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
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

        private static ASTNode handle(ASTNode node)
        {
            ASTNode res = node;

            if (node.GetType() == typeof(IntegerNode))
                res = (IntegerNode)node;
            else if (node.GetType() == typeof(ForLoopNode))
                res = handleForLoop((ForLoopNode)node);
            else if (node.GetType() == typeof(FloatNode))
                res = (FloatNode)node;
            else if (node.GetType() == typeof(BoolNode))
                res = (BoolNode)node;
            else if (node.GetType() == typeof(CharNode))
                res = (CharNode)node;
            else if (node.GetType() == typeof(StringNode))
                res = (StringNode)node;
            else if (node.GetType() == typeof(VariableNode))
                res = handleVariable((VariableNode)node);
            else if (node.GetType() == typeof(BinaryOpNode))
                res = handleBinaryOp((BinaryOpNode)node);
            else if (node.GetType() == typeof(UnaryOpNode))
                res = handleUnaryOp((UnaryOpNode)node);
            else if (node.GetType() == typeof(AssignmentNode))
                res = handleAssignment((AssignmentNode)node);
            else if (node.GetType() == typeof(BlockNode))
                res = handleBlock((BlockNode)node);
            else if (node.GetType() == typeof(IfNode))
                res = handleIf((IfNode)node);
            else if (node.GetType() == typeof(WhileNode))
                res = handleWhile((WhileNode)node);
            else if (node.GetType() == typeof(DeclarationNode))
                res = handleDeclaration((DeclarationNode)node);
            else if (node.GetType() == typeof(OutputNode))
                res = handlePrint((OutputNode)node);
            else if (node.GetType() == typeof(InputNode))
                res = handleInput((InputNode)node);

            return res;
        }

        private static ASTNode handleBlock(BlockNode block)
        {
            if (block == null) return null;
            List<ASTNode> queue = block.Statements;
            foreach (ASTNode node in queue)
            {
                if (node.GetType() == typeof(VariableNode))
                    throw new Exception("Unexpected Token");
                handle(node);
            }
            return null;
        }
        private static LiteralNodeBase handleUnaryOp(UnaryOpNode node)
        {
            LiteralNodeBase val = (LiteralNodeBase)handle(node.Operand);
            dynamic value = getLiteralValue(val);
            string op = node.OperatorToken.Value;
            if (op == "-")
                value = -value;
            else if (op == "++")
                value++;
            else if (op == "--")
                value--;
            else if (op == "!")
                value = !value;
            else if (op == "~")
                value = ~value;
            return valToLiteral(value);
        }

        private static LiteralNodeBase handleBinaryOp(BinaryOpNode curr)
        {
            LiteralNodeBase left = (LiteralNodeBase)handle(curr.Left);
            LiteralNodeBase right = (LiteralNodeBase)handle(curr.Right);
            LiteralNodeBase resNode = null;
            dynamic leftval = getLiteralValue(left);
            dynamic rightval = getLiteralValue(right);
            dynamic res = null;
            if (leftval.GetType() == typeof(char)) leftval = "" + leftval;
            if (rightval.GetType() == typeof(char)) rightval = "" + rightval;
            string op = curr.Operator;
            if (op != "UG" && op != "O")
            {
                if (leftval.GetType() == typeof(bool))
                if ((bool)leftval)
                    leftval = "OO";
                else
                    leftval = "DILI";
                if (rightval.GetType() == typeof(bool))
                    if ((bool)rightval)
                        rightval = "OO";
                    else
                        rightval = "DILI";
            }
            switch (op)
            {
                case "+": res = leftval + rightval; break;
                case "-": res = leftval - rightval; break;
                case "*": res = leftval * rightval; break;
                case "/": res = leftval / rightval; break;
                case "%": res = leftval % rightval; break;
                case "==": res = leftval == rightval; break;
                case "<>": res = leftval != rightval; break;
                case ">": res = leftval > rightval; break;
                case "<": res = leftval < rightval; break;
                case ">=": res = leftval >= rightval; break;
                case "<=": res = leftval <= rightval; break;
                case "UG": res = leftval && rightval; break;
                case "O": res = leftval || rightval; break;
                case "&": res = "" + leftval + rightval; break;
            }

            Type type = res?.GetType();
            if (type == typeof(float))
                resNode = new FloatNode((float)res);
            else if (type == typeof(int))
                resNode = new IntegerNode((int)res);
            else if (type == typeof(bool))
                resNode = new BoolNode((bool)res);
            else if (type == typeof(string))
                resNode = new StringNode((string)res);
                return resNode;
            throw new Exception($"Unsupported type: {type} for operator {op}.");
        }

        private static LiteralNodeBase handleAssignment(AssignmentNode curr)
        {
            string varName = curr.VariableName;
            ASTNode valnode = curr.Value;
            dynamic value = null;

            if (valnode is VariableNode varNode && (varNode.VariableName == "++" || varNode.VariableName == "--"))
            {
                dynamic currentVal = Env.Get(varName);
                value = varNode.VariableName == "++" ? currentVal + 1 : currentVal - 1;
            }
            else if (valnode is AssignmentNode nestedAssign)
            {
                var resultNode = handleAssignment(nestedAssign);
                value = getLiteralValue(resultNode);
            }
            else
            {
                value = getLiteralValue((LiteralNodeBase)handle(valnode));
            }

            Env.Set(varName, value);
            return valToLiteral(value);
        }

        private static ASTNode handleDeclaration(DeclarationNode curr)
        {
            string varName = curr.VariableName;
            Type t = curr.VariableType;
            if (t == typeof(string)) t = typeof(char);
            dynamic value = null;
            if (curr.InitialValue != null)
            {
                value = getLiteralValue(curr.InitialValue);
                value = Convert.ChangeType(value, t);
                if (value.GetType() == typeof(string) && value.Length == 1)
                    value = Convert.ChangeType(value, typeof(char));
                Env.Set(varName, value);
                return valToLiteral(value);
            }
            Env.Set(varName, Convert.ChangeType((byte)0,t));
            return null;
        }
        private static ASTNode handleVariable(VariableNode node)
        {
            dynamic value = Env.Get(node.VariableName);
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
            else if (type == typeof(string))
                res = new CharNode((char)value[0]);
            else
                throw new Exception("Unsupported type.");

            return res;
        }

        private static dynamic getLiteralValue(LiteralNodeBase node)
        {
            if (node.GetType() == typeof(BinaryOpNode))
                return getLiteralValue((LiteralNodeBase)handle((BinaryOpNode)node));
            if (node.GetType() == typeof(UnaryOpNode))
                return getLiteralValue((LiteralNodeBase)handle((UnaryOpNode)node));
            if (node.GetType() == typeof(IntegerNode))
                return ((IntegerNode)node).Value;
            if (node.GetType() == typeof(FloatNode))
                return ((FloatNode)node).Value;
            if (node.GetType() == typeof(BoolNode))
                return ((BoolNode)node).Value;
            if (node.GetType() == typeof(CharNode))
                return ((CharNode)node).Value;
            if (node.GetType() == typeof(StringNode))
                return ((StringNode)node).Value;
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
            if (node.Condition == null)
                if (node.ElseBranch != null)
                    return handle(node.ElseBranch);
                else if (node.ThenBranch != null)
                    return handle(node.ThenBranch);
            bool condition = (bool)(getLiteralValue(node.Condition));
            if (condition)
                return handle(node.ThenBranch);
            else if (node.ElseBranch != null)
                return handle(node.ElseBranch);
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
                handle(node.Body);
            }
            return null;
        }

        //Handle Print statement
        private static ASTNode handlePrint(OutputNode node)
        {
            LiteralNodeBase output = (LiteralNodeBase)(handle(node.Expression));
            dynamic value = getLiteralValue(output);
            if (value.GetType() == typeof(bool))
                if ((bool)value)
                    value = "OO";
                else
                    value = "DILI";
            Console.Write(value);
            return null;
        }
        // Commented out for debugging purposes, fix handleInput method to receive List<String> of variable names
        private static ASTNode handleInput(InputNode node)
        {
            int i = 0;
            string inputLine = Console.ReadLine();
            string[] inputs = inputLine.Split(",");
            List<string> varnames = node.VariableNames;
            while (i < node.VariableNames.Count && varnames[i] != null && i < inputs.Length && inputs[i] != null) {
                string inp = inputs[i].Trim();
                string varname = node.VariableNames[i];
                dynamic r = Env.Get(varname);
                if (r == null)
                    throw new Exception($"Variable {varname} does not exist in this scope.");
                Type t = r.GetType();
                if (t == typeof(string))
                    t = typeof(char);
                if (t == typeof(int))
                    r = int.Parse(inp);
                else if (t == typeof(char) || t == typeof(string))
                    r = inp[0];
                else if (t == typeof(float))
                    r = float.Parse(inp);
                else if (t == typeof(bool) && (inp == "OO" || inp == "DILI"))
                    r = (inp == "OO");
                else
                    throw new Exception($"Invalid Input, expected {t}");
                Env.Set(varname, r);
                i++;
            }
            return null;
        }
        private static ASTNode handleForLoop(ForLoopNode node)
        {
            // Step 1: Initialize the loop variable correctly
            handleAssignment(node.declaration); // This sets initial value into Env

            while (true)
            {
                // Step 2: Evaluate condition dynamically each iteration
                bool conditionValue = getLiteralValue((LiteralNodeBase)handle(node.condition));

                if (!conditionValue)
                    break;
                handle(node.Body);
                handleAssignment((AssignmentNode)node.increment);
            }

            return null;
        }

    }
}
