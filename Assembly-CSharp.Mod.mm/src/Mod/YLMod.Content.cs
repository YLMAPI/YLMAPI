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

    public static class Content {

        public class AssetDirectory { private AssetDirectory() { } }

        public static Action<TextManager, string[], string[][]> OnTextLoad;

        public readonly static Dictionary<string, AssetMetadata> Map = new Dictionary<string, AssetMetadata>();
        public readonly static Dictionary<string, AssetMetadata> MapDirs = new Dictionary<string, AssetMetadata>();

        public static bool TryGetMapped(string path, out AssetMetadata metadata, bool includeDirs = false) {
            if (includeDirs) {
                if (MapDirs.TryGetValue(path, out metadata)) return true;
                if (MapDirs.TryGetValue(path.ToLowerInvariant(), out metadata)) return true;
            }
            if (Map.TryGetValue(path, out metadata)) return true;
            if (Map.TryGetValue(path.ToLowerInvariant(), out metadata)) return true;

            return false;
        }
        public static AssetMetadata GetMapped(string path) {
            AssetMetadata metadata;
            TryGetMapped(path, out metadata);
            return metadata;
        }

        public static AssetMetadata AddMapping(string path, AssetMetadata metadata) {
            path = path.Replace('\\', '/');
            if (metadata.AssetType == null)
                path = RemoveExtension(path, out metadata.AssetType);
            if (metadata.AssetType == typeof(AssetDirectory))
                return MapDirs[path] = metadata;

            YLMod.Log("content", $"Mapping content: {path} ({metadata.AssetType?.ToString() ?? "Unknown"}, {metadata.Container})");

            return Map[path] = metadata;
        }

        public static string RemoveExtension(string file, out Type type) {
            type = typeof(object);

            if (file.EndsWith(".png")) {
                type = typeof(Texture2D);
                file = file.Substring(0, file.Length - 4);
            }

            return file;
        }

        public static void Crawl(string dir, string root = null) {
            if (Path.GetDirectoryName(dir).StartsWith("DUMP")) return;
            if (root == null) root = dir;
            string[] files = Directory.GetFiles(dir);
            for (int i = 0; i < files.Length; i++) {
                string file = files[i];
                AddMapping(file.Substring((root?.Length ?? 0) + 1), new AssetMetadata(file));
            }
            files = Directory.GetDirectories(dir);
            for (int i = 0; i < files.Length; i++) {
                string file = files[i];
                AddMapping(file.Substring((root?.Length ?? 0) + 1), new AssetMetadata(file) {
                    AssetType = typeof(AssetDirectory),
                    HasData = false
                });
                Crawl(file, root);
            }
        }

        public static void Crawl(Assembly asm) {
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; i++) {
                string name = resourceNames[i];
                int indexOfContent = name.IndexOf("Content");
                if (indexOfContent < 0)
                    continue;
                name = name.Substring(indexOfContent + 8);
                AddMapping(name, new AssetMetadata(asm, resourceNames[i]));
            }
        }


        public static T Load<T>(string path)
            => (T) Load(path, typeof(T));
        public static T Load<T>(string path, Action<T> modifier) {
            T t = (T) Load(path, typeof(T));
            modifier(t);
            return t;
        }
        public static object Load(string path, Type type) {
            AssetMetadata metadata;
            TryGetMapped(path, out metadata, true);

            // YLMod.Log("content-debug", $"Trying to load content: {path} ({type?.ToString() ?? "Unknown"})");

            if (metadata == null)
                goto NoMetadata;

            // YLMod.Log("content-debug", $"Found content: {metadata.AssetType?.ToString() ?? "Unknown"}, {metadata.Container}");

            if (metadata.AssetType == typeof(Texture2D)) {
                // YLMod.Log("content-debug", "Loading Texture2D");
                Texture2D tex = new Texture2D(2, 2);
                tex.name = Path.GetFileName(path);
                tex.LoadImage(metadata.Data);
                return tex;
            }

            NoMetadata:
            return null;
        }


    }

}
