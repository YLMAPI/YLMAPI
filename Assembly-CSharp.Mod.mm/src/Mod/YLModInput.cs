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

public static partial class YLModInput {

    public static Dictionary<string, Func<Player, bool>> ButtonMap = new Dictionary<string, Func<Player, bool>>();
    public static Dictionary<string, Func<Player, float>> AxisMap = new Dictionary<string, Func<Player, float>>();

    private static Dictionary<string, bool> _ButtonsPrev = new Dictionary<string, bool>();

    internal static void LateUpdate() {
        Player input = ReInput.players.GetSystemPlayer();
        if (input == null)
            return;

        // For debugging purposes - when one needs to find out button IDs
        /**/
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < 100; i++) {
            if (input.GetButtonDown(i))
                builder.Append(" b:").Append(i);
        }
        if (builder.Length != 0)
            Console.WriteLine("DEBUG CONTROLLER HELP: " + builder.ToString());
        /**/

        foreach (KeyValuePair<string, Func<Player, bool>> kvp in ButtonMap)
            _ButtonsPrev[kvp.Key] = kvp.Value?.Invoke(input) ?? false;
    }

    public static bool GetButton(string button) {
        Player input = ReInput.players.GetSystemPlayer();
        if (input == null)
            return false;
        Func<Player, bool> f;
        if (ButtonMap.TryGetValue(button, out f) && f != null)
            return f(input);
        return false;
    }

    public static bool GetButtonDown(string button) {
        bool prev;
        if (!_ButtonsPrev.TryGetValue(button, out prev))
            return false;
        return !prev && GetButton(button);
    }

    public static float GetAxis(string axis) => GetAxisRaw(axis);
    public static float GetAxisRaw(string axis) {
        Player input = ReInput.players.GetSystemPlayer();
        if (input == null)
            return 0f;
        Func<Player, float> f;
        if (AxisMap.TryGetValue(axis, out f) && f != null)
            return f(input);
        return 0f;
    }

    static YLModInput() {
        // TODO: This mapping list.
        _InitButtonMap(9, "Jump", "A");
        _InitButtonMap(10, "Fly");
        _InitButtonMap(12, "Context");
        _InitButtonMap(14, "TongueEdibleItem", "B");
        _InitButtonMap_Not(input => UEInput.GetMouseButton(3), "B");
        _InitButtonMap(15, "Invisibility");
        _InitButtonMap(16, "BasicAttack", "ShootEatenItem", "X");
        _InitButtonMap_Not(input => UEInput.GetMouseButton(1), "X");
        _InitButtonMap(18, "WheelSpin");
        _InitButtonMap(19, "FartBubble");
        _InitButtonMap(20, "Crouch", "LT", "L2");
        _InitButtonMap(21, "GroundPound");
        _InitButtonMap(22, "SwimUnderwater");
        _InitButtonMap(23, "Wheel", "RT", "R2");
        _InitButtonMap(24, "RB", "R1");
        _InitButtonMap_Not(50, "RB", "R1");
        _InitButtonMap(25, "LB", "L1");
        _InitButtonMap_Not(input => UEInput.mouseScrollDelta.y > 0f, "LB", "L1");
        _InitButtonMap(27, "SonarBlastAttack", "Y");
        _InitButtonMap(28, "SonarBoomAttack");
        _InitButtonMap(29, "SonarShieldAttack");
        _InitButtonMap(30, "Aiming");
        _InitButtonMap(39, "EmoteHappy", "DPadUp");
        _InitButtonMap(40, "EmoteTaunt", "DPadRight");
        _InitButtonMap(41, "EmoteDisappointed", "DPadDown");
        _InitButtonMap(42, "EmoteAngry", "DPadLeft");
        _InitButtonMap(49, "LS", "L3");
        _InitButtonMap(50, "RS", "R3");
        _InitButtonMap(51, "SwimUnderwaterAlt");

        AxisMap["Horizontal"] = input => input.GetAxis(0) + (UEInput.GetKey(KeyCode.A) ? -1f : UEInput.GetKey(KeyCode.D) ? 1f : 0f);
        AxisMap["Vertical"] = input => input.GetAxis(1) + (UEInput.GetKey(KeyCode.S) ? -1f : UEInput.GetKey(KeyCode.W) ? 1f : 0f);
        AxisMap["Mouse X"] = input => input.GetAxisRaw(4) * 0.5f + input.GetAxisRaw(47) * 0.07f;
        AxisMap["Mouse Y"] = input => input.GetAxisRaw(5) * 0.5f + input.GetAxisRaw(48) * 0.07f;

    }

    private static void _InitButtonMap(int id, params string[] names) {
        Func<Player, bool> f = input => input.GetButton(id);
        for (int i = names.Length - 1; i > -1; --i)
            ButtonMap[names[i]] = f;
    }

    private static void _InitButtonMap_Not(int id, params string[] names) {
        for (int i = names.Length - 1; i > -1; --i) {
            string name = names[i];
            Func<Player, bool> f = ButtonMap[name];
            ButtonMap[name] = input => !input.GetButton(id) && f(input);
        }
    }
    private static void _InitButtonMap_Not(Func<Player, bool> not, params string[] names) {
        for (int i = names.Length - 1; i > -1; --i) {
            string name = names[i];
            Func<Player, bool> f = ButtonMap[name];
            ButtonMap[name] = input => !not(input) && f(input);
        }
    }

}
