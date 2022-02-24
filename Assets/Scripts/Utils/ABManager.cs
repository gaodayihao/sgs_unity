using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

public class ABManager : Singleton<ABManager>
{
    // AB包路径
    private string ABUrl
    {
        get
        {
#if UNITY_EDITOR
            return "file:///" + Application.dataPath + "/../AssetBundles/WebGL/";
            // return Urls.STATIC_URL + "AssetBundles/WebGL/";
#elif UNITY_WEBGL
            return Urls.STATIC_URL + "AssetBundles/WebGL/";
#else
            return "file:///" + Application.dataPath + "/../AssetBundles/StandaloneWindows/";
#endif
        }
    }

    // 依赖项配置文件
    private AssetBundleManifest manifest = null;

    /// <summary>
    /// 已加载的AssetBundles字典
    /// </summary>
    public Dictionary<string, AssetBundle> ABMap { get; private set; } = new Dictionary<string, AssetBundle>();


    private event UnityAction<float> progressEvent;

    /// <summary>
    /// AssetBundle加载进度事件
    /// </summary>
    public event UnityAction<float> ProgressEvent { add => progressEvent += value; remove => progressEvent -= value; }

    /// <summary>
    /// 从服务端获取AssetBundle并加入ABs字典
    /// </summary>
    /// <param name="abName">Assetbundle name</param>
    private IEnumerator GetABFromServer(string abName)
    {
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(ABUrl + abName);
        www.SendWebRequest();

        while (!www.isDone)
        {
            progressEvent?.Invoke(www.downloadProgress);
            yield return new WaitForSeconds(.1f);
        }

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            ABMap.Add(abName, bundle);
            Debug.Log("load " + abName);
        }
    }

    /// <summary>
    /// 初始化manifest
    /// </summary>
    private IEnumerator LoadManifest()
    {
        string mainABName =
#if UNITY_EDITOR
        //"StandaloneWindows";
        "WebGL";
#elif UNITY_WEBGL
        "WebGL";
#else
        "StandaloneWindows";
#endif
        // 下载主包
        yield return GetABFromServer(mainABName);

        manifest = ABMap[mainABName].LoadAsset<AssetBundleManifest>("AssetBundleManifest");
    }


    /// <summary>
    /// 从外部下载AssetBundle
    /// </summary>
    /// <param name="abName">AssetBundle name</param>
    public IEnumerator LoadAssetBundle(string abName)
    {
        if (!ABMap.ContainsKey(abName))
        {
            // 获取所有依赖项
            if (manifest == null)
                yield return LoadManifest();

            string[] dependencies = manifest.GetAllDependencies(abName);
            foreach (string i in dependencies)
            {
                if (!ABMap.ContainsKey(i))
                {
                    yield return GetABFromServer(i);
                }
            }
            // 加载目标资源包
            yield return GetABFromServer(abName);
        }
        // else
        //     yield return null;
    }

    // 加载进度

    /// <summary>
    /// 卸载AssetBundle
    /// </summary>
    /// <param name="abName">AssetBundle name</param>
    public void Unload(string abName)
    {
        if (ABMap.ContainsKey(abName))
        {
            ABMap[abName].Unload(false);
            ABMap.Remove(abName);
            // Debug.Log(abName);
        }
    }

    public void LoadSgsMain()
    {
        if (!ABManager.Instance.ABMap.ContainsKey("sprite"))
        {
            ABMap.Add("sprite", AssetBundle.LoadFromFile(Application.dataPath + "/../AssetBundles/WebGL/sprite"));
        }
        
        if (!ABManager.Instance.ABMap.ContainsKey("sgsasset"))
        {
            ABMap.Add("sgsasset", AssetBundle.LoadFromFile(Application.dataPath + "/../AssetBundles/WebGL/sgsasset"));
        }

        if (!ABManager.Instance.ABMap.ContainsKey("fonts"))
        {
            ABMap.Add("fonts", AssetBundle.LoadFromFile(Application.dataPath + "/../AssetBundles/WebGL/fonts"));
        }
    }
}
