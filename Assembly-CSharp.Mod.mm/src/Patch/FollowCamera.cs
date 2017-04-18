#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MonoMod;

class patch_FollowCamera : FollowCamera {

    public extern void orig_updateFocus();
    private void updateFocus() {
        if (YLModFreeCamera.IsEnabled)
            return;

        orig_updateFocus();
    }

    public extern void orig_updatePlayerInfo();
    private void updatePlayerInfo() {
        if (YLModFreeCamera.IsEnabled)
            return;

        orig_updatePlayerInfo();
    }

}
