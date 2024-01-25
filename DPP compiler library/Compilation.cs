﻿using DPP_Compiler.Binding;
using DPP_Compiler.Diagnostics;
using DPP_Compiler.Lowering;
using DPP_Compiler.Miscellaneuos;
using DPP_Compiler.Symbols;
using System.Collections.Immutable;

namespace DPP_Compiler
{
    public sealed class Compilation
    {
        private BoundGlobalScope? _globalScope;

        public SyntaxTree SyntaxTree { get; private set; }

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    BoundGlobalScope scope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTree.Root);
                    Interlocked.CompareExchange(ref _globalScope, scope, null);
                }
                return _globalScope;
            }
        }

        public Compilation? Previous { get; }

        private Compilation(Compilation? previous, SyntaxTree syntaxTree)
        {
            Previous = previous;
            SyntaxTree = syntaxTree;
        }

        public Compilation(SyntaxTree syntaxTree) : this(null, syntaxTree) { }

        public Compilation ContinueWith(SyntaxTree syntaxTree) => new Compilation(this, syntaxTree);

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object?> variables)
        {   
            ImmutableArray<Diagnostic> diagnostics = SyntaxTree.Diagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
                return new EvaluationResult(diagnostics, null);

            BoundProgram program = Binder.BindProgram(GlobalScope);
            if (program.Diagnostics.Any())
                return new EvaluationResult(program.Diagnostics.ToImmutableArray(), null);

            Evaluator evaluator = new Evaluator(program.FunctionBodies, GetLoweredStatement(), variables);
            object? value = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }

        public void EmitTree(TextWriter writer)
        {
            GetLoweredStatement().WriteTo(writer);
        }

        private BoundBlockStatement GetLoweredStatement()
        {
            BoundStatement result = GlobalScope.Statement;
            return Lowerer.Lower(result);
        }
    }
}
