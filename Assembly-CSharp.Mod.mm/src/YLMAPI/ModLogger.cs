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

namespace YLMAPI {
    public static class ModLogger {

        public static void Log(string tag, string str) {
            Console.Write("(");
            Console.Write(DateTime.Now);
            Console.Write(") [YLMod] [");
            Console.Write(tag);
            Console.Write("] ");
            Console.WriteLine(str);

            ModGUI.Init();
            ModGUI.LogGroup.Children.Add(
                new SLabel($"[{tag}] {str}") {
                    With = { new SFadeInAnimation() }
                }
            );

            ModGUI.LogGroup.ScrollPosition = new Vector2(0f, float.MaxValue);
        }

    }
}
