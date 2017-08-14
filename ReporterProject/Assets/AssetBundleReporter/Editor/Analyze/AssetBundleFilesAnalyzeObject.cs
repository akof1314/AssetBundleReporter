using System.Collections.Generic;
using System.IO;
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
        public static void ObjectAddToFileInfo(Object o, SerializedObject serializedObject, AssetBundleFileInfo info)
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
                
                // 如果是内置的组件，就不当作是资源
                if (o as Component)
                {
                    return;
                }
            }
            else if (type == "UnityEditor.Animations.AnimatorController")
            {
                type = "AnimatorController";
            }
            else if (type == "UnityEditorInternal.AnimatorController")
            {
                type = "AnimatorController";
            }
            else if (type == "UnityEditor.MonoScript")
            {
                MonoScript ms = o as MonoScript;
                string type2 = ms.GetClass().ToString();
                if (type2.StartsWith("UnityEngine."))
                {
                    // 内置的脚本对象也不显示出来
                    return;
                }

                // 外部的组件脚本，保留下来
                type = "MonoScript";
            }
            else
            {
                // 外部的组件脚本，走上面的MonoScript
                if (o as Component)
                {
                    return;
                }
                // 外部的序列化对象，已经被脚本给分析完毕了，不需要再添加进来
                if (o as ScriptableObject)
                {
                    return;
                }

                Debug.LogError("What's this? " + type);
                return;
            }

            // 内建的资源排除掉
            string assetPath = AssetDatabase.GetAssetPath(o);
            if (!string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            long guid;
            if (info.isScene)
            {
                // 场景的话，没法根据PathID来确定唯一性，那么就认为每个场景用到的资源都不一样
                guid = (info.name + name2 + type).GetHashCode();
            }
            else
            {
                SerializedProperty pathIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");
#if UNITY_5 || UNITY_5_3_OR_NEWER
                guid = pathIdProp.longValue;
#else
                guid = pathIdProp.intValue;
#endif
            }

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
                info2.propertys = AnalyzeObject(o, serializedObject, info.rootPath, info.name);
            }

            info.assets.Add(info2);
        }

        private static List<KeyValuePair<string, object>> AnalyzeObject(Object o, SerializedObject serializedObject, string rootPath, string name)
        {
            Texture2D tex = o as Texture2D;
            if (tex)
            {
                ExportTexture2D(tex, rootPath, name);
                return AnalyzeTexture2D(tex, serializedObject);
            }

            Mesh mesh = o as Mesh;
            if (mesh)
            {
                return AnalyzeMesh(mesh, serializedObject);
            }

            Material mat = o as Material;
            if (mat)
            {
                return AnalyzeMaterial(mat, serializedObject);
            }

            AudioClip audioClip = o as AudioClip;
            if (audioClip)
            {
                return AnalyzeAudioClip(audioClip, serializedObject);
            }

            AnimationClip clip = o as AnimationClip;
            if (clip)
            {
                return AnalyzeAnimationClip(clip, serializedObject);
            }

            return null;
        }

        private static List<KeyValuePair<string, object>> AnalyzeTexture2D(Texture2D tex, SerializedObject serializedObject)
        {
            var propertys = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("宽度", tex.width),
                new KeyValuePair<string, object>("高度", tex.height),
                new KeyValuePair<string, object>("格式", tex.format.ToString()),
                new KeyValuePair<string, object>("MipMap功能", tex.mipmapCount > 1 ? "True" : "False")
            };

            var property = serializedObject.FindProperty("m_IsReadable");
            propertys.Add(new KeyValuePair<string, object>("Read/Write", property.boolValue.ToString()));

            property = serializedObject.FindProperty("m_CompleteImageSize");
            propertys.Add(new KeyValuePair<string, object>("内存占用", property.intValue));

            return propertys;
        }

        private static List<KeyValuePair<string, object>> AnalyzeMesh(Mesh mesh, SerializedObject serializedObject)
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

        private static List<KeyValuePair<string, object>> AnalyzeMaterial(Material mat, SerializedObject serializedObject)
        {
            var propertys = new List<KeyValuePair<string, object>>
            {
            };

            string texNames = System.String.Empty;

            var property = serializedObject.FindProperty("m_Shader");
            propertys.Add(new KeyValuePair<string, object>("依赖Shader", property.objectReferenceValue ? property.objectReferenceValue.name : "[其他AB内]"));

            property = serializedObject.FindProperty("m_SavedProperties");
            var property2 = property.FindPropertyRelative("m_TexEnvs");
            foreach (SerializedProperty property3 in property2)
            {
                SerializedProperty property4 = property3.FindPropertyRelative("second");
                SerializedProperty property5 = property4.FindPropertyRelative("m_Texture");

                if (property5.objectReferenceValue)
                {
                    if (!string.IsNullOrEmpty(texNames))
                    {
                        texNames += ", ";
                    }
                    texNames += property5.objectReferenceValue.name;
                }
                else
                {
                    if (!string.IsNullOrEmpty(texNames))
                    {
                        texNames += ", ";
                    }
                    texNames += "[其他AB内]";
                }
            }
            propertys.Add(new KeyValuePair<string, object>("依赖纹理", texNames));

            return propertys;
        }

        private static List<KeyValuePair<string, object>> AnalyzeAudioClip(AudioClip audioClip, SerializedObject serializedObject)
        {
            var propertys = new List<KeyValuePair<string, object>>
            {
#if UNITY_5 || UNITY_5_3_OR_NEWER
                new KeyValuePair<string, object>("加载方式", audioClip.loadType.ToString()),
                new KeyValuePair<string, object>("预加载", audioClip.preloadAudioData.ToString()),
#endif
                new KeyValuePair<string, object>("频率", audioClip.frequency),
                new KeyValuePair<string, object>("长度", audioClip.length)
            };

#if UNITY_5 || UNITY_5_3_OR_NEWER
            var property = serializedObject.FindProperty("m_CompressionFormat");
            propertys.Add(new KeyValuePair<string, object>("格式", ((AudioCompressionFormat)property.intValue).ToString()));
#else
            var property = serializedObject.FindProperty("m_Stream");
            propertys.Add(new KeyValuePair<string, object>("加载方式", ((AudioImporterLoadType)property.intValue).ToString()));
            property = serializedObject.FindProperty("m_Type");
            propertys.Add(new KeyValuePair<string, object>("格式", ((AudioType)property.intValue).ToString()));
#endif

            return propertys;
        }

        private static List<KeyValuePair<string, object>> AnalyzeAnimationClip(AnimationClip clip, SerializedObject serializedObject)
        {
            var stats = AnimationClipStatsInfo.GetAnimationClipStats(clip);
            var propertys = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("总曲线数", stats.totalCurves),
                new KeyValuePair<string, object>("Constant曲线数", stats.constantCurves),
                new KeyValuePair<string, object>("Dense曲线数", stats.denseCurves),
                new KeyValuePair<string, object>("Stream曲线数", stats.streamCurves),
#if UNITY_5 || UNITY_5_3_OR_NEWER
                new KeyValuePair<string, object>("事件数", clip.events.Length),
#else
                new KeyValuePair<string, object>("事件数", AnimationUtility.GetAnimationEvents(clip).Length),
#endif
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

        private static void ExportTexture2D(Texture2D tex, string rootPath, string name)
        {
            if (!AssetBundleFilesAnalyze.analyzeExport)
            {
                return;
            }

            string dirPath = Path.Combine(Path.GetDirectoryName(rootPath), Path.GetFileNameWithoutExtension(rootPath) + "Export");
            dirPath = Path.Combine(dirPath, name);
            dirPath = Path.Combine(dirPath, AssetFileInfoType.texture2D);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            RenderTexture rt = RenderTexture.GetTemporary(tex.width, tex.height, 0);
            Graphics.Blit(tex, rt);

            RenderTexture active = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D cont = new Texture2D(tex.width, tex.height);
            cont.hideFlags = HideFlags.HideAndDontSave;
            cont.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            cont.Apply();
            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(rt);

            File.WriteAllBytes(Path.Combine(dirPath, tex.name + ".png"), cont.EncodeToPNG());
        }
    }
}