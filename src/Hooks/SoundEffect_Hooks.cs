using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;

namespace InvSoundEffects;

public static class SoundEffect_Hooks
{
    public static void ApplyHooks()
    {
        try
        {
            IL.Player.Die += Player_Die;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Failed to apply Player.Die IL: {e}");
        }

        try
        {
            IL.Player.TerrainImpact += Player_TerrainImpact;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Failed to apply Player.TerrainImpact IL: {e}");
        }
    }

    private static void Player_Die(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)),
                x => x.Match(OpCodes.Call)))
        {
            throw new Exception("Goto Failed");
        }

        c.EmitDelegate<Func<bool>>(() => ModOptions.InvDeathSound.Value);
        c.Emit(OpCodes.Or);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Player, bool>>(self => self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel || !ModOptions.DisableInvDeathSound.Value);
        c.Emit(OpCodes.And);
    }

    private static void Player_TerrainImpact(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)),
                x => x.Match(OpCodes.Call)))
        {
            throw new Exception("Goto Failed");
        }

        c.EmitDelegate<Func<bool>>(() => ModOptions.InvImpactSound.Value);
        c.Emit(OpCodes.Or);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Player, bool>>(self => self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel || !ModOptions.DisableInvImpactSound.Value);
        c.Emit(OpCodes.And);
    }
}
