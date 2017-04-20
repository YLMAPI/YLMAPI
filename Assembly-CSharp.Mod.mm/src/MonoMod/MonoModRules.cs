using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MonoMod;
using MonoMod.InlineRT;

namespace MonoMod {
    /// <summary>
    /// This single class gets stripped out and executed by MonoMod when injecting.
    /// It allows mod-time configuration.
    /// </summary>
    internal static class MonoModRules {
        
        static MonoModRules() {
            // YLMAPI is accessing MonoMod. Tell MonoMod not to remove any MonoMod references.
            MMILRT.Modder.CleanupEnabled = false;
        }

    }
}
