using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WuHuan
{
    /// <summary>
    /// AB 文件信息
    /// </summary>
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
        /// 根路径
        /// </summary>
        public string rootPath;

        /// <summary>
        /// 文件大小
        /// </summary>
        public long size;

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

        /// <summary>
        /// 包含的全部对象字典（方便不会重复添加）
        /// </summary>
        public Dictionary<Object, SerializedObject> objDict = new Dictionary<Object, SerializedObject>();

        /// <summary>
        /// 是否是场景 AB 包
        /// </summary>
        public bool isScene;

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

        public bool IsAssetContain(long guid)
        {
            foreach (var asset in assets)
            {
                if (asset.guid == guid)
                {
                    return true;
                }
            }
            return false;
        }
    }
}