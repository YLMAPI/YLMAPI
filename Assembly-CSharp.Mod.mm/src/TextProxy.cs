using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextProxy {

    protected Transform root;
    protected Dictionary<string, Text> texts = new Dictionary<string, Text>();
    protected Dictionary<string, string> values = new Dictionary<string, string>();

    public string this[string key] {
        get {
            return values[key];
        }
        set {
            Text text;
            if (!texts.TryGetValue(key, out text) || text == null) {
                Transform child = root.Find(key);
                if ((texts[key] = child.GetComponent<Text>()) == null)
                    throw new KeyNotFoundException(string.Format("Text \"{0}\" not found in {1}", key, root));
            }
            texts[key].text = value;
            values[key] = value;
        }
    }

    public string this[Text key] {
        set {
            string name = key.name;
            if (name == "Text")
                name = key.transform.parent.name ?? name;
            texts[name] = key;
            values[name] = value;
        }
    }

    public TextProxy() {
    }

    public TextProxy(GameObject go)
        : this(go.transform) {
    }
    public TextProxy(Transform transform) {
        root = transform;
        transform.GetComponentsInChildren<Text>(true).ForEach((t, i) => this[t] = t.text);
    }

}

public class SharedTextProxy : MonoBehaviour {

    public TextProxy Proxy;

    public void Start() {
        Proxy = new TextProxy(transform);
    }

}

public static class TextProxyExt {

    public static TextProxy GetTextProxy(this GameObject go) {
        SharedTextProxy c = go.GetComponent<SharedTextProxy>();
        if (c == null)
            c = go.AddComponent<SharedTextProxy>();
        return c.Proxy ?? (c.Proxy = new TextProxy(go));
    }
    public static TextProxy GetTextProxy(this Transform t) {
        SharedTextProxy c = t.GetComponent<SharedTextProxy>();
        if (c == null)
            c = t.gameObject.AddComponent<SharedTextProxy>();
        return c.Proxy ?? (c.Proxy = new TextProxy(t));
    }

    public static string GetText(this GameObject go)
        => go.transform.GetText();
    public static string GetText(this Transform t)
        => t.GetComponentInChildren<Text>().text;
    public static void SetText(this GameObject go, string text)
        => go.transform.SetText(text);
    public static void SetText(this Transform t, string text)
        => t.GetComponentInChildren<Text>().text = text;

    public static string GetText(this GameObject go, string name)
        => go.transform.GetText(name);
    public static string GetText(this Transform t, string name)
        => t.FindChild(name).GetText();
    public static void SetText(this GameObject go, string name, string text)
        => go.transform.SetText(name, text);
    public static void SetText(this Transform t, string name, string text)
        => t.FindChild(name).SetText(text);

}
