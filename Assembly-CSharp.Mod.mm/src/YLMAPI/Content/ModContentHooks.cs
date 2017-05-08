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

namespace YLMAPI.Content {
    public static class ModContentHooks {

        public static bool IsInitialized { get; internal set; }

        static ModContentHooks() {
            Init();
        }
        public static void Init() {
            if (IsInitialized)
                return;
            IsInitialized = true;

            MethodInfo m_Resources_Load = ModContent.Types.Resources.GetMethod("Load", new Type[] { typeof(string), typeof(Type) });
            m_Resources_Load.Detour(ModContent.Types.ModContentHooks.GetMethod("LoadHook"));
            ModContent.Types.ModContentHooks.GetMethod("trampoline_LoadHook").Detour(m_Resources_Load.CreateOrigTrampoline());
        }

        internal delegate UnityEngine.Object d_LoadHook(string path, Type type);
        public static UnityEngine.Object trampoline_LoadHook(string path, Type type) { return null; }
        public static UnityEngine.Object LoadHook(string path, Type type) {
            object value = ModContent.Load(path, type);
            if (value == null)
                return trampoline_LoadHook(path, type);
            if (value is UnityEngine.Object)
                return (UnityEngine.Object) value;
            return new ModContentWrapper(value, type);
        }

    }
}
