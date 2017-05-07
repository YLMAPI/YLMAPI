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
    public class AnimatedTexture {
        public string Format { get; set; } = "{texture}.{frame}";
        public List<AnimatedTextureFrame> Frames { get; set; } = new List<AnimatedTextureFrame>();
    }

    public class AnimatedTextureFrame {
        public string Name { get; set; } = "";
    }

    public class AnimatedTextureBehaviour : MonoBehaviour {
    }
}
