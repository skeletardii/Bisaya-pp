using Bisaya__.src.Core;
string filePath = Path.Combine("tests","sample.txt");
string content = File.ReadAllText("..\\..\\..\\tests\\testcases\\test9.bpp");

static string padright(string s, int width)
{
    for (int i = s.Length; i < width; i++)
    {
        s += " ";
    }
    return s;
}
// See list of tokens in program
List<List<Token>> tokens = Lexer.tokenize(content);
foreach (List<Token> line in tokens)
{
    foreach (Token token in line)
    {
        Console.WriteLine($"{padright(token.Type.ToString(),20)} \t {padright(token.Value.ToString(), 10)} \t Line: {token.LineNumber} \t Col: {token.ColumnNumber}");
    }
}