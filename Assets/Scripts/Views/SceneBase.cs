// using System.Threading.Tasks;
// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.Networking;

namespace View
{
    /// <summary>
    /// 场景View基类
    /// </summary>
    public class SceneBase<T> : SingletonMono<T> where T : MonoBehaviour
    {
        private AudioSource bgm;
        private string bgmUrl;

        protected override void Awake()
        {
            base.Awake();
            if (GameObject.Find("Bgm") is null) DontDestroyOnLoad(new GameObject("Bgm").AddComponent<AudioSource>());
            bgm = GameObject.Find("Bgm").GetComponent<AudioSource>();
        }

        protected async void LoadBgm(string url)
        {
            if (url == bgmUrl) return;
            bgm.clip = await WebRequest.GetClip(url);
            bgmUrl = url;
            bgm.Play();
        }
    }
}
