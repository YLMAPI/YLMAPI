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
    public static class ModContentDumper {

        public static string DumpDirectory = Path.Combine(ModContent.ContentDirectory, "DUMP");

        private static int _DumpedThisFrame = 0;
        private const int _DumpsPerFrame = 32;
        private const int _MaxDumpsPerFrame = 512;

        private const string _DumpingUIFormat = "Currently dumping {1} of {2}: {0}";
        private static SGroup _DumpingUI = new SGroup() {
            OnUpdateStyle = elem => {
                elem.Fill(0);
                elem.Position.y = elem.Size.y - 32;
                elem.Size.y = 32;
            },
            Children = {
                new SPreloader() {
                    Count = new Vector2(4, 4),
                    Padding = new Vector2(1, 1),
                    OnUpdateStyle = elem => {
                        elem.Position = new Vector2(8, 8);
                        elem.Size = elem.InnerSize;
                    }
                },
                new SLabel() {
                    Alignment = TextAnchor.MiddleLeft,
                    OnUpdateStyle = elem => {
                        elem.Position.x = elem.Previous.Position.x + 4 + elem.Previous.Position.y;
                        elem.Position.y = 0;
                        elem.Size = elem.Parent.Size - elem.Position;
                        elem.Size.y -= 8;
                    }
                },
                new SImage(SGUIRoot.White, Color.white) {
                    OnUpdateStyle = elem => {
                        elem.Size.y = 8;
                        elem.Position = new Vector2(0, elem.Parent.Size.y - elem.Size.y);
                    }
                }
            }
        };
        private static SLabel _DumpingUILabel = (SLabel) _DumpingUI[1];
        private static SImage _DumpingUIBar = (SImage) _DumpingUI[2];

        public static IEnumerator DumpContent(Scene scene) {
            ModLogger.Log("dump", $"Dumping scene: {scene.name}");
            Scene scenePrev = SceneManager.GetActiveScene();
            if (scenePrev != scene) {
                SceneManager.SetActiveScene(scene);
                yield return null;
            }
            GameObject[] objs = Resources.FindObjectsOfTypeAll<GameObject>();
            if (scenePrev != scene) {
                SceneManager.SetActiveScene(scenePrev);
                yield return null;
            }
            _DumpingUILabel.Text = string.Format(_DumpingUIFormat, "", 0, objs.Length);
            _DumpingUIBar.Size.x = 0;
            _DumpingUI.Visible = true;
            for (int i = 0; i < objs.Length; i++)
                if (objs[i] != null) {
                    if (objs[i].scene != scene)
                        continue;
                    if (DumpContent(objs[i].transform))
                        _DumpedThisFrame++;
                    if (_DumpedThisFrame >= _DumpsPerFrame || i % _MaxDumpsPerFrame == 0) {
                        _DumpedThisFrame = 0;
                        _DumpingUILabel.Text = string.Format(_DumpingUIFormat, objs[i].name, i, objs.Length);
                        _DumpingUIBar.Size.x = ((float) i / objs.Length) * _DumpingUI.Size.x;
                        yield return null;
                    }
                }
            _DumpingUI.Visible = false;
        }

        public static bool DumpContent(Transform t) {
            if (t == null)
                return false;

            bool dumped = false;

            string prefix = t.name.Trim();
            int prefixTrimIndex = prefix.IndexOf(' ');
            if (prefixTrimIndex != -1)
                prefix = prefix.Substring(0, prefixTrimIndex);
            prefix = prefix + "/";

            Component[] components = t.GetComponents<Component>();
            for (int ci = 0; ci < components.Length; ci++)
                dumped |= DumpContent(components[ci], prefix);

            return dumped;
        }

        public static bool DumpContent(Component c, string prefix = "") {
            prefix = $"{prefix}{c.GetType().Name}.";

            if (c is Renderer)
                return DumpContent(c, ((Renderer) c).sharedMaterials, prefix);

            return false;
        }

        public static bool DumpContent(Component c, Material[] materials, string prefix = "") {
            bool dumped = false;
            for (int mi = 0; mi < materials.Length; mi++)
                dumped |= DumpContent(c, materials[mi], prefix);
            return dumped;
        }

        public static bool DumpContent(Component c, Material material, string prefix = "") {
            if (material == null)
                return false;
            bool dumped = false;

            prefix = $"{prefix}{material.name}";

            Texture2D tex = material.mainTexture as Texture2D;
            if (tex != null) {
                string suffix = "";
                if (material.name != tex.name)
                    suffix = tex.name.EmptyToNull() ?? ".main";
                if (suffix.StartsWith(material.name))
                    suffix = suffix.Substring(material.name.Length);
                dumped |= DumpContent(c, tex, prefix + suffix);
            }

            return dumped;
        }

        public static bool DumpContent(Component c, Texture2D tex, string path) {
            if (tex == null)
                return false;

            if (!string.IsNullOrEmpty(tex.name))
                path = Path.Combine("Textures", tex.name);
            path = Path.Combine(DumpDirectory, path.NormalizePath() + ".png");
            if (File.Exists(path))
                return false;

            bool copied = !tex.IsReadable();
            if (copied)
                tex = tex.GetRW();

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, tex.EncodeToPNG());

            if (copied)
                UnityEngine.Object.DestroyImmediate(tex);

            return true;
        }

    }
}
