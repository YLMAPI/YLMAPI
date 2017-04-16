#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MonoMod;

class patch_PauseMainScreenController : PauseMainScreenController {

    public bool IsModInitialized;

    // public UiMenuScreenController LevelSelectScreen;

    public extern void orig_Show(bool isNavigationForward);
    public override void Show(bool isNavigationForward) {
        orig_Show(isNavigationForward);

        // m_returnToHubButton.interactable = true;

        ModInit();
    }

    public void ModInit() {
        if (IsModInitialized)
            return;
        IsModInitialized = true;

        // Console.WriteLine(m_optionsScreen.transform.DumpHierarchy(new StringBuilder()).ToString());

        /*
        m_returnToHubButton.interactable = true;
        m_returnToHubButton.transform.SetText("Level Select");

        LevelSelectScreen = Instantiate(m_optionsScreen);
        LevelSelectScreen.name = "LevelSelectScreen";
        LevelSelectScreen.transform.SetParent(transform.parent, false);
        LevelSelectScreen.tag = m_optionsScreen.tag;
        LevelSelectScreen.transform.position = m_optionsScreen.transform.position;

        LevelSelectScreen.transform.ForEach(c => Destroy((GameObject) c));

        LevelSelectScreen.AddButton("Back", screen => screen.Hide(false));
        */
    }

    public extern void orig_OnContinueSelected();
    public new void OnContinueSelected() {
        // This also gets triggered when escaping out of the menu (pressing B on an XBOX controller).
        orig_OnContinueSelected();
    }

    public extern void orig_OnReturnToHubSelected();
    public new void OnReturnToHubSelected() {
        // m_menuController.PushScreen(LevelSelectScreen, false);
        orig_OnReturnToHubSelected();
    }

    /*
    private static IEnumerator ListScenes() {
        AddScene("Frontend_Menu");
        AddScene("Arcade_Frontend");
        AddScene("Arcade_Frontend_Standalone");

        SceneInfo[] scenes;
        while ((scenes = ScenesInfo.Instance?.ScenesData?.LookupTable) == null)
            yield return null;
        for (int i = 0; i < scenes.Length; i++) {
            SceneInfo scene = scenes[i];
            if (string.IsNullOrEmpty(scene.SceneName)) {
                YLMod.Log($"Found nameless scene info: {i} {scene.HashID} {scene.Scene?.name ?? "null"}");
                continue;
            }
            AddScene(scene.SceneName);
            yield return null;
        }

        ArcadeGameInfo[] arcadeGames;
        while ((arcadeGames = ArcadeGamesManager.instance?.arcadeGamesSetup?.data) == null)
            yield return null;
        for (int i = 0; i < arcadeGames.Length; i++) {
            AddScene(arcadeGames[i].sceneName + "_Standalone");
            yield return null;
        }
    }
    private static void AddScene(string scene) {
        ScenesGroup.Children.Add(new SButton(scene) {
            Alignment = TextAnchor.MiddleLeft,
            OnClick = button => {
                LoadingScreenController.LoadScene(scene, "", "");
            }
        });
    }
    */

}
