using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1
{
    public class Evaluation
    {
        // Evaluates the arithmetic expression provided as a string
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
                // Check for unsupported exponential operations
                if (i < tokens.Length - 1 && tokens[i] == '*' && tokens[i + 1] == '*')
                {
                    throw new ArgumentException("Exponential operations are not supported.");
                }
                // Check for invalid characters
                if (tokens[i] >= 'a' && tokens[i] <= 'z' || tokens[i] >= 'A' && tokens[i] <= 'Z')
                {
                    throw new ArgumentException("Invalid character in expression.");
                }
                // Processing numbers
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
                // Handling parentheses
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
                // Handling operators
                else if (tokens[i] == '+' || tokens[i] == '-' || tokens[i] == '*' || tokens[i] == '/')
                {
                    while (ops.Count > 0 && hasPrecedence(tokens[i], ops.Peek()))
                    {
                        values.Push(operate(ops.Pop(), values.Pop(), values.Pop()));
                    }
                    ops.Push(tokens[i]);
                }
            }
            // Final evaluation
            while (ops.Count > 0)
            {
                values.Push(operate(ops.Pop(), values.Pop(), values.Pop()));
            }
            return values.Pop();
        }

        // Determines if the current operator has precedence over the top of the operator stack
        private static bool hasPrecedence(char op1, char op2)
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

        // Performs the operation between two operands based on the operator
        private static int operate(char op, int b, int a)
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
    }
}
