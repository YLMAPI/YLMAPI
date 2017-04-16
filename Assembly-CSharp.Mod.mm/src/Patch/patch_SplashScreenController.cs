#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MonoMod;

class patch_SplashScreenController : SplashScreenController {

    public extern void orig_Awake();
    public new void Awake() {
        YLMod.EntryPoint();

        orig_Awake();
    }

}
