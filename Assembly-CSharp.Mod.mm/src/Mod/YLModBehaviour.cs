using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using SGUI;
using System.IO;
using System.Text;
using Rewired;

public class YLModBehaviour : MonoBehaviourSingleton<YLModBehaviour> {

    public new void Awake() {
        tag = "DoNotPause";
    }

    public void Update() {
        YLMod.OnUpdate?.Invoke();
    }

    public void LateUpdate() {
        YLMod.OnLateUpdate?.Invoke();
    }

}
