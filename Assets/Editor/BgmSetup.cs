using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Title シーンに BGM オブジェクトを配置する。メニュー「Build/Setup BGM」またはバッチモードで実行する。
/// </summary>
public static class BgmSetup
{
    const string ScenePath = "Assets/Scenes/Title.unity";
    const string ClipPath = "Assets/EXCITPENGUIN_UI/カナリアスキップ.mp3";

    [MenuItem("Build/Setup BGM")]
    public static void Setup()
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);
        if (clip == null)
            throw new System.Exception($"BGM が見つかりません: {ClipPath}");

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var player = Object.FindFirstObjectByType<BgmPlayer>();
        if (player == null)
            player = new GameObject("BGM").AddComponent<BgmPlayer>();

        var so = new SerializedObject(player);
        so.FindProperty("bgm").objectReferenceValue = clip;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"BGM setup done: {clip.name} -> {ScenePath}");
    }

    /// <summary>バッチモード用: BGM 配置後にそのまま WebGL ビルドする</summary>
    public static void SetupAndBuild()
    {
        Setup();
        WebGLBuilder.Build();
    }
}
