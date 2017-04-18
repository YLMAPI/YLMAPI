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
    public static SGroup ScenesGroup;
    public static SGroup SettingsGroup;

    private readonly static HashSet<Canvas> _HiddenCanvases = new HashSet<Canvas>();

    public static void Init() {
        if (Root != null)
            return;

        YLMod.OnUpdate += Update;
        SceneManager.activeSceneChanged += (sceneA, sceneB) => {
            ShowGameGUI();
        };


        Root = SGUIRoot.Setup();

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

    private static void _AddScene(string scene) {
        ScenesGroup.Children.Add(new SButton(scene) {
            Alignment = TextAnchor.MiddleLeft,
            OnClick = button => {
                LoadingScreenController.LoadScene(scene, "", "");
            }
        });
    }
    private static IEnumerator _AddScenes(params string[] scenes) {
        for (int i = 0; i < scenes.Length; i++) {
            _AddScene(scenes[i]);
            yield return null;
        }
    }
    private static void _ListScenes() {
        /*
        _AddScene("Frontend_Menu");
        _AddScene("Arcade_Frontend");
        _AddScene("Arcade_Frontend_Standalone");

        YLModBehaviour.instance.StartCoroutine(_ListMainScenes());
        YLModBehaviour.instance.StartCoroutine(_ListArcadeScenes());
        */

        YLModBehaviour.instance.StartCoroutine(_AddScenes(
            "Frontend_Menu",
            "Arcade_Frontend",
            "Arcade_Frontend_Standalone",
            "Level_01_Jungle",
            "Level_02_Glacier",
            "Level_03_Swamp",
            "Level_01_Jungle_Expanded",
            "Level_02_Glacier_Expanded",
            "Level_00_Hub_A",
            "Level_00_Hub_B",
            "Level_00_Hub_C",
            "Level_04_Casino",
            "Level_05_Space",
            "Quiz_01_Jungle",
            "Quiz_02_GlacierSwamp",
            "Quiz_03_CasinoSpace",
            "Arcade_Bees",
            "Arcade_Brawl",
            "Level_07_FinalBoss",
            "Level_05_Space_Expanded",
            "Level_03_Swamp_Expanded",
            "Level_04_Casino_Expanded",
            "Arcade_Brawl",
            "Arcade_Brawl_Standalone",
            "Arcade_Karts",
            "Arcade_Karts_Standalone",
            "Arcade_Bees",
            "Arcade_Bees_Standalone",
            "Arcade_Temple",
            "Arcade_Temple_Standalone",
            "Arcade_Shooter",
            "Arcade_Shooter_Standalone",
            "Arcade_Hurdles",
            "Arcade_Hurdles_Standalone",
            "Arcade_Flappy",
            "Arcade_Flappy_Standalone",
            "Arcade_Maze",
            "Arcade_Maze_Standalone"
        ));
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
            _AddScene(scene.SceneName);
            yield return null;
        }
    }
    private static IEnumerator _ListArcadeScenes() {
        ArcadeGameInfo[] arcadeGames;
        while ((arcadeGames = ArcadeGamesManager.instance?.arcadeGamesSetup?.data) == null)
            yield return null;
        for (int i = 0; i < arcadeGames.Length; i++) {
            _AddScene(arcadeGames[i].sceneName);
            _AddScene(arcadeGames[i].sceneName + "_Standalone");
            yield return null;
        }
    }

}
