#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using YLMAPI;

class patch_SavegameManager : SavegameManager {

    public extern IEnumerator orig_LoadSceneControl(string sceneName, LoadingScreenFade fadeMask);
    public new IEnumerator LoadSceneControl(string sceneName, LoadingScreenFade fadeMask)
        => ModEvents.LoadSceneControl(orig_LoadSceneControl(sceneName, fadeMask), sceneName, fadeMask);

}
