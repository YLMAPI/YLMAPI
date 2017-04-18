using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using SGUI;
using System.IO;
using UnityEngine.SceneManagement;

public static class YLModGUI {

    public const float Padding = 2;
    public const float PaddingColumnElements = 32;

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
    public static SGroup HierarchyGroup;
    public static SGroup InspectorGroup;

    private readonly static HashSet<Canvas> _HiddenCanvases = new HashSet<Canvas>();

    public static void Init() {
        if (Root != null)
            return;

        YLMod.OnUpdate += Update;
        SceneManager.activeSceneChanged += (sceneA, sceneB) => {
            ShowGameGUI();
        };


        Root = SGUIRoot.Setup();
        GameObject.Find("SGUI Root").tag = "DoNotPause";

        Root.Background = new Color(
            /*
            0.27f,
            0.31f,
            0.33f,
            */
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
                new SLabel($"Yooka-Laylee Mod {YLMod.BaseUIVersion}") {
                    Background = YLModGUI.HeaderBackground,
                    Foreground = YLModGUI.HeaderForeground
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
                                    Background = YLModGUI.HeaderBackground,
                                    Foreground = YLModGUI.HeaderForeground
                                },
                                new SLabel("Keyboard:") {
                                    Background = YLModGUI.HeaderBackground,
                                    Foreground = YLModGUI.HeaderForeground
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
                                    Background = YLModGUI.HeaderBackground,
                                    Foreground = YLModGUI.HeaderForeground
                                },
                                new SLabel("Keyboard:") {
                                    Background = YLModGUI.HeaderBackground,
                                    Foreground = YLModGUI.HeaderForeground
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
                            OnUpdateStyle = YLModGUI.SegmentGroupUpdateStyle,
                            Children = {
                                new SLabel("General:") {
                                    Background = YLModGUI.HeaderBackground,
                                    Foreground = YLModGUI.HeaderForeground
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

                new SLabel("Hierarchy:") {
                    OnUpdateStyle = elem => {
                        elem.Position = new Vector2(elem.Previous.Previous.Position.x + elem.Previous.Size.x + Padding, elem.Previous.Previous.Position.y);
                    }
                },

                (HierarchyGroup = new SGroup {
                    ScrollDirection = SGroup.EDirection.Vertical,
                    AutoLayout = elem => elem.AutoLayoutVertical,
                    AutoLayoutPadding = Padding,
                    OnUpdateStyle = elem => {
                        elem.Position = new Vector2(elem.Previous.Position.x, elem.Previous.Position.y + elem.Previous.Size.y + Padding);
                        elem.Size = new Vector2(256, elem.Parent.Size.y - elem.Position.y - Padding);
                    },
                    With = { new SGroupForceScrollModifier() }
                }),

                new SLabel("Inspector:") {
                    OnUpdateStyle = elem => {
                        elem.Position = new Vector2(elem.Previous.Previous.Position.x + elem.Previous.Size.x + Padding, elem.Previous.Previous.Position.y);
                    }
                },

                (InspectorGroup = new SGroup {
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
                new SLabel($"Yooka-Laylee Mod {YLMod.BaseUIVersion}"),
                new SLabel("DEBUG LOG"),
                new SLabel("Use HOME / POS 1 key on keyboard to hide / show."),
                new SLabel("Use F2 to switch between cornered and full log."),
                new SLabel("Use PAGE UP / DOWN to scroll."),
                new SLabel(),
                new SLabel("For all keybindings, hit F1."),
                new SLabel()
            }
        };

        _ListScenes();
        RefreshHierarchy();
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
            UnityEngine.Object.FindObjectsOfType<SimpleSmoothMouseLook>().ForEach((ssml, i) => ssml.enabled = !MainGroup.Visible);
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
        if ( IsGameHUDVisible)
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
        using (StreamReader reader = new StreamReader(YLModContent.GetMapped("ylmod/gui/scenes").Stream))
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
                YLMod.Log("main", $"Found nameless scene info: {i} {scene.HashID} {scene.Scene?.name ?? "null"}");
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

    private static Coroutine _C_RefreshHierarchy;
    public static void RefreshHierarchy() {
        _C_RefreshHierarchy?.StopGlobal();
        _C_RefreshHierarchy = _RefreshHierarchy().StartGlobal();
    }
    private static IEnumerator _RefreshHierarchy() {
        HierarchyGroup.Children.Clear();
        SPreloader preloader = new SPreloader();
        HierarchyGroup.Children.Add(preloader);
        yield return null;

        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject root in roots) {
            while (_AddTransformGroup(HierarchyGroup, root.transform).MoveNext())
                yield return null;
        }

        preloader.Modifiers.Add(new SFadeOutShrinkSequence());
    }
    private static IEnumerator _AddTransformGroup(SGroup parent, Transform t) {
        yield return null;

        SGroup group = new SGroup() {
            Parent = parent
        };
    }

}
