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
    public static class ModAPI {

        public readonly static Version Version = new Version(0, 0, 0);
        // The following line will be replaced by Travis.
        public readonly static int TravisBuild = 0;
        /// <summary>
        /// Version profile, used separately from Version.
        /// A higher profile ID means higher instability ("developerness").
        /// </summary>
        public readonly static ModProfile Profile =
#if TRAVIS
        new ModProfile(2, "travis");
#elif DEBUG
        new ModProfile(1, "dev");
#else
        new ModProfile(0, ""); // no tag
#endif

        public static string UIVersion {
            get {
                string v = Version.ToString(3);

                if (TravisBuild != 0) {
                    v += "-";
                    v += TravisBuild;
                }

                if (!string.IsNullOrEmpty(Profile.Name)) {
                    v += "-";
                    v += Profile.Name;
                }

                return v;
            }
        }

        public static string GameDirectory;

        public static bool IsInitialized { get; internal set; }

        static ModAPI() {
            EntryPoint();
        }
        internal static void EntryPoint() {
            if (IsInitialized)
                return;
            IsInitialized = true;
            Console.WriteLine($"Initializing Yooka-Laylee Mod API {UIVersion}");

            GameDirectory = Path.GetDirectoryName(Path.GetFullPath(Application.dataPath));
            Console.WriteLine($"Game directory: {GameDirectory}");

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            ModEvents.OnLateUpdate += ModInput.LateUpdate;

            // Even if this seems useless, this creates the instance.
            ModAPIBehaviour ylmb = ModAPIBehaviour.instance;

            ModGUI.Init();

            ModLoader.LoadMods();

            ModLoader.Invoke("Init");
        }

        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            ModLogger.Log("main", $"Loaded scene: {scene.name}");
            // scene.OnLoadFinished(s => Console.WriteLine(s.DumpHierarchy(new StringBuilder()).ToString()));
        }

        public static void OnSceneUnloaded(Scene scene) {
            ModLogger.Log("main", $"Unloaded scene: {scene.name}");
        }

    }
}
