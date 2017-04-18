using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class YLModExt {

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

    public static Dictionary<string, Text> GetTexts(this GameObject go) {
        return GetTexts(go.transform);
    }
    public static Dictionary<string, Text> GetTexts(this Transform t) {
        Dictionary<string, Text> texts = new Dictionary<string, Text>();
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
        => YLModBehaviour.instance.StartCoroutine(e);
    public static void StopGlobal(this Coroutine c)
        => YLModBehaviour.instance.StopCoroutine(c);

    public static Coroutine OnLoadFinished(this Scene scene, Action<Scene> a)
        => new WaitForSceneLoadFinish(scene, a).StartGlobal();

}
