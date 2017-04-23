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
using System.Security.Cryptography;
using MonoMod;
using Mono.Cecil;
using MonoMod.HarmonyCompat;
using Harmony;

namespace YLMAPI {
    public static class ModRuntimePatcher {

        public static MonoModder Modder;
        public static MMHarmonyInstance MMHarmony;

        public static void Init() {
            Modder = new MonoModder() {
                InputPath = Assembly.GetExecutingAssembly().Location,
                CleanupEnabled = false,
                DependencyDirs = {
                    ModRelinker.ManagedDirectory
                }
            };
            // DON'T. The assembly is already patched with the .mm.dlls in there!
            // Otherwise this code here wouldn't even run...
            // Modder.ReadMod(ModRelinker.ManagedDirectory);
            try {
                Modder.ReaderParameters.ReadSymbols = false;

                Modder.Read();
                Modder.MapDependencies();
            } catch (Exception e) {
                ModLogger.Log("rtpatcher", $"Failed initializing: {e}");
                return;
            }

            // Do black magic.
            HarmonyInstance.DEBUG = true;
            MMHarmony = new MMHarmonyInstance(Modder, Assembly.GetExecutingAssembly());
        }

        public static void LoadPatch(Stream stream) {
            try {
                ModLogger.Log("rtpatcher", "Loading new patch");
                // Reload main assembly.
                Modder.Module = null;
                Modder.Read();
                // Add new mod to mods list.
                Modder.ReadMod(stream);
                ModRelinker.AssemblyRelinkMap[Modder.Mods[Modder.Mods.Count - 1].Assembly.Name.Name] = ModRelinker.AssemblyRelinkedCache["Assembly-CSharp"];
                Modder.MapDependencies();
                // Auto-patch and then feed Harmony.
                ModLogger.Log("rtpatcher", $"Applying new patch {Modder.Mods[Modder.Mods.Count - 1].Assembly.Name.Name}");
                Modder.AutoPatch();
                MMHarmony.PatchAll();
            } catch (Exception e) {
                ModLogger.Log("rtpatcher", $"Failed patching: {e}");
                return;
            }
        }

    }
}
