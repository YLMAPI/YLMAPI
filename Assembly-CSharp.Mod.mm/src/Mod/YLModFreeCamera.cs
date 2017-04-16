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
                new SLabel("Press F11 to view hide all GUI."),
                new SLabel(),
                (GUIGameSpeed = new SLabel()),
                (GUIMoveSpeed = new SLabel()),
                new SLabel(),
                (GUISceneName = new SLabel()),
                (GUIPosition = new SLabel()),
                (GUIRotation = new SLabel()),
            }
        };

        YLMod.Input.ButtonMap["FreeCam Run"] =
            input => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || YLMod.Input.GetButton("RB");
        YLMod.Input.ButtonMap["FreeCam Internal Speed Switch"] =
            input => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || YLMod.Input.GetButton("LB");

        YLMod.Input.ButtonMap["FreeCam Internal Speed Reset"] =
            input => Input.GetMouseButton(2) || (YLMod.Input.GetButton("LT") && YLMod.Input.GetButton("RT"));
        YLMod.Input.ButtonMap["FreeCam Game Speed Reset"] =
            input =>  YLMod.Input.GetButton("FreeCam Internal Speed Switch") && YLMod.Input.GetButton("FreeCam Internal Speed Reset");
        YLMod.Input.ButtonMap["FreeCam Move Speed Reset"] =
            input => !YLMod.Input.GetButton("FreeCam Internal Speed Switch") && YLMod.Input.GetButton("FreeCam Internal Speed Reset");

        YLMod.Input.AxisMap["FreeCam Internal Speed"] =
            input =>
                Input.mouseScrollDelta.y +
                (YLMod.Input.GetButton("LT") ? -1f : YLMod.Input.GetButton("RT") ? 1f : 0f);
        YLMod.Input.AxisMap["FreeCam Game Speed"] =
            input =>
                ( YLMod.Input.GetButton("FreeCam Internal Speed Switch") ? YLMod.Input.GetAxis("FreeCam Internal Speed") : 0f);
        YLMod.Input.AxisMap["FreeCam Move Speed"] =
            input =>
                (!YLMod.Input.GetButton("FreeCam Internal Speed Switch") ? YLMod.Input.GetAxis("FreeCam Internal Speed") : 0f);

        YLMod.OnUpdate += Update;
    }

    public static void Update() {
        if (Input.GetKeyDown(KeyCode.F12)) {
            IsEnabled = !IsEnabled;

            if (!IsEnabled) {
                Time.timeScale = 1f;
                FreeCamera.enabled = false;
                if (PrevCamera != null)
                    PrevCamera.enabled = true;
                /*
                Camera.main.transform.position = FreeCamera.transform.position;
                Camera.main.transform.rotation = FreeCamera.transform.rotation;
                Camera.main.fieldOfView = FreeCamera.fieldOfView;
                */
            } else {
                PrevCamera = Camera.main;
                if (PrevCamera != null) {
                    FreeCamera.transform.position = PrevCamera.transform.position;
                    FreeCamera.transform.rotation = PrevCamera.transform.rotation;
                    FreeCamera.fieldOfView = PrevCamera.fieldOfView;
                    if (FreeCamera.fieldOfView < 10f)
                        FreeCamera.fieldOfView = 75f;
                    PrevCamera.enabled = false;
                }
                FreeCamera.enabled = true;

                FreeCamera.GetComponent<SimpleSmoothMouseLook>().targetDirection = FreeCamera.transform.rotation.eulerAngles;

                if (CameraManager.Instance != null)
                    ApplyDOFToFreeCam();
            }

            YLMod.Log($"{(IsEnabled ? "Enabled" : "Disabled")} free camera mode.");
        }

        if (CameraManager.Instance != null) {
            CameraManager.Instance.enabled = !IsEnabled;
        }

        GUIGroup.Visible = IsEnabled;

        if (!IsEnabled)
            return;

        /*
        if (CameraManager.Instance != null) {
            FreeCamera.enabled = true;
            if (Camera.main != null && Camera.main != FreeCamera)
                ApplyDOFToFreeCam();
        }
        */

        FreeCamera.enabled = true;

        Camera cam = FreeCamera;
        Transform camt = cam.transform;

        Speed = Mathf.Max(0f, Speed + 0.01f * YLMod.Input.GetAxis("FreeCam Move Speed"));
        if (YLMod.Input.GetButton("FreeCam Move Speed Reset"))
            Speed = DefaultSpeed;

        float speed = Speed;
        if (YLMod.Input.GetButton("FreeCam Run"))
            speed *= 4f;

        GUIGameSpeed.Text = $"Game speed: {Mathf.FloorToInt(Time.timeScale * 100f)}%";
        GUIMoveSpeed.Text = $"Movement speed: {(speed / DefaultSpeed * 100f).ToString("N0")}%";
        GUISceneName.Text = $"Scene (level): {SceneManager.GetActiveScene().name}";
        Vector3 pos = cam.transform.position;
        Vector3 rot = cam.transform.eulerAngles;
        GUIPosition.Text = $"Position: {pos.x.ToString("0000.00")}, {pos.y.ToString("0000.00")}, {pos.z.ToString("0000.00")}";
        GUIRotation.Text = $"Rotation: {rot.x.ToString("0000.00")}, {rot.y.ToString("0000.00")}, {rot.z.ToString("0000.00")}";

        Vector3 dir = Vector3.zero;

        dir += camt.forward * YLMod.Input.GetAxis("Vertical");

        float angleY = camt.rotation.eulerAngles.y;
        angleY = (angleY + 90f) / 180f * Mathf.PI;
        dir += new Vector3(Mathf.Sin(angleY), 0f, Mathf.Cos(angleY)) * YLMod.Input.GetAxis("Horizontal");

        if (dir != Vector3.zero) {
            dir.Normalize();
            camt.position += dir * speed * SpeedF;
        }

        camt.position += Vector3.up * YLMod.Input.GetAxis("Y Movement") * speed * SpeedF;

        float timeScalePrev = Time.timeScale;
        Time.timeScale = Mathf.Max(0f, Time.timeScale + YLMod.Input.GetAxis("FreeCam Game Speed") * (
            Time.timeScale < 1.999f ? 0.05f :
            Time.timeScale < 7.999 ? 0.5f :
            Time.timeScale < 15.999f ? 1f :
            4f
        ));

        if (YLMod.Input.GetButton("FreeCam Game Speed Reset"))
            Time.timeScale = 1f;

        int scaleRound = Mathf.FloorToInt(Time.timeScale * 100f);
        if (scaleRound % 10 == 9)
            Time.timeScale = (scaleRound + 1) / 100f;
        else if (scaleRound % 10 == 1)
            Time.timeScale = (scaleRound - 1) / 100f;

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
