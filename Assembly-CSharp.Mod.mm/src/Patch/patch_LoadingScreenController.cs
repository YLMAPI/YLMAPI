#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MonoMod;

class patch_LoadingScreenController : LoadingScreenController {

    public static extern void orig_LoadNextScene(string sceneName, LoadingScreenFade fadeMask);
    public static void LoadNextScene(string sceneName, LoadingScreenFade fadeMask) {
        YLMod.Log($"Entering loading screen for scene: {sceneName}");

        orig_LoadNextScene(sceneName, fadeMask);
    }

}
