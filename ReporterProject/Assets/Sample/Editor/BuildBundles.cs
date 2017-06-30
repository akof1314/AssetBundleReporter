using System;
using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;

public class BuildBundles
{
    [MenuItem("Tool/Build AssetBundle")]
    public static void Build()
    {
        string path = EditorUtility.SaveFolderPanel("AssetBundle 保存目录", "", "");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        path = Path.Combine(path, GetBuildTarget().ToString());
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.UncompressedAssetBundle, GetBuildTarget());
    }

    public static BuildTarget GetBuildTarget()
    {
        return EditorUserBuildSettings.activeBuildTarget;
    }

    [MenuItem("Tool/Build Reporter")]
    public static void Reporter()
    {
        string directoryPath = Path.GetDirectoryName(Application.dataPath);
        string bundlePath = Path.Combine(directoryPath, GetBuildTarget().ToString());
        string outputPath = Path.Combine(directoryPath, GetBuildTarget().ToString() + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");
        AssetBundleReporter.Print(bundlePath, outputPath);
    }
}
