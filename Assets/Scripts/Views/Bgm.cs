using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class Bgm : MonoBehaviour
    {
        private static Bgm instance;
        public static Bgm Instance
        {
            get
            {
                if (instance is null)
                {
                    instance = new GameObject("Bgm").AddComponent<Bgm>();
                    // instance.gameObject.AddComponent<AudioListener>();
                    DontDestroyOnLoad(instance);
                }
                return instance;
            }
        }

        private AudioSource audioSource;
        private string url;

        void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.volume = 0.4f;
#if UNITY_WEBGL
            audioSource.loop = true;
#endif
        }

        public async void Load(string url)
        {
            if (this.url == url) return;
            audioSource.Stop();
            audioSource.clip = await WebRequest.GetClip(url);
            this.url = url;
            audioSource.Play();
        }
    }
}