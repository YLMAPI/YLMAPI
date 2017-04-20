using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using YamlDotNet.Serialization;

namespace YLMAPI {
    /// <summary>
    /// Game mod class. All game mods should have a class / type extending this.
    /// </summary>
    public abstract class GameMod : IDisposable {

        /// <summary>
        /// Used by YLMAPI itself and other mods to cache the metadata of the mod in RAM.
        /// 
        /// YLMAPIules will have their metadata read from the metadata file in the archive.
        /// 
        /// ETGBackends will have a preset metadata.
        /// 
        /// This property can be overriden or set to mimic other mods in case of multi-mods if required.
        /// (Truly mimicing other mods is currently only possible by analyzing the current stacktrace and getting the getter that way.)
        /// </summary>
        public virtual GameModMetadata Metadata { get; set; }

        /// <summary>
        /// This method gets called when YLMAPI initializes the mod, after all mods have been loaded.
        /// Do not depend on any specific order in which the mods get initialized.
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// This method gets called when YLMAPI enters its first frame, after all mods have been loaded.
        /// Do not depend on any specific order in which the mods get started.
        /// </summary>
        public virtual void Start() { }

        /// <summary>
        /// This method gets called when the mod gets unloaded.
        /// </summary>
        public abstract void Dispose();

    }

    public class GameModMetadata {

        /// <summary>
        /// The path to the ZIP of the mod. In case of unzipped mods, an empty string.
        /// </summary>
        public virtual string Archive { get; set; }

        /// <summary>
        /// The path to the directory of the mod. In case of .zips, an empty string.
        /// </summary>
        public virtual string Directory { get; set; }

        /// <summary>
        /// The name of the mod.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The icon of the mod to be used in the mod menu.
        /// </summary>
        public virtual Texture2D Icon { get; set; }

        /// <summary>
        /// The mod version.
        /// 
        /// Following rules when adding API backends as dependencies in game mods:
        /// The major (X.) version must be the same (breaking changes).
        /// The minor (.X) version can be lower in the game mod metadata.
        /// Example using ExampleAPI as API backend and UsingIt as game mod:
        ///                               DEPends INStalled
        /// UsingIt depends on ExampleAPI 1.0 and 1.0 is installed. Pass.
        /// UsingIt depends on ExampleAPI 2.0 and 1.0 is installed. Fail.
        /// UsingIt depends on ExampleAPI 1.0 and 2.0 is installed. Fail.
        /// UsingIt depends on ExampleAPI 1.5 and 1.5 is installed. Fail.
        /// UsingIt depends on ExampleAPI 1.5 and 1.6 is installed. Pass.
        /// UsingIt depends on ExampleAPI 1.5 and 1.0 is installed. Fail.
        /// </summary>
        public virtual Version Version { get; set; } = new Version(1, 0);

        /// <summary>
        /// The path of the mod .dll inside the ZIP or the relative DLL path with extracted mods.
        /// </summary>
        public virtual string DLL { get; set; }

        /// <summary>
        /// Whether the mod has been prelinked or not.
        /// </summary>
        public virtual bool Prelinked { get; set; } = false;

        /// <summary>
        /// The base profile used to compile this mod.
        /// </summary>
        public virtual int ProfileID { get; set; }

        /// <summary>
        /// The dependencies of the mod.
        /// </summary>
        public virtual List<GameModMetadata> Dependencies { get; set; } = new List<GameModMetadata>();

        public override string ToString() {
            return Name + " " + Version;
        }

        internal static GameModMetadata Parse(string archive, string directory, StreamReader reader) {
            GameModMetadata metadata = YamlHelper.Deserializer.Deserialize<GameModMetadata>(reader);
            metadata.Archive = archive;
            metadata.Directory = directory;

            if (!string.IsNullOrEmpty(directory)) {
                metadata.DLL = Path.Combine(directory, metadata.DLL.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
            }

            // Add dependency to API 1.0 if missing.
            bool dependsOnAPI = false;
            foreach (GameModMetadata dependency in metadata.Dependencies) {
                if (dependency.Name == "YLMAPI") {
                    dependsOnAPI = true;
                    break;
                }
            }
            if (!dependsOnAPI) {
                Debug.Log("WARNING: No dependency to API found in " + metadata + "! Adding dependency to API 1.0...");
                metadata.Dependencies.Insert(0, new GameModMetadata() {
                    Name = "API",
                    Version = new Version(1, 0)
                });
            }

            return metadata;
        }

    }
}
