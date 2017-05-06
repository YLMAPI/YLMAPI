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
    public static class ModContent {

        public static string ContentDirectory;
        public static string PatchesDirectory;
        public static string TextsDirectory;

        public readonly static Dictionary<string, AssetMetadata> Map = new Dictionary<string, AssetMetadata>();
        public readonly static Dictionary<string, AssetMetadata> MapDirs = new Dictionary<string, AssetMetadata>();

        public readonly static Dictionary<string, object> Cache = new Dictionary<string, object>();
        public readonly static HashSet<Type> CacheableTypes = new HashSet<Type>() {
            typeof(Texture2D)
        };

        public static bool IsInitialized { get; internal set; }

        static ModContent() {
            Init();
        }
        public static void Init() {
            if (IsInitialized)
                return;
            IsInitialized = true;

            Directory.CreateDirectory(ContentDirectory = Path.Combine(ModAPI.GameDirectory, "Content"));
            Directory.CreateDirectory(PatchesDirectory = Path.Combine(ContentDirectory, "Patches"));
            Directory.CreateDirectory(TextsDirectory = Path.Combine(ContentDirectory, "Texts"));

            Crawl(Assembly.GetExecutingAssembly());
            Crawl(ContentDirectory);

            MethodInfo m_Resources_Load = typeof(Resources).GetMethod("Load", new Type[] { typeof(string), typeof(Type) });
            m_Resources_Load.Detour(typeof(ModContent).GetMethod("LoadHook"));
            typeof(ModContent).GetMethod("trampoline_LoadHook").Detour(m_Resources_Load.CreateOrigTrampoline());

            ModContentPatcher.Init();
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
            if (metadata.AssetType == typeof(AssetTypeDirectory))
                return MapDirs[path] = metadata;

            return Map[path] = metadata;
        }

        public static string RemoveExtension(string file, out Type type) {
            type = typeof(object);

            if (file.EndsWith(".dll")) {
                type = typeof(AssetTypeAssembly);

            } else if (file.EndsWith(".png")) {
                type = typeof(Texture2D);
                file = file.Substring(0, file.Length - 4);
            }

            return file;
        }

        public static void Crawl(string dir, string root = null) {
            if (Path.GetDirectoryName(dir).StartsWith("DUMP"))
                return;
            if (root == null)
                root = dir;
            string[] files = Directory.GetFiles(dir);
            for (int i = 0; i < files.Length; i++) {
                string file = files[i];
                AddMapping(file.Substring((root?.Length ?? 0) + 1), new AssetMetadata(file));
            }
            files = Directory.GetDirectories(dir);
            for (int i = 0; i < files.Length; i++) {
                string file = files[i];
                AddMapping(file.Substring((root?.Length ?? 0) + 1), new AssetMetadata(file) {
                    AssetType = typeof(AssetTypeDirectory),
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

        internal delegate UnityEngine.Object d_LoadHook(string path, Type type);
        public static UnityEngine.Object trampoline_LoadHook(string path, Type type) { return null; }
        public static UnityEngine.Object LoadHook(string path, Type type) {
            object value = Load(path, type);
            if (value == null)
                return trampoline_LoadHook(path, type);
            if (value is UnityEngine.Object)
                return (UnityEngine.Object) value;
            return new ModContentWrapper(value, type);
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
