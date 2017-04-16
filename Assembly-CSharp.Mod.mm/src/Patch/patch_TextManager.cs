#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using UnityEngine;

class patch_TextManager : TextManager {

    // Already defined in original TextManager.
    [MonoModIgnore]
    private string[][] stringData;
    [MonoModIgnore]
    private string[] tables;

    public extern void orig_LoadTables();
    // new as we're hiding TextManager's LoadTables.
    public new void LoadTables() {
        orig_LoadTables();

        /*
        for (int i = 0; i < stringData.Length; i++) {
            string[] strings = stringData[i];
            if (strings == null) // Who knows?
                continue;
            string key = tables[i] ?? "null";
            for (int j = 0; j < strings.Length; j++)
                strings[j] = $"{key} {{{i}, {j}}}";
        }
        */
    }

}
