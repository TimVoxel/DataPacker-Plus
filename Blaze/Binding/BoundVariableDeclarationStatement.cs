﻿using Blaze.Symbols;

namespace Blaze.Binding
{
    internal sealed class BoundVariableDeclarationStatement : BoundStatement
    {
        public VariableSymbol Variable { get; private set; }
        public BoundExpression Initializer { get; private set; }

        public override BoundNodeKind Kind => BoundNodeKind.VariableDeclarationStatement;

        public BoundVariableDeclarationStatement(VariableSymbol variable, BoundExpression initializer)
        {
            Variable = variable;
            Initializer = initializer;
        }
    }
}