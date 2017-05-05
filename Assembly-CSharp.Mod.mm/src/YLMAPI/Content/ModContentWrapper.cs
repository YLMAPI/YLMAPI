using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;
using SGUI;
using Rewired;
using UEInput = UnityEngine.Input;
using System.IO;
using System.Reflection;

namespace YLMAPI.Content {
    public class ModContentWrapper : UnityEngine.Object {

        public readonly bool HasValue;
        public readonly object Value;
        public readonly Type Type;

        public ModContentWrapper() {
            HasValue = false;
            Value = null;
            Type = null;
        }
        public ModContentWrapper(object value, Type type) {
            HasValue = true;
            Value = value;
            Type = type;
        }

    }
}
