using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景自动装配：无需手动在 Inspector 挂脚本。
/// MainMenu → 商店 / 成就面板
/// GameScene → 开局引导 / 设置面板 / 成就弹窗
/// </summary>
public static class UIBootstrap
{
    const string MenuScene = "MainMenu";
    const string GameScene = "GameScene";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        Attach(SceneManager.GetActiveScene());
        SceneManager.sceneLoaded += (scene, mode) => Attach(scene);
    }

    static void Attach(Scene scene)
    {
        if (!scene.isLoaded) return;

        if (scene.name == MenuScene)
        {
            EnsureComponent<ShopManager>();
            EnsureComponent<AchievementPanel>();
            EnsureComponent<SaveSlotPanel>();
        }
        else if (scene.name == GameScene)
        {
            EnsureComponent<RunBootstrap>();
            EnsureComponent<SettingsPanel>();
            EnsureComponent<AchievementToastUI>();
            EnsureComponent<SaveSlotPanel>();
        }
    }

    static void EnsureComponent<T>() where T : Component
    {
        if (Object.FindObjectOfType<T>() != null) return;

        var go = GameObject.Find("AutoBootstrap");
        if (go == null) go = new GameObject("AutoBootstrap");
        if (go.GetComponent<T>() == null) go.AddComponent<T>();
    }
}
