﻿using DPP_Compiler.SyntaxTokens;

namespace DPP_Compiler.Syntax_Nodes
{
    public sealed class ReturnTypeClauseSyntax : SyntaxNode
    {
        public SyntaxToken ColonToken { get; private set; }
        public SyntaxToken Identifier { get; private set; }

        public override SyntaxKind Kind => SyntaxKind.ReturnTypeClause;

        public ReturnTypeClauseSyntax(SyntaxToken colonToken, SyntaxToken identifier)
        {
            ColonToken = colonToken;
            Identifier = identifier;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ColonToken;
            yield return Identifier;
        }
    }
}
