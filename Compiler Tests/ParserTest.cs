﻿using DPP_Compiler.Syntax_Nodes;
using System.Runtime.Serialization;

namespace DPP_Compiler.Tests
{
    public class ParserTest
    {
        [Theory]
        [MemberData(nameof(GetBinaryOperatorPairsData))]
        public void Parser_Binary_Expression_HonorsPrecedences(SyntaxKind operator1, SyntaxKind operator2)
        {
            int operator1Precedence = SyntaxFacts.GetBinaryOperatorPrecedence(operator1);
            int operator2Precedence = SyntaxFacts.GetBinaryOperatorPrecedence(operator2);
            string? operator1Text = SyntaxFacts.GetText(operator1);
            string? operator2Text = SyntaxFacts.GetText(operator2);
            if (operator1Text == null || operator2Text == null) 
                return;

            string text = $"a {operator1Text} b {operator2Text} c";
            ExpressionSyntax expression = SyntaxTree.Parse(text).Root;

            if (operator1Precedence >= operator2Precedence)
            {
                using (AssertingEnumerator enumerator = new AssertingEnumerator(expression))
                {
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);
                    enumerator.AssertNode(SyntaxKind.IdentifierExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                    enumerator.AssertToken(operator1, operator1Text);
                    enumerator.AssertNode(SyntaxKind.IdentifierExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
                    enumerator.AssertToken(operator2, operator2Text);
                    enumerator.AssertNode(SyntaxKind.IdentifierExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "c");
                }
            }
            else
            {
                using (AssertingEnumerator enumerator = new AssertingEnumerator(expression))
                {
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);
                    enumerator.AssertNode(SyntaxKind.IdentifierExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                    enumerator.AssertToken(operator1, operator1Text);
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);
                    enumerator.AssertNode(SyntaxKind.IdentifierExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
                    enumerator.AssertToken(operator2, operator2Text);
                    enumerator.AssertNode(SyntaxKind.IdentifierExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "c");
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetUnaryOperatorPairsData))]
        public void Parser_Unary_Expression_HonorsPrecedences(SyntaxKind unaryKind, SyntaxKind binaryKind)
        {
            int unaryPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(unaryKind);
            int binaryPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(binaryKind);
            string? unaryText = SyntaxFacts.GetText(unaryKind);
            string? binaryText = SyntaxFacts.GetText(binaryKind);
            if (unaryText == null || binaryText == null)
                return;

            string text = $"{unaryText} a {binaryText} b";
            ExpressionSyntax expression = SyntaxTree.Parse(text).Root;

            if (unaryPrecedence >= binaryPrecedence)
            {
                using (AssertingEnumerator enumerator = new AssertingEnumerator(expression))
                {
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);
                    enumerator.AssertNode(SyntaxKind.UnaryExpression);
                    enumerator.AssertToken(unaryKind, unaryText);
                    enumerator.AssertNode(SyntaxKind.IdentifierExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                    enumerator.AssertToken(binaryKind, binaryText);
                    enumerator.AssertNode(SyntaxKind.IdentifierExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
                }
            }
            else
            {
                using (AssertingEnumerator enumerator = new AssertingEnumerator(expression))
                {
                    enumerator.AssertNode(SyntaxKind.UnaryExpression);
                    enumerator.AssertToken(unaryKind, unaryText);
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);
                    enumerator.AssertNode(SyntaxKind.IdentifierExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                    enumerator.AssertToken(binaryKind, binaryText);
                    enumerator.AssertNode(SyntaxKind.IdentifierExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
                }
            }
        }

        public static IEnumerable<object[]> GetBinaryOperatorPairsData()
        {
            foreach (SyntaxKind operator1 in SyntaxFacts.GetBinaryOperators())
                foreach (SyntaxKind operator2 in SyntaxFacts.GetBinaryOperators())
                    yield return new object[] { operator1, operator2 };
        }

        public static IEnumerable<object[]> GetUnaryOperatorPairsData()
        {
            foreach (SyntaxKind unary in SyntaxFacts.GetUnaryOperators())
                foreach (SyntaxKind binary in SyntaxFacts.GetBinaryOperators())
                    yield return new object[] { unary, binary };
        }

    }
}