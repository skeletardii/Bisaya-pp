using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Bisaya__.src.Core;

class Program
{
    public static bool verbose = false;
    static void Main(string[] args) //   !!! PLEASE I READ ANG NOTEPAD TAB !!!
    {
        //int tesno = 9;
        string readme = "--READ ME !!!\r\n--ang gi assign sa parser kay wala naka tiwas\r\n--lexer -> parser -> interpreter mn unta\r\n--so lexer ra maka run for now :|\r\n--pero naa ray functions ang evaluator dili lang ma test kay walay parser :| ";
        //Console.WriteLine(readme);
        // Load source code from test file
        string content = File.ReadAllText("..\\..\\..\\tests\\testcases\\12_ok.txt");

        // === LEXING ===
        Console.WriteLine("=== Lexing ===");
        List<List<Token>> tokens = Lexer.tokenize(content);

        foreach (List<Token> line in tokens)
        {
            foreach (Token token in line)
            {
                Console.WriteLine($"{PadRight(token.Type.ToString(), 20)} \t {PadRight(token.Value.ToString(), 10)} \t Line: {token.LineNumber} \t Col: {token.ColumnNumber}");
            }
        }

        // === PARSING ===
        Console.WriteLine("\n=== Parsing ===");
        BlockNode ast = null;
        //try
        //{
            Parser parser = new Parser(tokens);
            ast = (BlockNode)parser.ParseProgram();

            Console.WriteLine("✅ Parsing successful!");
            Console.WriteLine($"Root node type: {ast.GetType().Name}");
            Console.WriteLine($"Statements in root block: {ast.Statements.Count}");

            foreach (var stmt in ast.Statements)
            {
                Console.WriteLine($"• {stmt.GetType().Name}");
            }

            // === AST TREE PRINT ===
            Console.WriteLine("\n=== AST Tree ===");
            PrintAST(ast, 0);
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"❌ Parsing failed: {ex.Message}");
        //    Console.WriteLine(ex.StackTrace.ToString());
        //    Console.WriteLine("\n==============================\n" + readme + "\n==============================");
        //}

        // === EVALUATION ===
        Console.WriteLine("\n=== Evaluating ===");
        Evaluator.evaluate(ast);
    }

    // Helper to align text
    static string PadRight(string s, int width)
    {
        while (s.Length < width)
            s += " ";
        return s;
    }

    // Recursively print AST structure
    static void PrintAST(ASTNode node, int indent)
    {
        if (node == null)
        {
            string indentStrNull = new string(' ', indent * 2);
            Console.WriteLine($"{indentStrNull}<null>");
            return;
        }

        string indentStr = new string(' ', indent * 2);
        Console.WriteLine($"{indentStr}- {node.GetType().Name}");

        switch (node)
        {
            case BlockNode block:
                foreach (var stmt in block.Statements)
                    PrintAST(stmt, indent + 1);
                break;

            case AssignmentNode assign:
                Console.WriteLine($"{indentStr}  Variable: {assign.VariableName}");
                PrintAST(assign.Value, indent + 1);
                break;

            case BinaryOpNode bin:
                Console.WriteLine($"{indentStr}  Operator: {bin.Operator}");
                PrintAST(bin.Left, indent + 1);
                PrintAST(bin.Right, indent + 1);
                break;

            case FunctionCallNode func:
                Console.WriteLine($"{indentStr}  Call: {func.FunctionName}");
                foreach (var arg in func.Arguments)
                    PrintAST(arg, indent + 1);
                break;

            case IfNode ifNode:
                Console.WriteLine($"{indentStr}  Condition:");
                PrintAST(ifNode.Condition, indent + 1);

                Console.WriteLine($"{indentStr}  Then:");
                PrintAST(ifNode.ThenBranch, indent + 1);

                if (ifNode.ElseBranch != null)
                {
                    Console.WriteLine($"{indentStr}  Else:");
                    PrintAST(ifNode.ElseBranch, indent + 1);
                }
                break;

            case WhileNode loop:
                Console.WriteLine($"{indentStr}  Condition:");
                PrintAST(loop.Condition, indent + 1);

                Console.WriteLine($"{indentStr}  Body:");
                PrintAST(loop.Body, indent + 1);
                break;

            case DeclarationNode declaration:
                Console.WriteLine($"{indentStr}  Var: {declaration.VariableName}");
                if (declaration.InitialValue != null)
                    PrintAST(declaration.InitialValue, indent + 1);
                break;

            case OutputNode output:
                PrintAST(output.Expression, indent + 1);
                break;

            case VariableNode varnode:
                Console.WriteLine($"{indentStr}  Var: {varnode.VariableName}");
                break;

            case LiteralNodeBase literal:
                var valueProp = literal.GetType().GetProperty("Value");
                var val = valueProp?.GetValue(literal)?.ToString() ?? "null";
                Console.WriteLine($"{indentStr}  Value: {val}");
                break;

            case ForLoopNode forloop:
                Console.WriteLine($"{indentStr}");
                PrintAST(forloop.declaration, indent + 2);
                PrintAST(forloop.condition, indent + 2);
                PrintAST(forloop.increment, indent + 2);

                Console.WriteLine($"{indentStr}- Body:");
                PrintAST(forloop.Body, indent + 1);
                break;
            case UnaryOpNode unary:
                Console.WriteLine($"{indentStr}Operand: {unary.OperatorToken.Value}");
                PrintAST(unary.Operand, indent + 2);
                break;
        }
    }

}