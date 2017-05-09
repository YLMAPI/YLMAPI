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
using Object = UnityEngine.Object;
using System.Runtime.CompilerServices;

namespace YLMAPI.Content {
    internal static class ModContentHooks {

        private class HookAttribute : Attribute {
            public Type Type;
            public HookAttribute(Type type) {
                Type = type;
            }
        }

        public static bool IsInitialized { get; internal set; }

        static ModContentHooks() {
            Init();
        }
        public static void Init() {
            if (IsInitialized)
                return;
            IsInitialized = true;

            const BindingFlags bf_All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

            MethodInfo[] methods = ModContent.Types.ModContentHooks.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);
            Type t_Hooks = typeof(ModContentHooks);
            Type t_HookAttribute = typeof(HookAttribute);

            for (int i = 0; i < methods.Length; i++) {
                MethodInfo mHook = methods[i];
                object[] mHookInfos = mHook.GetCustomAttributes(t_HookAttribute, true);
                if (mHookInfos.Length == 0)
                    continue;
                HookAttribute mHookInfo = (HookAttribute) mHookInfos[0];

                ParameterInfo[] args = mHook.GetParameters();
                Type[] argTypes = new Type[args.Length];
                for (int ai = 0; ai < args.Length; ai++)
                    argTypes[ai] = args[ai].ParameterType;

                MethodInfo mTarget = mHookInfo.Type.GetMethod(mHook.Name, bf_All, null, argTypes, null);
                MethodInfo mTrampoline = t_Hooks.GetMethod("t_" + mHook.Name, bf_All, null, argTypes, null);

                mTarget.Detour(mHook);
                mTrampoline.Detour(mTarget.CreateOrigTrampoline());
            }
        }

        [MethodImpl((MethodImplOptions) 0x0100 /*AggressiveInlining*/)]
        public static void OnInstantiate(Object obj) {
            Console.WriteLine($"Instantiated: {obj} ({obj.GetType().FullName})");
            if (obj is GameObject)
                ModContentPatcher.PatchContentRecursive(((GameObject) obj).transform);
            else if (obj is Component)
                ModContentPatcher.PatchContentRecursive(((Component) obj).transform);
        }

        // Hooks

        public static Object t_Load(string path, Type type)
            => null;
        [Hook(typeof(Resources))]
        public static Object Load(string path, Type type) {
            object value = ModContent.Load(path, type);
            if (value == null)
                return t_Load(path, type);
            if (value is Object)
                return (Object) value;
            return new ModContentWrapper(value, type);
        }

        public static Object t_Internal_CloneSingle(Object data)
            => null;
        [Hook(typeof(Object))]
        public static Object Internal_CloneSingle(Object data) {
            data = t_Internal_CloneSingle(data);
            OnInstantiate(data);
            return data;
        }

        public static Object t_Internal_CloneSingleWithParent(Object data, Transform parent, bool worldPositionStays)
            => null;
        [Hook(typeof(Object))]
        public static Object Internal_CloneSingleWithParent(Object data, Transform parent, bool worldPositionStays) {
            data = t_Internal_CloneSingleWithParent(data, parent, worldPositionStays);
            OnInstantiate(data);
            return data;
        }

        public static Object t_INTERNAL_CALL_Internal_InstantiateSingle(Object data, ref Vector3 pos, ref Quaternion rot)
            => null;
        [Hook(typeof(Object))]
        public static Object INTERNAL_CALL_Internal_InstantiateSingle(Object data, ref Vector3 pos, ref Quaternion rot) {
            data = t_INTERNAL_CALL_Internal_InstantiateSingle(data, ref pos, ref rot);
            OnInstantiate(data);
            return data;
        }

        public static Object t_INTERNAL_CALL_Internal_InstantiateSingleWithParent(Object data, Transform parent, ref Vector3 pos, ref Quaternion rot)
            => null;
        [Hook(typeof(Object))]
        public static Object INTERNAL_CALL_Internal_InstantiateSingleWithParent(Object data, Transform parent, ref Vector3 pos, ref Quaternion rot) {
            data = t_INTERNAL_CALL_Internal_InstantiateSingleWithParent(data, parent, ref pos, ref rot);
            OnInstantiate(data);
            return data;
        }

    }
}
