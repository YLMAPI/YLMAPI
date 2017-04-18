#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MonoMod;

class patch_CameraManager : CameraManager {

    // There's no orig Awake / Start

    private void Start() {
        
    }

    public extern void orig_Update();
    public void Update() {
        orig_Update();
    }

}
