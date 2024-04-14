using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace ANTLR
{
    class EvalVisitor : ExprBaseVisitor<string>
    {
        private SymbolTable symbolTable;
        private ParseTreeProperty<Type> types;

        public EvalVisitor(SymbolTable symbolTable, ParseTreeProperty<Type> types)
        {
            this.symbolTable = symbolTable;
            this.types = types;
        }
        private int label = 0;
        private int NextLabel()
        {
            return label++;
        }

        public override string VisitProgram([NotNull] ExprParser.ProgramContext context)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var statement in context.command())
            {
                sb.Append(Visit(statement));
            }
            return sb.ToString();
        }

        public override string VisitCode_block([NotNull] ExprParser.Code_blockContext context)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var statement in context.command())
            {
                sb.Append(Visit(statement));
            }
            return sb.ToString();
        }

        public override string VisitEmpty([NotNull] ExprParser.EmptyContext context)
        {
            return "";
        }

        public override string VisitDeclaration([NotNull] ExprParser.DeclarationContext context)
        {
            StringBuilder result = new StringBuilder();

            foreach (var identifier in context.IDENTIFIER())
            {
                switch (types.Get(context.primitiveType()))
                {
                    case Type.Int:
                        result.AppendLine($"push I 0");
                        break;
                    case Type.Float:
                        result.AppendLine($"push F 0.0");
                        break;
                    case Type.String:
                        result.AppendLine($"push S \"\"");
                        break;
                    case Type.Bool:
                        result.AppendLine($"push B false");
                        break;
                    default:
                        break;
                }

                result.AppendLine($"save {identifier.GetText()}");
            }

            return result.ToString();
        }

        public override string VisitWrite([NotNull] ExprParser.WriteContext context)
        {
            StringBuilder result = new StringBuilder();

            foreach (var expression in context.expr())
            {
                result.Append(Visit(expression));
            }

            int numberOfValues = context.expr().Length;
            result.AppendLine($"print {numberOfValues}");

            return result.ToString();
        }

        public override string VisitRead([NotNull] ExprParser.ReadContext context)
        {
            StringBuilder result = new StringBuilder();

            foreach (var identifier in context.IDENTIFIER())
            {
                Type type = symbolTable[identifier.Symbol].Type;
                result.AppendLine($"read {type.ToString()[0]}");
                result.AppendLine($"save {identifier.GetText()}");
            }

            return result.ToString();
        }

        public override string VisitIf([NotNull] ExprParser.IfContext context)
        {
            StringBuilder result = new StringBuilder();

            result.Append($"{Visit(context.expr())}");
            int label = NextLabel();
            int label2 = NextLabel();
            result.AppendLine($"fjmp {label}");
            result.Append($"{Visit(context.command()[0])}");
            result.AppendLine($"jmp {label2}");
            result.AppendLine($"label {label}");

            if (context.command().Length == 2)
                result.Append($"{Visit(context.command()[0])}");

            result.AppendLine($"label {label2}");

            return result.ToString();
        }

        public override string VisitWhile([NotNull] ExprParser.WhileContext context)
        {
            StringBuilder result = new StringBuilder();

            int label = NextLabel();
            int label2 = NextLabel();

            result.AppendLine($"label {label}");
            result.Append($"{Visit(context.expr())}");
            result.AppendLine($"fjmp {label2}");
            result.Append($"{Visit(context.expr())}");
            result.AppendLine($"jmp {label}");
            result.AppendLine($"label {label2}");

            return result.ToString();
        }

        public override string VisitFor([NotNull] ExprParser.ForContext context)
        {
            StringBuilder result = new StringBuilder();

            int startLabel = NextLabel();   // Label for the start of the loop
            int endLabel = NextLabel();     // Label for the end of the loop (exit point)

            // Initialization (run once before the loop starts)
            if (context.expr(0) != null)
            {
                result.Append($"{Visit(context.expr(0))}");  // Execute the initialization expression
            }

            result.AppendLine($"label {startLabel}");  // Start label

            // Condition (checked at the beginning of each loop iteration)
            if (context.expr(1) != null)
            {
                result.Append($"{Visit(context.expr(1))}");  // Execute the condition expression
                result.AppendLine($"fjmp {endLabel}");  // If the condition is false, jump to the end label, exiting the loop
            }

            // Loop body
            if (context.command() != null)
            {
                result.Append($"{Visit(context.command())}");
            }

            // Iteration step (executed at the end of each loop iteration before re-checking the condition)
            if (context.expr(2) != null)
            {
                result.Append($"{Visit(context.expr(2))}");  // Execute the iteration expression
            }

            result.AppendLine($"jmp {startLabel}");  // Jump back to start to re-check the condition
            result.AppendLine($"label {endLabel}");  // End label

            return result.ToString();
        }


        public override string VisitExpression([NotNull] ExprParser.ExpressionContext context)
        {
            StringBuilder result = new StringBuilder();

            result.Append($"{Visit(context.expr())}");
            result.AppendLine("pop");

            return result.ToString();
        }

        public override string VisitPrimary([NotNull] ExprParser.PrimaryContext context)
        {
            StringBuilder result = new StringBuilder();

            if (context.expr() != null)
                result.Append(Visit(context.expr()));

            if (context.DECIMAL_LITERAL() != null)
                result.AppendLine($"push I {context.DECIMAL_LITERAL().GetText()}");
            if (context.FLOAT_LITERAL() != null)
                result.AppendLine($"push F {context.FLOAT_LITERAL().GetText()}");
            if (context.STRING_LITERAL() != null)
                result.AppendLine($"push S {context.STRING_LITERAL().GetText()}");
            if (context.BOOL_LITERAL() != null)
                result.AppendLine($"push B {context.BOOL_LITERAL().GetText()}");

            if (context.IDENTIFIER() != null)
                result.AppendLine($"load {context.IDENTIFIER().GetText()}");

            return result.ToString();
        }

        public override string VisitExpr([NotNull] ExprParser.ExprContext context)
        {
            StringBuilder result = new StringBuilder();

            if (context.primary() != null)
            {
                result.Append($"{Visit(context.primary())}");
                return result.ToString();
            }

            if (context.prefix != null)
            {
                if (context.prefix.Type == ExprParser.SUB)
                {
                    result.Append($"{Visit(context.expr()[0])}");
                    result.AppendLine("uminus");
                    return result.ToString();
                }
                if (context.prefix.Type == ExprParser.BANG)
                {
                    result.Append($"{Visit(context.expr()[0])}");
                    result.AppendLine("not");
                    return result.ToString();
                }


                return result.ToString();
            }

            if (context.IDENTIFIER() != null)
            {
                var a = context.IDENTIFIER();
                result.Append($"{Visit(context.expr()[0])}");

                Type expressionType = types.Get(context.expr()[0]);
                if (expressionType == Type.Int && symbolTable[context.IDENTIFIER().Symbol].Type == Type.Float)
                    result.AppendLine($"itof");

                result.AppendLine($"save {context.IDENTIFIER().GetText()}");
                result.AppendLine($"load {context.IDENTIFIER().GetText()}");

                return result.ToString();
            }

            if (context.op != null)
            {
                var left = types.Get(context.expr()[0]);
                var right = types.Get(context.expr()[1]);

                result.Append(Visit(context.expr()[0]));
                if (left == Type.Int && right == Type.Float)
                    result.AppendLine($"itof");
                result.Append(Visit(context.expr()[1]));
                if (right == Type.Int && left == Type.Float)
                    result.AppendLine($"itof");

                if (context.op.Type == ExprParser.ADD)
                    result.AppendLine($"add");
                if (context.op.Type == ExprParser.SUB)
                    result.AppendLine($"sub");
                if (context.op.Type == ExprParser.MUL)
                    result.AppendLine($"mul");
                if (context.op.Type == ExprParser.DIV)
                    result.AppendLine($"div");
                if (context.op.Type == ExprParser.MOD)
                    result.AppendLine($"mod");
                if (context.op.Type == ExprParser.COMMA)
                    result.AppendLine($"concat");
                if (context.op.Type == ExprParser.AND)
                    result.AppendLine($"and");
                if (context.op.Type == ExprParser.OR)
                    result.AppendLine($"or");
                if (context.op.Type == ExprParser.DOT)
                    result.AppendLine($"concat");
                if (context.op.Type == ExprParser.GT)
                    result.AppendLine($"gt");
                if (context.op.Type == ExprParser.LT)
                    result.AppendLine($"lt");
                if (context.op.Type == ExprParser.EQUAL)
                    result.AppendLine($"eq");
                if (context.op.Type == ExprParser.NOTEQUAL)
                {
                    result.AppendLine($"eq");
                    result.AppendLine($"not");
                }

                return result.ToString();
            }

            return result.ToString();
        }








    }


}