using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Project1
{
    public enum TokenType
    {
        Identifier, Number, Operator, Delimiter, Keyword
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Type}:{Value}";
        }
    }

    public class Lexical
    {
        public static List<Token> Analyze(string input)
        {
            var tokens = new List<Token>();
            var tokenDefinitions = new (Regex, TokenType)[]
            {
            (new Regex(@"\b(div|mod)\b"), TokenType.Keyword),
            (new Regex(@"[a-zA-Z]\w*"), TokenType.Identifier),
            (new Regex(@"\d+"), TokenType.Number),
            (new Regex(@"[\+\-\*\/]"), TokenType.Operator),
            (new Regex(@"[\(\);]"), TokenType.Delimiter)
            };

            // Remove comments
            var noComments = Regex.Replace(input, @"//.*", "");

            foreach (Match match in Regex.Matches(noComments, @"\S+"))
            {
                foreach (var (regex, tokenType) in tokenDefinitions)
                {
                    if (regex.IsMatch(match.Value))
                    {
                        tokens.Add(new Token(tokenType, match.Value));
                        break;
                    }
                }
            }

            return tokens;
        }
    }
}
