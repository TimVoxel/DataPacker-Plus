﻿using Blaze.SyntaxTokens;

namespace Blaze
{
    public static class SyntaxFacts
    {
        public static SyntaxKind[] AllSyntaxKinds => (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));
        public static SyntaxKind[] AllTokenKinds => AllSyntaxKinds.Where(k => IsToken(k)).ToArray();
            
        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 6;
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 5;
                case SyntaxKind.LessToken:
                case SyntaxKind.LessOrEqualsToken:
                case SyntaxKind.GreaterToken:
                case SyntaxKind.GreaterOrEqualsToken:
                    return 4;
                case SyntaxKind.DoubleEqualsToken:
                case SyntaxKind.NotEqualsToken:
                    return 3;
                case SyntaxKind.DoubleAmpersandToken:
                    return 2;
                case SyntaxKind.DoublePipeToken:
                    return 1;

                default: return 0;
            }
        }

        public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind) 
            {
                case SyntaxKind.ExclamationSignToken:
                    return 8;
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 7;

                default: return 0;
            }
        }

        public static bool IsKeyword(SyntaxKind token)
        {
            return token.ToString().EndsWith("Keyword");
        }

        public static bool IsToken(SyntaxKind kind)
        {
            return !IsTrivia(kind) && (IsKeyword(kind) || kind.ToString().EndsWith("Token"));
        }

        public static bool IsTrivia(SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.WhitespaceTrivia         => true,
                SyntaxKind.SingleLineCommentTrivia  => true,
                SyntaxKind.MultiLineCommentTrivia   => true,
                SyntaxKind.SkippedTextTrivia        => true,
                SyntaxKind.LineBreakTrivia          => true,
                _                                   => false
            };
        }

        public static bool IsComment(SyntaxKind kind) 
            => kind == SyntaxKind.SingleLineCommentTrivia || kind == SyntaxKind.MultiLineCommentTrivia;

        public static SyntaxKind GetKeywordKind(string text)
        {
            return text switch
            {
                "true"      => SyntaxKind.TrueKeyword,
                "false"     => SyntaxKind.FalseKeyword,
                "let"       => SyntaxKind.LetKeyword,
                "if"        => SyntaxKind.IfKeyword,
                "else"      => SyntaxKind.ElseKeyword,
                "while"     => SyntaxKind.WhileKeyword,
                "do"        => SyntaxKind.DoKeyword,
                "for"       => SyntaxKind.ForKeyword,
                "break"     => SyntaxKind.BreakKeyword,
                "continue"  => SyntaxKind.ContinueKeyword,
                "function"  => SyntaxKind.FunctionKeyword,
                "return"    => SyntaxKind.ReturnKeyword,
                _           => SyntaxKind.IdentifierToken,
            };
        }

        public static IEnumerable<SyntaxKind> GetBinaryOperators()
        {
            foreach (SyntaxKind kind in AllSyntaxKinds)
                if (GetBinaryOperatorPrecedence(kind) > 0)
                    yield return kind;
        }

        public static IEnumerable<SyntaxKind> GetUnaryOperators()
        {
            foreach (SyntaxKind kind in AllSyntaxKinds)
                if (GetUnaryOperatorPrecedence(kind) > 0)
                    yield return kind;
        }

        public static string? GetText(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.FalseKeyword:
                    return "false";
                case SyntaxKind.TrueKeyword:
                    return "true";
                case SyntaxKind.LetKeyword:
                    return "let";
                case SyntaxKind.IfKeyword:
                    return "if";
                case SyntaxKind.ElseKeyword:
                    return "else";
                case SyntaxKind.WhileKeyword:
                    return "while";
                case SyntaxKind.DoKeyword:
                    return "do";
                case SyntaxKind.ForKeyword:
                    return "for";
                case SyntaxKind.BreakKeyword:
                    return "break";
                case SyntaxKind.ContinueKeyword:
                    return "continue";
                case SyntaxKind.FunctionKeyword:
                    return "function";
                case SyntaxKind.ReturnKeyword:
                    return "return";
                case SyntaxKind.SemicolonToken:
                    return ";";
                case SyntaxKind.ColonToken:
                    return ":";
                case SyntaxKind.CommaToken:
                    return ",";
                case SyntaxKind.OpenBraceToken:
                    return "{";
                case SyntaxKind.CloseBraceToken:
                    return "}";
                case SyntaxKind.PlusToken:
                    return "+";
                case SyntaxKind.MinusToken:
                    return "-";
                case SyntaxKind.StarToken:
                    return "*";
                case SyntaxKind.SlashToken:
                    return "/";
                case SyntaxKind.OpenParenToken:
                    return "(";
                case SyntaxKind.CloseParenToken:
                    return ")";
                case SyntaxKind.ExclamationSignToken:
                    return "!";
                case SyntaxKind.EqualsToken:
                    return "=";
                case SyntaxKind.DoubleEqualsToken:
                    return "==";
                case SyntaxKind.LessToken:
                    return "<";
                case SyntaxKind.LessOrEqualsToken:
                    return "<=";
                case SyntaxKind.GreaterToken:
                    return ">";
                case SyntaxKind.GreaterOrEqualsToken:
                    return ">=";
                case SyntaxKind.DoubleAmpersandToken:
                    return "&&";
                case SyntaxKind.DoublePipeToken:
                    return "||";
                case SyntaxKind.DoubleDotToken:
                    return "..";
                case SyntaxKind.NotEqualsToken:
                    return "!=";
                default:
                    return null;
            }
        }
    }
}
