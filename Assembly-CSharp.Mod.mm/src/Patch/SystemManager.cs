#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MonoMod;

class patch_InputButton : InputButton {

    public extern void orig_Update(Player rewiredPlayer, int input, bool isEnabled);
    public void Update(Player rewiredPlayer, int input, bool isEnabled) {
        if (YLModFreeCamera.IsEnabled)
            return;
        orig_Update(rewiredPlayer, input, isEnabled);
    }

}
