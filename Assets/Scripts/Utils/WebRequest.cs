using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading.Tasks;


public static class WebRequest
{
    /// <summary>
    /// 下载字符串
    /// </summary>
    public static async Task<string> GetString(string url)
    {
            // Debug.Log(url);
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SendWebRequest();

        while (!www.isDone) await Task.Yield();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            Debug.Log(url);
            return null;
        }

        return www.downloadHandler.text;
    }

    /// <summary>
    /// 下载纹理
    /// </summary>
    public static async Task<Texture2D> GetTexture(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        www.SendWebRequest();

        while (!www.isDone) await Task.Yield();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            Debug.Log(url);
            return null;
        }

        return DownloadHandlerTexture.GetContent(www);
    }

    /// <summary>
    /// 下载AudioClip
    /// </summary>
    public static async Task<AudioClip> GetClip(string url)
    {
        // Debug.Log(url);
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
        www.SendWebRequest();

        while (!www.isDone) await Task.Yield();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            Debug.Log(url);
            return null;
        }

        return DownloadHandlerAudioClip.GetContent(www);
    }
}
