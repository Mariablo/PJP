using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Globalization;
using System.IO;
using System.Threading;


namespace ANTLR
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            foreach (var filename in new[] { "input4.txt" })
            {
                Console.WriteLine("Parsing: " + filename);
                var inputFile = new StreamReader(filename);
                AntlrInputStream input = new AntlrInputStream(inputFile);
                inputFile.Close();
                ExprLexer lexer = new ExprLexer(input);
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                ExprParser parser = new ExprParser(tokens);

                parser.AddErrorListener(new VerboseErrorListener());

                IParseTree tree = parser.program();

                if (parser.NumberOfSyntaxErrors == 0)
                {
                    //Console.WriteLine(tree.ToStringTree(parser));

                    SymbolTable symbolTable = new SymbolTable();

                    TypeChecking typeChecking = new TypeChecking(symbolTable);
                    ParseTreeWalker parseTree = new ParseTreeWalker();
                    parseTree.Walk(typeChecking, tree);

                    if (Errors.NumberOfErrors > 0)
                    {
                        Errors.PrintAndClearErrors();
                        //break;
                    }
                }
            }
        }
    }
}
