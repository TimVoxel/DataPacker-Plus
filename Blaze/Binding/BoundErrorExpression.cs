﻿using Blaze.Symbols;

namespace Blaze.Binding
{
    internal sealed class BoundErrorExpression : BoundExpression
    {
        public override TypeSymbol Type => TypeSymbol.Error;
        public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
    }
}