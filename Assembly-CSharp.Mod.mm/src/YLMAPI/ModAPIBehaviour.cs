using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using SGUI;
using System.IO;
using System.Text;
using Rewired;

namespace YLMAPI {
    public class ModAPIBehaviour : MonoBehaviourSingleton<ModAPIBehaviour> {

        public new void Awake() {
            gameObject.tag = "DoNotPause";
        }

        public void Update() {
            ModEvents.Update();
        }

        public void LateUpdate() {
            ModEvents.LateUpdate();
        }

    }
}
