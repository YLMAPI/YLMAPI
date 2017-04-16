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

public static class YLMod {

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

    public static Action OnUpdate;
    public static Action<TextManager, string[], string[][]> OnTextLoad;

    public static void EntryPoint() {
        Console.WriteLine($"Initializing Yooka-Laylee Mod {BaseUIVersion}");
        YLModBehaviour ylmb = YLModBehaviour.instance;

        GameDirectory = Path.GetDirectoryName(Path.GetFullPath(Application.dataPath));
        Console.WriteLine($"Game directory: {GameDirectory}");
        ModsDirectory = Path.Combine(GameDirectory, "mods");
        Directory.CreateDirectory(ModsDirectory);
        TextsDirectory = Path.Combine(ModsDirectory, "texts");
        Directory.CreateDirectory(TextsDirectory);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        OnTextLoad += (tm, tables, stringData) => {
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
        YLMod.Log($"Loaded scene: {scene.name}");
        // scene.OnLoadFinished(s => Console.WriteLine(s.DumpHierarchy(new StringBuilder()).ToString()));
    }

    public static void OnSceneUnloaded(Scene scene) {
        YLMod.Log($"Unloaded scene: {scene.name}");
    }

    public static void Log(string str) {
        Console.WriteLine(str);

        if (YLModGUI.Root == null)
            YLModGUI.Init();
        YLModGUI.LogGroup.Children.Add(
            new SLabel(str) {
                With = { new SFadeInAnimation() }
            }
        );

        YLModGUI.LogGroup.ScrollPosition = new Vector2(0f, float.MaxValue);
    }

    public static class Input {

        public static Dictionary<string, Func<Player, bool>> ButtonMap = new Dictionary<string, Func<Player, bool>>();
        public static bool GetButton(string button) {
            Player input = ReInput.players.GetSystemPlayer();
            if (input == null)
                return false;

            // TODO: This mapping list.
            switch (button) {
                case "Jump":
                case "A":
                    return input.GetButton(9);

                case "Fly":
                    return input.GetButton(10);

                case "Context":
                    return input.GetButton(12);

                case "TongueEdibleItem":
                case "B":
                    return input.GetButton(14);

                case "Invisibility":
                    return input.GetButton(15);

                case "BasicAttack":
                case "ShootEatenItem":
                case "X":
                    return input.GetButton(16);

                case "WheelSpin":
                    return input.GetButton(18);

                case "FartBubble":
                    return input.GetButton(19);

                case "Crouch":
                case "LT":
                case "L2":
                    return input.GetButton(20);

                case "GroundPound":
                    return input.GetButton(21);

                case "SwimUnderwater":
                    return input.GetButton(22);

                case "Wheel":
                case "RT":
                case "R2":
                    return input.GetButton(23);

                case "SonarBlastAttack":
                    return input.GetButton(27);

                case "SonarBoomAttack":
                    return input.GetButton(28);

                case "SonarShieldAttack":
                    return input.GetButton(29);

                case "Aiming":
                case "LS":
                case "L3":
                    return input.GetButton(30);

                case "EmoteHappy":
                case "DPadUp":
                    return input.GetButton(39);

                case "EmoteTaunt":
                case "DPadRight":
                    return input.GetButton(40);

                case "EmoteDisappointed":
                case "DPadDown":
                    return input.GetButton(41);

                case "EmoteAngry":
                case "DPadLeft":
                    return input.GetButton(42);

                case "SwimUnderwaterAlt":
                    return input.GetButton(51);
            }

            Func<Player, bool> f;
            if (ButtonMap.TryGetValue(button, out f) && f != null)
                return f(input);
            return false;
        }

        public static Dictionary<string, Func<Player, float>> AxisMap = new Dictionary<string, Func<Player, float>>();
        public static float GetAxis(string axis) => GetAxisRaw(axis);
        public static float GetAxisRaw(string axis) {
            Player input = ReInput.players.GetSystemPlayer();
            if (input == null)
                return 0f;

            switch (axis) {
                case "Horizontal":
                    return input.GetAxis(0) + (UEInput.GetKey(KeyCode.A) ? -1f : UEInput.GetKey(KeyCode.D) ? 1f : 0f);
                case "Vertical":
                    return input.GetAxis(1) + (UEInput.GetKey(KeyCode.S) ? -1f : UEInput.GetKey(KeyCode.W) ? 1f : 0f);
                case "Y Movement":
                    return UEInput.GetKey(KeyCode.F) ? -1f : UEInput.GetKey(KeyCode.R) ? 1f : 0f;
                case "Mouse X":
                    return input.GetAxisRaw(4) * 0.5f + input.GetAxisRaw(47) * 0.07f;
                case "Mouse Y":
                    return input.GetAxisRaw(5) * 0.5f + input.GetAxisRaw(48) * 0.07f;
            }

            Func<Player, float> f;
            if (AxisMap.TryGetValue(axis, out f) && f != null)
                return f(input);
            return 0f;
        }

    }

}
