using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class TextManagerHelper {

    public static string Locale => ((patch_TextManager) TextManager.instance).GetLocale();
    public static string LocaleOverride;

}
