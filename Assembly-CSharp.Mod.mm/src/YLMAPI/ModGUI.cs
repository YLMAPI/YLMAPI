using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using SGUI;
using System.IO;
using UnityEngine.SceneManagement;
using System.Reflection;
using YLMAPI.Content;

namespace YLMAPI {
    public static class ModGUI {

        public const float Padding = 2;
        public const float PaddingColumnElements = 32;
        public const float PaddingHierarchyDepth = 16;

        public static readonly Color HeaderBackground = new Color(0.9f, 0.9f, 0.9f, 1f);
        public static readonly Color HeaderForeground = new Color(0.1f, 0.1f, 0.1f, 1f);

        public static readonly Color Header2Background = new Color(0.7f, 0.7f, 0.7f, 1f);
        public static readonly Color Header2Foreground = new Color(0.2f, 0.2f, 0.2f, 1f);

        public static bool IsGameHUDVisible = true;

        public static SGUIRoot Root;

        public static SGroup LogGroup;
        public static bool IsLogBig = false;

        public static SGroup MainGroup;
        public static SGroup HelpGroup;
        public static SGroup SettingsGroup;

        public static SGroup ScenesGroup;

        private readonly static HashSet<Canvas> _HiddenCanvases = new HashSet<Canvas>();

        public static void Init() {
            if (Root != null)
                return;

            Root = SGUIRoot.Setup();
            GameObject.Find("SGUI Root").tag = "DoNotPause";

            ModEvents.OnUpdate += Update;
            SceneManager.activeSceneChanged += (sceneA, sceneB) => {
                ShowGameGUI();
            };

            Root.Background = new Color(
                0.17f,
                0.21f,
                0.23f,
                Root.Background.a
            );

            MainGroup = new SGroup() {
                Visible = false,

                OnUpdateStyle = elem => {
                    elem.Fill(0);
                },

                Children = {
                new SLabel($"Yooka-Laylee Mod {ModAPI.UIVersion}") {
                    Background = HeaderBackground,
                    Foreground = HeaderForeground
                },

                new SLabel("Help:") {
                    OnUpdateStyle = elem => {
                        elem.Position = new Vector2(elem.Previous.Position.x, elem.Previous.Position.y + elem.Previous.Size.y + Padding);
                    }
                },

                (HelpGroup = new SGroup {
                    ScrollDirection = SGroup.EDirection.Vertical,
                    AutoLayout = elem => elem.AutoLayoutVertical,
                    AutoLayoutPadding = PaddingColumnElements,
                    OnUpdateStyle = elem => {
                        elem.Position = new Vector2(elem.Previous.Position.x, elem.Previous.Position.y + elem.Previous.Size.y + Padding);
                        elem.Size = new Vector2(512, elem.Parent.Size.y - elem.Position.y - Padding);
                    },
                    With = { new SGroupForceScrollModifier() },
                    Children = {
                        new SGroup() {
                            Background = new Color(0f, 0f, 0f, 0f),
                            AutoLayout = elem => elem.AutoLayoutVertical,
                            AutoLayoutVerticalStretch = false,
                            AutoLayoutPadding = 0f,
                            OnUpdateStyle = SegmentGroupUpdateStyle,
                            Children = {
                                new SLabel("Debug Log:") {
                                    Background = HeaderBackground,
                                    Foreground = HeaderForeground
                                },
                                new SLabel("Keyboard:") {
                                    Background = HeaderBackground,
                                    Foreground = HeaderForeground
                                },
                                new SLabel("HOME / POS1: Toggle log"),
                                new SLabel("PAGE UP / DOWN: Scroll")
                            }
                        },

                        new SGroup() {
                            Background = new Color(0f, 0f, 0f, 0f),
                            AutoLayout = elem => elem.AutoLayoutVertical,
                            AutoLayoutVerticalStretch = false,
                            AutoLayoutPadding = 0f,
                            OnUpdateStyle = SegmentGroupUpdateStyle,
                            Children = {
                                new SLabel("Miscellaneous:") {
                                    Background = HeaderBackground,
                                    Foreground = HeaderForeground
                                },
                                new SLabel("Keyboard:") {
                                    Background = HeaderBackground,
                                    Foreground = HeaderForeground
                                },
                                new SLabel("F11: Toggle game GUI"),
                            }
                        }
                    }
                }),

                new SLabel("Settings:") {
                    OnUpdateStyle = elem => {
                        elem.Position = new Vector2(elem.Previous.Previous.Position.x + elem.Previous.Size.x + Padding, elem.Previous.Previous.Position.y);
                    }
                },

                (SettingsGroup = new SGroup {
                    ScrollDirection = SGroup.EDirection.Vertical,
                    AutoLayout = elem => elem.AutoLayoutVertical,
                    AutoLayoutPadding = PaddingColumnElements,
                    OnUpdateStyle = elem => {
                        elem.Position = new Vector2(elem.Previous.Position.x, elem.Previous.Position.y + elem.Previous.Size.y + Padding);
                        elem.Size = new Vector2(256, elem.Parent.Size.y - elem.Position.y - Padding);
                    },
                    With = { new SGroupForceScrollModifier() },
                    Children = {

                        new SGroup() {
                            Background = new Color(0f, 0f, 0f, 0f),
                            AutoLayout = elem => elem.AutoLayoutVertical,
                            OnUpdateStyle = SegmentGroupUpdateStyle,
                            Children = {
                                new SLabel("YLMAPI-DEV TOOLS:") {
                                    Background = HeaderBackground,
                                    Foreground = HeaderForeground
                                },

                                new SButton("Clear loaded content mod cache") {
                                    Alignment = TextAnchor.MiddleLeft,
                                    OnClick = b => {
                                        ModContent.Cache.Clear();
                                    }
                                },

                                new SButton("Recreate content mod tree") {
                                    Alignment = TextAnchor.MiddleLeft,
                                    OnClick = b => {
                                        ModContent.Recrawl();
                                    }
                                },

                                new SButton("Patch content in scene") {
                                    Alignment = TextAnchor.MiddleLeft,
                                    OnClick = b => {
                                        ModContentPatcher.PatchContent(SceneManager.GetActiveScene()).StartGlobal();
                                    }
                                },

                                new SButton("Dump content in scene") {
                                    Alignment = TextAnchor.MiddleLeft,
                                    OnClick = b => {
                                        ModContentDumper.DumpContent(SceneManager.GetActiveScene()).StartGlobal();
                                    }
                                }
                            }
                        },

                        new SGroup() {
                            Background = new Color(0f, 0f, 0f, 0f),
                            AutoLayout = elem => elem.AutoLayoutVertical,
                            OnUpdateStyle = SegmentGroupUpdateStyle,
                            Children = {
                                new SLabel("General:") {
                                    Background = HeaderBackground,
                                    Foreground = HeaderForeground
                                },

                                new SButton("Show Game GUI") {
                                    Alignment = TextAnchor.MiddleLeft,
                                    With = { new SCheckboxModifier() {
                                        GetValue = b => IsGameHUDVisible,
                                        SetValue = (b, v) => {
                                            if (v)
                                                ShowGameGUI();
                                            else
                                                HideGameGUI();
                                        }
                                    }}
                                },

                                new SButton("Show Mod Log") {
                                    Alignment = TextAnchor.MiddleLeft,
                                    With = { new SCheckboxModifier() {
                                        GetValue = b => LogGroup?.Visible ?? false,
                                        SetValue = (b, v) => LogGroup.Visible = v
                                    }}
                                },

                                new SButton("Fullscreen Log") {
                                    Alignment = TextAnchor.MiddleLeft,
                                    With = { new SCheckboxModifier() {
                                        GetValue = b => IsLogBig,
                                        SetValue = (b, v) => { IsLogBig = v; LogGroup?.UpdateStyle(); }
                                    }}
                                }
                            }
                        }

                    }
                }),

                new SLabel("Scenes:") {
                    OnUpdateStyle = elem => {
                        elem.Position = new Vector2(elem.Previous.Previous.Position.x + elem.Previous.Size.x + Padding, elem.Previous.Previous.Position.y);
                    }
                },

                (ScenesGroup = new SGroup {
                    ScrollDirection = SGroup.EDirection.Vertical,
                    AutoLayout = elem => elem.AutoLayoutVertical,
                    AutoLayoutPadding = Padding,
                    OnUpdateStyle = elem => {
                        elem.Position = new Vector2(elem.Previous.Position.x, elem.Previous.Position.y + elem.Previous.Size.y + Padding);
                        elem.Size = new Vector2(256, elem.Parent.Size.y - elem.Position.y - Padding);
                    },
                    With = { new SGroupForceScrollModifier() }
                }),

            }
            };

            LogGroup = new SGroup() {
                Visible = false,
                ScrollDirection = SGroup.EDirection.Vertical,
                AutoLayout = elem => elem.AutoLayoutVertical,
                AutoLayoutPadding = 0f,

                OnUpdateStyle = elem => {
                    if (IsLogBig) {
                        elem.Fill(0);
                    } else {
                        elem.Size = new Vector2(512, 512);
                        elem.Position = elem.Root.Size - elem.Size;
                    }

                },

                Children = {
                new SLabel($"Yooka-Laylee {File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "subversion.txt")).Trim()}"),
                new SLabel($"Yooka-Laylee Mod {ModAPI.UIVersion}"),
                new SLabel("DEBUG LOG"),
                new SLabel("Use HOME / POS 1 key on keyboard to hide / show."),
                new SLabel("Use F2 to switch between cornered and full log."),
                new SLabel("Use PAGE UP / DOWN to scroll."),
                new SLabel(),
                new SLabel("For all keybindings, hit F1."),
                new SLabel()
            }
            };

            _ListScenes().StartGlobal();
        }

        public static void SegmentGroupUpdateStyle(SElement elem) {
            // elem.Position = elem.Previous.Position + new Vector2(0, elem.Previous.Size.y + elem.Backend.LineHeight * 2);

            if (!(elem is SGroup))
                return;
            SGroup group = (SGroup) elem;

            group.Border = 0;

            float height = 0f;
            for (int i = 0; i < group.Children.Count; i++) {
                if (i > 0)
                    height += group.AutoLayoutPadding;
                height += group.Children[i].Size.y;
            }
            elem.Size = new Vector2(elem.Parent.InnerSize.x, height);
        }

        public static void Update() {
            if (Input.GetKey(KeyCode.PageUp))
                LogGroup.ScrollMomentum = new Vector2(0, -8f);
            if (Input.GetKey(KeyCode.PageDown))
                LogGroup.ScrollMomentum = new Vector2(0, +8f);
            if (Input.GetKeyDown(KeyCode.Home))
                LogGroup.Visible = !LogGroup.Visible;
            if (Input.GetKeyDown(KeyCode.F2)) {
                IsLogBig = !IsLogBig;
                LogGroup.UpdateStyle();
            }

            if (Input.GetKeyDown(KeyCode.F1)) {
                MainGroup.Visible = !MainGroup.Visible;
                Cursor.visible = MainGroup.Visible;
                Cursor.lockState = CursorLockMode.None;
            }

            if (Input.GetKeyDown(KeyCode.F11)) {
                ToggleGameGUI();
            }
        }

        public static void ToggleGameGUI() {
            IsGameHUDVisible = !IsGameHUDVisible;
            if (IsGameHUDVisible) {
                foreach (Canvas c in _HiddenCanvases)
                    if (c != null)
                        c.enabled = true;
                _HiddenCanvases.Clear();
            } else {
                UnityEngine.Object.FindObjectsOfType<Canvas>().ForEach((c, i) => {
                    if (!c.enabled)
                        return;
                    c.enabled = false;
                    _HiddenCanvases.Add(c);
                });
            }
        }

        public static void ShowGameGUI() {
            if (!IsGameHUDVisible)
                ToggleGameGUI();
        }
        public static void HideGameGUI() {
            if (IsGameHUDVisible)
                ToggleGameGUI();
        }

        public static SButton AddScene(string scene) {
            SButton button = new SButton(scene) {
                Alignment = TextAnchor.MiddleLeft,
                With = { new SFadeInAnimation() },
                OnClick = b => {
                    LoadingScreenController.LoadScene(scene, "", "");
                }
            };
            ScenesGroup.Children.Add(button);
            return button;
        }
        private static IEnumerator _ListScenes() {
            /*
            for (int i = 0; i <= 41; i++) {
                SButton button = new SButton($"Scene {i}") {
                    Alignment = TextAnchor.MiddleLeft,
                    With = { new SFadeInAnimation() },
                    OnClick = b => {
                        SceneManager.LoadScene(int.Parse(b.Text.Substring(6)));
                    }
                };
                ScenesGroup.Children.Add(button);
                yield return null;
            }
            */
            using (StreamReader reader = new StreamReader(ModContent.GetMapped("ylmapi/scenes.txt").Stream))
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine().Trim();
                    if (line.Length == 0)
                        continue;
                    AddScene(line);
                    yield return null;
                }
        }
        private static IEnumerator _ListMainScenes() {
            SceneInfo[] scenes;
            while ((scenes = ScenesInfo.Instance?.ScenesData?.LookupTable) == null)
                yield return null;
            for (int i = 0; i < scenes.Length; i++) {
                SceneInfo scene = scenes[i];
                if (string.IsNullOrEmpty(scene.SceneName)) {
                    ModLogger.Log("main", $"Found nameless scene info: {i} {scene.HashID} {scene.Scene?.name ?? "null"}");
                    continue;
                }
                AddScene(scene.SceneName);
                yield return null;
            }
        }
        private static IEnumerator _ListArcadeScenes() {
            ArcadeGameInfo[] arcadeGames;
            while ((arcadeGames = ArcadeGamesManager.instance?.arcadeGamesSetup?.data) == null)
                yield return null;
            for (int i = 0; i < arcadeGames.Length; i++) {
                AddScene(arcadeGames[i].sceneName);
                AddScene(arcadeGames[i].sceneName + "_Standalone");
                yield return null;
            }
        }

    }
}
