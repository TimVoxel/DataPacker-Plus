﻿using Blaze.Diagnostics;
using Blaze.Lowering;
using Blaze.Symbols;
using Blaze.Syntax_Nodes;
using Blaze.SyntaxTokens;
using Blaze.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Blaze.Binding
{
    internal sealed class Binder
    {
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly FunctionSymbol? _function;

        private Stack<(BoundLabel breakLabel, BoundLabel continueLabel)> _loopStack = new Stack<(BoundLabel breakLabel, BoundLabel continueLabel)>();
        private int _labelCounter = 0;

        private BoundScope _scope;

        public DiagnosticBag Diagnostics => _diagnostics;

        public Binder(BoundScope? parent, FunctionSymbol? function)
        {
            _scope = new BoundScope(parent);
            _function = function;

            if (_function != null)
                foreach (var parameter in _function.Parameters)
                    _scope.TryDeclareVariable(parameter);
        }

        public static BoundGlobalScope BindGlobalScope(ImmutableArray<SyntaxTree> syntaxTrees)
        {
            //1. Create a root scope that contains all the built-in functions 
            var parentScope = CreateRootScope();
            var binder = new Binder(parentScope, null);

            //2. Bind function declarations
            var functionDeclarations = syntaxTrees.SelectMany(st => st.Root.Members).OfType<FunctionDeclarationSyntax>();

            foreach (var function in functionDeclarations)
                binder.BindFunctionDeclaration(function);

            //3. Bind global statements
            var globalStatements = syntaxTrees.SelectMany(st => st.Root.Members).OfType<GlobalStatementSyntax>();
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            foreach (var globalStatement in globalStatements)
            {
                var boundStatement = binder.BindGlobalStatement(globalStatement.Statement);
                statements.Add(boundStatement);
            }

            //4. Bind main function. Check
            //   if it has the correct signature
            //   if there is not main function, fabricate it
            var functions = binder._scope.GetDeclaredFunctions();
            var mainFunction = functions.SingleOrDefault(f => f.Name == "main");

            if (mainFunction == null)
            {
                mainFunction = new FunctionSymbol("main", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Void, null);
            }
            else
            {
                Debug.Assert(mainFunction.Declaration != null);
                if (mainFunction.ReturnType != TypeSymbol.Void || mainFunction.Parameters.Any())
                    binder.Diagnostics.ReportMainFunctionMustHaveCorrectSignature(mainFunction.Declaration.Identifier.Location);
            }

            //5. Create the global scope
            var diagnostics = binder.Diagnostics.ToImmutableArray();
            var variables = binder._scope.GetDeclaredVariables();         
            return new BoundGlobalScope(diagnostics, mainFunction, variables, functions, statements.ToImmutable());
        }

        private static BoundScope CreateRootScope(BoundGlobalScope? globalScope = null)
        {
            var result = new BoundScope(null);
            foreach (var builtInFunction in BuiltInFunction.GetAll())
                result.TryDeclareFunction(builtInFunction);

            if (globalScope == null)
                return result;

            foreach (var function in globalScope.Functions)
                result.TryDeclareFunction(function);

            foreach (var function in globalScope.Variables)
                result.TryDeclareVariable(function);

            return result;
        }

        public static BoundProgram BindProgram(BoundGlobalScope globalScope)
        {
            //1. Create a root scope, which contains the built-in functions as well as 
            //   functions and variables declared in the global scope
            var parentScope = CreateRootScope(globalScope);

            //2. Bind every function body and lower it,
            //   connect it to the declaration
            var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundStatement>();
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            foreach (var function in globalScope.Functions)
            {
                var binder = new Binder(parentScope, function);
                if (function.Declaration != null)
                {
                    var body = binder.BindStatement(function.Declaration.Body);
                    var loweredBody = Lowerer.Lower(body);
                    var deepLoweredBody = Lowerer.DeepLower(body);

                    if (function.ReturnType != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn(deepLoweredBody))
                        binder._diagnostics.ReportAllPathsMustReturn(function.Declaration.Identifier.Location);

                    //Passing the lowered body, not the deep lowered one, as it is
                    //Easier to convert to mcfunctions
                    functionBodies.Add(function, loweredBody);
                    diagnostics.AddRange(binder.Diagnostics);
                }
            }

            //3. Bind the main function and connect it to the global statements
            //   If there are any
            var mainFunction = globalScope.MainFunction;

            if (mainFunction.Declaration == null && globalScope.Statements.Any())
            {
                var body = Lowerer.Lower(new BoundBlockStatement(globalScope.Statements));
                functionBodies.Add(mainFunction, body);
            }

            return new BoundProgram(diagnostics.ToImmutable(), mainFunction, functionBodies.ToImmutable());
        }

        private void BindFunctionDeclaration(FunctionDeclarationSyntax declaration)
        {
            var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
            var seenParameterNames = new HashSet<string>();

            foreach (var parameterSyntax in declaration.Parameters)
            {
                var name = parameterSyntax.Identifier.Text;
                var type = BindTypeClause(parameterSyntax.Type);

                if (type == null)
                    continue;

                if (!seenParameterNames.Add(name))
                    _diagnostics.ReportParameterAlreadyDeclared(parameterSyntax.Location, name);
                else
                    parameters.Add(new ParameterSymbol(name, type));
            }

            var returnType = (declaration.ReturnTypeClause == null) ? TypeSymbol.Void : BindReturnTypeClause(declaration.ReturnTypeClause);
            if (returnType == null)
                returnType = TypeSymbol.Void;

            var function = new FunctionSymbol(declaration.Identifier.Text, parameters.ToImmutable(), returnType, declaration);
            if (!_scope.TryDeclareFunction(function))
                _diagnostics.ReportFunctionAlreadyDeclared(declaration.Identifier.Location, function.Name);
        }

        private BoundStatement BindGlobalStatement(StatementSyntax syntax) => BindStatement(syntax, true);

        private BoundStatement BindStatement(StatementSyntax syntax, bool isGlobal = false)
        {
            var result = BindStatementInternal(syntax);

            if (isGlobal)
                return result;

            if (result is BoundExpressionStatement es)
            {
                var isAllowedExpression = es.Expression.Kind == BoundNodeKind.AssignmentExpression
                                        || es.Expression.Kind == BoundNodeKind.CallExpression
                                        || es.Expression.Kind == BoundNodeKind.ErrorExpression;

                if (!isAllowedExpression)
                    _diagnostics.ReportInvalidExpressionStatement(syntax.Location);
            }
            return result;
        }

        private BoundStatement BindStatementInternal(StatementSyntax syntax)
        {
            return syntax.Kind switch
            {
                SyntaxKind.BlockStatement => BindBlockStatement((BlockStatementSyntax)syntax),
                SyntaxKind.ExpressionStatement => BindExpressionStatement((ExpressionStatementSyntax)syntax),
                SyntaxKind.VariableDeclarationStatement => BindVariableDeclarationStatement((VariableDeclarationStatementSyntax)syntax),
                SyntaxKind.IfStatement => BindIfStatement((IfStatementSyntax)syntax),
                SyntaxKind.WhileStatement => BindWhileStatement((WhileStatementSyntax)syntax),
                SyntaxKind.ForStatement => BindForStatement((ForStatementSyntax)syntax),
                SyntaxKind.DoWhileStatement => BindDoWhileStatement((DoWhileStatementSyntax)syntax),
                SyntaxKind.BreakStatement => BindBreakStatement((BreakStatementSyntax)syntax),
                SyntaxKind.ContinueStatement => BindContinueStatement((ContinueStatementSyntax)syntax),
                SyntaxKind.ReturnStatement => BindReturnStatement((ReturnStatementSyntax)syntax),
                _ => throw new Exception($"Unexpected syntax {syntax.Kind}"),
            };
        }

        private BoundStatement BindVariableDeclarationStatement(VariableDeclarationStatementSyntax syntax)
        {
            var initializer = BindExpression(syntax.Initializer);
            TypeSymbol? type = null;
            if (syntax.DeclarationNode is TypeClauseSyntax typeClause)
                type = BindTypeClause(typeClause);

            var variableType = type ?? initializer.Type;
            var variable = BindVariable(syntax.Identifier, variableType, initializer.ConstantValue);
            var convertedInitializer = BindConversion(initializer, variableType, syntax.Initializer.Location);
            return new BoundVariableDeclarationStatement(variable, convertedInitializer);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
        {
            var boundExpression = BindExpression(syntax.Expression, true);
            return new BoundExpressionStatement(boundExpression);
        }

        private BoundStatement BindIfStatement(IfStatementSyntax syntax)
        {
            var boundCondition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var body = BindStatement(syntax.Body);
            var elseBody = (syntax.ElseClause == null) ? null : BindStatement(syntax.ElseClause.Body);
            return new BoundIfStatement(boundCondition, body, elseBody);
        }

        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            var boundCondition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var body = BindLoopBody(syntax.Body, out BoundLabel breakLabel, out BoundLabel continueLabel);
            return new BoundWhileStatement(boundCondition, body, breakLabel, continueLabel);
        }

        private BoundStatement BindDoWhileStatement(DoWhileStatementSyntax syntax)
        {
            var body = BindLoopBody(syntax.Body, out BoundLabel breakLabel, out BoundLabel continueLabel);
            var condition = BindExpression(syntax.Condition);
            return new BoundDoWhileStatement(body, condition, breakLabel, continueLabel);
        }

        private BoundStatement BindForStatement(ForStatementSyntax syntax)
        {
            var lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int);
            var upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int);
            var previous = _scope;
            _scope = new BoundScope(previous);

            var variable = BindVariable(syntax.Identifier, TypeSymbol.Int);
            var body = BindLoopBody(syntax.Body, out BoundLabel breakLabel, out BoundLabel continueLabel);
            _scope = previous;
            return new BoundForStatement(variable, lowerBound, upperBound, body, breakLabel, continueLabel);
        }

        private BoundStatement BindReturnStatement(ReturnStatementSyntax syntax)
        {
            var expression = (syntax.Expression == null) ? null : BindExpression(syntax.Expression);

            if (_function == null)
                _diagnostics.ReportReturnOutsideFunction(syntax.Location);
            else
            {
                if (_function.ReturnType == TypeSymbol.Void)
                {
                    if (syntax.Expression != null)
                        _diagnostics.ReportInvalidReturnExpression(syntax.Expression.Location, _function.Name);
                }
                else
                {
                    if (syntax.Expression == null || expression == null)
                        _diagnostics.ReportMissingReturnExpression(syntax.Keyword.Location, _function.Name, _function.ReturnType);
                    else
                        expression = BindConversion(expression, _function.ReturnType, syntax.Expression.Location);
                }
            }
            return new BoundReturnStatement(expression);
        }

        private BoundStatement BindLoopBody(StatementSyntax body, out BoundLabel breakLabel, out BoundLabel continueLabel)
        {
            _labelCounter++;
            breakLabel = new BoundLabel($"break{_labelCounter}");
            continueLabel = new BoundLabel($"continue{_labelCounter}");

            _loopStack.Push((breakLabel, continueLabel));
            var boundBody = BindStatement(body);
            _loopStack.Pop();
            return boundBody;
        }

        private BoundStatement BindBreakStatement(BreakStatementSyntax syntax)
        {
            if (_loopStack.Count == 0)
            {
                _diagnostics.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
                return BindErrorStatement();
            }
            var breakLabel = _loopStack.Peek().breakLabel;
            return new BoundBreakStatement(breakLabel);
        }

        private BoundStatement BindContinueStatement(ContinueStatementSyntax syntax)
        {
            if (_loopStack.Count == 0)
            {
                _diagnostics.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
                return BindErrorStatement();
            }
            var continueLabel = _loopStack.Peek().continueLabel;
            return new BoundContinueStatement(continueLabel);
        }

        private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
        {
            var boundStatements = ImmutableArray.CreateBuilder<BoundStatement>();
            var previous = _scope;
            _scope = new BoundScope(previous);

            foreach (var statement in syntax.Statements)
            {
                var boundStatement = BindStatement(statement);
                boundStatements.Add(boundStatement);
            }

            _scope = previous;
            return new BoundBlockStatement(boundStatements.ToImmutable());
        }

        private BoundStatement BindErrorStatement() => new BoundExpressionStatement(new BoundErrorExpression());

        private BoundExpression BindExpression(ExpressionSyntax expression, bool canBeVoid = false)
        {
            var result = BindExpressionInternal(expression);
            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                _diagnostics.ReportExpressionMustHaveValue(expression.Location);
                return new BoundErrorExpression();
            }
            return result;
        }

        private BoundExpression BindExpressionInternal(ExpressionSyntax expression)
        {
            return expression.Kind switch
            {
                SyntaxKind.LiteralExpression => BindLiteralExpression((LiteralExpressionSyntax)expression),
                SyntaxKind.BinaryExpression => BindBinaryExpression((BinaryExpressionSyntax)expression),
                SyntaxKind.UnaryExpression => BindUnaryExpression((UnaryExpressionSyntax)expression),
                SyntaxKind.ParenthesizedExpression => BindExpression(((ParenthesizedExpressionSyntax)expression).Expression),
                SyntaxKind.IdentifierExpression => BindIdentifierExpression((IdentifierExpressionSyntax)expression),
                SyntaxKind.AssignmentExpression => BindAssignmentExpression((AssignmentExpressionSyntax)expression),
                SyntaxKind.CallExpression => BindCallExpression((CallExpressionSyntax)expression),
                _ => throw new Exception($"Unexpected syntax {expression.Kind}"),
            };
        }

        private BoundExpression BindExpression(ExpressionSyntax expression, TypeSymbol desiredType) => BindConversion(expression, desiredType);

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax expression)
        {
            var value = expression.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax expression)
        {
            var boundLeft = BindExpression(expression.Left);
            var boundRight = BindExpression(expression.Right);

            if (boundLeft.Type.IsError || boundRight.Type.IsError)
                return new BoundErrorExpression();

            var op = BoundBinaryOperator.Bind(expression.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

            if (op == null)
            {
                _diagnostics.ReportUndefinedBinaryOperator(expression.OperatorToken.Location, expression.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return new BoundErrorExpression();
            }
            return new BoundBinaryExpression(boundLeft, op, boundRight);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax expression)
        {
            var operand = BindExpression(expression.Operand);
            if (operand.Type.IsError)
                return new BoundErrorExpression();

            var op = BoundUnaryOperator.Bind(expression.OperatorToken.Kind, operand.Type);

            if (op == null)
            {
                _diagnostics.ReportUndefinedUnaryOperator(expression.OperatorToken.Location, expression.OperatorToken.Text, operand.Type);
                return new BoundErrorExpression();
            }
            return new BoundUnaryExpression(op, operand);
        }

        private BoundExpression BindIdentifierExpression(IdentifierExpressionSyntax expression)
        {
            var name = expression.IdentifierToken.Text;
            if (expression.IdentifierToken.IsMissingText)
                return new BoundErrorExpression();

            var variable = _scope.TryLookupVariable(name);
            if (variable == null)
            {
                _diagnostics.ReportUndefinedName(expression.IdentifierToken.Location, name);
                return new BoundErrorExpression();
            }
            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindCallExpression(CallExpressionSyntax expression)
        {
            var name = expression.Identifier.Text;
            if (expression.Arguments.Count == 1 && TypeSymbol.Lookup(name) is TypeSymbol type)
                return BindConversion(expression.Arguments[0], type, true);
            
            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach (ExpressionSyntax argument in expression.Arguments)
                boundArguments.Add(BindExpression(argument));

            var function = _scope.TryLookupFunction(expression.Identifier.Text);
            if (function == null)
            {
                _diagnostics.ReportUndefinedFunction(expression.Identifier.Location, name);
                return new BoundErrorExpression();
            }
            if (function.Parameters.Length != expression.Arguments.Count)
            {
                _diagnostics.ReportWrongArgumentCount(expression.Location, function.Name, function.Parameters.Length, expression.Arguments.Count);
                return new BoundErrorExpression();
            }

            for (int i = 0; i < expression.Arguments.Count; i++)
            {
                var argumentLocation = expression.Arguments[i].Location;
                var parameter = function.Parameters[i];
                var boundArgument = boundArguments[i];
                boundArguments[i] = BindConversion(boundArgument, parameter.Type, argumentLocation);
            }
            
            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }
        
        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax expression)
        {
            var boundExpression = BindExpression(expression.Expression);
            var name = expression.IdentifierToken.Text;

            var variable = _scope.TryLookupVariable(name);
            if (variable == null)
            {
                _diagnostics.ReportUndefinedName(expression.IdentifierToken.Location, name);
                return boundExpression;
            }

            var convertedExpression = BindConversion(boundExpression, variable.Type, expression.Expression.Location);
            return new BoundAssignmentExpression(variable, convertedExpression);
        }

        private BoundExpression BindConversion(ExpressionSyntax syntax, TypeSymbol type, bool allowExplicit = false)
        {
            var expression = BindExpression(syntax);
            return BindConversion(expression, type, syntax.Location, allowExplicit);
        }

        private BoundExpression BindConversion(BoundExpression expression, TypeSymbol type, TextLocation diagnosticLocation, bool allowExplicit = false)
        {
            var conversion = Conversion.Classify(expression.Type, type);
            if (!conversion.Exists)
            {
                if (!expression.Type.IsError && !type.IsError)
                    _diagnostics.ReportCannotConvert(diagnosticLocation, expression.Type, type);

                return new BoundErrorExpression();
            }

            if (conversion == Conversion.Identity)
                return expression;

            if (conversion.IsExplicit && !allowExplicit)
                _diagnostics.ReportCannotConvertImplicitly(diagnosticLocation, expression.Type, type);
                
            return new BoundConversionExpression(type, expression);
        }

        private VariableSymbol BindVariable(SyntaxToken identifier, TypeSymbol type, BoundConstant? constant = null)
        {
            var name = identifier.Text;
            VariableSymbol variable = _function == null
                                ? new GlobalVariableSymbol(name, type, constant)
                                : new LocalVariableSymbol(name, type, constant);

            if (!_scope.TryDeclareVariable(variable))
                _diagnostics.ReportVariableAlreadyDeclared(identifier.Location, name);

            return variable;
        }

        private TypeSymbol? BindTypeClause(TypeClauseSyntax syntax)
        {
            var type = TypeSymbol.Lookup(syntax.Identifier.Text);
            if (type == null)
                _diagnostics.ReportUndefinedType(syntax.Identifier.Location, syntax.Identifier.Text);
            return type;
        }

        private TypeSymbol? BindReturnTypeClause(ReturnTypeClauseSyntax syntax)
        {
            var type = TypeSymbol.Lookup(syntax.Identifier.Text);
            if (type == null)
                _diagnostics.ReportUndefinedType(syntax.Identifier.Location, syntax.Identifier.Text);
            return type;
        }
    }
}