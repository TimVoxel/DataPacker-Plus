﻿using DPP_Compiler.SyntaxTokens;

namespace DPP_Compiler.Syntax_Nodes
{
    public sealed class ParameterSyntax : SyntaxNode
    {
        public TypeClauseSyntax Type { get; private set; }
        public SyntaxToken Identifier { get; private set; }

        public override SyntaxKind Kind => SyntaxKind.Parameter;

        public ParameterSyntax(TypeClauseSyntax type, SyntaxToken identifier)
        {
            Type = type;
            Identifier = identifier;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Type;
            yield return Identifier;
        }
    }
}