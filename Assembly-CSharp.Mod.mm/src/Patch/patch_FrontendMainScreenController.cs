#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MonoMod;

class patch_FrontendMainScreenController : FrontendMainScreenController {

    public bool IsModInitialized;

    private extern void orig_Awake();
    private void Awake() {
        orig_Awake();

        ModInit();
    }

    public void ModInit() {
        if (IsModInitialized)
            return;
        IsModInitialized = true;

        // Console.WriteLine(transform.DumpHierarchy(new StringBuilder()).ToString());

        YLModMenuExt.BaseButton = transform.GetComponentInChildren<UiMenuItemController>();

    }

}
