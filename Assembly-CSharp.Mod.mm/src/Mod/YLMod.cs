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

    public static void EntryPoint() {
        Console.WriteLine($"Initializing Yooka-Laylee Mod {BaseUIVersion}");
        YLModBehaviour ylmb = YLModBehaviour.instance;

        GameDirectory = Path.GetDirectoryName(Path.GetFullPath(Application.dataPath));
        Console.WriteLine($"Game directory: {GameDirectory}");
        ModsDirectory = Path.Combine(GameDirectory, "mods");
        Directory.CreateDirectory(ModsDirectory);
        TextsDirectory = Path.Combine(ModsDirectory, "texts");
        Directory.CreateDirectory(TextsDirectory);
        ContentDirectory = Path.Combine(ModsDirectory, "content");
        Directory.CreateDirectory(ContentDirectory);

        YLMod.Content.Crawl(Assembly.GetExecutingAssembly());
        YLMod.Content.Crawl(ContentDirectory);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        OnLateUpdate += Input.LateUpdate;

        YLMod.Content.OnTextLoad += (tm, tables, stringData) => {
            for (int i = 0; i < stringData.Length; i++) {
                string[] strings = stringData[i];
                if (strings == null) // Who knows?
                    continue;
                string key = tables[i] ?? $"texts_{i}";

                string file = Path.Combine(YLMod.TextsDirectory, tm.GetLocale());
                Directory.CreateDirectory(file);
                file = Path.Combine(file, key + ".txt");
                if (!File.Exists(file)) {
                    using (StreamWriter writer = new StreamWriter(file))
                        for (int j = 0; j < strings.Length; j++)
                            writer.WriteLine($"{j}: {strings[j]}");
                } else {
                    int index = -1;
                    string text = "";
                    using (StreamReader reader = new StreamReader(file))
                        while (!reader.EndOfStream) {
                            string line = reader.ReadLine();
                            if (line.StartsWith("#"))
                                continue;
                            int indexOfColon = line.IndexOf(':');
                            if (indexOfColon <= 0) {
                                text += "\n" + line;
                                continue;
                            }
                            int indexOld = index;
                            if (!int.TryParse(line.Substring(0, indexOfColon), out index)) {
                                index = -1;
                                text += "\n" + line;
                                continue;
                            }
                            if (indexOld != -1)
                                strings[indexOld] = text;
                            if (indexOfColon + 2 > line.Length)
                                text = "";
                            else
                                text = line.Substring(indexOfColon + 2);
                        }
                    if (index != -1)
                        strings[index] = text;
                }
            }
        };

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
        Console.Write("[");
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
