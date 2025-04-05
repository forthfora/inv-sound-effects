using Menu.Remix.MixedUI;

namespace InvSoundEffects;

public class ModOptions : OptionsTemplate
{
    public static ModOptions Instance { get; } = new();

    public static void RegisterOI()
    {
        if (MachineConnector.GetRegisteredOI(Plugin.MOD_ID) != Instance)
        {
            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Instance);
        }
    }

    public static Configurable<bool> InvDeathSound { get; } = Instance.config.Bind("invDeathSound", true, new ConfigurableInfo(
        "Whether all slugcats use Inv's death sounds.",
        null, "", "Death Sounds?"));

    public static Configurable<bool> InvImpactSound { get; } = Instance.config.Bind("invImpactSound", false, new ConfigurableInfo(
        "Whether all slugcats use Inv's impact sounds.",
        null, "", "Impact Sounds?"));

    public static Configurable<bool> DisableInvDeathSound { get; } = Instance.config.Bind(nameof(DisableInvDeathSound), false, new ConfigurableInfo(
        "Whether the death sound is disabled for Inv only.",
        null, "", "Disable Inv Death Sounds?"));

    public static Configurable<bool> DisableInvImpactSound { get; } = Instance.config.Bind(nameof(DisableInvImpactSound), false, new ConfigurableInfo(
        "Whether the impact sound is disabled for Inv only.",
        null, "", "Disable Inv Impact Sounds?"));


    private const int NUMBER_OF_TABS = 1;

    public override void Initialize()
    {
        base.Initialize();
        Tabs = new OpTab[NUMBER_OF_TABS];
        var tabIndex = -1;

        AddTab(ref tabIndex, "General");

        AddCheckBox(InvDeathSound);
        AddCheckBox(InvImpactSound);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddCheckBox(DisableInvDeathSound);
        AddCheckBox(DisableInvImpactSound);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLinesUntilEnd();
        DrawBox(ref Tabs[tabIndex]);
    }
}
