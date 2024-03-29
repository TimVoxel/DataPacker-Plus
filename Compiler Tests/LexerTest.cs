using Blaze.Diagnostics;
using Blaze.SyntaxTokens;
using Blaze.Text;
using System.Collections.Immutable;

namespace Blaze.Tests
{
    public class LexerTest
    {
        [Fact]
        public void Lexer_Lexes_UnterminatedString()
        {
            IEnumerable<SyntaxToken> tokens = SyntaxTree.ParseTokens("\"test", out ImmutableArray<Diagnostic> diagnostics);

            SyntaxToken token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind);
            Assert.Equal("\"test", token.Text);

            Diagnostic diagnostic = Assert.Single(diagnostics);
            Assert.Equal(new TextSpan(0, 1), diagnostic.Location.Span);
            Assert.Equal("Unterminated string literal", diagnostic.Message);
        }

        [Fact]
        public void Lexer_Tests_All_Tokens()
        {
            IEnumerable<SyntaxKind> allTokenKinds = SyntaxFacts.AllTokenKinds
                                                .Where(k => k != SyntaxKind.SingleLineCommentTrivia
                                                         && k != SyntaxKind.MultiLineCommentTrivia);

            IEnumerable<SyntaxKind> testedTokenKinds = GetTokens().Concat(GetSeparators()).Select(t => t.kind);

            SortedSet<SyntaxKind> notTestedTokenKinds = new SortedSet<SyntaxKind>(allTokenKinds);
            notTestedTokenKinds.Remove(SyntaxKind.IncorrectToken);
            notTestedTokenKinds.Remove(SyntaxKind.EndOfFileToken);
            notTestedTokenKinds.ExceptWith(testedTokenKinds);
            
            Assert.Empty(notTestedTokenKinds);
        }

        [Theory]
        [MemberData(nameof(GetTokensData))]
        public void Lexer_Lexes_Token(SyntaxKind kind, string text)
        {
            IEnumerable<SyntaxToken> tokens = SyntaxTree.ParseTokens(text);
            SyntaxToken token = Assert.Single(tokens);
            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }

        [Theory]
        [MemberData(nameof(GetSeparatorData))]
        public void Lexer_Lexes_Separator(SyntaxKind kind, string text)
        {
            IEnumerable<SyntaxToken> tokens = SyntaxTree.ParseTokens(text, true);

            var token = Assert.Single(tokens);
            var trivia = Assert.Single(token.LeadingTrivia);
            Assert.Equal(kind, trivia.Kind);
            Assert.Equal(text, trivia.Text);
        }

        [Theory]
        [MemberData(nameof(GetTokenPairsData))]
        public void Lexer_Lexes_Token_Pairs(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)
        {
            string text = t1Text + t2Text;
            SyntaxToken[] tokens = SyntaxTree.ParseTokens(text).ToArray();
            
            Assert.Equal(2, tokens.Length);
            Assert.Equal(tokens[0].Kind, t1Kind);
            Assert.Equal(tokens[1].Kind, t2Kind);
            Assert.Equal(tokens[0].Text, t1Text);
            Assert.Equal(tokens[1].Text, t2Text);
        }

        [Theory]
        [MemberData(nameof(GetTokenPairsWithSeparatorData))]
        public void Lexer_Lexes_Token_Pairs_With_Separators(SyntaxKind t1Kind, string t1Text, SyntaxKind sepKind, string sepText, SyntaxKind t2Kind, string t2Text)
        {
            var text = t1Text + sepText + t2Text;
            var tokens = SyntaxTree.ParseTokens(text).ToArray();

            Assert.Equal(2, tokens.Length);
            Assert.Equal(tokens[0].Kind, t1Kind);

            var separator = Assert.Single(tokens[0].TrailingTrivia);
            Assert.Equal(sepKind, separator.Kind);
            Assert.Equal(sepText, separator.Text);
            Assert.Equal(tokens[0].Text, t1Text);
            Assert.Equal(tokens[1].Kind, t2Kind);
            Assert.Equal(tokens[1].Text, t2Text);
        }

        public static IEnumerable<object[]> GetTokensData()
        {
            foreach (var token in GetTokens())
                yield return new object[] { token.kind, token.text };
        }

        public static IEnumerable<object[]> GetSeparatorData()
        {
            foreach (var token in GetSeparators())
                yield return new object[] { token.kind, token.text };
        }

        public static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
        {
            var fixedTokens = SyntaxFacts.AllSyntaxKinds.Select(k => (kind: k, text: SyntaxFacts.GetText(k))).Where(t => t.text != null);

            var dynamicTokens = new[]
            {
                (SyntaxKind.IdentifierToken, "a"),
                (SyntaxKind.IdentifierToken, "abc"),
                (SyntaxKind.IntegerLiteralToken, "12"),
                (SyntaxKind.IntegerLiteralToken, "69"),
                (SyntaxKind.IntegerLiteralToken, "420"),
                (SyntaxKind.IntegerLiteralToken, "1000000"),
                (SyntaxKind.StringLiteralToken, "\"Test\""),
                (SyntaxKind.StringLiteralToken, "\"Te\\\"st\""),
            };
#pragma warning disable CS8620
            return fixedTokens.Concat(dynamicTokens);
#pragma warning restore CS8620
        }

        public static IEnumerable<(SyntaxKind kind, string text)> GetSeparators()
        {
            return new[]
            {
                (SyntaxKind.WhitespaceTrivia, " "),
                (SyntaxKind.WhitespaceTrivia, "  "),
                (SyntaxKind.LineBreakTrivia, "\r"),
                (SyntaxKind.LineBreakTrivia, "\n"),
                (SyntaxKind.LineBreakTrivia, "\r\n"),
            };
        }

        private static bool RequiresSeparator(SyntaxKind t1Kind, SyntaxKind t2Kind)
        {
            bool t1IsKeyword = SyntaxFacts.IsKeyword(t1Kind);
            bool t2IsKeyword = SyntaxFacts.IsKeyword(t2Kind);

            if (t1Kind == SyntaxKind.IdentifierToken && t2Kind == SyntaxKind.IdentifierToken)
                return true;

            if (t1IsKeyword && t2Kind == SyntaxKind.IntegerLiteralToken)
                return true;

            if (t2IsKeyword && t1Kind == SyntaxKind.IntegerLiteralToken)
                return true;

            if (t1Kind == SyntaxKind.IdentifierToken && t2Kind == SyntaxKind.IntegerLiteralToken)
                return true;

            if (t1IsKeyword && t2IsKeyword)
                return true;

            if (t1IsKeyword && t2Kind == SyntaxKind.IdentifierToken)
                return true;

            if (t2IsKeyword && t1Kind == SyntaxKind.IdentifierToken)
                return true;

            if (t1Kind == SyntaxKind.IntegerLiteralToken && t2Kind == SyntaxKind.IntegerLiteralToken)
                return true;

            if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsToken || t2Kind == SyntaxKind.DoubleEqualsToken)
                return true;

            if (t1Kind == SyntaxKind.ExclamationSignToken && t2Kind == SyntaxKind.EqualsToken || t2Kind == SyntaxKind.DoubleEqualsToken)
                return true;

            if (t1Kind == SyntaxKind.LessToken && t2Kind == SyntaxKind.EqualsToken)
                return true;

            if (t1Kind == SyntaxKind.GreaterToken && t2Kind == SyntaxKind.EqualsToken)
                return true;

            if (t1Kind == SyntaxKind.SlashToken && t2Kind == SyntaxKind.StarToken)
                return true;

            if (t1Kind == SyntaxKind.SlashToken && t2Kind == SyntaxKind.SlashToken)
                return true;

            if (t1Kind == SyntaxKind.StringLiteralToken && t2Kind == SyntaxKind.StringLiteralToken)
                return true;

            return false;
        }

        public static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)> GetTokenPairs()
        {
            foreach (var t1 in GetTokens())
            {
                foreach (var t2 in GetTokens())
                {
                    if (!RequiresSeparator(t1.kind, t2.kind))
                        yield return (t1.kind, t1.text, t2.kind, t2.text);
                }                
            }   
        }

        public static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind sepKind, string sepText, SyntaxKind t2Kind, string t2Text)> GetTokenPairsWithSeparators()
        {
            foreach (var t1 in GetTokens())
            {
                foreach (var t2 in GetTokens())
                {
                    if (RequiresSeparator(t1.kind, t2.kind))
                    {
                        foreach (var separator in GetSeparators())
                            yield return (t1.kind, t1.text, separator.kind, separator.text, t2.kind, t2.text);
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetTokenPairsData()
        {
            foreach (var t in GetTokenPairs())
                yield return new object[] { t.t1Kind, t.t1Text, t.t2Kind, t.t2Text };
        }

        public static IEnumerable<object[]> GetTokenPairsWithSeparatorData()
        {
            foreach (var t in GetTokenPairsWithSeparators())
                yield return new object[] { t.t1Kind, t.t1Text, t.sepKind, t.sepText, t.t2Kind, t.t2Text };
        }
    }
}