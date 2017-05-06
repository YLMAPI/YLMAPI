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

        private static int _PatchedThisFrame = 0;
        private const int _PatchesPerFrame = 16;
        private const int _MaxPatchesPerFrame = 64;

        public static bool IsInitialized { get; internal set; }

        static ModContentPatcher() {
            Init();
        }
        public static void Init() {
            if (IsInitialized)
                return;
            IsInitialized = true;

            ModEvents.OnTextsLoaded += OnTextsLoaded;
            ModEvents.OnLoadSceneControl += OnLoadSceneControl;
            ModEvents.OnProcessScene += OnProcessScene;
        }

        public static void OnTextsLoaded(TextManager tm, string[] tables, string[][] stringData) {
            for (int i = 0; i < stringData.Length; i++) {
                string[] strings = stringData[i];
                if (strings == null)
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

        public static IEnumerator OnLoadSceneControl(IEnumerator loader, string sceneName, LoadingScreenFade fadeMask)
            => new SceneLoadWrapper(loader, sceneName, ModEvents.ProcessScene);

        public static IEnumerator OnProcessScene(IEnumerator loader, Scene scene) {
            // The loading screen is so finetuned, adding just one yield return null causes the lighting to break!
            // We need to work around this.

            SceneFreezeInfo freeze = scene.Freeze();

            Scene scenePrev = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(scene);
            yield return null;
            SceneManager.SetActiveScene(scenePrev);

            yield return PatchContent(scene);

            freeze.Unfreeze();
            
            yield return loader;
        }

        public static IEnumerator PatchContent(Scene scene) {
            yield return ModContentDumper.DumpContent(scene);
            yield break;

            ModLogger.Log("content", $"Patching scene content: {scene.name}");
            Scene scenePrev = SceneManager.GetActiveScene();
            if (scenePrev != scene)
                SceneManager.SetActiveScene(scene);
            GameObject[] objs = UnityEngine.Object.FindObjectsOfType<GameObject>();
            if (scenePrev != scene)
                SceneManager.SetActiveScene(scenePrev);
            for (int i = 0; i < objs.Length; i++)
                if (objs[i] != null) {
                    if (PatchContent(objs[i].transform))
                        _PatchedThisFrame++;
                    if (_PatchedThisFrame >= _PatchesPerFrame || i % _MaxPatchesPerFrame == 0) {
                        _PatchedThisFrame = 0;
                        yield return null;
                    }
                }
        }

        public static bool PatchContent(Transform t) {
            if (t == null)
                return false;

            bool patched = false;

            string prefix = t.name.Trim();
            int prefixTrimIndex = prefix.IndexOf(' ');
            if (prefixTrimIndex != -1)
                prefix = prefix.Substring(0, prefixTrimIndex);
            prefix = prefix + "/";

            Component[] components = t.GetComponents<Component>();
            for (int ci = 0; ci < components.Length; ci++)
                patched |= PatchContent(components[ci], prefix);

            return patched;
        }

        public static bool PatchContent(Component c, string prefix = "") {
            prefix = $"{prefix}{c.GetType().Name}";

            if (c is Renderer)
                return PatchContent(c, ((Renderer) c).sharedMaterials, prefix);

            return false;
        }

        public static bool PatchContent(Component c, Material[] materials, string prefix = "") {
            bool patched = false;
            for (int mi = 0; mi < materials.Length; mi++)
                patched |= PatchContent(c, materials[mi], prefix);
            return patched;
        }

        public static bool PatchContent(Component c, Material material, string prefix = "") {
            if (material == null)
                return false;
            bool patched = false;

            prefix = $"{prefix}{material.name}";

            Texture2D tex = material.mainTexture as Texture2D;
            if (tex != null) {
                string suffix = "";
                if (material.name != tex.name)
                    suffix = tex.name.EmptyToNull() ?? ".main";
                if (suffix.StartsWith(material.name))
                    suffix = suffix.Substring(material.name.Length);
                patched |= PatchContent(ref tex, prefix + suffix);
                material.mainTexture = tex;
            }

            return patched;
        }

        public static bool PatchContent(ref Texture2D tex, string path) {
            if (tex == null)
                return false;
            if (!string.IsNullOrEmpty(tex.name))
                path = Path.Combine("Textures", tex.name);
            path = Path.Combine(ModContent.PatchesDirectory, path.NormalizePath() + ".png");
            if (!File.Exists(path))
                return false;

            bool copied = !tex.IsReadable();
            if (copied)
                tex = tex.GetRW();

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, tex.EncodeToPNG());

            return true;
        }

    }
}
