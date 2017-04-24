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

namespace YLMAPI {
    public static class ModRuntimePatcher {

        public static void Init() {
            // TODO: ModRuntimePatcher
        }

        public static void LoadPatch(Stream stream) {
            // TODO: ModRuntimePatcher
            // For now just add the PatchDLL to the relink map.
            using (ModuleDefinition patch = ModuleDefinition.ReadModule(stream))
                try {
                    ModLogger.Log("rtpatcher", $"Loading new patch: {patch.Assembly.Name.Name}");
                    ModRelinker.AssemblyRelinkMap[patch.Assembly.Name.Name] = ModRelinker.AssemblyRelinkedCache["Assembly-CSharp"];
                } catch (Exception e) {
                    ModLogger.Log("rtpatcher", $"Failed patching: {e}");
                    return;
                }
        }

    }
}
#endif
