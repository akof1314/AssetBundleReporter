using System.Collections.Generic;
using System.IO;
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

    public static void Analyze(string directoryPath, string abExt = ".assetbundle")
    {
        if (!Directory.Exists(directoryPath))
        {
            Debug.LogError(directoryPath + " is not exists!");
            return;
        }

        sAssetBundleFileInfos = new List<AssetBundleFileInfo>();

        var files = Directory.GetFiles(directoryPath, "*" + abExt, SearchOption.AllDirectories);
        foreach (var file in files)
        {
            AssetBundle ab = AssetBundle.LoadFromFile(file);
            if (ab)
            {
                try
                {
                    AssetBundleFileInfo info = new AssetBundleFileInfo
                    {
                        name = ab.name,
                        path = file
                    };

                    Object[] objs = ab.LoadAllAssets<Object>();
                    foreach (var o in objs)
                    {
                        AssetFileInfo info2 = new AssetFileInfo
                        {
                            name = o.name,
                            guid = o.GetInstanceID(),
                            type = o.GetType().ToString()
                        };
                        info2.includedBundles.Add(info.name);
                        if (info2.type.StartsWith("UnityEngine."))
                        {
                            info2.type = info2.type.Substring(12);
                        }

                        info.assets.Add(info2);
                    }

                    sAssetBundleFileInfos.Add(info);
                }
                finally
                {
                    ab.Unload(true);
                }
            }
        }
    }
}