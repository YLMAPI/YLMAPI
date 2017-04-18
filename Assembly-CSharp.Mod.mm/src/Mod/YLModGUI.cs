using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using SGUI;
using System.IO;
using UnityEngine.SceneManagement;
using System.Reflection;

public static class YLModGUI {

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
                    With = { new SGroupMinimumContentSizeModifier(), new SGroupForceScrollModifier() }
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
                    With = { new SGroupMinimumContentSizeModifier(), new SGroupForceScrollModifier() }
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
        SceneManager.activeSceneChanged += (sceneA, sceneB) => RefreshHierarchy();
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
        _C_RefreshHierarchy = _RefreshHierarchy(_C_RefreshHierarchy).StartGlobal();
        Inspect(null);
    }
    private static IEnumerator _RefreshHierarchy(Coroutine prev) {
        if (prev != null) {
            prev.StopGlobal();
            yield return null;
        }

        Scene scene = SceneManager.GetActiveScene();
        while (!scene.isLoaded)
            yield return null;

        HierarchyGroup.Children.Clear();
        SPreloader preloader = new SPreloader() {
            Parent = HierarchyGroup
        };
        yield return null;

        new SButton("Refresh") {
            Parent = HierarchyGroup,
            Icon = YLModContent.Load<Texture2D>("ylmod/gui/refresh"),
            IconScale = new Vector2(0.25f, 0.25f),
            Alignment = TextAnchor.MiddleLeft,
            OnClick = elem => {
                RefreshHierarchy();
            }
        };

        IEnumerator e = _AddTransformChildrenGroups(null, null);
        while (e.MoveNext())
            yield return e.Current;

        preloader.Modifiers.Add(new SFadeOutShrinkSequence());
    }
    private static IEnumerator _AddTransformChildrenGroups(SGroup parent, Transform t) {
        if (ReferenceEquals(t, null)) {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();

            foreach (GameObject root in roots) {
                yield return null;
                if (root == null)
                    continue;
                AddTransformGroup(parent, root.transform);
            }
            yield break;
        }

        if (t == null)
            yield break;

        for (int i = 0; i < t.childCount; i++) {
            yield return null;
            AddTransformGroup(parent, t.GetChild(i));
        }

        yield return null;
        HierarchyGroup.UpdateStyle();
    }
    public static SGroup AddTransformGroup(SGroup parent, Transform t) {
        if (t == null)
            return null;

        bool childrenAdded = false;
        SGroup group = new SGroup() {
            Parent = parent ?? HierarchyGroup,
            Border = 0,
            OnUpdateStyle = elem => {
                SGroup g = (SGroup) elem;
                SegmentGroupUpdateStyle(elem);
                g.ContentSize.y = g.Size.y;
                if (elem[1].Visible)
                    return;
                g.Size.y = g[0].Size.y;
            },
            Children = {
                new SButton(t.name) {
                    Alignment = TextAnchor.MiddleLeft,
                    OnClick = elem => {
                        Inspect(t);
                        elem.Next.Visible = !elem.Next.Visible;
                        HierarchyGroup.UpdateStyle();
                    }
                },
                new SGroup() {
                    Visible = false,
                    AutoLayout = elem => elem.AutoLayoutVertical,
                    AutoLayoutPadding = Padding,
                    OnUpdateStyle = elem => {
                        if (elem.Visible && !childrenAdded) {
                            childrenAdded = true;
                            _AddTransformChildrenGroups((SGroup) elem, t).StartGlobal();
                        }
                        SegmentGroupUpdateStyle(elem);
                        elem.Position = new Vector2(
                            PaddingHierarchyDepth,
                            elem.Previous.Position.y + elem.Previous.Size.y
                        );
                        elem.Size.x = elem.Parent.InnerSize.x - PaddingHierarchyDepth;
                    },
                }
            }
        };

        return group;
    }
    public static void Inspect(Transform t) {
        InspectorGroup.Children.Clear();
        if (t == null)
            return;

        new SButton("Refresh") {
            Parent = InspectorGroup,
            Icon = YLModContent.Load<Texture2D>("ylmod/gui/refresh"),
            IconScale = new Vector2(0.25f, 0.25f),
            Alignment = TextAnchor.MiddleLeft,
            OnClick = elem => {
                Inspect(t);
            }
        };

        new SLabel(t.name) {
            Parent = InspectorGroup,
            Alignment = TextAnchor.MiddleCenter
        };
        Vector3 pos = t.position;
        Vector3 rot = t.eulerAngles;
        new SLabel($"Position: {pos.x.ToString("0000.00")}, {pos.y.ToString("0000.00")}, {pos.z.ToString("0000.00")}") {
            Parent = InspectorGroup,
            Alignment = TextAnchor.MiddleLeft
        };
        new SLabel($"Rotation: {rot.x.ToString("0000.00")}, {rot.y.ToString("0000.00")}, {rot.z.ToString("0000.00")}") {
            Parent = InspectorGroup,
            Alignment = TextAnchor.MiddleLeft
        };
        new SButton("Is Object Active") {
            Parent = InspectorGroup,
            Alignment = TextAnchor.MiddleLeft,
            With = { new SCheckboxModifier() {
                GetValue = b => t.gameObject.activeSelf,
                SetValue = (b, v) => t.gameObject.SetActive(v)
            }}
        };
        new SButton("Move Camera To Object") {
            Parent = InspectorGroup,
            Icon = YLModContent.Load<Texture2D>("ylmod/gui/camera"),
            IconScale = new Vector2(0.25f, 0.25f),
            Alignment = TextAnchor.MiddleLeft,
            OnClick = elem => {
                if (t == null || Camera.main == null)
                    return;
                Camera.main.transform.position = t.position;
            }
        };

        Behaviour[] components = t.GetComponents<Behaviour>();
        for (int i = 0; i < components.Length; i++) {
            Behaviour c = components[i];
            new SButton(c.GetType().Name) {
                Parent = InspectorGroup,
                Alignment = TextAnchor.MiddleLeft,
                With = { new SCheckboxModifier() {
                    GetValue = b => c.enabled,
                    SetValue = (b, v) => c.enabled = v
                }}
            };
        }
    }

}
