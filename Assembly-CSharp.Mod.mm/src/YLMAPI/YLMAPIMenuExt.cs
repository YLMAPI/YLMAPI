using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class YLMAPIMenuExt {

    private static UiMenuItemController _BaseButton;
    public static UiMenuItemController BaseButton {
        get {
            if (_BaseButton == null)
                return null;
            return UnityEngine.Object.Instantiate(_BaseButton);
        }
        set {
            if (_BaseButton != null)
                UnityEngine.Object.Destroy(_BaseButton);
            if (value == null) {
                _BaseButton = null;
                return;
            }
            value.gameObject.SetActive(false);
            UiMenuItemController button = UnityEngine.Object.Instantiate(value);
            UnityEngine.Object.DontDestroyOnLoad(button);
            value.gameObject.SetActive(true);
            _BaseButton = button;
        }
    }

    public static UiMenuItemController AddButton(this UiMenuScreenController screen, string text, Action<UiMenuScreenController> a) {
        UiMenuItemController button = BaseButton;
        button.name = text;
        button.transform.SetParent(screen.transform, false);
        button.transform.SetText(text);

        button.gameObject.SetActive(true);
        return button;
    }

}
