using System;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace TeleCore.Loader;

public class TeleCoreMod : Mod
{
    private static Harmony? _harmony;
    
    public static TeleCoreMod Mod { get; private set; }
    
    public static TeleCoreSettings Settings => (TeleCoreSettings)Mod.modSettings;
    
    public static Harmony HarmonyInt
    {
        get
        {
            Harmony.DEBUG = false;
            return _harmony ??= new Harmony("telefonmast.telecore");
        }
    }
    
    public TeleCoreMod(ModContentPack content) : base(content)
    {
        Mod = this;
        TLog.Message($"[TeleCore] - Init", Color.cyan);
        modSettings = GetSettings<TeleCoreSettings>();
        
        var discoveredAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in discoveredAssemblies)
        {
            if (assembly.GetCustomAttributes(typeof(TeleIdentifierAttribute), false).Length <= 0) continue;
            Log.Message($"Discovered TeleCore assembly: {assembly.FullName}");
            HarmonyInt.PatchAll(assembly);
        }
    }
    
    public override string SettingsCategory()
    {
        return "TeleCore";
    }
    
    public override void DoSettingsWindowContents(Rect inRect)
    {
        Listing_Standard ls = new Listing_Standard(GameFont.Small);
        ls.Begin(inRect);
        ls.Label("Some Settings");
        ls.CheckboxLabeled("Show Tele Tools in main menu", ref Settings.showToolsInMainMenu);
        ls.NewColumn();
        ls.End();
    }
}