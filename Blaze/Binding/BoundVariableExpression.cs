﻿using Blaze.Symbols;

namespace Blaze.Binding
{
    internal sealed class BoundVariableExpression : BoundExpression
    {
        public VariableSymbol Variable { get; private set; }

        public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
        public override TypeSymbol Type => Variable.Type;
        public override BoundConstant? ConstantValue => Variable.Constant;

        public BoundVariableExpression(VariableSymbol symbol)
        {
            Variable = symbol;
        }
    }
}