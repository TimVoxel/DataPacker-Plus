﻿using Blaze.Diagnostics;
using Blaze.Syntax_Nodes;
using Blaze.SyntaxTokens;
using Blaze.Text;
using Mono.Cecil.Cil;
using System.Collections.Immutable;

namespace Blaze
{
    internal class Parser
    {
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly SyntaxTree _syntaxTree;

        private int _position;
        
        private SyntaxToken Current => Peek(0);
        private SyntaxToken Next => Peek(1);
        public DiagnosticBag Diagnostics => _diagnostics;

        public Parser(SyntaxTree syntaxTree)
        {
            _syntaxTree = syntaxTree;

            var tokens = new List<SyntaxToken>();
            var incorrectTokens = new List<SyntaxToken>();

            var lexer = new Lexer(syntaxTree);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();

                if (token.Kind == SyntaxKind.IncorrectToken)
                    incorrectTokens.Add(token); 
                else
                {
                    if (incorrectTokens.Any())
                    {
                        var leadingTrivia = token.LeadingTrivia.ToBuilder();
                        var index = 0;

                        foreach (var incorrectToken in incorrectTokens) 
                        {
                            foreach (var lt in incorrectToken.LeadingTrivia)
                                leadingTrivia.Insert(index++, lt);
                                
                            var trivia = new Trivia(syntaxTree, SyntaxKind.SkippedTextTrivia, incorrectToken.Position, incorrectToken.Text);
                            leadingTrivia.Insert(index++, trivia);

                            foreach (var tt in incorrectToken.TrailingTrivia)
                                leadingTrivia.Insert(index++, tt);    
                        }

                        incorrectTokens.Clear();
                        token = new SyntaxToken(token.Tree, token.Kind, token.Position, token.Text, token.Value, leadingTrivia.ToImmutable(), token.TrailingTrivia);
                    }
                    tokens.Add(token);
                }                    
            }
            while (token.Kind != SyntaxKind.EndOfFileToken);

            _tokens = tokens.ToImmutableArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }

        private SyntaxToken TryConsume(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return Consume();

            TextLocation location = new TextLocation(_syntaxTree.Text, Current.Span);
            _diagnostics.ReportUnexpectedToken(location, Current.Kind, kind);
            return new SyntaxToken(_syntaxTree, kind, Current.Position, null, null, ImmutableArray<Trivia>.Empty, ImmutableArray<Trivia>.Empty);
        }

        private SyntaxToken Consume()
        {
            SyntaxToken current = Current;
            _position++;
            return current;
        }

        private SyntaxToken Peek(int offset)
        {
            int index = _position + offset;
            if (index >= _tokens.Length)
                return _tokens[_tokens.Length - 1];
            return _tokens[index];
        }

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var usings = ParseUsings();
            var namespaces = ParseNamespaces();
            var endOfFileToken = TryConsume(SyntaxKind.EndOfFileToken);
            return new CompilationUnitSyntax(_syntaxTree, usings, namespaces, endOfFileToken);
        }
        
        private ImmutableArray<UsingNamespaceSyntax> ParseUsings()
        {
            var usings = ImmutableArray.CreateBuilder<UsingNamespaceSyntax>();

            /*
            while (Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var startToken = Current;
                members.Add(ParseNamespace());

                if (Current == startToken)
                    Consume();
            }
            */

            return usings.ToImmutable();
        }

        private ImmutableArray<NamespaceDeclarationSyntax> ParseNamespaces()
        {
            var members = ImmutableArray.CreateBuilder<NamespaceDeclarationSyntax>();

            while (Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var startToken = Current;
                members.Add(ParseNamespace());

                if (Current == startToken)
                    Consume();
            }

            return members.ToImmutable();
        }

        private NamespaceDeclarationSyntax ParseNamespace()
        {
            var namespaceKeyword = TryConsume(SyntaxKind.NamespaceKeyword);
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            var done = false;

            while (!done && Current.Kind != SyntaxKind.OpenBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var identifier = TryConsume(SyntaxKind.IdentifierToken);
                nodesAndSeparators.Add(identifier);

                if (Current.Kind == SyntaxKind.DotToken)
                {
                    var dot = TryConsume(SyntaxKind.DotToken);
                    nodesAndSeparators.Add(dot);
                }
                else
                    done = true;
            }
            var identifierPath = new SeparatedSyntaxList<SyntaxToken>(nodesAndSeparators.ToImmutable());

            var openBraceToken = TryConsume(SyntaxKind.OpenBraceToken);
            var members = ParseMembers();
            var closeBraceToken = TryConsume(SyntaxKind.CloseBraceToken);

            return new NamespaceDeclarationSyntax(_syntaxTree, namespaceKeyword, identifierPath, openBraceToken, members, closeBraceToken);
        }

        private ImmutableArray<MemberSyntax> ParseMembers()
        {
            var members = ImmutableArray.CreateBuilder<MemberSyntax>();

            while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var startToken = Current;
                members.Add(ParseMember());

                if (Current == startToken)
                    Consume();
            }

            return members.ToImmutable();
        }

        private MemberSyntax ParseMember()
        {
            if (SyntaxFacts.IsFunctionModifier(Current.Kind) || Current.Kind == SyntaxKind.FunctionKeyword)
                return ParseFunctionDeclaration();
            else
                return ParseGlobalStatement();
        }

        private MemberSyntax ParseFunctionDeclaration()
        {
            var modifiers = ImmutableArray.CreateBuilder<SyntaxToken>();
            var modifierKinds = new HashSet<SyntaxKind>();

            while (SyntaxFacts.IsFunctionModifier(Current.Kind))
            {
                if (modifierKinds.Contains(Current.Kind))
                {
                    var location = new TextLocation(_syntaxTree.Text, Current.Span);
                    _diagnostics.ReportDuplicateFunctionModifier(location);
                }
                else
                {
                    var current = Consume();
                    modifiers.Add(current);
                    modifierKinds.Add(current.Kind);
                }
            }

            var functionKeyword = TryConsume(SyntaxKind.FunctionKeyword);
            var identifier = TryConsume(SyntaxKind.IdentifierToken);

            var openParen = TryConsume(SyntaxKind.OpenParenToken);
            var parameters = ParseParameters();
            var closeParen = TryConsume(SyntaxKind.CloseParenToken);
            
            ReturnTypeClauseSyntax? returnTypeClause = null;
            if (Current.Kind == SyntaxKind.ColonToken)
                returnTypeClause = ParseReturnTypeClause();

            var body = ParseBlockStatement();
            return new FunctionDeclarationSyntax(_syntaxTree, modifiers.ToImmutable(), functionKeyword, identifier, openParen, parameters, closeParen, returnTypeClause, body);
        }

        private MemberSyntax ParseGlobalStatement()
        {
            StatementSyntax statement = ParseStatement();
            return new GlobalStatementSyntax(_syntaxTree, statement);
        }

        private StatementSyntax ParseStatement()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenBraceToken:
                    return ParseBlockStatement();
                case SyntaxKind.LetKeyword:
                    return ParseVariableDeclarationStatement();
                case SyntaxKind.IfKeyword:
                    return ParseIfStatement();
                case SyntaxKind.WhileKeyword:
                    return ParseWhileStatement();
                case SyntaxKind.DoKeyword:
                    return ParseDoWhileStatement();
                case SyntaxKind.ForKeyword:
                    return ParseForStatement();
                case SyntaxKind.BreakKeyword:
                    return ParseBreakStatement();
                case SyntaxKind.ContinueKeyword:
                    return ParseContinueStatement();
                case SyntaxKind.ReturnKeyword:
                    return ParseReturnStatement();
                case SyntaxKind.OpenParenToken:
                    return ParseExpressionStatement();
                default:
                    {
                        if (Next.Kind == SyntaxKind.IdentifierToken)
                            return ParseVariableDeclarationStatement();
                        return ParseExpressionStatement();
                    } 
            }
        }

        private StatementSyntax ParseReturnStatement()
        {
            var returnKeyword = TryConsume(SyntaxKind.ReturnKeyword);
            ExpressionSyntax? expression = null;
            if (Current.Kind != SyntaxKind.SemicolonToken)
                expression = ParseExpression();

            var semicolon = TryConsume(SyntaxKind.SemicolonToken);
            return new ReturnStatementSyntax(_syntaxTree, returnKeyword, expression, semicolon);
        }

        private StatementSyntax ParseIfStatement()
        {
            var keyword = TryConsume(SyntaxKind.IfKeyword);
            var openParen = TryConsume(SyntaxKind.OpenParenToken);
            var expression = ParseExpression();
            var closeParen = TryConsume(SyntaxKind.CloseParenToken);
            var body = ParseStatement();

            if (Current.Kind == SyntaxKind.ElseKeyword)
            {
                var elseClause = ParseElseClause();
                return new IfStatementSyntax(_syntaxTree, keyword, openParen, expression, closeParen, body, elseClause);
            }
            return new IfStatementSyntax(_syntaxTree, keyword, openParen, expression, closeParen, body);
        }

        private ElseClauseSyntax ParseElseClause()
        {
            var keyword = TryConsume(SyntaxKind.ElseKeyword);
            var statement = ParseStatement();
            return new ElseClauseSyntax(_syntaxTree, keyword, statement);
        }

        private WhileStatementSyntax ParseWhileStatement()
        {
            var keyword = TryConsume(SyntaxKind.WhileKeyword);
            var openParen = TryConsume(SyntaxKind.OpenParenToken);
            var condition = ParseExpression();
            var closeParen = TryConsume(SyntaxKind.CloseParenToken);
            var body = ParseStatement();
            return new WhileStatementSyntax(_syntaxTree,  keyword, openParen, condition, closeParen, body);
        }

        private DoWhileStatementSyntax ParseDoWhileStatement()
        {
            var doKeyword = TryConsume(SyntaxKind.DoKeyword);
            var body = ParseStatement();
            var whileKeyword = TryConsume(SyntaxKind.WhileKeyword);
            var openParen = TryConsume(SyntaxKind.OpenParenToken);
            var condition = ParseExpression();
            var closeParen = TryConsume(SyntaxKind.CloseParenToken);
            var semicolon = TryConsume(SyntaxKind.SemicolonToken);
            return new DoWhileStatementSyntax(_syntaxTree, doKeyword, body, whileKeyword, openParen, condition, closeParen, semicolon);
        }

        private StatementSyntax ParseForStatement()
        {
            //For now only supports range for loops
            var keyword = TryConsume(SyntaxKind.ForKeyword);
            var openParen = TryConsume(SyntaxKind.OpenParenToken);
            var identifier = TryConsume(SyntaxKind.IdentifierToken);
            var equalsSign = TryConsume(SyntaxKind.EqualsToken);
            var lowerBound = ParseExpression();
            var doubleDot = TryConsume(SyntaxKind.DoubleDotToken);
            var upperBound = ParseExpression();
            var closeParen = TryConsume(SyntaxKind.CloseParenToken);
            var body = ParseStatement();

            return new ForStatementSyntax(_syntaxTree, keyword, openParen, identifier, equalsSign, lowerBound, doubleDot, upperBound, closeParen, body);
        }

        private BlockStatementSyntax ParseBlockStatement()
        {
            var statements = ImmutableArray.CreateBuilder<StatementSyntax>();
            var openBraceToken = TryConsume(SyntaxKind.OpenBraceToken);

            while (Current.Kind != SyntaxKind.EndOfFileToken && Current.Kind != SyntaxKind.CloseBraceToken)
            {
                var startToken = Current;
                statements.Add(ParseStatement());

                if (Current == startToken)
                   Consume();
            }

            var closeBraceToken = TryConsume(SyntaxKind.CloseBraceToken);
            return new BlockStatementSyntax(_syntaxTree, openBraceToken, statements.ToImmutable(), closeBraceToken);
        }
        
        private VariableDeclarationStatementSyntax ParseVariableDeclarationStatement()
        {
            SyntaxNode declarationNode;
            if (Current.Kind == SyntaxKind.IdentifierToken)
                declarationNode = ParseTypeClause();
            else
                declarationNode = TryConsume(SyntaxKind.LetKeyword);

            var identifierToken = TryConsume(SyntaxKind.IdentifierToken);
            var equalsToken = TryConsume(SyntaxKind.EqualsToken);
            var initializer = ParseExpression();
            var semicolon = TryConsume(SyntaxKind.SemicolonToken);
            return new VariableDeclarationStatementSyntax(_syntaxTree, declarationNode, identifierToken, equalsToken, initializer, semicolon);
        }

        private BreakStatementSyntax ParseBreakStatement()
        {
            var keyword = TryConsume(SyntaxKind.BreakKeyword);
            var semicolon = TryConsume(SyntaxKind.SemicolonToken);
            return new BreakStatementSyntax(_syntaxTree, keyword, semicolon);
        }

        private ContinueStatementSyntax ParseContinueStatement()
        {
            var keyword = TryConsume(SyntaxKind.ContinueKeyword);
            var semicolon = TryConsume(SyntaxKind.SemicolonToken);
            return new ContinueStatementSyntax(_syntaxTree, keyword, semicolon);
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            var identifier = TryConsume(SyntaxKind.IdentifierToken);
            return new TypeClauseSyntax(_syntaxTree, identifier);
        }

        private ReturnTypeClauseSyntax ParseReturnTypeClause()
        {
            var colon = TryConsume(SyntaxKind.ColonToken);
            var identifier = TryConsume(SyntaxKind.IdentifierToken);
            return new ReturnTypeClauseSyntax(_syntaxTree, colon, identifier);
        }

        private ExpressionStatementSyntax ParseExpressionStatement()
        {
            var expression = ParseExpression();
            var semicolon = TryConsume(SyntaxKind.SemicolonToken);
            return new ExpressionStatementSyntax(_syntaxTree, expression, semicolon);
        }

        private ExpressionSyntax ParseExpression() => ParseAssignmentExpression();

        private ExpressionSyntax ParseAssignmentExpression()
        {
            if (Current.Kind == SyntaxKind.IdentifierToken && Next.Kind == SyntaxKind.DoubleMinusToken
                || Next.Kind == SyntaxKind.DoublePlusToken)
            {
                var identifierToken = Consume();
                var assignmentToken = Consume();
                return new IncrementExpressionSyntax(_syntaxTree, identifierToken, assignmentToken);
            }

            if (Current.Kind == SyntaxKind.IdentifierToken && Next.Kind.GetAssignmentOperatorPrecedence() != 0)
            {
                var identifierToken = Consume();
                var equalsToken = Consume();
                var expression = ParseBinaryExpression();
                return new AssignmentExpressionSyntax(_syntaxTree, identifierToken, equalsToken, expression);
            }
            return ParseBinaryExpression();
        }

        private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
        {
            ExpressionSyntax left;
            var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var operatorToken = Consume();
                var operand = ParseBinaryExpression(unaryOperatorPrecedence);
                left = new UnaryExpressionSyntax(_syntaxTree, operatorToken, operand);
            }
            else
                left = ParsePrimaryExpression();
            
            while (true)
            {
                var precedence = Current.Kind.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;

                var operatorToken = Consume();
                var right = ParseBinaryExpression(precedence);
                left = new BinaryExpressionSyntax(_syntaxTree, left, operatorToken, right);
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenParenToken:
                    return ParseParenthesizedExpression();
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                    return ParseBooleanLiteral();
                case SyntaxKind.IntegerLiteralToken:
                    return ParseIntegerLiteral();
                case SyntaxKind.StringLiteralToken:
                    return ParseStringLiteral();
                default:
                    if (Next.Kind == SyntaxKind.DotToken)
                        return ParseMemberAccessExpression();
                    else 
                        return ParseIdentifierOrCallExpression();
            }
        }

        private ExpressionSyntax ParseMemberAccessExpression()
        {
            var identifier = TryConsume(SyntaxKind.IdentifierToken);
            var dotToken = TryConsume(SyntaxKind.DotToken);
            var memberExpression = ParseExpression();
            return new MemberAccessExpressionSyntax(_syntaxTree, identifier, dotToken, memberExpression);
        }

        private ExpressionSyntax ParseIntegerLiteral()
        {
            var numberToken = TryConsume(SyntaxKind.IntegerLiteralToken);
            return new LiteralExpressionSyntax(_syntaxTree, numberToken);
        }

        private ExpressionSyntax ParseBooleanLiteral()
        {
            var isTrue = Current.Kind == SyntaxKind.TrueKeyword;
            var keywordToken = (isTrue) ? TryConsume(SyntaxKind.TrueKeyword) : TryConsume(SyntaxKind.FalseKeyword);
            return new LiteralExpressionSyntax(_syntaxTree, keywordToken, isTrue);
        }

        private ExpressionSyntax ParseStringLiteral()
        {
            var stringToken = TryConsume(SyntaxKind.StringLiteralToken);
            return new LiteralExpressionSyntax(_syntaxTree, stringToken);
        }

        private ExpressionSyntax ParseIdentifierOrCallExpression()
        {
            if (Current.Kind == SyntaxKind.IdentifierToken && Next.Kind == SyntaxKind.OpenParenToken)
                return ParseCallExpression();
            else
                return ParseIdentifierExpression();
        }

        private ExpressionSyntax ParseIdentifierExpression()
        {
            var identifier = TryConsume(SyntaxKind.IdentifierToken);
            return new IdentifierExpressionSyntax(_syntaxTree, identifier);
        }

        private ExpressionSyntax ParseCallExpression()
        {
            var identifier = TryConsume(SyntaxKind.IdentifierToken);
            var openParen = TryConsume(SyntaxKind.OpenParenToken);
            var arguments = ParseArguments();
            var closeParen = TryConsume(SyntaxKind.CloseParenToken);
            return new CallExpressionSyntax(_syntaxTree, identifier, openParen, arguments, closeParen);
        }

        private SeparatedSyntaxList<ExpressionSyntax> ParseArguments()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            var done = false;
            while (!done && Current.Kind != SyntaxKind.CloseParenToken && Current.Kind != SyntaxKind.EndOfFileToken)
            {
                nodesAndSeparators.Add(ParseExpression());

                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    var comma = TryConsume(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
                else
                    done = true;
            }
            return new SeparatedSyntaxList<ExpressionSyntax>(nodesAndSeparators.ToImmutable());
        }

        private SeparatedSyntaxList<ParameterSyntax> ParseParameters()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            var done = false;
            while (!done && Current.Kind != SyntaxKind.CloseParenToken && Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var typeClause = ParseTypeClause();
                var identifier = TryConsume(SyntaxKind.IdentifierToken);
                nodesAndSeparators.Add(new ParameterSyntax(_syntaxTree, typeClause, identifier));

                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    var comma = TryConsume(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
                else
                    done = true;
            }
            return new SeparatedSyntaxList<ParameterSyntax>(nodesAndSeparators.ToImmutable());
        }

        private ParenthesizedExpressionSyntax ParseParenthesizedExpression()
        {
            var left = TryConsume(SyntaxKind.OpenParenToken);
            var expression = ParseExpression();
            var right = TryConsume(SyntaxKind.CloseParenToken);
            return new ParenthesizedExpressionSyntax(_syntaxTree, left, expression, right);
        }
    }
}
