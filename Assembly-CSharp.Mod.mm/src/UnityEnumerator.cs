using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;

public abstract class UnityEnumerator : IEnumerator {

    public object Current { get; internal set; }

    public abstract bool MoveNext();

    public void Reset() {
        throw new NotSupportedException();
    }

}

public abstract class DUnityEnumerator : UnityEnumerator {

    public Func<bool> OnMoveNext;

    public DUnityEnumerator() {
    }
    public DUnityEnumerator(Func<bool> onMoveNext) {
        OnMoveNext = onMoveNext;
    }


    public override bool MoveNext() => OnMoveNext();

}

public class WaitForSceneLoadFinish : UnityEnumerator {

    public Scene Scene;
    public Action<Scene> Action;

    public WaitForSceneLoadFinish(Scene scene, Action<Scene> action = null) {
        Scene = scene;
        Action = action;
    }

    public override bool MoveNext() {
        if (Scene.isLoaded) {
            Action?.Invoke(Scene);
            return false;
        }
        return true;
    }

}
