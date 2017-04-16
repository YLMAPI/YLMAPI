#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MonoMod;
using Rewired;

class patch_InputStickXZ : InputStickXZ {

    public extern void orig_UpdateWRTCamera(Player rewiredPlayer, int horizontal, int vertical);
    public new void UpdateWRTCamera(Player rewiredPlayer, int horizontal, int vertical) {
        if (YLModFreeCamera.IsEnabled)
            return;
        orig_UpdateWRTCamera(rewiredPlayer, horizontal, vertical);
    }

}
