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
using System.Linq;
using Ionic.Zip;
using MonoMod.InlineRT;
using ReflectionHelper = MonoMod.InlineRT.ReflectionHelper;

namespace YLMAPI {
    public static class ModLoader {

        // A shared object a day keeps the GC away!
        private readonly static Type[] _EmptyTypeArray = new Type[0];
        private readonly static object[] _EmptyObjectArray = new object[0];

        public readonly static List<GameMod> Mods = new List<GameMod>();
        private static List<Type> _ModuleTypes = new List<Type>();
        private static List<Dictionary<string, MethodInfo>> _ModuleMethods = new List<Dictionary<string, MethodInfo>>();

        public static string ModsDirectory;
        public static string ModsCacheDirectory;
        public static string ModsBlacklistFile;

        private static List<string> _ModsBlacklist = new List<string>();

        public static void LoadMods() {
            Directory.CreateDirectory(ModsDirectory = Path.Combine(ModAPI.GameDirectory, "Mods"));
            Directory.CreateDirectory(ModsCacheDirectory = Path.Combine(ModsDirectory, "Cache"));
            ModsBlacklistFile = Path.Combine(ModsDirectory, "blacklist.txt");

            ModLogger.Log("loader", "Loading game mods");

            if (File.Exists(ModsBlacklistFile)) {
                _ModsBlacklist = File.ReadAllLines(ModsBlacklistFile).Select(l => (l.StartsWith("#") ? "" : l).Trim()).ToList();
            } else {
                using (StreamWriter writer = File.CreateText(ModsBlacklistFile)) {
                    writer.WriteLine("# This is a blacklist file. Lines starting with # are ignored.");
                    writer.WriteLine("ExampleFolder");
                    writer.WriteLine("SomeMod.zip");
                }
            }

            string[] files = Directory.GetFiles(ModsDirectory);
            for (int i = 0; i < files.Length; i++) {
                string file = Path.GetFileName(files[i]);
                if (!file.EndsWith(".zip"))
                    continue;
                LoadMod(file);
            }
            files = Directory.GetDirectories(ModsDirectory);
            for (int i = 0; i < files.Length; i++) {
                string file = Path.GetFileName(files[i]);
                if (file == "RelinkCache")
                    continue;
                LoadMod(file);
            }

        }

        public static void LoadMod(string path) {
            if (path.EndsWith(".zip")) {
                LoadModZIP(path);
            } else if (Directory.Exists(path)) {
                // TODO: Mod dirs
                // LoadModDir(path);
            }
        }

        public static void LoadModZIP(string archive) {
            if (!File.Exists(archive)) {
                // Probably a mod in the mod directory
                archive = Path.Combine(ModsDirectory, archive);
            }
            if (!File.Exists(archive)) {
                // It just doesn't exist.
                return;
            }

            ModLogger.Log("loader", $"Loading mod .zip: {archive}");

            Assembly asm = null;

            GameModMetadata meta = null;
            using (ZipFile zip = ZipFile.Read(archive)) {
                Texture2D icon = null;
                // First read the metadata, ...
                foreach (ZipEntry entry in zip.Entries) {
                    if (entry.FileName == "metadata.txt") {
                        using (MemoryStream ms = new MemoryStream()) {
                            entry.Extract(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            using (StreamReader reader = new StreamReader(ms))
                                meta = GameModMetadata.Parse(archive, "", reader);
                        }
                        continue;
                    }
                    if (entry.FileName == "icon.png") {
                        icon = new Texture2D(2, 2);
                        icon.name = "icon";
                        using (MemoryStream ms = new MemoryStream()) {
                            entry.Extract(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            icon.LoadImage(ms.GetBuffer());
                        }
                        icon.filterMode = FilterMode.Point;
                        continue;
                    }
                }

                if (meta != null) {
                    // In case the icon appears before the metadata in the .zip
                    if (icon != null) {
                        meta.Icon = icon;
                    }

                    // ... then check if the mod runs on this profile ...
                    if (meta.ProfileID > ModAPI.Profile.Id) {
                        ModLogger.Log("loader", "Mod meant for an in-dev YLMAPI version!");
                        return;
                    }

                    // ... then check if the dependencies are loaded ...
                    foreach (GameModMetadata dep in meta.Dependencies)
                        if (!DependencyLoaded(dep)) {
                            ModLogger.Log("loader", $"Dependency {dep} of mod {meta} not loaded!");
                            return;
                        }

                    // ... then add an AssemblyResolve handler for all the .zip-ped libraries
                    AppDomain.CurrentDomain.AssemblyResolve += meta._GenerateModAssemblyResolver();
                }

                // ... then everything else
                foreach (ZipEntry entry in zip.Entries) {
                    string entryName = entry.FileName.Replace("\\", "/");
                    if (meta != null && entryName == meta.DLL) {
                        using (MemoryStream ms = new MemoryStream()) {
                            entry.Extract(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            if (meta.Prelinked) {
                                asm = Assembly.Load(ms.GetBuffer());
                            } else {
                                asm = meta.GetRelinkedAssembly(ms);
                            }
                        }
                    } else {
                        ModContent.AddMapping(entryName, new AssetMetadata(archive, entryName) {
                            AssetType = entry.IsDirectory ? typeof(ModContent.AssetDirectory) : null
                        });
                    }
                }
            }

            if (meta == null || asm == null) {
                return;
            }

            ModContent.Crawl(asm);

            Type[] types = asm.GetTypes();
            for (int i = 0; i < types.Length; i++) {
                Type type = types[i];
                if (!typeof(GameMod).IsAssignableFrom(type) || type.IsAbstract)
                    continue;

                GameMod mod = (GameMod) type.GetConstructor(_EmptyTypeArray).Invoke(_EmptyObjectArray);

                mod.Metadata = meta;

                Mods.Add(mod);
                _ModuleTypes.Add(type);
                _ModuleMethods.Add(new Dictionary<string, MethodInfo>());
            }

            ModLogger.Log("loader", $"Mod {meta.Name} initialized.");

        }


        /// <summary>
        /// Checks if an dependency is loaded.
        /// Can be used by mods manually to f.e. activate / disable functionality if an API is (not) existing.
        /// </summary>
        /// <param name="dependency">Dependency to check for. Name and Version will be checked.</param>
        /// <returns></returns>
        public static bool DependencyLoaded(GameModMetadata dep) {
            string depName = dep.Name;
            Version depVersion = dep.Version;

            if (depName == "API") {
                if (ModAPI.Version.Major != depVersion.Major)
                    return false;
                if (ModAPI.Version.Minor < depVersion.Minor)
                    return false;
                return true;
            }

            foreach (GameMod mod in Mods) {
                GameModMetadata meta = mod.Metadata;
                if (meta.Name != depName)
                    continue;
                Version version = meta.Version;
                if (version.Major != depVersion.Major)
                    return false;
                if (version.Minor < depVersion.Minor)
                    return false;
                return true;
            }

            return false;
        }

        private static ResolveEventHandler _GenerateModAssemblyResolver(this GameModMetadata meta) {
            if (!string.IsNullOrEmpty(meta.Archive)) {
                return delegate (object sender, ResolveEventArgs args) {
                    string asmName = new AssemblyName(args.Name).Name + ".dll";
                    using (ZipFile zip = ZipFile.Read(meta.Archive)) {
                        foreach (ZipEntry entry in zip.Entries) {
                            if (entry.FileName != asmName) {
                                continue;
                            }
                            using (MemoryStream ms = new MemoryStream()) {
                                entry.Extract(ms);
                                ms.Seek(0, SeekOrigin.Begin);
                                return Assembly.Load(ms.GetBuffer());
                            }
                        }
                    }
                    return null;
                };
            }
            if (!string.IsNullOrEmpty(meta.Directory)) {
                return delegate (object sender, ResolveEventArgs args) {
                    string asmPath = Path.Combine(meta.Directory, new AssemblyName(args.Name).Name + ".dll");
                    if (!File.Exists(asmPath)) {
                        return null;
                    }
                    return Assembly.LoadFrom(asmPath);
                };
            }
            return null;
        }


        /// <summary>
        /// Calls a method in every mod.
        /// </summary>
        /// <param name="methodName">Method name of the method to call.</param>
        /// <param name="args">Arguments to pass - null for none.</param>
        public static void CallInEachMod(string methodName, object[] args = null) {
            Type[] argsTypes = null;
            if (args == null) {
                args = _EmptyObjectArray;
                argsTypes = _EmptyTypeArray;
            }
            for (int i = 0; i < _ModuleTypes.Count; i++) {
                GameMod mod = Mods[i];
                Dictionary<string, MethodInfo> moduleMethods = _ModuleMethods[i];
                MethodInfo method;
                if (moduleMethods.TryGetValue(methodName, out method)) {
                    method?.GetDelegate()?.Invoke(Mods[i], args);
                    continue;
                }

                if (argsTypes == null)
                    argsTypes = Type.GetTypeArray(args);
                method = _ModuleTypes[i].GetMethod(methodName, argsTypes);
                moduleMethods[methodName] = method;
                method?.GetDelegate()?.Invoke(Mods[i], args);
            }
        }

    }
}
