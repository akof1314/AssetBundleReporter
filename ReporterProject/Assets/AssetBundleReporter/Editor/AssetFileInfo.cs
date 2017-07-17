using System.Collections.Generic;

namespace WuHuan
{
    /// <summary>
    /// 资产文件信息
    /// </summary>
    public class AssetFileInfo
    {
        /// <summary>
        /// 资产名称（有可能重名）
        /// </summary>
        public string name;

        /// <summary>
        /// 唯一ID 
        /// 需要取得 PathID 才能确保唯一性
        /// </summary>
        public long guid;

        /// <summary>
        /// 类型
        /// </summary>
        public string type;

        /// <summary>
        /// 属性
        /// </summary>
        public List<KeyValuePair<string, object>> propertys;

        /// <summary>
        /// 被包含所在的AssetBundle文件名称列表
        /// </summary>
        public HashSet<AssetBundleFileInfo> includedBundles = new HashSet<AssetBundleFileInfo>();

        /// <summary>
        /// Excel 工作簿的详细链接
        /// </summary>
        public OfficeOpenXml.ExcelHyperLink detailHyperLink;

        public override string ToString()
        {
            return name;
        }
    }
}