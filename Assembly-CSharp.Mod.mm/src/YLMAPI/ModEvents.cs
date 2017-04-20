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
    public static partial class ModEvents {

        public static event Action OnUpdate;
        public static void Update()
            => OnUpdate?.Invoke();
        public static event Action OnLateUpdate;
        public static void LateUpdate()
            => OnLateUpdate?.Invoke();

        public static event Func<PlayerInputStore, bool> OnPlayerInputUpdate;
        public static bool PlayerInputUpdate(PlayerInputStore p)
            => OnPlayerInputUpdate?.InvokeWhileTrue(p) ?? true;

        public static event Action<TextManager, string[], string[][]> OnTextsLoaded;
        public static void TextsLoaded(TextManager manager, string[] tables, string[][] stringData)
            => OnTextsLoaded?.Invoke(manager, tables, stringData);

        public static event Action<Transform> OnInspect;
        public static void Inspect(Transform t)
            => OnInspect?.Invoke(t);

        /// <summary>
        /// Invokes all delegates in the invocation list, passing on the result to the next.
        /// </summary>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <param name="md">The multicast delegate.</param>
        /// <param name="val">The initial value and first parameter.</param>
        /// <param name="args">Any other arguments that may be passed.</param>
        /// <returns>The result of all delegates.</returns>
        public static T InvokePassing<T>(this MulticastDelegate md, T val, params object[] args) {
            if (md == null)
                return default(T);

            object[] args_ = new object[args.Length + 1];
            args_[0] = val;
            Array.Copy(args, 0, args_, 1, args.Length);

            Delegate[] ds = md.GetInvocationList();
            for (int i = 0; i < ds.Length; i++)
                args_[0] = ds[i].DynamicInvoke(args_);

            return (T) args_[0];
        }

        /// <summary>
        /// Invokes all delegates in the invocation list, as long as the last invoked .
        /// </summary>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <param name="md">The multicast delegate.</param>
        /// <param name="args">Any arguments that may be passed.</param>
        /// <returns>The result of the last invoked delegate.</returns>
        public static bool InvokeWhileTrue(this MulticastDelegate md, params object[] args) {
            if (md == null)
                return true;

            Delegate[] ds = md.GetInvocationList();
            for (int i = 0; i < ds.Length; i++)
                if (!((bool) ds[i].DynamicInvoke(args)))
                    return false;

            return true;
        }

    }
}
