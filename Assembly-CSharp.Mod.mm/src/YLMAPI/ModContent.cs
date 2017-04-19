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

namespace YLMAPI {
    public static partial class ModContent {

        public class AssetDirectory { private AssetDirectory() { } }

        public readonly static Dictionary<string, AssetMetadata> Map = new Dictionary<string, AssetMetadata>();
        public readonly static Dictionary<string, AssetMetadata> MapDirs = new Dictionary<string, AssetMetadata>();

        public readonly static Dictionary<string, object> Cache = new Dictionary<string, object>();
        public readonly static HashSet<Type> CacheableTypes = new HashSet<Type>() {
            typeof(Texture2D)
        };

        static ModContent() {
            Crawl(Assembly.GetExecutingAssembly());
            Crawl(ModAPI.ContentDirectory);
            ModEvents.OnTextsLoaded += (tm, tables, stringData) => {
                for (int i = 0; i < stringData.Length; i++) {
                    string[] strings = stringData[i];
                    if (strings == null) // Who knows?
                        continue;
                    string key = tables[i] ?? $"texts_{i}";

                    string file = Path.Combine(ModAPI.TextsDirectory, tm.GetLocale());
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
        }

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
            object obj;
            if (Cache.TryGetValue(path, out obj))
                return obj;

            obj = LoadUncached(path, type);

            if (CacheableTypes.Contains(type))
                Cache[path] = obj;
            return obj;
        }

        public static T LoadUncached<T>(string path)
            => (T) LoadUncached(path, typeof(T));
        public static T LoadUncached<T>(string path, Action<T> modifier) {
            T t = (T) LoadUncached(path, typeof(T));
            modifier(t);
            return t;
        }
        public static object LoadUncached(string path, Type type) {
            AssetMetadata metadata;
            TryGetMapped(path, out metadata, true);

            if (metadata == null)
                goto NoMetadata;

            if (metadata.AssetType == typeof(Texture2D)) {
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
