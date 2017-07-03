using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WuHuan
{
    /// <summary>
    /// 分析对象
    /// </summary>
    public class AssetBundleFilesAnalyzeObject
    {
        public static void ObjectAddToFileInfo(Object o, AssetBundleFileInfo info)
        {
            string name2 = o.name;
            string type = o.GetType().ToString();
            if (type.StartsWith("UnityEngine."))
            {
                type = type.Substring(12);
            }
            else
            {
                // FIXME 其他认为都是脚本？
                name2 = type;
                type = AssetFileInfoType.monoScript;
            }

            int guid = (name2 + type).GetHashCode();
            if (info.IsAssetContain(guid))
            {
                return;
            }

            AssetFileInfo info2 = AssetBundleFilesAnalyze.GetAssetFileInfo(guid);
            info2.name = name2;
            info2.type = type;
            info2.includedBundles.Add(info);
            info2.propertys = AnalyzeObject(o);

            info.assets.Add(info2);
        }

        private static List<KeyValuePair<string, string>> AnalyzeObject(Object o)
        {
            Texture2D tex = o as Texture2D;
            if (tex)
            {
                return AnalyzeTexture2D(tex);
            }

            Mesh mesh = o as Mesh;
            if (mesh)
            {
                return AnalyzeMesh(mesh);
            }

            return null;
        }

        private static List<KeyValuePair<string, string>> AnalyzeTexture2D(Texture2D tex)
        {
            var propertys = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("宽度", tex.width.ToString()),
                new KeyValuePair<string, string>("高度", tex.height.ToString()),
                new KeyValuePair<string, string>("格式", tex.format.ToString()),
                new KeyValuePair<string, string>("MipMap功能", tex.mipmapCount > 1 ? "True" : "False")
            };

            var serializedObject = new SerializedObject(tex);

            var property = serializedObject.FindProperty("m_IsReadable");
            propertys.Add(new KeyValuePair<string, string>("Read/Write", property.boolValue.ToString()));

            property = serializedObject.FindProperty("m_CompleteImageSize");
            propertys.Add(new KeyValuePair<string, string>("内存占用", EditorUtility.FormatBytes(property.intValue)));

            serializedObject.Dispose();
            return propertys;
        }

        private static List<KeyValuePair<string, string>> AnalyzeMesh(Mesh mesh)
        {
            var propertys = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("顶点数", mesh.vertexCount.ToString()),
                new KeyValuePair<string, string>("面数", (mesh.triangles.Length / 3f).ToString()),
                new KeyValuePair<string, string>("子网格数", mesh.subMeshCount.ToString()),
                new KeyValuePair<string, string>("网格压缩", MeshUtility.GetMeshCompression(mesh).ToString()),
                new KeyValuePair<string, string>("Read/Write", mesh.isReadable.ToString())
            };
            return propertys;
        }
    }
}