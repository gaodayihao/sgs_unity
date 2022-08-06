using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bgm : MonoBehaviour
{
    private static Bgm instance;
    public static Bgm Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = new GameObject("Bgm").AddComponent<Bgm>();
            DontDestroyOnLoad(instance);
            return instance;
        }
    }

    private AudioSource audioSource;
    private string url;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.4f;
    }

    public async void Load(string url)
    {
        if (this.url == url) return;
        audioSource.clip = await WebRequest.GetClip(url);
        this.url = url;
        audioSource.Play();
    }
}