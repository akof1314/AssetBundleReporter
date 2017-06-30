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
                    Object[] objs = ab.LoadAllAssets<Object>();
                    foreach (var o in objs)
                    {
                        info.AddAsset(o);
                    }

                    // 处理被依赖打包进的资源
                    foreach (var o in objs)
                    {
                        AnalyzeObjectReference(info, o);
                        //string type = o.GetType().ToString();
                        //switch (type)
                        //{
                        //    case "UnityEngine.Material":
                        //        AnalyzeMaterial(info, o);
                        //        break;
                        //}
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

    private static void AnalyzeMaterial(AssetBundleFileInfo info, Object o)
    {
        var mat = o as Material;
        if (!mat)
        {
            return;
        }

        var shader = mat.shader;
        if (shader)
        {
            info.AddAsset(shader);

            SerializedObject serializedObject = new SerializedObject(shader);
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                Debug.LogWarning(iterator.name);
            }
            serializedObject.Dispose();
            serializedObject = null;
        }

        var pros = MaterialEditor.GetMaterialProperties(new Object[] {mat});
        foreach (var pro in pros)
        {
            var tex = pro.textureValue;
            if (tex)
            {
                info.AddAsset(tex);
            }
        }
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