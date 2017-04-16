#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MonoMod;

class patch_PauseMainScreenController : PauseMainScreenController {

    public extern void orig_Show(bool isNavigationForward);
    public override void Show(bool isNavigationForward) {
        orig_Show(isNavigationForward);

        // transform.LogHierarchy();

        // transform.SetText("Continue", "Skip Cutscene");
    }

    public extern void orig_OnContinueSelected();
    public new void OnContinueSelected() {
        // This also gets triggered when escaping out of the menu (pressing B on an XBOX controller).
        orig_OnContinueSelected();
    }

}
