using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;
using System.Security;
using System.Collections.Generic;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete


namespace InvSoundEffects
{
    [BepInPlugin(MOD_ID + "." + AUTHOR, MOD_NAME, VERSION)]
    internal class InvSoundEffects : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; } = null!;

        public const string VERSION = "1.0.0";
        public const string MOD_NAME = "Inv Sound Effects";
        public const string MOD_ID = "invsoundeffects";
        public const string AUTHOR = "forthbridge";

        public void OnEnable()
        {
            Logger = base.Logger;

            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            MachineConnector.SetRegisteredOI(MOD_ID, Options.instance);

            try
            {
                IL.Player.Die += Player_Die;
                IL.Player.TerrainImpact += Player_TerrainImpact;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        // Replace the Sofanthiel enum checks with the specified config
        private void Player_Die(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdsfld<ModManager>("MSC"),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdarg(0));

            c.RemoveRange(6);
            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Func<Player, bool>>((self) =>
            {
                if (Options.invDeathSound.Value)
                {
                    return true;
                }

                return false;
            });
        }

        private void Player_TerrainImpact(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdsfld<ModManager>("MSC"),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Player>("SlugCatClass"));

            c.RemoveRange(6);
            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Func<Player, bool>>((self) => 
            {
                if (Options.invImpactSound.Value)
                {
                    return true;
                }

                return false;
            });
        }
    }
}

