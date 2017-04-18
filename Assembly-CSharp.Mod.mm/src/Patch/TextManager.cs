#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using System.IO;
using UnityEngine;

class patch_TextManager : TextManager {

    // Already defined in original TextManager.
    [MonoModIgnore]
    private string[][] stringData;
    [MonoModIgnore]
    public new string[] tables;

    [MonoModIgnore]
    private extern string GetLocale();
    public string INTERNAL_GetLocale() => GetLocale();

    public extern void orig_LoadTables();
    // new as we're hiding TextManager's LoadTables.
    public new void LoadTables() {
        orig_LoadTables();

        // This runs before YLMod.EntryPoint
        // TODO: Once the YLMod.EntryPoint runs before TextManager, don't manually invoke YLMod.OnTextLoad on EntryPoint
        YLMod.Content.OnTextLoad?.Invoke(this, tables, stringData);
    }

}
public static class TextManagerExt {

    public static string GetLocale(this TextManager tm)
        => ((patch_TextManager) tm).INTERNAL_GetLocale();

}
