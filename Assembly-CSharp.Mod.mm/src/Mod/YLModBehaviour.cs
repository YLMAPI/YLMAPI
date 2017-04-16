using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using SGUI;
using System.IO;

public class YLModBehaviour : MonoBehaviourSingleton<YLModBehaviour> {

    public new void Awake() {
    }

    public void Update() {
        YLMod.OnUpdate?.Invoke();
    }

}
