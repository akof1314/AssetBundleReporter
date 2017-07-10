using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WuHuan
{
    /// <summary>
    /// AB 文件分析器
    /// </summary>
    public static class AssetBundleFilesAnalyze
    {
        private static List<AssetBundleFileInfo> sAssetBundleFileInfos;
        private static Dictionary<int, AssetFileInfo> sAssetFileInfos;

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
        public static Dictionary<int, AssetFileInfo> GetAllAssetFileInfo()
        {
            return sAssetFileInfos;
        }

        public static AssetFileInfo GetAssetFileInfo(int guid)
        {
            if (sAssetFileInfos == null)
            {
                sAssetFileInfos = new Dictionary<int, AssetFileInfo>();
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

            EditorUtility.UnloadUnusedAssetsImmediate();
            System.GC.Collect();
        }

        public static bool Analyze(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Debug.LogError(directoryPath + " is not exists!");
                return false;
            }

            sAssetBundleFileInfos = AnalyzeCustomDepend(directoryPath);
            if (sAssetBundleFileInfos == null)
            {
                sAssetBundleFileInfos = AnalyzeManifestDepend(directoryPath);
            }
            if (sAssetBundleFileInfos == null)
            {
                return false;
            }

            AnalyzeBundleFiles(sAssetBundleFileInfos);
            return true;
        }

        /// <summary>
        /// 自定义分析依赖构成
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        private static List<AssetBundleFileInfo> AnalyzeCustomDepend(string directoryPath)
        {
            return null;
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
                Debug.LogError(manifestPath + " is not exists!");
                return null;
            }

            AssetBundle manifestAb = AssetBundle.LoadFromFile(manifestPath);
            if (!manifestAb)
            {
                Debug.LogError(manifestPath + " ab load faild!");
                return null;
            }

            Debug.Log("parse assetbundlemanifest");
            List<AssetBundleFileInfo> infos = new List<AssetBundleFileInfo>();
            AssetBundleManifest assetBundleManifest = manifestAb.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
            var bundles = assetBundleManifest.GetAllAssetBundles();
            foreach (var bundle in bundles)
            {
                AssetBundleFileInfo info = new AssetBundleFileInfo
                {
                    name = bundle,
                    path = Path.Combine(directoryPath, bundle),
                    rootPath = directoryPath,
                    directDepends = assetBundleManifest.GetDirectDependencies(bundle),
                    allDepends = assetBundleManifest.GetAllDependencies(bundle)
                };
                infos.Add(info);
            }
            manifestAb.Unload(true);
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
            Debug.Log("parse all assetbundle");
            foreach (var info in infos)
            {
                AssetBundle ab = AssetBundle.LoadFromFile(info.path);
                if (ab)
                {
                    try
                    {
                        if (!ab.isStreamedSceneAssetBundle)
                        {
                            Object[] objs = ab.LoadAllAssets<Object>();
                            foreach (var o in objs)
                            {
                                AssetBundleFilesAnalyzeObject.ObjectAddToFileInfo(o, info);
                            }

                            // 处理被依赖打包进的资源
                            foreach (var o in objs)
                            {
                                AnalyzeObjectReference(info, o);
                                AnalyzeComponent(info, o);
                            }
                        }
                        else
                        {
                            List<GameObject> gos = new List<GameObject>();
                            foreach (var scenePath in ab.GetAllScenePaths())
                            {
                                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                                scene.GetRootGameObjects(gos);
                                foreach (var go in gos)
                                {
                                    Debug.Log(go.name);
                                }
                                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                            }
                        }
                    }
                    finally
                    {
                        ab.Unload(true);
                    }
                }
            }

            Debug.Log("parse all assetbundle succeed");
        }

        /// <summary>
        /// 分析对象的引用
        /// </summary>
        /// <param name="info"></param>
        /// <param name="o"></param>
        private static void AnalyzeObjectReference(AssetBundleFileInfo info, Object o)
        {
            var serializedObject = new SerializedObject(o);
            var it = serializedObject.GetIterator();
            while (it.NextVisible(true))
            {
                if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null)
                {
                    AssetBundleFilesAnalyzeObject.ObjectAddToFileInfo(it.objectReferenceValue, info);

                    AnalyzeObjectReference(info, it.objectReferenceValue);
                }
            }
            serializedObject.Dispose();
        }

        /// <summary>
        /// 分析脚本的引用（这只在脚本在工程里时才有效）
        /// </summary>
        /// <param name="info"></param>
        /// <param name="o"></param>
        private static void AnalyzeComponent(AssetBundleFileInfo info, Object o)
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

                if (component as MonoBehaviour)
                {
                    string type = component.GetType().ToString();
                    if (!type.StartsWith("UnityEngine."))
                    {
                        AnalyzeObjectReference(info, component);
                    }
                }
                else
                {
                    AnalyzeObjectReference(info, component);
                    AnalyzeAnimator(info, component);
                }
            }
        }

        private static void AnalyzeAnimator(AssetBundleFileInfo info, Object o)
        {
            var animator = o as Animator;
            if (!animator)
            {
                return;
            }

            RuntimeAnimatorController rac = animator.runtimeAnimatorController;
            if (!rac)
            {
                return;
            }

            AnimatorOverrideController aoc = rac as AnimatorOverrideController;
            if (aoc)
            {
                AssetBundleFilesAnalyzeObject.ObjectAddToFileInfo(aoc.runtimeAnimatorController, info);

                try
                {
                    foreach (var clipPair in aoc.clips)
                    {
                        AssetBundleFilesAnalyzeObject.ObjectAddToFileInfo(clipPair.originalClip, info);
                        AssetBundleFilesAnalyzeObject.ObjectAddToFileInfo(clipPair.overrideClip, info);
                    }
                }
                catch (System.Exception)
                {
                    foreach (var clip in rac.animationClips)
                    {
                        AssetBundleFilesAnalyzeObject.ObjectAddToFileInfo(clip, info);
                    }
                }
            }
            else
            {
                foreach (var clip in rac.animationClips)
                {
                    AssetBundleFilesAnalyzeObject.ObjectAddToFileInfo(clip, info);
                }
            }
        }
    }
}