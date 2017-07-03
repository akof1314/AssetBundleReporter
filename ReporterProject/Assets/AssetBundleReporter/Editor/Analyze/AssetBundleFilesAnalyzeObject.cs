using System.Collections.Generic;
using System.Reflection;
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
            if (!o)
            {
                return;
            }

            string name2 = o.name;
            string type = o.GetType().ToString();
            if (type.StartsWith("UnityEngine."))
            {
                type = type.Substring(12);
            }
            else if (type == "UnityEditor.Animations.AnimatorController")
            {
                type = "AnimatorController";
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
            if (info2.detailHyperLink == null)
            {
                // 初次创建对象时链接为空
                info2.detailHyperLink = new OfficeOpenXml.ExcelHyperLink(System.String.Empty, info2.name);
                info2.propertys = AnalyzeObject(o);
            }

            info.assets.Add(info2);
        }

        private static List<KeyValuePair<string, object>> AnalyzeObject(Object o)
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

            Material mat = o as Material;
            if (mat)
            {
                return AnalyzeMaterial(mat);
            }

            AnimationClip clip = o as AnimationClip;
            if (clip)
            {
                return AnalyzeAnimationClip(clip);
            }

            return null;
        }

        private static List<KeyValuePair<string, object>> AnalyzeTexture2D(Texture2D tex)
        {
            var propertys = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("宽度", tex.width),
                new KeyValuePair<string, object>("高度", tex.height),
                new KeyValuePair<string, object>("格式", tex.format.ToString()),
                new KeyValuePair<string, object>("MipMap功能", tex.mipmapCount > 1 ? "True" : "False")
            };

            var serializedObject = new SerializedObject(tex);

            var property = serializedObject.FindProperty("m_IsReadable");
            propertys.Add(new KeyValuePair<string, object>("Read/Write", property.boolValue.ToString()));

            property = serializedObject.FindProperty("m_CompleteImageSize");
            propertys.Add(new KeyValuePair<string, object>("内存占用", property.intValue));

            serializedObject.Dispose();
            return propertys;
        }

        private static List<KeyValuePair<string, object>> AnalyzeMesh(Mesh mesh)
        {
            var propertys = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("顶点数", mesh.vertexCount),
                new KeyValuePair<string, object>("面数", (mesh.triangles.Length / 3f)),
                new KeyValuePair<string, object>("子网格数", mesh.subMeshCount),
                new KeyValuePair<string, object>("网格压缩", MeshUtility.GetMeshCompression(mesh).ToString()),
                new KeyValuePair<string, object>("Read/Write", mesh.isReadable.ToString())
            };
            return propertys;
        }

        private static List<KeyValuePair<string, object>> AnalyzeMaterial(Material mat)
        {
            var propertys = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("依赖Shader", mat.shader? mat.shader.name: System.String.Empty),
            };

            string texNames = System.String.Empty;
            var pros = MaterialEditor.GetMaterialProperties(new Object[] { mat });
            foreach (var pro in pros)
            {
                var tex = pro.textureValue;
                if (tex)
                {
                    texNames += tex.name;
                }
            }
            propertys.Add(new KeyValuePair<string, object>("依赖纹理", texNames));
            return propertys;
        }

        private static List<KeyValuePair<string, object>> AnalyzeAnimationClip(AnimationClip clip)
        {
            var stats = AnimationClipStatsInfo.GetAnimationClipStats(clip);
            var propertys = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("总曲线数", stats.totalCurves),
                new KeyValuePair<string, object>("Constant曲线数", stats.constantCurves),
                new KeyValuePair<string, object>("Dense曲线数", stats.denseCurves),
                new KeyValuePair<string, object>("Stream曲线数", stats.streamCurves),
                new KeyValuePair<string, object>("事件数", clip.events.Length),
                new KeyValuePair<string, object>("内存占用", stats.size),
            };
            return propertys;
        }

        private class AnimationClipStatsInfo
        {
            public int size;
            public int totalCurves;
            public int constantCurves;
            public int denseCurves;
            public int streamCurves;

            private static MethodInfo getAnimationClipStats;
            private static FieldInfo sizeInfo;
            private static FieldInfo totalCurvesInfo;
            private static FieldInfo constantCurvesInfo;
            private static FieldInfo denseCurvesInfo;
            private static FieldInfo streamCurvesInfo;

            public static AnimationClipStatsInfo GetAnimationClipStats(AnimationClip clip)
            {
                if (getAnimationClipStats == null)
                {
                    getAnimationClipStats = typeof(AnimationUtility).GetMethod("GetAnimationClipStats", BindingFlags.Static | BindingFlags.NonPublic);
                    var aniclipstats = typeof(AnimationUtility).Assembly.GetType("UnityEditor.AnimationClipStats");
                    sizeInfo = aniclipstats.GetField("size", BindingFlags.Public | BindingFlags.Instance);
                    totalCurvesInfo = aniclipstats.GetField("totalCurves", BindingFlags.Public | BindingFlags.Instance);
                    constantCurvesInfo = aniclipstats.GetField("constantCurves", BindingFlags.Public | BindingFlags.Instance);
                    denseCurvesInfo = aniclipstats.GetField("denseCurves", BindingFlags.Public | BindingFlags.Instance);
                    streamCurvesInfo = aniclipstats.GetField("streamCurves", BindingFlags.Public | BindingFlags.Instance);
                }

                var stats = getAnimationClipStats.Invoke(null, new object[] { clip });
                var stats2 = new AnimationClipStatsInfo
                {
                    size = (int)sizeInfo.GetValue(stats),
                    totalCurves = (int)totalCurvesInfo.GetValue(stats),
                    constantCurves = (int)constantCurvesInfo.GetValue(stats),
                    denseCurves = (int)denseCurvesInfo.GetValue(stats),
                    streamCurves = (int)streamCurvesInfo.GetValue(stats),
                };
                return stats2;
            }
        }
    }
}