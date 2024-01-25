﻿namespace DPP_Compiler
{
    public enum SyntaxKind
    {
        //Tokens
        IncorrectToken,
        EndOfFileToken,
        WhitespaceToken,
        SemicolonToken,
        ColonToken,
        CommaToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        OpenParenToken,
        CloseParenToken,
        OpenBraceToken,
        CloseBraceToken,
        ExclamationSignToken,
        DoubleAmpersandToken,
        DoublePipeToken,
        DoubleEqualsToken,
        DoubleDotToken,
        NotEqualsToken,
        EqualsToken,
        GreaterToken,
        GreaterOrEqualsToken,
        LessToken,
        LessOrEqualsToken,
        IdentifierToken,

        //Literals
        IntegerLiteralToken,
        StringLiteralToken,

        //Keywords
        FalseKeyword,
        TrueKeyword,
        LetKeyword,
        IfKeyword,
        ElseKeyword,
        WhileKeyword,
        DoKeyword,
        ForKeyword,
        FunctionKeyword,

        //Nodes
        CompilationUnit,
        TypeClause,
        ReturnTypeClause,
        ElseClause,
        GlobalStatement,
        FunctionDeclaration,
        Parameter,

        //Expressions
        LiteralExpression,
        IdentifierExpression,
        BinaryExpression,
        ParenthesizedExpression,
        UnaryExpression,
        AssignmentExpression,
        CallExpression,

        //Statements
        BlockStatement,
        ExpressionStatement,
        VariableDeclarationStatement,
        IfStatement,
        WhileStatement,
        DoWhileStatement,
        ForStatement,
    }
}
