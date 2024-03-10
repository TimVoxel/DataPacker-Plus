﻿namespace Blaze
{
    public enum SyntaxKind
    {
        //Tokens
        IncorrectToken,
        EndOfFileToken,
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
        BreakKeyword,
        ContinueKeyword,
        FunctionKeyword,
        ReturnKeyword,

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
        BreakStatement,
        ContinueStatement,
        ReturnStatement,

        //Trivia 
        WhitespaceTrivia,
        LineBreakTrivia,
        SingleLineCommentTrivia,
        MultiLineCommentTrivia,
        SkippedTextTrivia,
    }
}
