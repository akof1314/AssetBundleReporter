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
    /// 直接依赖的AssetBundle列表
    /// </summary>
    public string[] directDepends;

    /// <summary>
    /// 所有依赖的AssetBundle列表
    /// </summary>
    public string[] allDepends;

    /// <summary>
    /// 所有被依赖的AssetBundle列表
    /// </summary>
    public string[] beDepends;

    /// <summary>
    /// 包含的资源名称
    /// </summary>
    public List<AssetFileInfo> assets = new List<AssetFileInfo>();

    /// <summary>
    /// Excel 工作簿的详细链接
    /// </summary>
    public OfficeOpenXml.ExcelHyperLink detailHyperLink;

    public override string ToString()
    {
        return name;
    }

    /// <summary>
    /// 获取相同类型的资产数量
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public int GetAssetCount(string type)
    {
        int count = 0;
        foreach (var info in assets)
        {
            if (info.type == type)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 添加资产
    /// </summary>
    /// <param name="o"></param>
    public void AddAsset(Object o)
    {
        int guid = o.GetInstanceID();

        foreach (var asset in assets)
        {
            if (asset.guid == guid)
            {
                return;
            }
        }

        AssetFileInfo info2 = new AssetFileInfo
        {
            name = o.name,
            guid = guid,
            type = o.GetType().ToString()
        };
        if (info2.type.StartsWith("UnityEngine."))
        {
            info2.type = info2.type.Substring(12);
        }
        else
        {
            // FIXME 其他认为都是脚本？
            info2.name = info2.type;
            info2.type = AssetFileInfoType.monoScript;
        }

        assets.Add(info2);
    }
}