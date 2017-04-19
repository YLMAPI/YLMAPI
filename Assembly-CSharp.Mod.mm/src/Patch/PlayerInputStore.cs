#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MonoMod;
using YLMAPI;

class patch_PlayerInputStore : PlayerInputStore {

    public extern void orig_MyUpdate();
    public new void MyUpdate() {
        if (ModEvents.PlayerInputUpdate(this))
            orig_MyUpdate();
    }

}
