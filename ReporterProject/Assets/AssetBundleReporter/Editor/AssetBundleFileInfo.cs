using System.Collections.Generic;
using UnityEngine;

public class AssetBundleFileInfo
{
    /// <summary>
    /// 名称（不会重名）
    /// </summary>
    public string name;

    /// <summary>
    /// 文件路径
    /// </summary>
    public string path;

    /// <summary>
    /// 依赖的AssetBundle列表
    /// </summary>
    public List<string> depends;

    /// <summary>
    /// 包含的资源名称
    /// </summary>
    public List<AssetFileInfo> assets = new List<AssetFileInfo>();
}