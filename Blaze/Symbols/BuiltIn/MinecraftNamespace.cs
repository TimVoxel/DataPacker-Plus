﻿namespace Blaze.Symbols.BuiltIn
{
    internal sealed class MinecraftNamespace : BuiltInNamespace
    {
        public ChatNamespace Chat { get; }
        public GeneralNamespace General { get; }

        public MinecraftNamespace() : base("minecraft")
        {
            Chat = new ChatNamespace(this);
            General = new GeneralNamespace(this);
        }

        internal sealed class ChatNamespace : BuiltInNamespace
        {
            public FunctionSymbol Say { get; }
            public FunctionSymbol Print { get; }

            public ChatNamespace(MinecraftNamespace parent) : base("chat", parent)
            {
                Say = Function("say", TypeSymbol.Void, Parameter("text", TypeSymbol.String));
                Print = Function("print", TypeSymbol.Void, Parameter("text", TypeSymbol.String));
            }
        }

        internal sealed class GeneralNamespace : BuiltInNamespace
        {
            public NamedTypeSymbol Pos { get; }

            public GeneralNamespace(MinecraftNamespace parent) : base("general", parent)
            {
                //Pos class
                var x = Parameter("x", TypeSymbol.Int);
                var y = Parameter("y", TypeSymbol.Int);
                var z = Parameter("z", TypeSymbol.Int);

                Pos = Class("Pos", Constructor(x, y, z));
                var xField = Field(Pos, "x", TypeSymbol.Int);
                var yField = Field(Pos, "y", TypeSymbol.Int);
                var zField = Field(Pos, "z", TypeSymbol.Int);

                var posConstructorBlock = AssignFieldsBlock(Pos, x, y, z);
                Pos.Constructor.FunctionBody = posConstructorBlock;
            }

        }
    }
}
