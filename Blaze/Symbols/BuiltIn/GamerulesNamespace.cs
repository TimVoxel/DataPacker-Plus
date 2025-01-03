﻿namespace Blaze.Symbols.BuiltIn
{
    internal sealed class GamerulesNamespace : BuiltInNamespace
    {
        //TODO: Add limitations to short values
        public FunctionSymbol SetGamerule { get; private set; }
        public FieldSymbol AnnounceAdvancements { get; }
        public FieldSymbol BlockExplosionDropDecay { get; }
        public FieldSymbol CommandBlockOutput { get; }
        public FieldSymbol CommandModificationBlockLimit { get; }
        public FieldSymbol DisableElytraMovementCheck { get; }
        public FieldSymbol DisableRaids { get; }
        public FieldSymbol DoDaylightCycle { get; }
        public FieldSymbol DoEntityDrops { get; }
        public FieldSymbol DoFireTick { get; }
        public FieldSymbol DoImmediateRespawn { get; }
        public FieldSymbol DoInsomnia { get; }
        public FieldSymbol DoLimitedCrafting { get; }
        public FieldSymbol DoMobLoot { get; }
        public FieldSymbol DoMobSpawning { get; }
        public FieldSymbol DoPatrolSpawning { get; }
        public FieldSymbol DoTileDrops { get; }
        public FieldSymbol DoTraderSpawning { get; }
        public FieldSymbol DoVinesSpread { get; }
        public FieldSymbol DoWardenSpawning { get; }
        public FieldSymbol DoWeatherCycle { get; }
        public FieldSymbol DrowningDamage { get; }
        public FieldSymbol EnderPearlsVanishOnDeath { get; }
        public FieldSymbol FallDamage { get; }
        public FieldSymbol FireDamage { get; }
        public FieldSymbol ForgiveDeadPlayers { get; }
        public FieldSymbol FreezeDamage { get; }
        public FieldSymbol GlobalSoundEffects { get; }
        public FieldSymbol KeepInventory { get; }
        public FieldSymbol LavaSourceConversion { get; }
        public FieldSymbol LogAdminCommands { get; }
        public FieldSymbol MaxCommandChainLength { get; }
        public FieldSymbol MaxCommandForkCount { get; }
        public FieldSymbol MaxEntityCramming { get; }
        public FieldSymbol MobExplosionDropDecay { get; }
        public FieldSymbol MobGriefing { get; }
        public FieldSymbol NaturalRegeneration { get; }
        public FieldSymbol PlayersNetherPortalCreativeDelay { get; }
        public FieldSymbol PlayersNetherPortalDefaultDelay { get; }
        public FieldSymbol PlayersSleepingPercentage { get; }
        public FieldSymbol ProjectilesCanBreakBlocks { get; }
        public FieldSymbol Pvp { get; }
        public FieldSymbol RandomTickSpeed { get; }
        public FieldSymbol ReducedDebugInfo { get; }
        public FieldSymbol SendCommandFeedback { get; }
        public FieldSymbol ShowDeathMessages { get; }
        public FieldSymbol SnowAccumulationHeight { get; }
        public FieldSymbol SpawnChunkRadius { get; }
        public FieldSymbol SpawnRadius { get; }
        public FieldSymbol SpectatorsGenerateChunks { get; }
        public FieldSymbol TntExplosionDropDecay { get; }
        public FieldSymbol UniversalAnger { get; }
        public FieldSymbol WaterSourceConversion { get; }

        private static List<FieldSymbol> _gamerules = new List<FieldSymbol>(74);

        public GamerulesNamespace(GeneralNamespace parent) : base("gamerules", parent)
        {
            SetGamerule = Function("set_gamerule", TypeSymbol.Void, new ParameterSymbol("rule", TypeSymbol.String), new ParameterSymbol("value", TypeSymbol.Int));

            AnnounceAdvancements = Field(Symbol, "announceAdvancements", TypeSymbol.Bool);
            BlockExplosionDropDecay = Field(Symbol, "blockExplosionDropDecay", TypeSymbol.Bool);
            CommandBlockOutput = Field(Symbol, "commandBlockOutput", TypeSymbol.Bool);
            CommandModificationBlockLimit = Field(Symbol, "commandModificationBlockLimit", TypeSymbol.Int);
            DisableElytraMovementCheck = Field(Symbol, "disableElytraMovementCheck", TypeSymbol.Bool);
            DisableRaids = Field(Symbol, "disableRaids", TypeSymbol.Bool);
            DoDaylightCycle = Field(Symbol, "doDaylightCycle", TypeSymbol.Bool);
            DoEntityDrops = Field(Symbol, "doEntityDrops", TypeSymbol.Bool);
            DoFireTick = Field(Symbol, "doFireTick", TypeSymbol.Bool);
            DoImmediateRespawn = Field(Symbol, "doImmediateRespawn", TypeSymbol.Bool);
            DoInsomnia = Field(Symbol, "doInsomnia", TypeSymbol.Bool);
            DoLimitedCrafting = Field(Symbol, "doLimitedCrafting", TypeSymbol.Bool);
            DoMobLoot = Field(Symbol, "doMobLoot", TypeSymbol.Bool);
            DoMobSpawning = Field(Symbol, "doMobSpawning", TypeSymbol.Bool);
            DoPatrolSpawning = Field(Symbol, "doPatrolSpawning", TypeSymbol.Bool);
            DoTileDrops = Field(Symbol, "doTileDrops", TypeSymbol.Bool);
            DoTraderSpawning = Field(Symbol, "doTraderSpawning", TypeSymbol.Bool);
            DoVinesSpread = Field(Symbol, "doVinesSpread", TypeSymbol.Bool);
            DoWardenSpawning = Field(Symbol, "doWardenSpawning", TypeSymbol.Bool);
            DoWeatherCycle = Field(Symbol, "doWeatherCycle", TypeSymbol.Bool);
            DrowningDamage = Field(Symbol, "drowningDamage", TypeSymbol.Bool);
            EnderPearlsVanishOnDeath = Field(Symbol, "enderPearlsVanishOnDeath", TypeSymbol.Bool);
            FallDamage = Field(Symbol, "fallDamage", TypeSymbol.Bool);
            FireDamage = Field(Symbol, "fireDamage", TypeSymbol.Bool);
            ForgiveDeadPlayers = Field(Symbol, "forgiveDeadPlayers", TypeSymbol.Bool);
            FreezeDamage = Field(Symbol, "freezeDamage", TypeSymbol.Bool);
            GlobalSoundEffects = Field(Symbol, "globalSoundEffects", TypeSymbol.Bool);
            KeepInventory = Field(Symbol, "keepInventory", TypeSymbol.Bool);
            LavaSourceConversion = Field(Symbol, "lavaSourceConversion", TypeSymbol.Bool);
            LogAdminCommands = Field(Symbol, "logAdminCommands", TypeSymbol.Bool);
            MaxCommandChainLength = Field(Symbol, "maxCommandChainLength", TypeSymbol.Int);
            MaxCommandForkCount = Field(Symbol, "maxCommandForkCount", TypeSymbol.Int);
            MaxEntityCramming = Field(Symbol, "maxEntityCramming", TypeSymbol.Int);
            MobExplosionDropDecay = Field(Symbol, "mobExplosionDropDecay", TypeSymbol.Bool);
            MobGriefing = Field(Symbol, "mobGriefing", TypeSymbol.Bool);
            NaturalRegeneration = Field(Symbol, "naturalRegeneration", TypeSymbol.Bool);
            PlayersNetherPortalCreativeDelay = Field(Symbol, "playersNetherPortalCreativeDelay", TypeSymbol.Int);
            PlayersNetherPortalDefaultDelay = Field(Symbol, "playersNetherPortalDefaultDelay", TypeSymbol.Int);
            PlayersSleepingPercentage = Field(Symbol, "playersSleepingPercentage", TypeSymbol.Int);
            ProjectilesCanBreakBlocks = Field(Symbol, "projectilesCanBreakBlocks", TypeSymbol.Bool);
            Pvp = Field(Symbol, "pvp", TypeSymbol.Bool);
            RandomTickSpeed = Field(Symbol, "randomTickSpeed", TypeSymbol.Int);
            ReducedDebugInfo = Field(Symbol, "reducedDebugInfo", TypeSymbol.Bool);
            SendCommandFeedback = Field(Symbol, "sendCommandFeedback", TypeSymbol.Bool);
            ShowDeathMessages = Field(Symbol, "showDeathMessages", TypeSymbol.Bool);
            SnowAccumulationHeight = Field(Symbol, "snowAccumulationHeight", TypeSymbol.Int);
            SpawnChunkRadius = Field(Symbol, "spawnChunkRadius", TypeSymbol.Int);
            SpawnRadius = Field(Symbol, "spawnRadius", TypeSymbol.Int);
            SpectatorsGenerateChunks = Field(Symbol, "spectatorsGenerateChunks", TypeSymbol.Bool);
            TntExplosionDropDecay = Field(Symbol, "tntExplosionDropDecay", TypeSymbol.Bool);
            UniversalAnger = Field(Symbol, "universalAnger", TypeSymbol.Bool);
            WaterSourceConversion = Field(Symbol, "waterSourceConversion", TypeSymbol.Bool);

            _gamerules.Clear();
            _gamerules.AddRange(new[]
            {
                    AnnounceAdvancements,
                    BlockExplosionDropDecay,
                    CommandBlockOutput,
                    CommandModificationBlockLimit,
                    DisableElytraMovementCheck,
                    DisableRaids,
                    DoDaylightCycle,
                    DoEntityDrops,
                    DoFireTick,
                    DoImmediateRespawn,
                    DoInsomnia,
                    DoLimitedCrafting,
                    DoMobLoot,
                    DoMobSpawning,
                    DoPatrolSpawning,
                    DoTileDrops,
                    DoTraderSpawning,
                    DoVinesSpread,
                    DoWardenSpawning,
                    DoWeatherCycle,
                    DrowningDamage,
                    EnderPearlsVanishOnDeath,
                    FallDamage,
                    FireDamage,
                    ForgiveDeadPlayers,
                    FreezeDamage,
                    GlobalSoundEffects,
                    KeepInventory,
                    LavaSourceConversion,
                    LogAdminCommands,
                    MaxCommandChainLength,
                    MaxCommandForkCount,
                    MaxEntityCramming,
                    MobExplosionDropDecay,
                    MobGriefing,
                    NaturalRegeneration,
                    PlayersNetherPortalCreativeDelay,
                    PlayersNetherPortalDefaultDelay,
                    PlayersSleepingPercentage,
                    ProjectilesCanBreakBlocks,
                    Pvp,
                    RandomTickSpeed,
                    ReducedDebugInfo,
                    SendCommandFeedback,
                    ShowDeathMessages,
                    SnowAccumulationHeight,
                    SpawnChunkRadius,
                    SpawnRadius,
                    SpectatorsGenerateChunks,
                    TntExplosionDropDecay,
                    UniversalAnger,
                    WaterSourceConversion
                });
        }

        public bool IsGamerule(FieldSymbol field) => _gamerules.Contains(field);
    }
}
