using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace View
{
    /// <summary>
    /// 场景View基类
    /// </summary>
    public class SceneBase : MonoBehaviour
    {
        private RawImage backgroundImage;

        /// <summary>
        /// 背景图片适配屏幕
        /// </summary>
        public void AdaptScreen()
        {
            Texture texture = backgroundImage.texture;
            Vector2 canvasSize = GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>().sizeDelta;
            float radio = Mathf.Max(canvasSize.x / texture.width, canvasSize.y / texture.height);

            backgroundImage.rectTransform.sizeDelta *= radio;
        }

        /// <summary>
        /// 下载背景图片，附加到"BackgroundImage"(RawImage)对象上，并以等比例填充屏幕
        /// </summary>
        /// <param name="url">图片地址</param>
        // public IEnumerator SetBackgroundImage(string url)
        // {
        //     // 获取图片
        //     // yield return Utils.GetImageTexture(backgroundImage, url);
        //     UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        //     yield return www.SendWebRequest();

        //     if (www.result == UnityWebRequest.Result.ConnectionError)
        //         Debug.Log(www.error);
        //     else
        //     {
        //         backgroundImage = GameObject.Find("BackgroundImage").GetComponent<RawImage>();
        //         backgroundImage.texture = DownloadHandlerTexture.GetContent(www);
        //         // 调整原始图像大小，以使其像素精准。
        //         backgroundImage.SetNativeSize();
        //     }
        //     // 适配屏幕
        //     AdaptScreen();
        // }

        public async void SetBackgroundImage(string url)
        {
            // 获取图片
            // yield return Utils.GetImageTexture(backgroundImage, url);
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            www.SendWebRequest();
            while(!www.isDone) await Task.Yield();

            if (www.result == UnityWebRequest.Result.ConnectionError)
                Debug.Log(www.error);
            else
            {
                backgroundImage = GameObject.Find("BackgroundImage").GetComponent<RawImage>();
                backgroundImage.texture = DownloadHandlerTexture.GetContent(www);
                // 调整原始图像大小，以使其像素精准。
                backgroundImage.SetNativeSize();
                AdaptScreen();
            }
            // 适配屏幕
            // AdaptScreen();
        }

        void Awake()
        {
            backgroundImage = GameObject.Find("BackgroundImage").GetComponent<RawImage>();
            if(backgroundImage != null) Debug.Log("find background");
            // 适配屏幕
            AdaptScreen();
        }
    }
}
