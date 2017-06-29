using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class AssetBundleFilesAnalyze
{
    public static void Analyze(string directoryPath, string abExt = ".assetbundle")
    {
        if (!Directory.Exists(directoryPath))
        {
            Debug.LogError(directoryPath + " is not exists!");
            return;
        }

        var files = Directory.GetFiles(directoryPath, "*" + abExt, SearchOption.AllDirectories);
        foreach (var file in files)
        {
             AssetBundle.LoadFromFile(file);
        }
    }
}