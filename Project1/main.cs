using System;
using System.Collections.Generic;
using System.Text;
//ahoj jak se mas druha zmena hihi
public class EvaluateString
{
    public static int evaluate(string expression)
    {
        char[] tokens = expression.ToCharArray();
        Stack<int> values = new Stack<int>();
        Stack<char> ops = new Stack<char>();
        for (int i = 0; i < tokens.Length; i++)
        {
            if (tokens[i] == ' ')
            {
                continue;
            }
            if (i < tokens.Length - 1 && tokens[i] == '*' && tokens[i + 1] == '*')  // Check for **
            {
                throw new ArgumentException("Exponential operations are not supported.");
            }
            if (tokens[i] >= 'a' && tokens[i] <= 'z' || tokens[i] >= 'A' && tokens[i] <= 'Z')
            {
                throw new ArgumentException("Invalid character in expression.");
            }
            if (tokens[i] >= '0' && tokens[i] <= '9')
            {
                StringBuilder sbuf = new StringBuilder();
                while (i < tokens.Length && tokens[i] >= '0' && tokens[i] <= '9')
                {
                    sbuf.Append(tokens[i++]);
                }
                values.Push(int.Parse(sbuf.ToString()));
                i--;
            }
            else if (tokens[i] == '(')
            {
                ops.Push(tokens[i]);
            }
            else if (tokens[i] == ')')
            {
                while (ops.Peek() != '(')
                {
                    values.Push(operate(ops.Pop(), values.Pop(), values.Pop()));
                }
                ops.Pop();
            }
            else if (tokens[i] == '+' || tokens[i] == '-' || tokens[i] == '*' || tokens[i] == '/')
            {
                while (ops.Count > 0 && hasPrecedence(tokens[i], ops.Peek()))
                {
                    values.Push(operate(ops.Pop(), values.Pop(), values.Pop()));
                }
                ops.Push(tokens[i]);
            }
        }
        while (ops.Count > 0)
        {
            values.Push(operate(ops.Pop(), values.Pop(), values.Pop()));
        }
        return values.Pop();
    }

    public static bool hasPrecedence(char op1, char op2)
    {
        if (op2 == '(' || op2 == ')')
        {
            return false;
        }
        if ((op1 == '*' || op1 == '/') && (op2 == '+' || op2 == '-'))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static int operate(char op, int b, int a)
    {
        switch (op)
        {
            case '+':
                return a + b;
            case '-':
                return a - b;
            case '*':
                return a * b;
            case '/':
                if (b == 0)
                {
                    throw new DivideByZeroException("Cannot divide by zero.");
                }
                return a / b;
            default:
                throw new ArgumentException("Unsupported operation: " + op);
        }
    }

    public static void Main(string[] args)
    {
        string input = Console.ReadLine();
        if (!int.TryParse(input, out int count_of_operation))
        {
            Console.WriteLine("You must enter a number.");
            return;
        }

        List<string> results = new List<string>(); // Use a list to store results or errors

        for (int i = 0; i < count_of_operation; i++)
        {
            string method = Console.ReadLine();
            try
            {
                int result = evaluate(method);
                results.Add(result.ToString()); // Store the result as a string
            }
            catch (Exception ex)
            {
                results.Add("ERROR"); // Store "ERROR" in case of an exception
            }
        }

        // After evaluating all expressions, print all results
        foreach (string result in results)
        {
            Console.WriteLine(result);
        }

        Console.ReadLine();
    }
}
