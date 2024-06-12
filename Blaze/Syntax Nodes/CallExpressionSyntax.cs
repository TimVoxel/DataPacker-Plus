﻿using Blaze.SyntaxTokens;

namespace Blaze.Syntax_Nodes
{
    public sealed class CallExpressionSyntax : ExpressionSyntax
    {
        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenParen { get; }
        public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
        public SyntaxToken CloseParen { get; }

        public override SyntaxKind Kind => SyntaxKind.CallExpression;

        internal CallExpressionSyntax(SyntaxTree tree, SyntaxToken identifier, SyntaxToken openParen, SeparatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken closeParen) : base(tree)
        {
            Identifier = identifier;
            OpenParen = openParen;
            Arguments = arguments;
            CloseParen = closeParen;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            yield return OpenParen;
            foreach (SyntaxNode node in Arguments.GetWithSeparators())
                yield return node;
            yield return CloseParen;
        }
    }
}
