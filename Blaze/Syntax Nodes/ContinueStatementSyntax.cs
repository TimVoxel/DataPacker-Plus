﻿using Blaze.SyntaxTokens;

namespace Blaze.Syntax_Nodes
{
    public sealed class ContinueStatementSyntax : StatementSyntax
    {
        public SyntaxToken Keyword { get; private set; }
        public SyntaxToken Semicolon { get; private set; }

        public override SyntaxKind Kind => SyntaxKind.ContinueStatement;

        internal ContinueStatementSyntax(SyntaxTree tree, SyntaxToken keyword, SyntaxToken semicolon) : base(tree)
        {
            Keyword = keyword;
            Semicolon = semicolon;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
            yield return Semicolon;
        }
    }
}
