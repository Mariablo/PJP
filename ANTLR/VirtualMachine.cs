using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR
{
    class VirtualMachine
    {
        private Stack<string> stack = new Stack<string>();
        private Dictionary<string, string> variables = new Dictionary<string, string>();
        private string[] operations = { "add", "sub", "mul", "div", "mod" };

		public VirtualMachine(string filename)
		{
			IEnumerable<string> lines = File.ReadLines(filename);

			foreach (var line in lines)
			{
				Parser(line);
			}
		}

		private void Parser(string instruction)
		{
			string[] items;
			if (instruction.Contains("push"))
			{
				// 0 => command, 1 => type, 2 => value
				items = instruction.Split(' ', 3);
				Push(items[2]);
			}
			if (instruction.Contains("print"))
			{
				items = instruction.Split(' ', 2);
				Print(items[1]);
			}
			if (instruction.Contains("load"))
			{
				items = instruction.Split(' ', 2);
				Load(items[1]);

			}
			if (instruction.Contains("save"))
			{
				items = instruction.Split(' ', 2);
				Save(items[1]);
			}
			if (instruction.Contains("pop"))
			{
				Pop("");
			}
			if (operations.Contains(instruction))
			{
				NumberOperation(instruction);
			}
			if (instruction.Contains("concat"))
			{
				Concat();
			}
			if (instruction == "uminus")
			{
				string value = stack.Pop();
				if (value[0] != '-')
					value = "-" + value;
				else
					value = value.Replace("-", "");

				stack.Push(value);
			}
			if (instruction.Contains("read"))
			{
				items = instruction.Split(' ', 2);
				Read(items[1]);
			}
			if (instruction == "and" || instruction == "or")
			{
				LogicOperation(instruction);
			}
			if (instruction == "gt" || instruction == "lt" || instruction == "eq")
			{
				CompareOperation(instruction);
			}
			if (instruction == "not")
			{
				Negation();
			}
		}

		private void Push(string token)
		{
			stack.Push(token);
		}

		private void Load(string var)
		{
			var = var.Trim();
			stack.Push(variables[var]);
		}

		private void Save(string var)
		{
			if (!variables.ContainsKey(var))
				variables.Add(var, stack.Pop());
			else
				variables[var] = stack.Pop();
		}

		private void Read(string type)
		{
			string input;
			bool pass = false;
			switch (type.Trim())
			{
				case "I":
					int i;
                    Console.WriteLine("Int: ");
					input = Console.ReadLine();
					pass = int.TryParse(input, out i);
					if (pass)
						stack.Push(i.ToString());
					break;
				case "F":
					float f;
					Console.WriteLine("Float: ");
					input = Console.ReadLine();
					pass = float.TryParse(input, out f);
					if (pass)
						stack.Push(f.ToString());
					break;
				case "B":
					bool b;
					Console.WriteLine("Bool: ");
					input = Console.ReadLine();
					pass = bool.TryParse(input, out b);
					if (pass)
						stack.Push(b.ToString());
					break;
				case "S":
					Console.WriteLine("String: ");
					input = Console.ReadLine();
					pass = true;
					stack.Push(input);
					break;
				default:
					input = Console.ReadLine();
					stack.Push(input);
					break;
			}

			if (!pass)
				Environment.Exit(1);
		}

		private void Pop(string token)
		{
			stack.Pop();
		}

		private void Print(string n)
		{
			StringBuilder sb = new StringBuilder();
			int c = int.Parse(n);
			string[] tokens = new string[100];

			for (int i = 0; i < c; i++)
			{
				tokens[i] = stack.Pop().Trim('"');
			}

			for (int i = tokens.Length - 1; i >= 0; i--)
			{
				sb.Append(tokens[i]);
			}

			if (sb.ToString() == "\"\"") Console.WriteLine();
			else Console.WriteLine(sb.ToString());
		}

		private void NumberOperation(string op)
		{
			string right = stack.Pop();
			string left = stack.Pop();

			var r = float.Parse(right);
			var l = float.Parse(left);
			float result = 0;

			if (op == "add")
				result = l + r;
			if (op == "sub")
				result = l - r;
			if (op == "mul")
				result = l * r;
			if (op == "div")
				result = l / r;
			if (op == "mod")
				result = l % r;

			stack.Push(result.ToString());
		}

		private void LogicOperation(string op)
		{
			string right = stack.Pop();
			string left = stack.Pop();
			bool l = bool.Parse(left);
			bool r = bool.Parse(right);

			switch (op)
			{
				case "and":
					if (l && r)
						stack.Push("true");
					else
						stack.Push("false");
					break;
				case "or":
					if (l || r)
						stack.Push("true");
					else
						stack.Push("false");
					break;
				default:
					break;
			}
		}

		private void CompareOperation(string op)
		{
			string right = stack.Pop();
			string left = stack.Pop();
			float l, r;

			switch (op.Trim())
			{
				case "lt":
					float.TryParse(left, out l);
					float.TryParse(right, out r);
					if (l < r)
						stack.Push("true");
					else
						stack.Push("false");
					break;
				case "gt":
					float.TryParse(left, out l);
					float.TryParse(right, out r);
					if (l > r)
						stack.Push("true");
					else
						stack.Push("false");
					break;
				case "eq":
					if (left == right)
						stack.Push("true");
					else
						stack.Push("false");
					break;
				default:
					break;
			}
		}

		private void Negation()
		{
			string value = stack.Pop();
			bool b;
			if (bool.TryParse(value, out b))
			{
				if (b)
					stack.Push("false");
				else
					stack.Push("true");
			}
			else
			{
				Environment.Exit(1);
			}
		}

		private void Concat()
		{
			string r = stack.Pop().Trim('"');
			string l = stack.Pop().Trim('"');

			string result = l + r;
			stack.Push(result);
		}
	}
}
