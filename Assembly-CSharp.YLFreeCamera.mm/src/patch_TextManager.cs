#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod.Detour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class patch_TextManager : TextManager {

    private extern string orig_GetLocale();
    private string GetLocale() {
        if (TextManagerHelper.LocaleOverride != null)
            return TextManagerHelper.LocaleOverride;
        return orig_GetLocale();
    }

}
