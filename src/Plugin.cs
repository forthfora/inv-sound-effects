using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;
using System.Security;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete


namespace InvSoundEffects
{
    [BepInPlugin(AUTHOR + "." + MOD_ID, MOD_NAME, VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; } = null!;

        public const string VERSION = "1.0.1";
        public const string MOD_NAME = "Inv Sound Effects";
        public const string MOD_ID = "invsoundeffects";
        public const string AUTHOR = "forthbridge";

        public void OnEnable()
        {
            Logger = base.Logger;

            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }

        private bool isInit = false;

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            try
            {
                if (isInit) return;
                isInit = true;

                MachineConnector.SetRegisteredOI(MOD_ID, Options.instance);


                IL.Player.Die += Player_Die;
                IL.Player.TerrainImpact += Player_TerrainImpact;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            finally
            {
                orig(self);
            }
        }



        // Replace the Sofanthiel enum checks with the specified config
        private void Player_Die(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld<MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)),
                x => x.Match(OpCodes.Call));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Player, bool>>((self) => Options.invDeathSound.Value);

            c.Emit(OpCodes.Or);
        }

        private void Player_TerrainImpact(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld<MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)),
                x => x.Match(OpCodes.Call));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Player, bool>>((self) => Options.invImpactSound.Value);

            c.Emit(OpCodes.Or);
        }
    }
}

