using System.Collections.Generic;
using UnityEngine;

public class AssetFileInfo
{
    /// <summary>
    /// 资产名称（有可能重名）
    /// </summary>
    public string name;

    /// <summary>
    /// 唯一ID 
    /// 需要取得 PathID 才能确保唯一性
    /// 这里只能用名称和类型代替
    /// </summary>
    public int guid;

    /// <summary>
    /// 类型
    /// </summary>
    public string type;

    /// <summary>
    /// 被包含所在的AssetBundle文件名称列表
    /// </summary>
    public HashSet<AssetBundleFileInfo> includedBundles = new HashSet<AssetBundleFileInfo>();

    public override string ToString()
    {
        return name;
    }
}