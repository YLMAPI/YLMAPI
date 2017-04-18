using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;
using SGUI;
using Rewired;
using UEInput = UnityEngine.Input;
using System.IO;
using System.Reflection;

public static partial class YLMod {

    public readonly static Version BaseVersion = new Version(0, 3, 0);
    // The following line will be replaced by Travis.
    public readonly static int BaseTravisBuild = 0;
    /// <summary>
    /// Base version profile, used separately from BaseVersion.
    /// A higher profile ID means higher instability ("developerness").
    /// </summary>
    public readonly static ModProfile BaseProfile =
#if TRAVIS
        new ModProfile(2, "travis");
#elif DEBUG
        new ModProfile(1, "init-dev");
#else
        new ModProfile(0, "init"); // no tag
#endif

    public static string BaseUIVersion {
        get {
            string v = BaseVersion.ToString(3);

            if (BaseTravisBuild != 0) {
                v += "-";
                v += BaseTravisBuild;
            }

            if (!string.IsNullOrEmpty(BaseProfile.Name)) {
                v += "-";
                v += BaseProfile.Name;
            }

            return v;
        }
    }

    public static string GameDirectory;
    public static string ModsDirectory;
    public static string TextsDirectory;
    public static string ContentDirectory;

    public static Action OnUpdate;
    public static Action OnLateUpdate;

    public static bool IsInitialized;

    static YLMod() {
        EntryPoint();
    }
    public static void EntryPoint() {
        if (IsInitialized)
            return;
        IsInitialized = true;
        Console.WriteLine($"Initializing Yooka-Laylee Mod {BaseUIVersion}");

        GameDirectory = Path.GetDirectoryName(Path.GetFullPath(Application.dataPath));
        Console.WriteLine($"Game directory: {GameDirectory}");
        ModsDirectory = Path.Combine(GameDirectory, "mods");
        Directory.CreateDirectory(ModsDirectory);
        TextsDirectory = Path.Combine(ModsDirectory, "texts");
        Directory.CreateDirectory(TextsDirectory);
        ContentDirectory = Path.Combine(ModsDirectory, "content");
        Directory.CreateDirectory(ContentDirectory);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        OnLateUpdate += YLModInput.LateUpdate;

        YLModBehaviour ylmb = YLModBehaviour.instance;

        YLModGUI.Init();

        YLModFreeCamera.Init();
    }

    public static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        YLMod.Log("main", $"Loaded scene: {scene.name}");
        // scene.OnLoadFinished(s => Console.WriteLine(s.DumpHierarchy(new StringBuilder()).ToString()));
    }

    public static void OnSceneUnloaded(Scene scene) {
        YLMod.Log("main", $"Unloaded scene: {scene.name}");
    }

    public static void Log(string tag, string str) {
        Console.Write("[YLMod] [");
        Console.Write(tag);
        Console.Write("] ");
        Console.WriteLine(str);

        if (YLModGUI.Root == null)
            YLModGUI.Init();
        YLModGUI.LogGroup.Children.Add(
            new SLabel($"[{tag}] {str}") {
                With = { new SFadeInAnimation() }
            }
        );

        YLModGUI.LogGroup.ScrollPosition = new Vector2(0f, float.MaxValue);
    }

}
