﻿using System.Collections;
using Blaze.Symbols;
using Blaze.Text;

namespace Blaze.Diagnostics
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private List<Diagnostic> _diagnostics = new List<Diagnostic>();

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddRange(IEnumerable<Diagnostic> diagnostics) => _diagnostics.AddRange(diagnostics);

        private void Report(TextLocation location, string message)
        {
            Diagnostic diagnostic = new Diagnostic(location, message);
            _diagnostics.Add(diagnostic);
        }

        public void ReportStrayCharacter(TextLocation location, char character)
        {
            string message = $"Stray \'{character}\' in input";
            Report(location, message);
        }

        public void ReportInvalidNumber(TextLocation location, string text, TypeSymbol type)
        {
            string message = $"The number \"{text}\" can not be represented by <{type}>";
            Report(location, message);
        }

        public void ReportUnterminatedString(TextLocation location)
        {
            string message = $"Unterminated string literal";
            Report(location, message);
        }

        public void ReportUnexpectedToken(TextLocation location, SyntaxKind kind, SyntaxKind expectedKind)
        {
            string message = $"Unexpected token <{kind}>, expected <{expectedKind}>";
            Report(location, message);
        }

        public void ReportUndefinedUnaryOperator(TextLocation location, string operatorText, TypeSymbol operandType)
        {
            string message = $"Unary operator '{operatorText}' is not defined for type {operandType}";
            Report(location, message);
        }

        public void ReportUndefinedBinaryOperator(TextLocation location, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
        {
            string message = $"Binary operator '{operatorText}' is not defined for types {leftType} and {rightType}";
            Report(location, message);
        }

        public void ReportUndefinedName(TextLocation location, string name)
        {
            string message = $"Variable \"{name}\" doesn't exist";
            Report(location, message);
        }

        public void ReportCannotConvert(TextLocation location, TypeSymbol from, TypeSymbol to)
        {
            string message = $"Can not convert type {from} to type {to}";
            Report(location, message);
        }

        public void ReportCannotConvertImplicitly(TextLocation location, TypeSymbol from, TypeSymbol to)
        {
            string message = $"Can not implicitly convert type {from} to type {to}. An explicit conversion exists (are you missing a cast?)";
            Report(location, message);
        }

        public void ReportVariableAlreadyDeclared(TextLocation location, string name)
        {
            string message = $"Variable \"{name}\" is already declared";
            Report(location, message);
        }

        public void ReportUndefinedFunction(TextLocation location, string text)
        {
            string message = $"Function \"{text}\" doesn't exist";
            Report(location, message);
        }

        public void ReportUndefinedType(TextLocation location, string text)
        {
            string message = $"Type {text} doesn't exist";
            Report(location, message);
        }

        public void ReportWrongArgumentCount(TextLocation location, string name, int expectedCount, int actualCount)
        {
            string message = $"Function {name} requires {expectedCount} arguments, but {actualCount} were given";
            Report(location, message);
        }

        public void ReportWrongArgumentType(TextLocation location, string name, string parameterName, TypeSymbol expectedType, TypeSymbol actualType)
        {
            string message = $"Parameter \"{parameterName}\" of function \"{name}\" is of type {expectedType}, but was given a value of type {actualType}";
            Report(location, message);
        }


        public void ReportExpressionMustHaveValue(TextLocation location)
        {
            string message = $"Expression must have a value";
            Report(location, message);
        }

        public void ReportParameterAlreadyDeclared(TextLocation location, string name)
        {
            string message = $"Parameter \"{name}\" is already declared";
            Report(location, message);
        }

        public void ReportFunctionAlreadyDeclared(TextLocation location, string name)
        {
            string message = $"Function \"{name}\" is already declared ";
            Report(location, message);
        }

        public void ReportInvalidBreakOrContinue(TextLocation location, string text)
        {
            string message = $"No enclosing loop of which to {text}";
            Report(location, message);
        }

        internal void ReportReturnOutsideFunction(TextLocation location)
        {
            string message = "Return statements can't be used outside of functions";
            Report(location, message);
        }

        public void ReportInvalidReturnExpression(TextLocation location, string functionName)
        {
            string message = $"Function \"{functionName}\" does not return a value, return can not be followed by an expression";
            Report(location, message);
        }

        public void ReportMissingReturnExpression(TextLocation location, string functionName, TypeSymbol typeSymbol)
        {
            string message = $"Function \"{functionName}\" must return a value of type {typeSymbol}, but no expression was given after return";
            Report(location, message);
        }

        public void ReportAllPathsMustReturn(TextLocation location)
        {
            string message = $"Not all code paths return a value";
            Report(location, message);
        }
    }
}
