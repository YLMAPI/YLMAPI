using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class patch_TextManager : TextManager {

    private string GetLocale() {
        if (TextManagerHelper.LocaleOverride != null)
            return TextManagerHelper.LocaleOverride;
        SystemManager systemManager = SystemManager.instance;
        SystemLanguage systemLanguage = SystemLanguage.German;
        for (int i = 0; i < languageSetup.data.Length; i++)
            if (languageSetup.data[i].language == systemLanguage)
                return languageSetup.data[i].locale;
        return "en_gb";
    }

}
