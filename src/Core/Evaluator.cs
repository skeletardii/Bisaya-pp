using System;
using System.Collections.Generic;
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

        private static ASTNode autoExec(ASTNode node)
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
            //else if (node.GetType() == typeof(FunctionCallNode))
            //    res = handleFunctionCall((FunctionCallNode)node);
            else if (node.GetType() == typeof(OutputNode))
                res = handlePrint((OutputNode)node);
            else if (node.GetType() == typeof(InputNode))
                res = handleInput((InputNode)node);

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
                case "<>":
                    res = leftval != rightval;
                    break;
                case ">":
                    res = leftval > rightval;
                    break;
                case "<":
                    res = leftval < rightval;
                    break;
                case ">=":
                    res = leftval >= rightval;
                    break;
                case "<=":
                    res = leftval <= rightval;
                    break;


                case "UG":
                    res = leftval && rightval;
                    break;
                case "O":
                    res = leftval || rightval;
                    break;

                case "&":
                    res = "" + leftval + rightval;
                    break;
            }

            Type type = res?.GetType();
            if (type == typeof(float))
                resNode = new FloatNode((float)res);
            else if (type == typeof(int))
                resNode = new IntegerNode((int)res);
            else if (type == typeof(bool))
                resNode = new BoolNode((bool)res);
            else
                resNode = new StringNode((string)res);
                return resNode;
        }

        private static LiteralNodeBase handleAssignment(AssignmentNode curr)
        {
            //Console.WriteLine($"Assigning {curr.VariableName} = {curr.Value}");
            string varName = curr.VariableName;
            ASTNode valnode = curr.Value;
            dynamic value = null;
            if (valnode.GetType()==typeof(VariableNode) && (((VariableNode)valnode).VariableName == "++" || ((VariableNode)valnode).VariableName == "--"))
                value = Env.Get(varName) + 1;
            else
                value = getLiteralValue(curr.Value);
            Env.Set(varName, value);
            return valToLiteral(value);
        }
        private static ASTNode handleDeclaration(DeclarationNode curr)
        {
            string varName = curr.VariableName;
            Type t = curr.VariableType;
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
            else
                throw new Exception("Unsupported type.");

            return res;
        }

        private static dynamic getLiteralValue(LiteralNodeBase node)
        {
            if (node.GetType() == typeof(BinaryOpNode))
                return getLiteralValue((LiteralNodeBase)autoExec((BinaryOpNode)node));
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
        //private static ASTNode handOutputNode(OutputNode node)
        //{
        //    if (node.FunctionName == "IPAKITA")
        //    {
        //        foreach (var arg in node.Arguments)
        //        {
        //            dynamic value = getLiteralValue((LiteralNodeBase)autoExec(arg));
        //            Console.Write(value);
        //        }
        //    }
        //    return null;
        //}

        //Handle Print statement
        private static ASTNode handlePrint(OutputNode node)
        {
            //Console.WriteLine(node.Expression);
            LiteralNodeBase output = (LiteralNodeBase)(autoExec(node.Expression));
            //Console.WriteLine(output);
            dynamic value = getLiteralValue(output);
            Console.Write(value);
            return null;
        }
        private static ASTNode handleInput(InputNode node)
        {
            string inp = Console.ReadLine();
            string varname = node.VariableName;
            dynamic r = Env.Get(varname);
            if (r == null)
                throw new Exception($"Variable {varname} does not exist in this scope.");
            Type t = r.GetType();
            if (t == typeof(int))
                r = int.Parse(inp);
            else if (t == typeof(char))
                r = int.Parse(inp);
            else if (t == typeof(float))
                r = float.Parse(inp);
            else if (t == typeof(bool) && (inp == "\"OO\"" || inp == "\"DILI\""))
                r = (inp == "\"OO\"");
            else
                throw new Exception($"Invalid Input, expected {t}");
            Env.Set(varname, r);
            return valToLiteral(r);
        }
        private static ASTNode handleForLoop(ForLoopNode node)
        {
            string varname = node.declaration.VariableName;
            dynamic loopvar = node.declaration;
            BinaryOpNode condition = node.condition;
            ASTNode increment = node.increment;
            bool loopVarExists = ( Env.Get(varname) != null );
            Env.Set(varname, loopvar);
            while (getLiteralValue(condition) == true)
            {
                //Console.WriteLine("ASS");
                autoExec(node.Body);
                autoExec(node.increment);
            }
            return null;
        }
    }
}
