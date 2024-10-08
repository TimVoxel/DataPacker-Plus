﻿using Blaze.Symbols;

namespace Blaze.Binding
{
    internal sealed class BoundConversionExpression : BoundExpression
    {
        public override TypeSymbol Type { get; }
        public BoundExpression Expression { get; private set; }
        public override BoundConstant? ConstantValue { get; }

        public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;

        public BoundConversionExpression(TypeSymbol type, BoundExpression expression)
        {
            Type = type;
            Expression = expression;
            ConstantValue = ConstantFolding.ComputeCast(expression, type);
        }
    }
}