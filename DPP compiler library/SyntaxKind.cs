﻿namespace DPP_Compiler
{
    public enum SyntaxKind
    {
        //Tokens
        IncorrectToken,
        EndOfFileToken,
        WhiteSpaceToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        OpenParenToken,
        CloseParenToken,
        ExclamationSignToken,
        DoubleAmpersandToken,
        DoublePipeToken,
        DoubleEqualsToken,
        NotEqualsToken,
        EqualsToken,
        IdentifierToken,

        //Literals
        IntegerLiteralToken,

        //Keywords
        FalseKeyword,
        TrueKeyword,
        
        //Expressions
        LiteralExpression,
        IdentifierExpression,
        BinaryExpression,
        ParenthesizedExpression,
        UnaryExpression,
        AssignmentExpression,
    }
}