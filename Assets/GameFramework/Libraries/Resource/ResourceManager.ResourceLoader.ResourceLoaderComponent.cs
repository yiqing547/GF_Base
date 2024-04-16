using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private sealed partial class ResourceLoader
        {
            internal sealed class SceneAsset
            {
            }

            /// <summary>
            /// AB包管理器 全局唯一 使用单例模式
            /// </summary>
            public class ResourceLoaderComponent : MonoBehaviour
            {
                private ResourceManager m_ResourceManager;
                private ResourceLoader m_ResourceLoader;
                private readonly List<string> m_WaitLoadAssets = new();
                protected static ResourceLoaderComponent instance;
                public static ResourceLoaderComponent Instance
                {
                    get
                    {
                        if (instance == null)
                        {
                            GameObject gameObject = new()
                            {
                                name = nameof(ResourceLoaderComponent)
                            };
                            instance = gameObject.AddComponent<ResourceLoaderComponent>();
                            DontDestroyOnLoad(gameObject);
                        }
                        return instance;
                    }
                }

                public static ResourceLoaderComponent GetInstance() => instance;

                protected virtual void Awake()
                {
                    if (instance == null)
                    {
                        instance = this;
                    }
                }

                public void InitComponent(ResourceManager resourceManager, ResourceLoader resourceLoader)
                {
                    m_ResourceManager = resourceManager;
                    m_ResourceLoader = resourceLoader;
                }

                public AssetBundle LoadAB(ResourceInfo resourceInfo)
                {
                    ResourceObject resourceObject = m_ResourceLoader.m_ResourcePool.Spawn(resourceInfo.ResourceName.Name);
                    if (resourceInfo.LoadType == LoadType.LoadFromFile && resourceObject == null)
                    {
                        string fullPath = Utility.Path.GetRegularPath(Path.Combine(resourceInfo.StorageInReadOnly ? m_ResourceManager.m_ReadOnlyPath : m_ResourceManager.m_ReadWritePath, resourceInfo.UseFileSystem ? resourceInfo.FileSystemName : resourceInfo.ResourceName.FullName));
                        AssetBundle assetBundle = AssetBundle.LoadFromFile(fullPath);
                        if (assetBundle != null)
                        {
                            Debug.LogError($"ReadFile fullPath = {fullPath} time = {Time.time}");
                            resourceObject = ResourceObject.Create(resourceInfo.ResourceName.Name, assetBundle, m_ResourceManager.m_ResourceHelper, m_ResourceLoader);
                            m_ResourceLoader.m_ResourcePool.Register(resourceObject, true);
                        }
                        else
                        {
                            Debug.LogError($"数据加载错误 fullPath = {fullPath} time = {Time.time}");
                        }
                        return assetBundle;
                    }
                    return resourceObject.Target as AssetBundle;
                }

                public void LoadAssetAsync(ResourceInfo resourceInfo, string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks, object userData)
                {
                    AssetObject assetObject = m_ResourceLoader.m_AssetPool.Spawn(assetName);
                    if (assetObject != null)
                    {
                        loadAssetCallbacks.LoadAssetSuccessCallback(assetName, assetObject.Target, 0, userData);
                    }
                    else if (m_WaitLoadAssets.Contains(assetName))
                    {
                        StartCoroutine(ILoadAssetAsync(assetName, loadAssetCallbacks, userData));
                    }
                    else
                    {
                        AssetBundle assetBundle = LoadAB(resourceInfo);
                        if (assetBundle != null)
                        {
                            m_WaitLoadAssets.Add(assetName);
                            StartCoroutine(ILoadAssetAsync(assetBundle, assetName, assetType, loadAssetCallbacks, userData));
                        }
                        else
                        {
                            Debug.LogError($"ab包加载错误 {resourceInfo.ResourceName.Name}");
                        }
                    }
                }

                IEnumerator ILoadAssetAsync(string assetName, LoadAssetCallbacks loadAssetCallbacks, object userData)
                {
                    yield return new WaitUntil(() => !m_WaitLoadAssets.Contains(assetName));
                    AssetObject assetObject = m_ResourceLoader.m_AssetPool.Spawn(assetName);
                    if (assetObject != null)
                    {
                        loadAssetCallbacks.LoadAssetSuccessCallback(assetName, assetObject.Target, 0, userData);
                    }
                }

                IEnumerator ILoadAssetAsync(AssetBundle assetBundle, string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks, object userData)
                {
                    Debug.LogError($"LoadAssetAsync Begin assetName = {assetName} time = {Time.time}");

                    float duration = Time.time;
                    AssetBundleRequest assetBundleRequest;
                    if (assetType != null)
                    {
                        assetBundleRequest = assetBundle.LoadAssetAsync(assetName, assetType);
                    }
                    else
                    {
                        assetBundleRequest = assetBundle.LoadAssetAsync(assetName);
                    }
                    yield return assetBundleRequest;

                    if (assetBundleRequest.asset != null)
                    {
                        m_WaitLoadAssets.Remove(assetName);

                        Debug.LogError($"LoadAssetAsync Done assetName = {assetName} time = {Time.time}");
                        AssetObject assetObject = AssetObject.Create(assetName, assetBundleRequest.asset, new List<object>(), assetBundle, m_ResourceManager.m_ResourceHelper, m_ResourceLoader);
                        m_ResourceLoader.m_AssetPool.Register(assetObject, true);
                        m_ResourceLoader.m_AssetToResourceMap.Add(assetBundleRequest.asset, assetBundle);

                        loadAssetCallbacks.LoadAssetSuccessCallback(assetName, assetBundleRequest.asset, Time.time - duration, userData);
                    }
                    else
                    {
                        Debug.LogError($"LoadAssetAsync 加载错误 assetName = {assetName} time = {Time.time}");
                    }
                }

                public void LoadSceneAsync(ResourceInfo resourceInfo, string sceneName, LoadSceneCallbacks loadSceneCallbacks, object userData)
                {
                    AssetObject assetObject = m_ResourceLoader.m_AssetPool.Spawn(sceneName);
                    if (assetObject != null)
                    {
                        loadSceneCallbacks.LoadSceneSuccessCallback(sceneName, 0, userData);
                    }
                    else
                    {
                        AssetBundle assetBundle = LoadAB(resourceInfo);
                        if (assetBundle != null)
                        {
                            StartCoroutine(ILoadSceneAsync(assetBundle, sceneName, loadSceneCallbacks, userData));
                        }
                        else
                        {
                            Debug.LogError($"ab包加载错误 {resourceInfo.ResourceName.Name}");
                        }
                    }
                }

                IEnumerator ILoadSceneAsync(AssetBundle assetBundle, string assetName, LoadSceneCallbacks loadSceneCallbacks, object userData)
                {
                    int sceneNamePositionStart = assetName.LastIndexOf('/');
                    int sceneNamePositionEnd = assetName.LastIndexOf('.');
                    if (sceneNamePositionStart <= 0 || sceneNamePositionEnd <= 0 || sceneNamePositionStart > sceneNamePositionEnd)
                    {
                        Debug.LogError($"LoadSceneAsync 场景名错误 {assetName}");
                        yield return null;
                    }
                    float duration = Time.time;
                    string sceneName = assetName.Substring(sceneNamePositionStart + 1, sceneNamePositionEnd - sceneNamePositionStart - 1);
                    AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                    yield return asyncOperation;
                    if (asyncOperation.allowSceneActivation)
                    {
                        SceneAsset sceneAsset = new();
                        AssetObject assetObject = AssetObject.Create(assetName, sceneAsset, new List<object>(), assetBundle, m_ResourceManager.m_ResourceHelper, m_ResourceLoader);
                        m_ResourceLoader.m_AssetPool.Register(assetObject, true);
                        m_ResourceLoader.m_SceneToAssetMap.Add(assetName, sceneAsset);

                        loadSceneCallbacks.LoadSceneSuccessCallback(assetName, Time.time - duration, userData);
                    }
                }
            }
        }
    }
}