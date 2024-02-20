using System;
using System.Collections.Generic;
using System.Text;
//ahoj jak se mas druha zmena hihi
namespace Project1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Read the number of operations to perform
            string input = Console.ReadLine();
            if (!int.TryParse(input, out int count_of_operation))
            {
                Console.WriteLine("You must enter a number.");
                return;
            }

            List<string> results = new List<string>(); // Use a list to store results or errors

            for (int i = 0; i < count_of_operation; i++)
            {
                string expression = Console.ReadLine();
                try
                {
                    // Evaluate the expression using the Evaluation class
                    int result = Evaluation.evaluate(expression);
                    results.Add(result.ToString()); // Store the result as a string
                }
                catch (Exception ex)
                {
                    results.Add("ERROR"); // Store "ERROR" in case of an exception
                }
            }

            // Print all results or errors
            foreach (string result in results)
            {
                Console.WriteLine(result);
            }

            Console.ReadLine(); // Wait for user input before closing
        }
    }
}
