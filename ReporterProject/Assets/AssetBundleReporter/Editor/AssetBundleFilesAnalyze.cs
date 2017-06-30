using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AssetBundleFilesAnalyze
{
    private static List<AssetBundleFileInfo> sAssetBundleFileInfos;

    public static List<AssetBundleFileInfo> GetAllAssetBundleFileInfos()
    {
        return sAssetBundleFileInfos;
    }

    public static void Clear()
    {
        if (sAssetBundleFileInfos != null)
        {
            sAssetBundleFileInfos.Clear();
            sAssetBundleFileInfos = null;
        }
    }

    public static bool Analyze(string directoryPath, string abExt = ".assetbundle")
    {
        if (!Directory.Exists(directoryPath))
        {
            Debug.LogError(directoryPath + " is not exists!");
            return false;
        }

        string manifestName = Path.GetFileName(directoryPath);
        string manifestPath = Path.Combine(directoryPath, manifestName);
        if (!File.Exists(manifestPath))
        {
            Debug.LogError(manifestPath + " is not exists!");
            return false;
        }

        AssetBundle manifestAb = AssetBundle.LoadFromFile(manifestPath);
        if (!manifestAb)
        {
            Debug.LogError(manifestPath + " ab load faild!");
            return false;
        }

        Debug.Log("parse assetbundlemanifest");
        sAssetBundleFileInfos = new List<AssetBundleFileInfo>();
        AssetBundleManifest assetBundleManifest = manifestAb.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
        var bundles = assetBundleManifest.GetAllAssetBundles();
        foreach (var bundle in bundles)
        {
            AssetBundleFileInfo info = new AssetBundleFileInfo
            {
                name = bundle,
                path = Path.Combine(directoryPath, bundle),
                directDepends = assetBundleManifest.GetDirectDependencies(bundle),
                allDepends = assetBundleManifest.GetAllDependencies(bundle)
            };
            sAssetBundleFileInfos.Add(info);
        }
        manifestAb.Unload(true);

        // 以下不能保证百分百找到所有的资源，最准确的方式是解密AssetBundle格式
        Debug.Log("parse all assetbundle");
        foreach (var info in sAssetBundleFileInfos)
        {
            AssetBundle ab = AssetBundle.LoadFromFile(info.path);
            if (ab)
            {
                try
                {
                    if (!ab.isStreamedSceneAssetBundle)
                    {
                        Object[] objs = ab.LoadAllAssets<Object>();
                        foreach (var o in objs)
                        {
                            info.AddAsset(o);
                        }

                        // 处理被依赖打包进的资源
                        foreach (var o in objs)
                        {
                            AnalyzeObjectReference(info, o);
                        }
                    }
                }
                finally
                {
                    ab.Unload(true);
                }
            }
        }

        Debug.Log("parse all assetbundle succeed");
        return true;
    }

    private static void AnalyzeObjectReference(AssetBundleFileInfo info, Object o)
    {
        var serializedObject = new SerializedObject(o);
        var it = serializedObject.GetIterator();
        while (it.NextVisible(true))
        {
            if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null)
            {
                info.AddAsset(it.objectReferenceValue);

                AnalyzeObjectReference(info, it.objectReferenceValue);
            }
        }
        serializedObject.Dispose();
    }
}