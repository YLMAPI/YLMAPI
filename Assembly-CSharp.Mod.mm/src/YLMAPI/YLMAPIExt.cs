using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using YLMAPI;
using System.IO;
using MonoMod.Helpers;

public static class YLMAPIExt {

    public static T GetRandomElement<T>(this T[] c) {
        return c[UnityEngine.Random.Range(0, c.Length)];
    }
    public static T GetRandomElement<T>(this IList<T> c) {
        return c[UnityEngine.Random.Range(0, c.Count)];
    }

    public static void ForEach<T>(this T[] c, Action<T, int> a) {
        for (int i = 0; i < c.Length; i++)
            a(c[i], i);
    }
    public static void ForEach<T>(this IList<T> c, Action<T, int> a) {
        for (int i = 0; i < c.Count; i++)
            a(c[i], i);
    }

    public static void ForEach(this GameObject go, Action<Transform> a) {
        ForEach(go.transform, a);
    }
    public static void ForEach(this Transform t, Action<Transform> a) {
        int count = t.childCount;
        for (int i = 0; i < count; i++)
            a(t.GetChild(i));
    }

    public static void ForEach(this GameObject go, Action<GameObject> a) {
        ForEach(go.transform, a);
    }
    public static void ForEach(this Transform t, Action<GameObject> a) {
        int count = t.childCount;
        for (int i = 0; i < count; i++)
            a(t.GetChild(i).gameObject);
    }

    public static string NormalizePath(this string s) {
        s = s.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

        RemoveSuffix:
        if (s.EndsWith("(Clone)")) {
            s = s.Substring(0, s.Length - 7);
            goto RemoveSuffix;
        }
        if (s.EndsWith("(Instance)")) {
            s = s.Substring(0, s.Length - 10);
            goto RemoveSuffix;
        }

        return s;
    }

    public static string EmptyToNull(this string s)
        => string.IsNullOrEmpty(s) ? null : s;
    public static string NullToEmpty(this string s)
        => s == null ? "" : s;

    public static IDictionary<string, Text> GetTexts(this GameObject go) {
        return GetTexts(go.transform);
    }
    public static IDictionary<string, Text> GetTexts(this Transform t) {
        FastDictionary<string, Text> texts = new FastDictionary<string, Text>();
        t.ForEach((GameObject c) => {
            Text text = c.GetComponent<Text>();
            if (text != null)
                texts[c.name.ToLowerInvariant()] = text;
        });
        return texts;
    }

    public static T GetOrAddComponent<T>(this GameObject go) where T : Component {
        T c = go.GetComponent<T>();
        if (c == null)
            c = go.AddComponent<T>();
        return c;
    }
    public static T GetOrAddComponent<T>(this Transform t) where T : Component {
        T c = t.GetComponent<T>();
        if (c == null)
            c = t.gameObject.AddComponent<T>();
        return c;
    }

    public static Coroutine StartGlobal(this IEnumerator e)
        => ModAPIBehaviour.instance.StartCoroutine(e);
    public static void StopGlobal(this Coroutine c)
        => ModAPIBehaviour.instance.StopCoroutine(c);

    public static Coroutine OnLoadFinished(this Scene scene, Action<Scene> a)
        => new WaitForSceneLoadFinish(scene, a).StartGlobal();

}
