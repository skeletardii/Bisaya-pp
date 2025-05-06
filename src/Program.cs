using Bisaya__.src.Core;

string filePath = Path.Combine("tests", "sample.txt");
string content = File.ReadAllText("..\\..\\..\\tests\\sample.txt");
Lexer.tokenize(content);

//Evaluator.evaluate();