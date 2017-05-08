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
using YLMAPI.Content.OBJ;
using Ionic.Zip;

namespace YLMAPI.Content {
    public static class ModContent {

        /// <summary>
        /// Cached type references. Microoptimization to replace ldtoken and token to ref conversion call with ldfld.
        /// </summary>
        public static class Types {
            public readonly static Type Object = typeof(object);
            public readonly static Type UnityObject = typeof(UnityEngine.Object);

            public readonly static Type ModContent = typeof(ModContent);
            public readonly static Type ModContentHooks = typeof(ModContentHooks);
            public readonly static Type Resources = typeof(Resources);

            public readonly static Type AssetTypeDirectory = typeof(AssetTypeDirectory);
            public readonly static Type AssetTypeAssembly = typeof(AssetTypeAssembly);

            public readonly static Type Texture = typeof(Texture);
            public readonly static Type Texture2D = typeof(Texture2D);

            public readonly static Type Meshes = typeof(List<Mesh>);
            public readonly static Type Mesh = typeof(Mesh);

            public readonly static Type OBJData = typeof(OBJData);
            public readonly static Type OBJObject = typeof(OBJObject);
        }

        public static string ContentDirectory;
        public static string PatchesDirectory;
        public static string PatchesPrefix;
        public static string TextsDirectory;
        public static string TextsPrefix;

        public readonly static Dictionary<string, AssetMetadata> Map = new Dictionary<string, AssetMetadata>();
        public readonly static Dictionary<string, AssetMetadata> MapDirs = new Dictionary<string, AssetMetadata>();

        public readonly static Dictionary<string, object> Cache = new Dictionary<string, object>();
        public readonly static HashSet<Type> CacheableTypes = new HashSet<Type>() {
            Types.Texture,
            Types.Texture2D,
            Types.Meshes,
            Types.Mesh,
            Types.OBJData,
            Types.OBJObject
        };

        public readonly static List<ContentModMetadata> Mods = new List<ContentModMetadata>();

        public static bool IsInitialized { get; internal set; }

        static ModContent() {
            Init();
        }
        public static void Init() {
            if (IsInitialized)
                return;
            IsInitialized = true;

            Directory.CreateDirectory(ContentDirectory = Path.Combine(ModAPI.GameDirectory, "Content"));

            Directory.CreateDirectory(PatchesDirectory = Path.Combine(ContentDirectory, PatchesPrefix = "Patches"));
            PatchesPrefix += "/";

            Directory.CreateDirectory(TextsDirectory = Path.Combine(ContentDirectory, TextsPrefix = "Texts"));
            TextsPrefix += "/";

            Crawl(null, Assembly.GetExecutingAssembly());
            Crawl(null, ContentDirectory);

            OBJParser.StreamProvider = (s, type, path) => new StreamReader(GetMapped(path)?.Stream);

            ModContentHooks.Init();
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
                path = ParseType(path, out metadata.AssetType, out metadata.AssetFormat);
            if (metadata.AssetType == Types.AssetTypeDirectory)
                return MapDirs[path] = metadata;

            return Map[path] = metadata;
        }

        public static string ParseType(string file, out Type type, out string format) {
            type = Types.Object;
            format = file.Length < 4 ? null : file.Substring(file.Length - 3);

            if (file.EndsWith(".dll")) {
                type = Types.AssetTypeAssembly;

            } else if (file.EndsWith(".png")) {
                type = Types.Texture2D;
                file = file.Substring(0, file.Length - 4);
            } else if (file.EndsWith(".obj")) {
                type = Types.Meshes;
                file = file.Substring(0, file.Length - 4);
            }

            // TODO: Check for .patch.*, handle patches separately.

            return file;
        }

        public static void Recrawl() {
            Cache.Clear();

            Map.Clear();
            MapDirs.Clear();

            for (int i = 0; i < Mods.Count; i++)
                Crawl(Mods[i]);
        }

        public static void Crawl(ContentModMetadata meta) {
            if (meta.Directory != null)
                Crawl(meta, meta.Directory);
            else if (meta.Archive != null)
                using (ZipFile zip = ZipFile.Read(meta.Archive))
                    Crawl(meta, meta.Archive, zip);
            else if (meta.Assembly != null)
                Crawl(meta, meta.Assembly);
        }

        public static void Crawl(ContentModMetadata meta, string dir, string root = null) {
            if (meta == null)
                Mods.Add(meta = new ContentModMetadata() {
                    Directory = dir
                });

            if (Path.GetFileName(dir).StartsWith("DUMP"))
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
                    AssetType = Types.AssetTypeDirectory,
                    HasData = false
                });
                Crawl(meta, file, root);
            }
        }

        public static void Crawl(ContentModMetadata meta, Assembly asm) {
            if (meta == null)
                Mods.Add(meta = new ContentModMetadata() {
                    Assembly = asm
                });

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

        public static void Crawl(ContentModMetadata meta, string archive, ZipFile zip) {
            if (meta == null)
                Mods.Add(meta = new ContentModMetadata() {
                    Archive = archive
                });

            foreach (ZipEntry entry in zip.Entries) {
                string entryName = entry.FileName.Replace("\\", "/");
                AddMapping(entryName, new AssetMetadata(archive, entryName) {
                    AssetType = entry.IsDirectory ? typeof(AssetTypeDirectory) : null
                });
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

            if ((type == Types.Texture || type == Types.Texture2D) &&
                metadata.AssetType == Types.Texture2D) {
                Texture2D tex = new Texture2D(2, 2);
                tex.name = Path.GetFileName(path);
                tex.LoadImage(metadata.Data);
                return tex;
            }

            if (metadata.AssetType == Types.Meshes &&
                metadata.AssetFormat == "obj") {
                OBJData data = OBJParser.ParseOBJ(path);

                if (type == Types.OBJData)
                    return data;
                else if (type == Types.OBJObject)
                    return data.Objects.Count == 0 ? null : data.Objects[0];

                else if (type == Types.Meshes)
                    return data.ToMeshes();
                else
                    return data.Objects.Count == 0 ? null : data.Objects[0].ToMesh();
            }

            NoMetadata:
            return null;
        }

    }

    public class ContentModMetadata {

        /// <summary>
        /// The path to the ZIP of the mod, if this is a .zip mod.
        /// </summary>
        public virtual string Archive { get; set; }

        /// <summary>
        /// The path to the directory of the mod, if this is a directory mod.
        /// </summary>
        public virtual string Directory { get; set; }

        /// <summary>
        /// The assembly containing the resources, if the source is an assembly.
        /// </summary>
        public virtual Assembly Assembly { get; set; }

    }
}
