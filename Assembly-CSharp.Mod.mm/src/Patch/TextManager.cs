#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using System;
using System.IO;
using UnityEngine;
using YLMAPI;

class patch_TextManager : TextManager {

    // Already defined in original TextManager.
    [MonoModIgnore]
    private string[][] stringData;
    [MonoModIgnore]
    public new string[] tables;

    [MonoModIgnore]
    private extern string GetLocale();
    public string INTERNAL_GetLocale() => GetLocale();

    private extern void orig_Awake();
    private void Awake() {
        // TODO: Find an even earlier entry point that isn't UnityEngine.ClassLibraryInitializer::Init
        ModAPI.EntryPoint();

        orig_Awake();
    }

    public extern void orig_LoadTables();
    // new as we're hiding TextManager's LoadTables.
    public new void LoadTables() {
        orig_LoadTables();

        ModEvents.TextsLoaded(this, tables, stringData);
    }

}
public static class TextManagerExt {

    public static string GetLocale(this TextManager tm)
        => ((patch_TextManager) tm).INTERNAL_GetLocale();

}
