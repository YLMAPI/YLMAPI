using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using SGUI;
using System.IO;
using UnityStandardAssets.ImageEffects;
using UnityEngine.SceneManagement;

public static class YLModFreeCamera {

    public static bool IsInitialized;
    public static bool IsEnabled;
    public static bool IsGUIVisible = true;

    public static SGroup GUIGroup;
    public static SLabel GUIGameSpeed;
    public static SLabel GUIMoveSpeed;
    public static SLabel GUISceneName;
    public static SLabel GUIPosition;
    public static SLabel GUIRotation;

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

            _FreeCamera = new GameObject("YLMod Free Camera").AddComponent<Camera>();
            _FreeCamera.tag = "MainCamera";
            _FreeCamera.enabled = false;

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
            Parent = YLModGUI.HelpGroup,
            Background = new Color(0f, 0f, 0f, 0f),
            AutoLayout = elem => elem.AutoLayoutVertical,
            AutoLayoutVerticalStretch = false,
            AutoLayoutPadding = 0f,
            OnUpdateStyle = YLModGUI.HelpGroupUpdateStyle,
            Children = {
                new SLabel("Free-Roam Camera:") {
                    Background = Color.white,
                    Foreground = Color.black
                },
                new SLabel("Special thanks to Shesez (Boundary Break)!") {
                    Background = Color.white,
                    Foreground = Color.black
                },

                new SLabel("Controller:") {
                    Background = Color.white,
                    Foreground = Color.black
                },
                new SLabel("Press L3 and R3 (into the two sticks) at the same time."),
                new SLabel("Movement:") {
                    Background = Color.grey,
                    Foreground = Color.black
                },
                new SLabel("Left stick: First person movement"),
                new SLabel("Right stick: Rotate camera"),
                new SLabel("LB / L1: Move straight down"),
                new SLabel("RB / R1: Move straight up"),
                new SLabel("Speed manipulation:") {
                    Background = Color.grey,
                    Foreground = Color.black
                },
                new SLabel("LT / L2: Reduce move speed"),
                new SLabel("RT / R2: Increase move speed"),
                new SLabel("LT + RT / L2 + R2: Reset move speed"),
                new SLabel("DPad left: Freeze game"),
                new SLabel("DPad right: Reset game speed"),
                new SLabel("LT + RT / L2 + R2: Reset move speed"),
                new SLabel("GUI / HUD:") {
                    Background = Color.grey,
                    Foreground = Color.black
                },
                new SLabel("B / Circle: Toggle info in bottom-right corner"),
                new SLabel("X / Square: Toggle game GUI / HUD"),

                new SLabel("Keyboard:") {
                    Background = Color.white,
                    Foreground = Color.black
                },
                new SLabel("Press F12."),
                new SLabel("Movement:") {
                    Background = Color.grey,
                    Foreground = Color.black
                },
                new SLabel("WASD: First person movement"),
                new SLabel("R / F: Move straight down"),
                new SLabel("Q / E: Move straight up"),
                new SLabel("Mouse: Rotate camera"),
                new SLabel("Shift: Run"),
                new SLabel("Speed manipulation:") {
                    Background = Color.grey,
                    Foreground = Color.black
                },
                new SLabel("1 / Scroll up: Reduce move* speed"),
                new SLabel("2 / Scroll down: Increase move* speed"),
                new SLabel("3 / Middle mouse button: Reset move* speed"),
                new SLabel("Hold control + scroll = modify game speed"),
                new SLabel("4: Reduce game speed"),
                new SLabel("5: Increase game speed"),
                new SLabel("6: Reset game speed"),
                new SLabel("7: Freeze game"),
                new SLabel("GUI / HUD:") {
                    Background = Color.grey,
                    Foreground = Color.black
                },
                new SLabel("F3: Toggle info in bottom-right corner"),

            }
        };

        GUIGroup = new SGroup() {
            ScrollDirection = SGroup.EDirection.Vertical,
            AutoLayout = elem => elem.AutoLayoutVertical,
            AutoLayoutPadding = 0f,

            OnUpdateStyle = elem => {
                elem.Size = new Vector2(256, elem.Backend.LineHeight * elem.Children.Count);
                elem.Position = elem.Root.Size - elem.Size;
            },

            Children = {
                new SLabel("FREE CAMERA") {
                    Background = Color.white,
                    Foreground = Color.black
                },
                new SLabel("Press F1 to view controls."),
                new SLabel(),
                (GUIGameSpeed = new SLabel()),
                (GUIMoveSpeed = new SLabel()),
                new SLabel(),
                (GUISceneName = new SLabel()),
                (GUIPosition = new SLabel()),
                (GUIRotation = new SLabel()),
            }
        };

        YLMod.Input.ButtonMap["FreeCam Toggle"] =
            input => Input.GetKey(KeyCode.F12) || (YLMod.Input.GetButton("LS") && YLMod.Input.GetButton("RS"));
        YLMod.Input.ButtonMap["FreeCam GUI Toggle"] =
            input => Input.GetKey(KeyCode.F3) || YLMod.Input.GetButton("B");
        YLMod.Input.ButtonMap["FreeCam Game GUI Toggle Ext"] =
            input => YLMod.Input.GetButton("X");

        YLMod.Input.ButtonMap["FreeCam Run"] =
            input => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        YLMod.Input.ButtonMap["FreeCam Internal Speed Switch"] =
            input => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        YLMod.Input.ButtonMap["FreeCam Internal Speed Reset"] =
            input => Input.GetMouseButton(2) || (YLMod.Input.GetButton("LT") && YLMod.Input.GetButton("RT"));
        YLMod.Input.ButtonMap["FreeCam Move Speed Reset"] =
            input =>
                Input.GetKey(KeyCode.Alpha3) ||
                (!YLMod.Input.GetButton("FreeCam Internal Speed Switch") && YLMod.Input.GetButton("FreeCam Internal Speed Reset"));
        YLMod.Input.ButtonMap["FreeCam Game Speed Reset"] =
            input =>
                Input.GetKey(KeyCode.Alpha6) || YLMod.Input.GetButton("DPadRight") ||
                (YLMod.Input.GetButton("FreeCam Internal Speed Switch") && YLMod.Input.GetButton("FreeCam Internal Speed Reset"));
        YLMod.Input.ButtonMap["FreeCam Game Speed Freeze"] =
            input => Input.GetKey(KeyCode.Alpha7) || YLMod.Input.GetButton("DPadLeft");

        YLMod.Input.AxisMap["FreeCam Y Movement"] =
            input =>
                Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.Q) || YLMod.Input.GetButton("LB") ? -1f :
                Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.E) || YLMod.Input.GetButton("RB") ?  1f :
                0f;

        YLMod.Input.AxisMap["FreeCam Internal Speed"] =
            input =>
                Input.mouseScrollDelta.y +
                (YLMod.Input.GetButton("LT") ? -0.4f : YLMod.Input.GetButton("RT") ? 0.4f : 0f);
        YLMod.Input.AxisMap["FreeCam Move Speed"] =
            input =>
                (Input.GetKey(KeyCode.Alpha1) ? -0.4f : Input.GetKey(KeyCode.Alpha2) ? 0.4f : 0f) +
                (!YLMod.Input.GetButton("FreeCam Internal Speed Switch") ? YLMod.Input.GetAxis("FreeCam Internal Speed") : 0f);
        YLMod.Input.AxisMap["FreeCam Game Speed"] =
            input =>
                (YLMod.Input.GetButton("DPadUp") ? 0.4f : YLMod.Input.GetButton("DPadDown") ? -0.4f : 0f) +
                (Input.GetKey(KeyCode.Alpha4) ? -0.4f : Input.GetKey(KeyCode.Alpha5) ? 0.4f : 0f) +
                ( YLMod.Input.GetButton("FreeCam Internal Speed Switch") ? YLMod.Input.GetAxis("FreeCam Internal Speed") : 0f);

        YLMod.OnUpdate += Update;
    }

    public static void Update() {
        if (YLMod.Input.GetButtonDown("FreeCam Toggle")) {
            IsEnabled = !IsEnabled;

            if (!IsEnabled) {
                Time.timeScale = 1f;
                FreeCamera.enabled = false;
                if (PrevCamera != null)
                    PrevCamera.enabled = true;
            } else {
                PrevCamera = Camera.main;
                if (PrevCamera != null) {
                    FreeCamera.transform.position = PrevCamera.transform.position;
                    FreeCamera.transform.rotation = PrevCamera.transform.rotation;
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

            YLMod.Log($"{(IsEnabled ? "Enabled" : "Disabled")} free camera mode.");
        }

        if (CameraManager.Instance != null) {
            CameraManager.Instance.enabled = !IsEnabled;
        }

        GUIGroup.Visible = IsEnabled && IsGUIVisible;

        if (!IsEnabled)
            return;

        if (YLMod.Input.GetButtonDown("FreeCam GUI Toggle"))
            IsGUIVisible = !IsGUIVisible;
        if (YLMod.Input.GetButtonDown("FreeCam Game GUI Toggle Ext"))
            YLModGUI.ToggleGameGUI();

        /*
        if (CameraManager.Instance != null) {
            FreeCamera.enabled = true;
            if (Camera.main != null && Camera.main != FreeCamera)
                ApplyDOFToFreeCam();
        }
        */

        FreeCamera.enabled = true;

        Transform camt = FreeCamera.transform;

        Speed = Mathf.Max(0.01f, Speed + 0.01f * YLMod.Input.GetAxis("FreeCam Move Speed"));
        if (YLMod.Input.GetButtonDown("FreeCam Move Speed Reset"))
            Speed = DefaultSpeed;

        float speed = Speed;
        if (YLMod.Input.GetButton("FreeCam Run"))
            speed *= 4f;

        Vector3 dir = Vector3.zero;

        dir += camt.forward * YLMod.Input.GetAxis("Vertical");

        float angleY = camt.rotation.eulerAngles.y;
        angleY = (angleY + 90f) / 180f * Mathf.PI;
        if (camt.rotation.eulerAngles.z == 180f)
            angleY += Mathf.PI;
        dir += new Vector3(Mathf.Sin(angleY), 0f, Mathf.Cos(angleY)) * YLMod.Input.GetAxis("Horizontal");

        if (dir != Vector3.zero) {
            dir.Normalize();
            camt.position += dir * speed * SpeedF;
        }

        camt.position += Vector3.up * YLMod.Input.GetAxis("FreeCam Y Movement") * speed * SpeedF;

        float timeScalePrev = Time.timeScale;
        Time.timeScale = Mathf.Clamp(Time.timeScale + YLMod.Input.GetAxis("FreeCam Game Speed") * (
            Time.timeScale < 0.24999f ? 0.01f :
            Time.timeScale < 1.99999f ? 0.05f :
            Time.timeScale < 7.99999f ? 0.5f :
            Time.timeScale < 15.99999f ? 1f :
            4f
        ), 0f, 100f);

        if (YLMod.Input.GetButtonDown("FreeCam Game Speed Reset"))
            Time.timeScale = 1f;

        if (YLMod.Input.GetButtonDown("FreeCam Game Speed Freeze"))
            Time.timeScale = 0f;

        int scaleRound = Mathf.FloorToInt(Time.timeScale * 100f);
        if (Time.timeScale >= 0.25f && scaleRound % 10 == 9)
            Time.timeScale = (scaleRound + 1) / 100f;

        GUIGameSpeed.Text = $"Game speed: {Mathf.FloorToInt(Time.timeScale * 100f)}%";
        GUIMoveSpeed.Text = $"Movement speed: {(speed / DefaultSpeed * 100f).ToString("N0")}%";
        GUISceneName.Text = $"Scene (level): {SceneManager.GetActiveScene().name}";
        Vector3 pos = camt.position;
        Vector3 rot = camt.eulerAngles;
        GUIPosition.Text = $"Position: {pos.x.ToString("0000.00")}, {pos.y.ToString("0000.00")}, {pos.z.ToString("0000.00")}";
        GUIRotation.Text = $"Rotation: {rot.x.ToString("0000.00")}, {rot.y.ToString("0000.00")}, {rot.z.ToString("0000.00")}";

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
