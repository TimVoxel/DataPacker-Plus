﻿namespace Compiler_snapshot.Binding
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public BoundExpression Left { get; private set; }
        public BoundBinaryOperator Operator { get; private set; }
        public BoundExpression Right { get; private set; }

        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
        public override Type Type => Operator.ResultType;

        public BoundBinaryExpression(BoundExpression left, BoundBinaryOperator op, BoundExpression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }
}