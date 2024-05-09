using System.Collections.Generic;
using System.IO;
using Verse;

namespace TeleCore.Loader;

public class TeleCoreSettings : ModSettings
{
    public bool enableProjectileGraphicRandomFix;
    public bool showToolsInMainMenu;

    //
    private List<string> keyList;

    //Tools.General
    //internal Dictionary<string, ScribeDictionary<string, bool>> DataBrowserSettings = new();

    //Tools.Animation
    private string userDefinedAnimationDefLocation;
    private List<Dictionary<string, bool>> valueList;

    //Properties
    public string SaveAnimationDefLocation => userDefinedAnimationDefLocation;
    public bool ProjectileGraphicRandomFix => enableProjectileGraphicRandomFix;

    //Data Notifiers
    /*
    internal bool AllowsModInDataBrowser(Type forType, ModContentPack mod)
    {
        if (!DataBrowserSettings.TryGetValue(forType.ToString(), out var settings)) return true;
        return !settings.TryGetValue(mod.Name, out var value) || value;
    }

    internal void SetDataBrowserSettings(Type forType, string packName, bool value)
    {
        if (!DataBrowserSettings.TryGetValue(forType.ToString(), out var settings))
        {
            settings = new ScribeDictionary<string, bool>(LookMode.Value, LookMode.Value);
            DataBrowserSettings.Add(forType.ToString(), settings);
        }
        if (!settings.ContainsKey(packName))
        {
            settings.Add(packName, value);
            return;
        }
        settings[packName] = value;
        Write();
    }
    */

    internal void SetAnimationDefLocation(string newPath, bool write = true)
    {
        userDefinedAnimationDefLocation = newPath;
        if (write)
            Write();
    }

    internal void ResetAnimationDefLocation()
    {
        SetAnimationDefLocation(DefaultAnimationDefLocation);
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref enableProjectileGraphicRandomFix, "enableProjectileGraphicRandomFix");
        Scribe_Values.Look(ref userDefinedAnimationDefLocation, "userDefinedAnimationDefLocation");
        Scribe_Values.Look(ref showToolsInMainMenu, "showToolsInMainMenu");
        //Scribe_Collections.Look(ref DataBrowserSettings, "DataBrowserSettings", LookMode.Value, LookMode.Deep);

        //TODO: Move module specific settings to their own classes and inject
        if (userDefinedAnimationDefLocation == null)
            SetAnimationDefLocation(DefaultAnimationDefLocation, false);

        /*
        if (DataBrowserSettings == null)
        {
            DataBrowserSettings = new Dictionary<string, ScribeDictionary<string, bool>>();
        }
        */
    }
    
    internal static string DefaultAnimationDefLocation = Path.Combine(GenFilePaths.FolderUnderSaveData("Animations"), "Defs");
}