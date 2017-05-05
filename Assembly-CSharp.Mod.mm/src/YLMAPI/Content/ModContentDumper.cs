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
        private const int _DumpsPerFrame = 4;

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

        public static IEnumerator DumpContent() {
            Scene scene = SceneManager.GetActiveScene();
            ModLogger.Log("dump", $"Dumping scene: {scene.name}");
            GameObject[] objs = UnityEngine.Object.FindObjectsOfType<GameObject>();
            _DumpingUILabel.Text = string.Format(_DumpingUIFormat, "", 0, objs.Length);
            _DumpingUIBar.Size.x = 0;
            _DumpingUI.Visible = true;
            for (int i = 0; i < objs.Length; i++)
                if (objs[i] != null) {
                    DumpContent(objs[i].transform);
                    _DumpedThisFrame++;
                    if (_DumpedThisFrame >= _DumpsPerFrame) {
                        _DumpedThisFrame = 0;
                        _DumpingUILabel.Text = string.Format(_DumpingUIFormat, objs[i].name, i, objs.Length);
                        _DumpingUIBar.Size.x = ((float) i / objs.Length) * _DumpingUI.Size.x;
                        yield return null;
                    }
                }
            _DumpingUI.Visible = false;
        }

        public static void DumpContent(Transform t) {
            if (t == null)
                return;

            string prefix = t.name.Trim();
            int prefixTrimIndex = prefix.IndexOf(' ');
            if (prefixTrimIndex != -1)
                prefix = prefix.Substring(0, prefixTrimIndex);
            prefix = prefix + "/";

            Component[] components = t.GetComponents<Component>();
            for (int ci = 0; ci < components.Length; ci++)
                DumpContent(components[ci], prefix);
        }

        public static void DumpContent(Component c, string prefix = "") {
            prefix = $"{prefix}{c.GetType().Name}.";

            if (c is Renderer)
                DumpContent(c, ((Renderer) c).sharedMaterials, prefix);
        }

        public static void DumpContent(Component c, Material[] materials, string prefix = "") {
            for (int mi = 0; mi < materials.Length; mi++)
                DumpContent(c, materials[mi], prefix);
        }

        public static void DumpContent(Component c, Material material, string prefix = "") {
            if (material == null)
                return;

            prefix = $"{prefix}{material.name}";

            Texture2D tex = material.mainTexture as Texture2D;
            if (tex != null) {
                string suffix = "";
                if (material.name != tex.name)
                    suffix = tex.name.EmptyToNull() ?? ".main";
                if (suffix.StartsWith(material.name))
                    suffix = suffix.Substring(material.name.Length);
                DumpContent(tex, prefix + suffix + ".png");
            }
        }

        public static void DumpContent(Texture2D tex, string path) {
            if (tex == null)
                return;
            tex = tex.GetRW();
            // TODO: This may break future loading.
            if (!string.IsNullOrEmpty(tex.name))
                path = Path.Combine("Textures", tex.name + ".png");
            path = Path.Combine(DumpDirectory, path.NormalizePath());
            if (File.Exists(path))
                return;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, tex.EncodeToPNG());
        }

    }
}
