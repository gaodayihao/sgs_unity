using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace View
{
    public static class Utils
    {
        /// <summary>
        /// 下载图片并附加到RawImage对象上
        /// </summary>
        /// <param name="rawImage">作用对象</param>
        /// <param name="url">图片地址</param>
        public static IEnumerator GetImageTexture(RawImage rawImage, string url)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
                Debug.Log(www.error);
            else
            {
                rawImage.texture = DownloadHandlerTexture.GetContent(www);
                // 调整原始图像大小，以使其像素精准。
                rawImage.SetNativeSize();
            }
        }
    }
}