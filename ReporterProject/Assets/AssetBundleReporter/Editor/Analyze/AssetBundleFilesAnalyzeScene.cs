using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WuHuan
{
    public class AssetBundleFilesAnalyzeScene
    {
        private class GlobalBehaviour : MonoBehaviour {}

        private class BundleSceneInfo
        {
            public AssetBundle ab;
            public string sceneName;
            public string scenePath;
            public AssetBundleFileInfo fileInfo;
        }

        private readonly Queue<BundleSceneInfo> m_BundleSceneInfos = new Queue<BundleSceneInfo>();
        private GlobalBehaviour m_GlobalBehaviour;

        public void AddBundleSceneInfo(AssetBundleFileInfo info, string[] scenePaths)
        {
            foreach (var scenePath in scenePaths)
            {
                m_BundleSceneInfos.Enqueue(new BundleSceneInfo()
                {
                    fileInfo = info,
                    sceneName = Path.GetFileNameWithoutExtension(scenePath),
                    scenePath = scenePath
                });
            }
        }

        public void Analyze()
        {
            if (m_BundleSceneInfos.Count > 0)
            {
                AnalyzeInter();
            }
        }

        public bool IsAnalyzing()
        {
            return m_BundleSceneInfos.Count > 0;
        }

        private void AnalyzeInter()
        {
            if (EditorApplication.isPlaying)
            {
                AnalyzeBundleScenePrepare();
            }
            else
            {
                Debug.LogWarning("没有处在播放模式，将会放弃解析场景AssetBundle包！");
                ClearAllBundleScenes();

                // 如果不在播放模式，需要InitializeOnLoad方式监听播放模式改变
                //EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
                //EditorApplication.isPaused = false;
                //EditorApplication.isPlaying = true;
            }
        }

        private void AnalyzeBundleScenePrepare()
        {
            Scene defaultScene = SceneManager.CreateScene("empty" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            int sceneCount = SceneManager.sceneCount;
            for (int i = sceneCount - 1; i >= 0; i--)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene != defaultScene)
                {
                    SceneManager.UnloadScene(scene);
                }
            }
            SceneManager.SetActiveScene(defaultScene);
            GameObject go = new GameObject("Global");
            m_GlobalBehaviour = go.AddComponent<GlobalBehaviour>();

            SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
            SceneManager.sceneUnloaded += SceneManagerOnSceneUnloaded;
            LoadNextBundleScene();
        }

        private void PlaymodeStateChanged()
        {
            if (EditorApplication.isPaused)
            {
                return;
            }
            if (!(EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode))
            {
                return;
            }
            EditorApplication.playmodeStateChanged -= PlaymodeStateChanged;
            AnalyzeBundleScenePrepare();
        }

        private void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            // 并没有真正加载完整，需要下一帧才能取到对象
            m_GlobalBehaviour.StartCoroutine(AnalyzeBundleScene(scene));
        }

        private void SceneManagerOnSceneUnloaded(Scene scene)
        {
            BundleSceneInfo info = m_BundleSceneInfos.Peek();
            if (info.sceneName != scene.name)
            {
                Debug.LogError("What's scene? " + scene.path);
                return;
            }

            m_BundleSceneInfos.Dequeue();
            LoadNextBundleScene();
        }

        private IEnumerator AnalyzeBundleScene(Scene scene)
        {
            yield return new WaitForEndOfFrame();

            BundleSceneInfo info = m_BundleSceneInfos.Peek();
            if (info.sceneName != scene.name)
            {
                Debug.LogError("What's scene? " + scene.path);
                yield break;
            }

            GameObject[] gos = scene.GetRootGameObjects();
            foreach (var go in gos)
            {
                AssetBundleFilesAnalyze.AnalyzeObjectComponent(info.fileInfo, go);
            }
            AssetBundleFilesAnalyze.AnalyzeObjectsCompleted(info.fileInfo);

            info.ab.Unload(true);
            info.ab = null;
            SceneManager.UnloadScene(scene);
        }

        private void LoadNextBundleScene()
        {
            if (m_BundleSceneInfos.Count <= 0)
            {
                SceneManager.sceneLoaded -= SceneManagerOnSceneLoaded;
                SceneManager.sceneUnloaded -= SceneManagerOnSceneUnloaded;

                if (AssetBundleFilesAnalyze.analyzeCompleted != null)
                {
                    AssetBundleFilesAnalyze.analyzeCompleted();
                }
                return;
            }

            // 释放一下内存，以免爆掉
            Resources.UnloadUnusedAssets();
            GC.Collect();

            BundleSceneInfo info = m_BundleSceneInfos.Peek();
            info.ab = AssetBundle.LoadFromFile(info.fileInfo.path);
            SceneManager.LoadScene(info.sceneName, LoadSceneMode.Additive);
        }

        private void ClearAllBundleScenes()
        {
            foreach (var sceneInfo in m_BundleSceneInfos)
            {
                if (sceneInfo.ab)
                {
                    sceneInfo.ab.Unload(true);
                }
            }

            m_BundleSceneInfos.Clear();
        }
    }
}