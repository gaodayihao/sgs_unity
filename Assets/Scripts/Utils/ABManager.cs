using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Threading.Tasks;

public class ABManager : Singleton<ABManager>
{

    // 依赖项配置文件
    private AssetBundleManifest manifest = null;

    /// <summary>
    /// 已加载的AssetBundles字典
    /// </summary>
    public Dictionary<string, AssetBundle> ABMap { get; private set; } = new Dictionary<string, AssetBundle>();

    private Dictionary<string, GameObject> sgsAsset = new Dictionary<string, GameObject>();

    public GameObject GetSgsAsset(string name)
    {
        if (!sgsAsset.ContainsKey(name))
        {
            sgsAsset.Add(name, ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>(name));
        }
        return sgsAsset[name];
    }

    private event UnityAction<float> progressEvent;

    /// <summary>
    /// AssetBundle加载进度事件
    /// </summary>
    public event UnityAction<float> ProgressEvent { add => progressEvent += value; remove => progressEvent -= value; }

    /// <summary>
    /// 从服务端获取AssetBundle并加入ABs字典
    /// </summary>
    /// <param name="abName">Assetbundle name</param>
    private async Task GetABFromServer(string abName)
    {
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(Urls.ABUrl + abName);
        www.SendWebRequest();

        while (!www.isDone)
        {
            progressEvent?.Invoke(www.downloadProgress);
            await Task.Yield();
        }

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            Debug.Log(Urls.ABUrl + abName);
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
    private async Task LoadManifest()
    {
        string mainABName = "AssetBundles";
        // #if UNITY_EDITOR
        //         //"StandaloneWindows";
        //         "WebGL";
        // #elif UNITY_WEBGL
        //         "WebGL";
        // #else
        //         "StandaloneWindows";
        // #endif
        // 下载主包
        await GetABFromServer(mainABName);

        manifest = ABMap[mainABName].LoadAsset<AssetBundleManifest>("AssetBundleManifest");
    }


    /// <summary>
    /// 从外部下载AssetBundle
    /// </summary>
    /// <param name="abName">AssetBundle name</param>
    public async Task LoadAssetBundle(string abName)
    {
        if (!ABMap.ContainsKey(abName))
        {
            // 获取所有依赖项
            if (manifest == null)
                await LoadManifest();

            string[] dependencies = manifest.GetAllDependencies(abName);
            foreach (string i in dependencies)
            {
                if (!ABMap.ContainsKey(i))
                {
                    await GetABFromServer(i);
                }
            }
            // 加载目标资源包
            await GetABFromServer(abName);
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
        string url = Application.streamingAssetsPath + "/AssetBundles/";
        if (!ABManager.Instance.ABMap.ContainsKey("sprite"))
        {
            ABMap.Add("sprite", AssetBundle.LoadFromFile(url + "sprite"));
        }

        if (!ABManager.Instance.ABMap.ContainsKey("sgsasset"))
        {
            ABMap.Add("sgsasset", AssetBundle.LoadFromFile(url + "sgsasset"));
        }

        if (!ABManager.Instance.ABMap.ContainsKey("fonts"))
        {
            ABMap.Add("fonts", AssetBundle.LoadFromFile(url + "fonts"));
        }
    }
}
