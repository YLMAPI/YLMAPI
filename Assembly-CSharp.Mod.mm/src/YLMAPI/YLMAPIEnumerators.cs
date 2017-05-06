using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine.SceneManagement;
using YLMAPI;

public class SceneLoadWrapper : UnityEnumerator {

    private static FieldInfo f_PC;

    public readonly IEnumerator Inner;
    public readonly string SceneName;

    public readonly bool IsMainLoader;

    public Func<IEnumerator, Scene, IEnumerator> OnLoadFinished;

    private LoadingState _State = LoadingState.None;
    public LoadingState State => _State;

    public SceneLoadWrapper(IEnumerator inner, string sceneName, Func<IEnumerator, Scene, IEnumerator> onLoadFinished) {
        Inner = inner;
        SceneName = sceneName;
        OnLoadFinished += onLoadFinished;

        Type t = inner.GetType();
        IsMainLoader = t.FullName.StartsWith("SavegameManager+<LoadSceneControl>");
        if (IsMainLoader) {
            if (f_PC == null)
                f_PC = t.GetField("$PC", BindingFlags.NonPublic | BindingFlags.Instance);
            _State = LoadingState.Pre;
        }
    }

    public override bool MoveNext() {
        if (IsMainLoader && _State == LoadingState.Pre && (int) f_PC.GetValue(Inner) == 3)
            _State = LoadingState.Loading;

        if (_State == LoadingState.Loading) {
            Current = null;
            if (MultiSceneLoader.Instance != null && !MultiSceneLoader.Instance.HasFinishedLoading)
                return true; // Wait until loading finished.

            _State = LoadingState.Post;
            // Return control to the inner or any further wrapper.
            Current = OnLoadFinished?.InvokePassing(Inner, SceneManager.GetSceneByName(SceneName)) ?? Inner;
            return true;
        }

        if (_State == LoadingState.Post) {
            Current = null;
            return false;
        }

        if (_State != LoadingState.Loading) {
            bool move = Inner.MoveNext();
            Current = Inner.Current;
            return move;
        }

        return true;
    }

    public enum LoadingState {
        None,
        Pre,
        Loading,
        Post
    }

}
