using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
#if UNITY_5 || UNITY_5_3_OR_NEWER
using UnityEditor.Animations;
#else
using UnityEditorInternal;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace WuHuan
{
    /// <summary>
    /// AB 文件分析器
    /// </summary>
    public static class AssetBundleFilesAnalyze
    {
        #region 对外接口

        /// <summary>
        /// 自定义分析依赖
        /// </summary>
        public static System.Func<string, List<AssetBundleFileInfo>> analyzeCustomDepend;

        /// <summary>
        /// 分析的时候，也导出资源
        /// </summary>
        public static bool analyzeExport { get; set; }

        /// <summary>
        /// 分析的时候，只分析场景，这需要播放运行才能分析场景
        /// </summary>
        public static bool analyzeOnlyScene { get; set; }

        #endregion

        #region 内部实现

        private static List<AssetBundleFileInfo> sAssetBundleFileInfos;
        private static Dictionary<long, AssetFileInfo> sAssetFileInfos;
        private static AssetBundleFilesAnalyzeScene sAnalyzeScene;

        public static UnityAction analyzeCompleted;

        /// <summary>
        /// 获取所有的AB文件信息
        /// </summary>
        /// <returns></returns>
        public static List<AssetBundleFileInfo> GetAllAssetBundleFileInfos()
        {
            return sAssetBundleFileInfos;
        }

        public static AssetBundleFileInfo GetAssetBundleFileInfo(string name)
        {
            return sAssetBundleFileInfos.Find(info => info.name == name);
        }

        /// <summary>
        /// 获取所有的资产文件信息
        /// </summary>
        /// <returns></returns>
        public static Dictionary<long, AssetFileInfo> GetAllAssetFileInfo()
        {
            return sAssetFileInfos;
        }

        public static AssetFileInfo GetAssetFileInfo(long guid)
        {
            if (sAssetFileInfos == null)
            {
                sAssetFileInfos = new Dictionary<long, AssetFileInfo>();
            }

            AssetFileInfo info;
            if (!sAssetFileInfos.TryGetValue(guid, out info))
            {
                info = new AssetFileInfo { guid = guid };
                sAssetFileInfos.Add(guid, info);
            }
            return info;
        }

        public static void Clear()
        {
            if (sAssetBundleFileInfos != null)
            {
                sAssetBundleFileInfos.Clear();
                sAssetBundleFileInfos = null;
            }
            if (sAssetFileInfos != null)
            {
                sAssetFileInfos.Clear();
                sAssetFileInfos = null;
            }
            sAnalyzeScene = null;

#if UNITY_5 || UNITY_5_3_OR_NEWER
            EditorUtility.UnloadUnusedAssetsImmediate();
#endif
            System.GC.Collect();
        }

        public static bool Analyze(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Debug.LogError(directoryPath + " is not exists!");
                return false;
            }

            if (analyzeCustomDepend != null)
            {
                sAssetBundleFileInfos = analyzeCustomDepend(directoryPath);
            }
            if (sAssetBundleFileInfos == null)
            {
#if UNITY_5 || UNITY_5_3_OR_NEWER
                sAssetBundleFileInfos = AnalyzeManifestDepend(directoryPath);
#endif
            }
            if (sAssetBundleFileInfos == null)
            {
                sAssetBundleFileInfos = AnalyzAllFiles(directoryPath);
            }
            if (sAssetBundleFileInfos == null)
            {
                return false;
            }

            sAnalyzeScene = new AssetBundleFilesAnalyzeScene();
            AnalyzeBundleFiles(sAssetBundleFileInfos);
            sAnalyzeScene.Analyze();

            if (!sAnalyzeScene.IsAnalyzing())
            {
                if (analyzeCompleted != null)
                {
                    analyzeCompleted();
                }
            }
            return true;
        }

        /// <summary>
        /// 分析Unity5方式的依赖构成
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        private static List<AssetBundleFileInfo> AnalyzeManifestDepend(string directoryPath)
        {
            string manifestName = Path.GetFileName(directoryPath);
            string manifestPath = Path.Combine(directoryPath, manifestName);
            if (!File.Exists(manifestPath))
            {
                Debug.LogWarning(manifestPath + " is not exists! Use AnalyzAllFiles ...");
                return null;
            }
#if UNITY_5_3_OR_NEWER
            AssetBundle manifestAb = AssetBundle.LoadFromFile(manifestPath);
#else
            AssetBundle manifestAb = AssetBundle.CreateFromMemoryImmediate(File.ReadAllBytes(manifestPath));
#endif
            if (!manifestAb)
            {
                Debug.LogError(manifestPath + " ab load faild!");
                return null;
            }

            List<AssetBundleFileInfo> infos = new List<AssetBundleFileInfo>();
#if UNITY_5 || UNITY_5_3_OR_NEWER
            AssetBundleManifest assetBundleManifest = manifestAb.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
            var bundles = assetBundleManifest.GetAllAssetBundles();
            foreach (var bundle in bundles)
            {
                string path = Path.Combine(directoryPath, bundle);
                AssetBundleFileInfo info = new AssetBundleFileInfo
                {
                    name = bundle,
                    path = path,
                    rootPath = directoryPath,
                    size = new FileInfo(path).Length,
                    directDepends = assetBundleManifest.GetDirectDependencies(bundle),
                    allDepends = assetBundleManifest.GetAllDependencies(bundle)
                };
                infos.Add(info);
            }
#endif
            manifestAb.Unload(true);
            return infos;
        }

        /// <summary>
        /// 直接递归所有文件
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        private static List<AssetBundleFileInfo> AnalyzAllFiles(string directoryPath)
        {
            List<AssetBundleFileInfo> infos = new List<AssetBundleFileInfo>();
            string bom = "Unity";
            char[] flag = new char[5];
            string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                using (StreamReader streamReader = new StreamReader(file))
                {
                    if (streamReader.Read(flag, 0, flag.Length) == flag.Length && new string(flag) == bom)
                    {
                        AssetBundleFileInfo info = new AssetBundleFileInfo
                        {
                            name = file.Substring(directoryPath.Length + 1),
                            path = file,
                            rootPath = directoryPath,
                            size = streamReader.BaseStream.Length,
                            directDepends = new string[] { },
                            allDepends = new string[] { }
                        };
                        infos.Add(info);
                    }
                }
            }

            return infos;
        }

        private static void AnalyzeBundleFiles(List<AssetBundleFileInfo> infos)
        {
            // 分析被依赖的关系
            foreach (var info in infos)
            {
                List<string> beDepends = new List<string>();
                foreach (var info2 in infos)
                {
                    if (info2.name == info.name)
                    {
                        continue;
                    }

                    if (info2.allDepends.Contains(info.name))
                    {
                        beDepends.Add(info2.name);
                    }
                }
                info.beDepends = beDepends.ToArray();
            }

            // 以下不能保证百分百找到所有的资源，最准确的方式是解密AssetBundle格式
            foreach (var info in infos)
            {
#if UNITY_5_3_OR_NEWER
                AssetBundle ab = AssetBundle.LoadFromFile(info.path);
#else
                AssetBundle ab = AssetBundle.CreateFromMemoryImmediate(File.ReadAllBytes(info.path));
#endif
                if (ab)
                {
                    try
                    {
#if UNITY_5_3_OR_NEWER
                        if (!ab.isStreamedSceneAssetBundle)
#else
                        if (true)
#endif
                        {
                            if (!analyzeOnlyScene)
                            {
#if UNITY_5 || UNITY_5_3_OR_NEWER
                                Object[] objs = ab.LoadAllAssets<Object>();
#else
                                Object[] objs = ab.LoadAll();
#endif
                                foreach (var o in objs)
                                {
                                    AnalyzeObjectReference(info, o);
                                    AnalyzeObjectComponent(info, o);
                                }
                                AnalyzeObjectsCompleted(info);
                            }
                        }
                        else
                        {
#if UNITY_5_3_OR_NEWER
                            info.isScene = true;
                            sAnalyzeScene.AddBundleSceneInfo(info, ab.GetAllScenePaths());
#endif
                        }
                    }
                    finally
                    {
                        ab.Unload(true);
                    }
                }
            }
        }

        private static PropertyInfo inspectorMode;

        /// <summary>
        /// 分析对象的引用
        /// </summary>
        /// <param name="info"></param>
        /// <param name="o"></param>
        public static void AnalyzeObjectReference(AssetBundleFileInfo info, Object o)
        {
            if (o == null || info.objDict.ContainsKey(o))
            {
                return;
            }

            var serializedObject = new SerializedObject(o);
            info.objDict.Add(o, serializedObject);

            if (inspectorMode == null)
            {
                inspectorMode = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            inspectorMode.SetValue(serializedObject, InspectorMode.Debug, null);

            var it = serializedObject.GetIterator();
            while (it.NextVisible(true))
            {
                if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null)
                {
                    AnalyzeObjectReference(info, it.objectReferenceValue);
                }
            }

            // 只能用另一种方式获取的引用
            AnalyzeObjectReference2(info, o);
        }

        /// <summary>
        /// 动画控制器比较特殊，不能通过序列化得到
        /// </summary>
        /// <param name="info"></param>
        /// <param name="o"></param>
        private static void AnalyzeObjectReference2(AssetBundleFileInfo info, Object o)
        {
            AnimatorController ac = o as AnimatorController;
            if (ac)
            {
#if UNITY_5 || UNITY_5_3_OR_NEWER
                foreach (var clip in ac.animationClips)
                {
                    AnalyzeObjectReference(info, clip);
                }
#else
                List<State> list = new List<State>();
                for (int i = 0; i < ac.layerCount; i++)
                {
                    AnimatorControllerLayer layer = ac.GetLayer(i);
                    list.AddRange(AnimatorStateMachine_StatesRecursive(layer.stateMachine));
                }
                foreach (State state in list)
                {
                    var clip = state.GetMotion() as AnimationClip;
                    if (clip)
                    {
                        AnalyzeObjectReference(info, clip);
                    }
                }
#endif
            }
        }

#if !(UNITY_5 || UNITY_5_3_OR_NEWER)
        private static List<State> AnimatorStateMachine_StatesRecursive(StateMachine stateMachine)
        {
            List<State> list = new List<State>();
            for (int i = 0; i < stateMachine.stateCount; i++)
            {
                list.Add(stateMachine.GetState(i));
            }
            for (int i = 0; i < stateMachine.stateMachineCount; i++)
            {
                list.AddRange(AnimatorStateMachine_StatesRecursive(stateMachine.GetStateMachine(i)));
            }
            return list;
        }
#endif

        /// <summary>
        /// 分析脚本的引用（这只在脚本在工程里时才有效）
        /// </summary>
        /// <param name="info"></param>
        /// <param name="o"></param>
        public static void AnalyzeObjectComponent(AssetBundleFileInfo info, Object o)
        {
            var go = o as GameObject;
            if (!go)
            {
                return;
            }

            var components = go.GetComponentsInChildren<Component>(true);
            foreach (var component in components)
            {
                if (!component)
                {
                    continue;
                }

                AnalyzeObjectReference(info, component);
            }
        }

        public static void AnalyzeObjectsCompleted(AssetBundleFileInfo info)
        {
            foreach (var kv in info.objDict)
            {
                AssetBundleFilesAnalyzeObject.ObjectAddToFileInfo(kv.Key, kv.Value, info);
                kv.Value.Dispose();
            }
            info.objDict.Clear();
        }

        #endregion
    }
}