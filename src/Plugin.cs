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
    [BepInPlugin(AUTHOR + "." + MOD_ID, MOD_NAME, VERSION)]
    internal class Plugin : BaseUnityPlugin
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

        private bool isInit = false;

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            if (isInit) return;
            isInit = true;

            MachineConnector.SetRegisteredOI(MOD_ID, Options.instance);

            try
            {
                IL.Player.Die += Player_Die;
                IL.Player.TerrainImpact += Player_TerrainImpact;

                // An example IL Hook, I'll use this whenever someone asks for help lol
                // IL.Player.TerrainImpact += Player_TerrainImpactIL;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        private void Player_TerrainImpactIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            // Match the following OpCode instructions
            // Typically being this precise is not needed to locate the correct place
            // You could get away with only the first 3 matches, this is just for clarity
            c.GotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<UpdatableAndDeletable>("room"),
                i => i.MatchLdsfld<SoundID>("Slugcat_Terrain_Impact_Death"),
                i => i.MatchLdarg(0),
                i => i.MatchCallOrCallvirt<Creature>("get_mainBodyChunk"),
                i => i.MatchCallOrCallvirt<Room>("PlaySound"),
                i => i.MatchPop(),
                i => i.MatchLdstr("Fall damage death"),
                i => i.MatchCallOrCallvirt<Debug>("Log"),
                i => i.MatchLdarg(0),
                i => i.MatchCallOrCallvirt<Creature>("Die")
                );

            // Another instruction branches to IL_0415, so we must skip it to preserve it
            // Therefore, we increment the current instruction the cursor points to by one
            c.Index++;

            // Remove the next 10 instructions
            c.RemoveRange(10);

            // Normally, we must push argument 0 to the stack, like this
            // c.Emit(OpCodes.Ldarg_0);

            // Argument 0 for non static methods is always an instance of 'this', in this case the creature
            // In this case, it isn't necessary (and would actually break the hook) because IL_0415 is Ldarg_0 and just did it for us

            // Emit a delegate which will run some complex logic for us
            // Here, we pass in Action<Creature> as the generic, which will give us the instance of creature we just pushed onto the stack
            // The delegate immediately consumes this instance and pops it from the stack
            // Mainly useful for getting some information from the instance
            c.EmitDelegate<Action<Creature>>((creature) => {
                // Use whatever logger you want here
                Logger.LogWarning("Handling death impact scenario for " + creature.GetType());
            });

            //// If you want the delegate to push something onto the stack, you could do something like this for example
            //c.Emit(OpCodes.Ldarg_0);
            //c.EmitDelegate<Func<Creature, bool>>((creature) => {
            //    // Run some complex logic, return a corresponding value
            //    return true;
            //});
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

            c.EmitDelegate<Func<Player, bool>>((self) => Options.invDeathSound.Value);
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

            c.EmitDelegate<Func<Player, bool>>((self) => Options.invImpactSound.Value);
        }
    }
}

