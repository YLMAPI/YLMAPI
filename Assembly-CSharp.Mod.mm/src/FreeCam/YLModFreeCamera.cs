using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using SGUI;
using System.IO;
using UnityStandardAssets.ImageEffects;
using UnityEngine.SceneManagement;
using YLMAPI;

public static class YLModFreeCamera {

    public static bool IsInitialized { get; internal set; }
    public static bool IsEnabled;
    public static bool IsGUIVisible = true;

    private static bool WasFullBright;
    public static bool IsFullBright;
    public static Vector4 OriginalAmbienceColor;

    public static SGroup GUIInfoGroup;
    public static SLabel GUIInfoGameSpeed;
    public static SLabel GUIInfoMoveSpeed;
    public static SLabel GUIInfoSceneName;
    public static SLabel GUIInfoPosition;
    public static SLabel GUIInfoRotation;

    public static SGroup GUISettingsGroup;

    public const float DefaultSpeed = 0.1f;
    public static float Speed = DefaultSpeed;

    public static float TimeSpeed = 1f / 120f;
    public static float ToSpeedF(this float time) {
        return time / TimeSpeed;
    }
    public static float SpeedF {
        get {
            return Time.unscaledDeltaTime.ToSpeedF();
        }
    }

    private static Camera PrevCamera;
    private static Camera _FreeCamera;
    public static Camera FreeCamera {
        get {
            if (_FreeCamera != null)
                return _FreeCamera;

            _FreeCamera = new GameObject("YLMod MAGIC CAMERA™").AddComponent<Camera>();
            _FreeCamera.tag = "MainCamera";
            _FreeCamera.enabled = false;

            _FreeCamera.nearClipPlane = 0.3f;
            _FreeCamera.farClipPlane = 4000f;

            Antialiasing aa = _FreeCamera.gameObject.AddComponent<Antialiasing>();
            aa.dlaaShader = Shader.Find("Hidden/DLAA");
            aa.shaderFXAAII = Shader.Find("Hidden/FXAA II");
            aa.shaderFXAAIII = Shader.Find("Hidden/FXAA III (Console)");
            aa.shaderFXAAPreset2 = Shader.Find("Hidden/FXAA Preset 2");
            aa.shaderFXAAPreset3 = Shader.Find("Hidden/FXAA Preset 3");
            aa.nfaaShader = Shader.Find("Hidden/NFAA");
            aa.ssaaShader = Shader.Find("Hidden/SSAA");
            aa.mode = AAMode.SSAA;

            /*
            DepthOfField dof = _FreeCamera.gameObject.AddComponent<DepthOfField>();
            dof.dofHdrShader = Shader.Find("Hidden/Dof/DepthOfFieldHdr");
            dof.dx11BokehShader = Shader.Find("Hidden/Dof/DX11Dof

            Bloom bloom = _FreeCamera.gameObject.AddComponent<Bloom>();
            bloom.blurAndFlaresShader = Shader.Find("Hidden/BlurAndFlares");
            bloom.brightPassFilterShader = Shader.Find("Hidden/BrightPassFilter2");
            bloom.lensFlareShader = Shader.Find("Hidden/LensFlareCreate");
            bloom.screenBlendShader = Shader.Find("Hidden/BlendForBloom");

            ScreenSpaceAmbientOcclusionOptimized ssaoo = _FreeCamera.gameObject.AddComponent<ScreenSpaceAmbientOcclusionOptimized>();
            ssaoo.m_SSAOShader = Shader.Find("Hidden/SSAOOptimized");

            GlobalFog fog = _FreeCamera.gameObject.AddComponent<GlobalFog>();
            fog.fogShader = Shader.Find("Hidden/GlobalFog");
            */

            _FreeCamera.gameObject.AddComponent<SimpleSmoothMouseLook>();

            return _FreeCamera;
        }
    }

    public static void Init() {
        if (IsInitialized)
            return;
        IsInitialized = true;

        new SGroup() {
            Parent = ModGUI.HelpGroup,
            Background = new Color(0f, 0f, 0f, 0f),
            AutoLayout = elem => elem.AutoLayoutVertical,
            AutoLayoutVerticalStretch = false,
            AutoLayoutPadding = 0f,
            OnUpdateStyle = ModGUI.SegmentGroupUpdateStyle,
            Children = {
                new SLabel("Magic Camera™:") {
                    Background = ModGUI.HeaderBackground,
                    Foreground = ModGUI.HeaderForeground
                },
                new SLabel("Special thanks to Shesez (Boundary Break)!") {
                    Background = ModGUI.HeaderBackground,
                    Foreground = ModGUI.HeaderForeground
                },

                new SLabel("Controller:") {
                    Background = ModGUI.HeaderBackground,
                    Foreground = ModGUI.HeaderForeground
                },
                new SLabel("Press L3 and R3 (into the two sticks) at the same time."),
                new SLabel("Movement:") {
                    Background = ModGUI.Header2Background,
                    Foreground = ModGUI.Header2Foreground
                },
                new SLabel("Left stick: First person movement"),
                new SLabel("Right stick: Rotate camera"),
                new SLabel("LB / L1: Move straight down"),
                new SLabel("RB / R1: Move straight up"),
                new SLabel("Speed manipulation:") {
                    Background = ModGUI.Header2Background,
                    Foreground = ModGUI.Header2Foreground
                },
                new SLabel("LT / L2: Reduce move speed"),
                new SLabel("RT / R2: Increase move speed"),
                new SLabel("LT + RT / L2 + R2: Reset move speed"),
                new SLabel("DPad left: Freeze game"),
                new SLabel("DPad right: Reset game speed"),
                new SLabel("LT + RT / L2 + R2: Reset move speed"),
                new SLabel("Other:") {
                    Background = ModGUI.Header2Background,
                    Foreground = ModGUI.Header2Foreground
                },
                new SLabel("B / Circle: Toggle info in bottom-right corner"),
                new SLabel("X / Square: Toggle game GUI / HUD"),
                new SLabel("Y / Triangle: Toggle neutral lighting"),

                new SLabel("Keyboard:") {
                    Background = ModGUI.HeaderBackground,
                    Foreground = ModGUI.HeaderForeground
                },
                new SLabel("Press F12."),
                new SLabel("Movement:") {
                    Background = ModGUI.Header2Background,
                    Foreground = ModGUI.Header2Foreground
                },
                new SLabel("WASD: First person movement"),
                new SLabel("R / F: Move straight down"),
                new SLabel("Q / E: Move straight up"),
                new SLabel("Mouse: Rotate camera"),
                new SLabel("Shift: Run"),
                new SLabel("Speed manipulation:") {
                    Background = ModGUI.Header2Background,
                    Foreground = ModGUI.Header2Foreground
                },
                new SLabel("1 / Scroll up: Reduce move* speed"),
                new SLabel("2 / Scroll down: Increase move* speed"),
                new SLabel("3 / Middle mouse button: Reset move* speed"),
                new SLabel("Hold control + scroll = modify game speed"),
                new SLabel("4: Reduce game speed"),
                new SLabel("5: Increase game speed"),
                new SLabel("6: Reset game speed"),
                new SLabel("7: Freeze game"),
                new SLabel("Other:") {
                    Background = ModGUI.Header2Background,
                    Foreground = ModGUI.Header2Foreground
                },
                new SLabel("F3: Toggle info in bottom-right corner"),
                new SLabel("F4: Toggle neutral lighting")

            }
        };

        GUISettingsGroup = new SGroup() {
            Parent = ModGUI.SettingsGroup,
            Background = new Color(0f, 0f, 0f, 0f),
            AutoLayout = elem => elem.AutoLayoutVertical,
            OnUpdateStyle = ModGUI.SegmentGroupUpdateStyle,
            Children = {
                new SLabel("Magic Camera™:") {
                    Background = ModGUI.HeaderBackground,
                    Foreground = ModGUI.HeaderForeground
                },

                new SButton("Show Camera Info") {
                    Alignment = TextAnchor.MiddleLeft,
                    With = { new SCheckboxModifier() {
                        GetValue = b => IsGUIVisible,
                        SetValue = (b, v) => IsGUIVisible = v
                    }}
                },

                new SButton("Neutral Ambient Lighting") {
                    Alignment = TextAnchor.MiddleLeft,
                    With = { new SCheckboxModifier() {
                        GetValue = b => IsFullBright,
                        SetValue = (b, v) => IsFullBright = v
                    }}
                }

            }
        };

        GUIInfoGroup = new SGroup() {
            ScrollDirection = SGroup.EDirection.Vertical,
            AutoLayout = elem => elem.AutoLayoutVertical,
            AutoLayoutPadding = 0f,

            OnUpdateStyle = elem => {
                elem.Size = new Vector2(256, elem.Backend.LineHeight * elem.Children.Count);
                elem.Position = elem.Root.Size - elem.Size;
            },

            Children = {
                new SLabel("MAGIC CAMERA™") {
                    Background = ModGUI.HeaderBackground,
                    Foreground = ModGUI.HeaderForeground
                },
                new SLabel("Press F1 to view controls."),
                new SLabel(),
                (GUIInfoGameSpeed = new SLabel()),
                (GUIInfoMoveSpeed = new SLabel()),
                new SLabel(),
                (GUIInfoSceneName = new SLabel()),
                (GUIInfoPosition = new SLabel()),
                (GUIInfoRotation = new SLabel()),
            }
        };


        ModInput.ButtonMap["FreeCam Toggle"] =
            input => Input.GetKey(KeyCode.F12) || (ModInput.GetButton("LS") && ModInput.GetButton("RS"));
        ModInput.ButtonMap["FreeCam GUI Toggle"] =
            input => Input.GetKey(KeyCode.F3) || ModInput.GetButton("B");
        ModInput.ButtonMap["FreeCam Game GUI Toggle Ext"] =
            input => ModInput.GetButton("X");

        ModInput.ButtonMap["FreeCam Light Toggle"] =
            input => Input.GetKey(KeyCode.F4) || ModInput.GetButton("Y");

        ModInput.ButtonMap["FreeCam Run"] =
            input => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        ModInput.ButtonMap["FreeCam Internal Speed Switch"] =
            input => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        ModInput.ButtonMap["FreeCam Internal Speed Reset"] =
            input => Input.GetMouseButton(2) || (ModInput.GetButton("LT") && ModInput.GetButton("RT"));
        ModInput.ButtonMap["FreeCam Move Speed Reset"] =
            input =>
                Input.GetKey(KeyCode.Alpha3) ||
                (!ModInput.GetButton("FreeCam Internal Speed Switch") && ModInput.GetButton("FreeCam Internal Speed Reset"));
        ModInput.ButtonMap["FreeCam Game Speed Reset"] =
            input =>
                Input.GetKey(KeyCode.Alpha6) || ModInput.GetButton("DPadRight") ||
                (ModInput.GetButton("FreeCam Internal Speed Switch") && ModInput.GetButton("FreeCam Internal Speed Reset"));
        ModInput.ButtonMap["FreeCam Game Speed Freeze"] =
            input => Input.GetKey(KeyCode.Alpha7) || ModInput.GetButton("DPadLeft");

        ModInput.AxisMap["FreeCam Y Movement"] =
            input =>
                Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.Q) || ModInput.GetButton("LB") ? -1f :
                Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.E) || ModInput.GetButton("RB") ?  1f :
                0f;

        ModInput.AxisMap["FreeCam Internal Speed"] =
            input =>
                Input.mouseScrollDelta.y +
                (ModInput.GetButton("LT") ? -0.4f : ModInput.GetButton("RT") ? 0.4f : 0f);
        ModInput.AxisMap["FreeCam Move Speed"] =
            input =>
                (Input.GetKey(KeyCode.Alpha1) ? -0.4f : Input.GetKey(KeyCode.Alpha2) ? 0.4f : 0f) +
                (!ModInput.GetButton("FreeCam Internal Speed Switch") ? ModInput.GetAxis("FreeCam Internal Speed") : 0f);
        ModInput.AxisMap["FreeCam Game Speed"] =
            input =>
                (ModInput.GetButton("DPadUp") ? 0.4f : ModInput.GetButton("DPadDown") ? -0.4f : 0f) +
                (Input.GetKey(KeyCode.Alpha4) ? -0.4f : Input.GetKey(KeyCode.Alpha5) ? 0.4f : 0f) +
                ( ModInput.GetButton("FreeCam Internal Speed Switch") ? ModInput.GetAxis("FreeCam Internal Speed") : 0f);

        SceneManager.activeSceneChanged += (sceneA, sceneB) => {
            WasFullBright = IsFullBright = false;
        };

        ModEvents.OnUpdate += Update;
    }

    public static void Update() {
        if (ModInput.GetButtonDown("FreeCam Toggle")) {
            IsEnabled = !IsEnabled;

            if (!IsEnabled) {
                Time.timeScale = 1f;

                FreeCamera.enabled = false;
                if (PrevCamera != null)
                    PrevCamera.enabled = true;
            } else {
                QualitySettings.lodBias = 1f;
                QualitySettings.maximumLODLevel = 0;

                PrevCamera = Camera.main;
                if (PrevCamera != null) {
                    FreeCamera.transform.position = PrevCamera.transform.position;
                    FreeCamera.transform.eulerAngles = new Vector3(0f, PrevCamera.transform.eulerAngles.y, 0f);
                    FreeCamera.GetComponent<SimpleSmoothMouseLook>().targetDirection = PrevCamera.transform.rotation.eulerAngles;
                    FreeCamera.GetComponent<SimpleSmoothMouseLook>().mouseAbsolute = Vector2.zero;
                    FreeCamera.fieldOfView = PrevCamera.fieldOfView;
                    if (FreeCamera.fieldOfView < 10f)
                        FreeCamera.fieldOfView = 75f;
                    PrevCamera.enabled = false;
                }
                FreeCamera.enabled = true;

                if (CameraManager.Instance != null)
                    ApplyDOFToFreeCam();
            }

            ModLogger.Log("freecam", $"{(IsEnabled ? "Enabled" : "Disabled")} MAGIC CAMERA™ mode.");
        }

        if (IsEnabled && ModInput.GetButtonDown("FreeCam Light Toggle"))
            IsFullBright = !IsFullBright;
        if (!WasFullBright && IsFullBright) {
            OriginalAmbienceColor = RenderSettings.ambientLight;
            RenderSettings.ambientLight = new Color(1f, 1f, 1f, 1f);
        } else if (WasFullBright && !IsFullBright) {
            RenderSettings.ambientLight = OriginalAmbienceColor;
        }
        WasFullBright = IsFullBright;

        if (CameraManager.Instance != null) {
            CameraManager.Instance.enabled = !IsEnabled;
        }

        GUIInfoGroup.Visible = IsEnabled && IsGUIVisible;

        if (!IsEnabled)
            return;

        if (ModInput.GetButtonDown("FreeCam GUI Toggle"))
            IsGUIVisible = !IsGUIVisible;
        if (ModInput.GetButtonDown("FreeCam Game GUI Toggle Ext"))
            ModGUI.ToggleGameGUI();

        /*
        if (CameraManager.Instance != null) {
            FreeCamera.enabled = true;
            if (Camera.main != null && Camera.main != FreeCamera)
                ApplyDOFToFreeCam();
        }
        */

        FreeCamera.enabled = true;

        Transform camt = FreeCamera.transform;

        Speed = Mathf.Max(0.01f, Speed + 0.01f * ModInput.GetAxis("FreeCam Move Speed"));
        if (ModInput.GetButton("FreeCam Move Speed Reset"))
            Speed = DefaultSpeed;

        float speed = Speed;
        if (ModInput.GetButton("FreeCam Run"))
            speed *= 4f;

        Vector3 dir = Vector3.zero;

        dir += camt.forward * ModInput.GetAxis("Vertical");

        float angleY = camt.rotation.eulerAngles.y;
        angleY = (angleY + 90f) / 180f * Mathf.PI;
        if (camt.rotation.eulerAngles.z == 180f)
            angleY += Mathf.PI;
        dir += new Vector3(Mathf.Sin(angleY), 0f, Mathf.Cos(angleY)) * ModInput.GetAxis("Horizontal");

        if (dir != Vector3.zero) {
            dir.Normalize();
            camt.position += dir * speed * SpeedF;
        }

        camt.position += Vector3.up * ModInput.GetAxis("FreeCam Y Movement") * speed * SpeedF;

        float timeScalePrev = Time.timeScale;
        Time.timeScale = Mathf.Clamp(Time.timeScale + ModInput.GetAxis("FreeCam Game Speed") * (
            Time.timeScale < 0.24999f ? 0.01f :
            Time.timeScale < 1.99999f ? 0.05f :
            Time.timeScale < 7.99999f ? 0.5f :
            Time.timeScale < 15.99999f ? 1f :
            4f
        ), 0f, 100f);

        if (ModInput.GetButton("FreeCam Game Speed Reset"))
            Time.timeScale = 1f;

        if (ModInput.GetButton("FreeCam Game Speed Freeze"))
            Time.timeScale = 0f;

        int scaleRound = Mathf.FloorToInt(Time.timeScale * 100f);
        if (Time.timeScale >= 0.25f && scaleRound % 10 == 9)
            Time.timeScale = (scaleRound + 1) / 100f;

        GUIInfoGameSpeed.Text = $"Game speed: {Mathf.FloorToInt(Time.timeScale * 100f)}%";
        GUIInfoMoveSpeed.Text = $"Movement speed: {(speed / DefaultSpeed * 100f).ToString("N0")}%";
        GUIInfoSceneName.Text = $"Scene (level): {SceneManager.GetActiveScene().name}";
        Vector3 pos = camt.position;
        Vector3 rot = camt.eulerAngles;
        GUIInfoPosition.Text = $"Position: {pos.x.ToString("0000.00")}, {pos.y.ToString("0000.00")}, {pos.z.ToString("0000.00")}";
        GUIInfoRotation.Text = $"Rotation: {rot.x.ToString("0000.00")}, {rot.y.ToString("0000.00")}, {rot.z.ToString("0000.00")}";

    }

    public static void ApplyDOFToFreeCam() {
        if (CameraManager.Instance == null)
            return;

        DepthOfFieldParams dofParams = CameraManager.Instance.GetDefaultDOFParams();
        DepthOfField dof = FreeCamera.GetComponent<DepthOfField>();
        if (dof == null)
            return;
        dof.visualizeFocus = dofParams.Visualize;
        dof.focalLength = dofParams.FocalDistance;
        dof.focalSize = dofParams.FocalSize;
        dof.focalTransform = dofParams.FocusOnTransform;
        dof.aperture = dofParams.Aperture;
        dof.blurSampleCount = dofParams.BlurSampleCount;
        dof.maxBlurSize = dofParams.MaxBlurSize;
        dof.highResolution = dofParams.HighResolution;
        dof.nearBlur = dofParams.NearBlur;
        dof.foregroundOverlap = dofParams.ForegroundOverlap;
    }

}
