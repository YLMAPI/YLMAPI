#if !MMHARMONY
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
using MonoMod.Detour;

namespace YLMAPI {
    public static class ModRuntimePatcher {

        public static MonoModDetourer Detourer;

        public static void Init() {
            try {
                Detourer = new MonoModDetourer() {
                    InputPath = Assembly.GetExecutingAssembly().Location,
                    CleanupEnabled = false,
                    DependencyDirs = {
                        ModRelinker.ManagedDirectory
                    }
                };

                Detourer.ReaderParameters.ReadSymbols = false;

                // DON'T. The assembly is already patched with the .mm.dlls in there!
                // Otherwise this code here wouldn't even run...
                // Modder.ReadMod(ModRelinker.ManagedDirectory);

                Detourer.Read();
                Detourer.MapDependencies();
            } catch (Exception e) {
                ModLogger.Log("rtpatcher", $"Failed initializing: {e}");
                return;
            }
        }

        public static void LoadPatch(Stream stream) {
            try {
                ModLogger.Log("rtpatcher", "Loading new patch");
                Detourer.ReadMod(stream);
                ModRelinker.AssemblyRelinkMap[Detourer.Mods[Detourer.Mods.Count - 1].Assembly.Name.Name] = ModRelinker.AssemblyRelinkedCache["Assembly-CSharp"];
                ModLogger.Log("rtpatcher", $"Applied new patch {Detourer.Mods[Detourer.Mods.Count - 1].Assembly.Name.Name}");
            } catch (Exception e) {
                ModLogger.Log("rtpatcher", $"Failed patching: {e}");
                return;
            }
        }

    }
}
#endif
