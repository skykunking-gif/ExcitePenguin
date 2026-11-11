using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// unityroom 向け WebGL ビルド。メニュー「Build/WebGL (unityroom)」またはバッチモードの -executeMethod WebGLBuilder.Build で実行する。
/// </summary>
public static class WebGLBuilder
{
    const string OutputPath = "Builds/WebGL";

    [MenuItem("Build/WebGL (unityroom)")]
    public static void Build()
    {
        // unityroom はファイル名でビルドファイルを判別するので、ハッシュ名にしない
        PlayerSettings.WebGL.nameFilesAsHashes = false;
        // サーバー側で Content-Encoding が付くので、JS のフォールバック解凍は不要
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = false;

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"WebGL build {report.summary.result}: {report.summary.totalSize / (1024f * 1024f):F1} MB -> {OutputPath}");

        if (Application.isBatchMode)
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
