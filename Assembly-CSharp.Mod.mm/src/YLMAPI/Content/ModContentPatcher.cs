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
using MonoMod.Detour;

namespace YLMAPI.Content {
    public static class ModContentPatcher {

        public static bool IsInitialized { get; internal set; }

        static ModContentPatcher() {
            Init();
        }
        public static void Init() {
            if (IsInitialized)
                return;

            ModEvents.OnTextsLoaded += OnTextsLoaded;
            ModEvents.OnLoadSceneControl += OnLoadSceneControl;
            ModEvents.OnProcessScene += OnProcessScene;
        }

        public static void OnTextsLoaded(TextManager tm, string[] tables, string[][] stringData) {
            for (int i = 0; i < stringData.Length; i++) {
                string[] strings = stringData[i];
                if (strings == null) // Who knows?
                    continue;
                string key = tables[i] ?? $"texts_{i}";

                string file = Path.Combine(ModContent.TextsDirectory, tm.GetLocale());
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
        }

        public static IEnumerator OnLoadSceneControl(IEnumerator loader, string sceneName, LoadingScreenFade fadeMask) {
            while (loader.MoveNext()) {
                object current = loader.Current;
                if (current == Yielders.EndOfFrame)
                    yield return ModEvents.ProcessScene(SceneManager.GetSceneByName(sceneName)) ?? current;
                yield return current;
            }
        }

        public static IEnumerator OnProcessScene(IEnumerator loader, Scene scene) {
            if (loader != null)
                yield return loader;
            ModLogger.Log("main", $"OnProcessScene: {scene.name}");
            // yield return ModContentDumper.DumpContent(scene);
            // PatchContent(scene);
        }

        public static void PatchContent(Scene scene) {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
                PatchContent(roots[i].transform);
        }

        public static void PatchContent(Transform t) {
            string prefix = t.name.Trim();
            int prefixTrimIndex = prefix.IndexOf(' ');
            if (prefixTrimIndex != -1)
                prefix = prefix.Substring(0, prefixTrimIndex);
            prefix = prefix + "/";

            Component[] components = t.GetComponents<Component>();
            for (int ci = 0; ci < components.Length; ci++)
                PatchContent(components[ci], prefix);

            int children = t.childCount;
            for (int ci = 0; ci < children; ci++)
                PatchContent(t.GetChild(ci));
        }

        public static void PatchContent(Component c, string prefix = "") {
            prefix = $"{prefix}{c.GetType().Name}";

            if (c is Renderer)
                PatchContent(c, ((Renderer) c).sharedMaterials, prefix);
        }

        public static void PatchContent(Component c, Material[] materials, string prefix = "") {
            for (int mi = 0; mi < materials.Length; mi++)
                PatchContent(c, materials[mi], $"{prefix}_");
        }

        public static void PatchContent(Component c, Material material, string prefix = "") {
            if (material == null)
                return;

            prefix = $"{prefix}{material.name}";

            Texture2D tex = material.mainTexture as Texture2D;
            if (tex != null) {
                string path = prefix;
                if (material.name != tex.name)
                    path = $"{path}.{tex.name.EmptyToNull() ?? "main"}";
                path = path + ".png";
                PatchContent(ref tex, path);
                material.mainTexture = tex;
            }
        }

        public static void PatchContent(ref Texture2D tex, string path) {
            if (tex == null)
                return;
            tex = tex.GetRW();

            path = Path.Combine(ModContent.PatchesDirectory, path.NormalizePath());
            if (!File.Exists(path))
                return;
        }

    }
}
