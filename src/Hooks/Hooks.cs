namespace InvSoundEffects;

public static class Hooks
{
    private static bool IsInit { get; set; }

    public static void ApplyInit()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
    }

    private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        try
        {
            ModOptions.RegisterOI();

            if (IsInit)
            {
                return;
            }

            IsInit = true;

            ApplyHooks();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("OnModsInit:\n" + e);
        }
        finally
        {
            orig(self);
        }
    }

    private static void ApplyHooks()
    {
        SoundEffect_Hooks.ApplyHooks();
    }
}
