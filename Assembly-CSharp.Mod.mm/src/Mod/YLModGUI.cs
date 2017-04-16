using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using SGUI;
using System.IO;
using UnityEngine.SceneManagement;

public static class YLModGUI {

    public static SGUIRoot Root;

    public const float Padding = 2;

    public static SGroup LogGroup;
    public static bool IsLogBig = false;

    public static SGroup MainGroup;
    public static SGroup HelpGroup;
    public static SGroup SceneGroup;

    private static HashSet<Canvas> _HiddenCanvases = new HashSet<Canvas>();

    public static void Init() {
        if (Root != null)
            return;

        YLMod.OnUpdate += Update;

        Root = SGUIRoot.Setup();

        MainGroup = new SGroup() {
            Visible = false,

            OnUpdateStyle = elem => {
                elem.Fill(0);
            },

            Children = {
                new SLabel($"Yooka-Laylee Mod {YLMod.BaseUIVersion}") {
                    Foreground = Color.grey
                },

                new SLabel("Help:") {
                    OnUpdateStyle = elem => {
                        elem.Position = new Vector2(elem.Previous.Position.x, elem.Previous.Position.y + elem.Previous.Size.y + Padding);
                    }
                },

                (HelpGroup = new SGroup {
                    AutoLayout = elem => elem.AutoLayoutVertical,
                    AutoLayoutPadding = 16f,
                    OnUpdateStyle = elem => {
                        elem.Position = new Vector2(elem.Previous.Position.x, elem.Previous.Position.y + elem.Previous.Size.y + Padding);
                        elem.Size = new Vector2(512, elem.Parent.Size.y - elem.Position.y - Padding);
                    },
                    Children = {
                        new SGroup() {
                            Background = new Color(0f, 0f, 0f, 0f),
                            AutoLayout = elem => elem.AutoLayoutVertical,
                            AutoLayoutVerticalStretch = false,
                            AutoLayoutPadding = 0f,
                            OnUpdateStyle = HelpGroupUpdateStyle,
                            Children = {
                                new SLabel("Debug Log:") {
                                    Background = Color.white,
                                    Foreground = Color.black
                                },
                                new SLabel("Keyboard:") {
                                    Background = Color.white,
                                    Foreground = Color.black
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
                            OnUpdateStyle = HelpGroupUpdateStyle,
                            Children = {
                                new SLabel("Miscellaneous:") {
                                    Background = Color.white,
                                    Foreground = Color.black
                                },
                                new SLabel("Keyboard:") {
                                    Background = Color.white,
                                    Foreground = Color.black
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

                (SceneGroup = new SGroup {
                    AutoLayout = elem => elem.AutoLayoutVertical,
                    AutoLayoutPadding = 16f,
                    OnUpdateStyle = elem => {
                        elem.Position = new Vector2(elem.Previous.Position.x, elem.Previous.Position.y + elem.Previous.Size.y + Padding);
                        elem.Size = new Vector2(512, elem.Parent.Size.y - elem.Position.y - Padding);
                    },
                    With = {
                        new SDModifier {
                            OnInit = elem => {
                                for (int i = 0; i < SceneManager.sceneCount; i++) {
                                    Scene scene = SceneManager.GetSceneAt(i);
                                    elem.Children.Add(new SButton($"{i}: {scene.name}") {
                                        OnClick = button => {
                                            LoadingScreenController.LoadScene(scene.name, "", "");
                                        }
                                    });
                                }
                            }
                        }
                    }
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
    }

    public static void HelpGroupUpdateStyle(SElement elem) {
        // elem.Position = elem.Previous.Position + new Vector2(0, elem.Previous.Size.y + elem.Backend.LineHeight * 2);
        elem.Size = new Vector2(elem.Parent.Size.x, elem.Backend.LineHeight * elem.Children.Count);
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
            ToggleAll();
        }
    }

    public static void ToggleAll() {
        Root.Visible = !Root.Visible;
        if (Root.Visible) {
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

    public static void ShowAll() {
        if (!Root.Visible)
            ToggleAll();
    }
    public static void HideAll() {
        if ( Root.Visible)
            ToggleAll();
    }

}
