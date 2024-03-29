﻿namespace Blaze.Syntax_Nodes
{
    public sealed class GlobalStatementSyntax : MemberSyntax
    {
        public StatementSyntax Statement { get; private set; }

        public override SyntaxKind Kind => SyntaxKind.GlobalStatement;

        internal GlobalStatementSyntax(SyntaxTree tree, StatementSyntax statement) : base(tree)
        {
            Statement = statement;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Statement;
        }
    }
}
