using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace ANTLR
{
    class TypeChecking : ExprBaseListener
    {
        private SymbolTable symbolTable;

        public TypeChecking(SymbolTable symbolTable)
        {
            this.symbolTable = symbolTable;
        }

        public ParseTreeProperty<Type> Types { get; } = new ParseTreeProperty<Type>();


        public override void ExitPrimary([NotNull] ExprParser.PrimaryContext context)
        {
            if (context.expr() != null)
            {
                Types.Put(context, Types.Get(context.expr()));
            }
            else if (context.BOOL_LITERAL() != null)
            {
                Types.Put(context, Type.Bool);
            }
            else if (context.DECIMAL_LITERAL() != null)
            {
                Types.Put(context, Type.Int);
            }
            else if (context.FLOAT_LITERAL() != null)
            {
                Types.Put(context, Type.Float);
            }
            else if (context.STRING_LITERAL() != null)
            {
                Types.Put(context, Type.String);
            }
            else if (context.IDENTIFIER() != null)
            {
                Types.Put(context, symbolTable[context.IDENTIFIER().Symbol].Type);
            }
            else
            {
                Errors.ReportError(context.Start, $"Unexpected symbol: {context.GetText()} in expression.");
                Types.Put(context, Type.Error);
            }
        }

        public override void ExitPrimitiveType([NotNull] ExprParser.PrimitiveTypeContext context)
        {
            var text = context.GetText();
            var result = Type.Error;
            foreach (var type in Enum.GetValues<Type>())
            {
                if (type.ToString().ToLower() == text)
                    result = type;
            }
            Types.Put(context, result);
        }

        public override void ExitDeclaration([NotNull] ExprParser.DeclarationContext context)
        {
            var type = Types.Get(context.primitiveType());
            foreach (var identifier in context.IDENTIFIER())
                symbolTable.Add(identifier.Symbol, type);
        }

        public override void ExitIf([NotNull] ExprParser.IfContext context)
        {
            var type = Types.Get(context.expr());
            if (type != Type.Bool && type != Type.Error)
                Errors.ReportError(context.expr().Start, "Type of the expression inside IF should be bool.");
        }

        public override void ExitWhile([NotNull] ExprParser.WhileContext context)
        {
            var type = Types.Get(context.expr());
            if (type != Type.Bool && type != Type.Error)
                Errors.ReportError(context.expr().Start, "Type of the expression inside WHILE should be bool.");
        }

        public override void ExitExpr([NotNull] ExprParser.ExprContext context)
        {
            if (context.primary() != null)
            {
                Types.Put(context, Types.Get(context.primary()));
            }
            else if (context.prefix != null)
            {
                var type = Types.Get(context.expr()[0]);
                if (context.prefix.Type == ExprParser.SUB)
                {
                    if (type == Type.Int || type == Type.Float)
                        Types.Put(context, type);
                    else
                    {
                        Types.Put(context, Type.Error);
                        Errors.ReportError(context.Start, "Unary minus must be used with int or float.");
                    }
                }
                else if (type == Type.Bool && context.prefix.Type == ExprParser.BANG)
                {
                    Types.Put(context, type);
                    return;
                }
                else
                {
                    Types.Put(context, Type.Error);
                    Errors.ReportError(context.Start, "Operator ! must be used with bool.");
                }
            }
            else if (context.op != null)
            {
                var left = Types.Get(context.expr()[0]);
                var right = Types.Get(context.expr()[1]);

                if (left == Type.Error || right == Type.Error)
                {
                    Types.Put(context, Type.Error);
                    return;
                }

                var binaryResult = CompareType(left, right);

                if (left != Type.Error && right != Type.Error && binaryResult == Type.Error)
                {
                    Errors.ReportError(context.op, $"Operation {context.op.Text} can not be used with {left} and {right}.");
                    Types.Put(context, Type.Error);
                    return;
                }

                if (context.op.Type == ExprParser.LT || context.op.Type == ExprParser.GT)
                {
                    if (binaryResult == Type.Bool || binaryResult == Type.String)
                    {
                        Errors.ReportError(context.op, $"Operation {context.op.Text} can not be used with {left} and {right}.");
                        Types.Put(context, Type.Error);
                        return;
                    }

                    Types.Put(context, Type.Bool);
                    return;
                }
                if (context.op.Type == ExprParser.EQUAL || context.op.Type == ExprParser.NOTEQUAL)
                {
                    if (binaryResult == Type.Bool)
                    {
                        Errors.ReportError(context.op, $"Operation {context.op.Text} can not be used with {left} and {right}.");
                        Types.Put(context, Type.Error);
                        return;
                    }

                    Types.Put(context, Type.Bool);
                    return;
                }
                if ((context.op.Type == ExprParser.AND || context.op.Type == ExprParser.OR) && binaryResult != Type.Bool) //
                {
                    Errors.ReportError(context.op, $"Operation {context.op.Text} need bool values.");
                    Types.Put(context, Type.Error);
                    return;
                }
                //String concat
                if (context.op.Type == ExprParser.DOT && binaryResult != Type.String)
                {
                    Errors.ReportError(context.op, $"Operation {context.op.Text} need string values.");
                    Types.Put(context, Type.Error);
                    return;
                }
                //Mod with values
                if (context.op.Type == ExprParser.MOD && binaryResult != Type.Int)
                {
                    Errors.ReportError(context.op, $"Operation {context.op.Text} need int values.");
                    Types.Put(context, Type.Error);
                    return;
                }

                //Math with strings
                if ((context.op.Type == ExprParser.ADD || context.op.Type == ExprParser.SUB ||
                    context.op.Type == ExprParser.MUL || context.op.Type == ExprParser.DIV)
                    && binaryResult != Type.Int && binaryResult != Type.Float)
                {
                    Errors.ReportError(context.op, $"Operation {context.op.Text} can not be used with {left} and {right}.");
                    Types.Put(context, Type.Error);
                    return;
                }
                Types.Put(context, binaryResult);
            }
            else if (context.IDENTIFIER() != null)
            {
                var variable = symbolTable[context.IDENTIFIER().Symbol];
                var rightSide = Types.Get(context.expr()[0]);
                var result = CompareType(variable.Type, rightSide);
                if (result == variable.Type)
                    Types.Put(context, variable.Type);
                else
                {
                    Errors.ReportError(context.Start, $"The {variable.Type} of variable {context.IDENTIFIER().GetText()} can not hold value of type: {result}.");
                    Types.Put(context, Type.Error);
                }
            }
        }


        private Type CompareType(Type t1, Type t2)
        {
            if (t1 == Type.Int && t2 == Type.Float || t1 == Type.Float && t2 == Type.Int)
            {
                return Type.Float;
            }
            if(t1 == Type.Int && t2 == Type.Bool)
            {
                return Type.Bool;
            }
            if (t1 == Type.Int && t2 == Type.String)
            {
                return Type.String;
            }

            if(t1 == Type.Float && t2 == Type.Bool)
            {
                return Type.Bool;
            }
            if (t1 == Type.Float && t2 == Type.String)
            {
                return Type.String;
            }

            if (t1 == Type.Bool && t2 == Type.Int)
            {
                return Type.Int;
            }
            if (t1 == Type.Bool && t2 == Type.Float)
            {
                return Type.Float;
            }
            if (t1 == Type.Bool && t2 == Type.String)
            {
                return Type.String;
            }

            if (t1 == Type.String && t2 == Type.Int)
            {
                return Type.Int;
            }
            if (t1 == Type.String && t2 == Type.Float)
            {
                return Type.Float;
            }
            if (t1 == Type.String && t2 == Type.Bool)
            {
                return Type.Bool;
            }


            if (t1 == t2)
            {
                return t1;
            }

            if (t1 == Type.Error || t2 == Type.Error)
            {
                return Type.Error;
            }

            return Type.Error;
        }
    }
}
