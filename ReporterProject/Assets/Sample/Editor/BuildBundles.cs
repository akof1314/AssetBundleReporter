using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using WuHuan;

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
#if UNITY_5 || UNITY_5_3_OR_NEWER
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.UncompressedAssetBundle, GetBuildTarget());
#else
        path += "/all.assetbundle";
        UnityEngine.Object[] selectedAsset = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        BuildPipeline.BuildAssetBundle(null, selectedAsset, path, BuildAssetBundleOptions.CollectDependencies, GetBuildTarget());
#endif
    }

    public static BuildTarget GetBuildTarget()
    {
        return EditorUserBuildSettings.activeBuildTarget;
    }

    [MenuItem("Tool/Build Reporter")]
    public static void Reporter()
    {
        //WuHuan.AssetBundleFilesAnalyze.analyzeExport = true;
        //WuHuan.AssetBundleFilesAnalyze.analyzeOnlyScene = true;
        string directoryPath = Path.GetDirectoryName(Application.dataPath);
        string bundlePath = Path.Combine(directoryPath, GetBuildTarget().ToString());
        string outputPath = Path.Combine(directoryPath, GetBuildTarget().ToString() + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");
        WuHuan.AssetBundleReporter.AnalyzePrint(bundlePath, outputPath, () => System.Diagnostics.Process.Start(outputPath));
    }

    public static void MyAnalyzeCustomDepend()
    {
        AssetBundleFilesAnalyze.analyzeExport = true;
        AssetBundleFilesAnalyze.analyzeCustomDepend = directoryPath =>
        {
            List<AssetBundleFileInfo> infos = new List<AssetBundleFileInfo>();

            // 添加每个 AssetBundle 信息
            AssetBundleFileInfo info = new AssetBundleFileInfo
            {
                //name = bundle,
                //path = path,
                //rootPath = directoryPath,
                //size = new FileInfo(path).Length,
                //directDepends = assetBundleManifest.GetDirectDependencies(bundle),
                //allDepends = assetBundleManifest.GetAllDependencies(bundle)
            };
            infos.Add(info);

            return infos;
        };
    }
}
