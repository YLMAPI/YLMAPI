using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using YamlDotNet.Serialization;

public class SceneFreezeInfo {

    public readonly Scene Scene;
    private readonly List<GameObject> _Frozen = new List<GameObject>();

    public SceneFreezeInfo(Scene scene) {
        Scene = scene;
    }

    public void Freeze() {
        _Frozen.Clear();
        Scene.GetRootGameObjects(_Frozen);
        for (int i = _Frozen.Count - 1; i > -1; --i) {
            GameObject root = _Frozen[i];
            if (root == null || !root.activeSelf) {
                _Frozen.RemoveAt(i);
                continue;
            }
            root.SetActive(false);
        }
    }

    public void Unfreeze() {
        for (int i = _Frozen.Count - 1; i > -1; --i) {
            GameObject root = _Frozen[i];
            if (root == null || root.activeSelf) {
                continue;
            }
            root.SetActive(true);
        }
        _Frozen.Clear();
    }

}

public static class SceneFreezeInfoExt {

    public static SceneFreezeInfo Freeze(this Scene scene) {
        SceneFreezeInfo freeze = new SceneFreezeInfo(scene);
        freeze.Freeze();
        return freeze;
    }

}
