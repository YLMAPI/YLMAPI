using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using SGUI;
using System.IO;

public static class YLModGUI {

    public static SGUIRoot Root;

    public static SGroup LogGroup;
    public static bool IsLogBig = false;

    public static SGroup HelpGroup;

    public static void Init() {
        if (Root != null)
            return;

        YLMod.OnUpdate += Update;

        Root = SGUIRoot.Setup();

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

        HelpGroup = new SGroup() {
            Visible = false,

            OnUpdateStyle = elem => {
                elem.Fill(0);
            },

            Children = {
                new SLabel($"Yooka-Laylee Mod {YLMod.BaseUIVersion}"),

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
                        new SLabel("HOME / POS1: Show and hide"),
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
                        new SLabel("Free-Roam Camera:") {
                            Background = Color.white,
                            Foreground = Color.black
                        },
                        new SLabel("Special thanks to Shesez (Boundary Break)!") {
                            Background = Color.white,
                            Foreground = Color.black
                        },

                        new SLabel("Controller (not finished):") {
                            Background = Color.white,
                            Foreground = Color.black
                        },
                        new SLabel("Press L3 and R3 (into the two sticks) at the same time."),
                        new SLabel("Left stick: First person movement"),
                        new SLabel("Right stick: Rotate camera"),
                        new SLabel("L1 / LB (hold): Switch between game speed / move speed change") {
                            Foreground = Color.gray
                        },
                        new SLabel("R1 / RB (hold): Run") {
                            Foreground = Color.gray
                        },
                        new SLabel("L2 / LT: Reduce move* speed") {
                            Foreground = Color.gray
                        },
                        new SLabel("R2 / RT: Increase move* speed") {
                            Foreground = Color.gray
                        },
                        new SLabel("L2 + R2 / LT + RT: Reset move* speed") {
                            Foreground = Color.gray
                        },

                        new SLabel("Keyboard:") {
                            Background = Color.white,
                            Foreground = Color.black
                        },
                        new SLabel("Press F12."),
                        new SLabel("WASD: First person movement"),
                        new SLabel("R / F: Move straight up / down"),
                        new SLabel("Mouse: Rotate camera"),
                        new SLabel("Shift (hold): Run"),
                        new SLabel("Control (hold): Switch between game / move speed change"),
                        new SLabel("Scroll up: Reduce move* speed"),
                        new SLabel("Scroll down: Increase move* speed"),
                        new SLabel("Middle mouse button: Reset move* speed")

                    }
                },

                new SGroup() {
                    Background = new Color(0f, 0f, 0f, 0f),
                    AutoLayout = elem => elem.AutoLayoutVertical,
                    AutoLayoutVerticalStretch = false,
                    AutoLayoutPadding = 0f,
                    OnUpdateStyle = HelpGroupUpdateStyle,
                    Children = {
                        new SLabel("Other:") {
                            Background = Color.white,
                            Foreground = Color.black
                        },
                        new SLabel("Keyboard:") {
                            Background = Color.white,
                            Foreground = Color.black
                        },
                        new SLabel("F11: Show and hide game GUI"),
                    }
                },

            }
        };
    }

    public static void HelpGroupUpdateStyle(SElement elem) {
        elem.Position = elem.Previous.Position + new Vector2(0, elem.Previous.Size.y + elem.Backend.LineHeight * 2);
        elem.Size = new Vector2(512, elem.Backend.LineHeight * elem.Children.Count);
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

        if (Input.GetKeyDown(KeyCode.F1))
            HelpGroup.Visible = !HelpGroup.Visible;

        if (Input.GetKeyDown(KeyCode.F11)) {
            Root.Visible = !Root.Visible;
            UnityEngine.Object.FindObjectsOfType<Canvas>().ForEach((c, i) => c.enabled = Root.Visible);
        }
    }

}
