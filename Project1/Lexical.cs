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
            switch (Type)
            {
                case TokenType.Operator: return $"OP:{Value}";
                case TokenType.Number: return $"NUM:{Value}";
                case TokenType.Identifier: return $"ID:{Value}";
                case TokenType.Delimiter:
                    switch (Value)
                    {
                        case "(": return "LPAR";
                        case ")": return "RPAR";
                        case ";": return "SEMICOLON";
                        default: return $"DELIMITER:{Value}";
                    }
                case TokenType.Keyword:
                    return Value.ToUpper();
                default:
                    return $"UNKNOWN:{Value}";
            }
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

            // Handle special cases by inserting spaces around delimiters and operators
            var processedInput = Regex.Replace(noComments, @"(\()|(\))|(;)", " $1$2$3 ");
            processedInput = Regex.Replace(processedInput, @"(?<=\d)-", " - "); // Handle negative numbers

            foreach (Match match in Regex.Matches(processedInput, @"\S+"))
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

            // Additional processing for specific cases, such as separating operators from numbers
            var refinedTokens = new List<Token>();
            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Number && token.Value.StartsWith("-"))
                {
                    refinedTokens.Add(new Token(TokenType.Operator, "-"));
                    refinedTokens.Add(new Token(TokenType.Number, token.Value.Substring(1)));
                }
                else
                {
                    refinedTokens.Add(token);
                }
            }

            return refinedTokens;
        }
    }
}
