using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class SingletonCtrl<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance = null;

        public static T Instance
        {
            get
            {
                if (instance == null)
                    // instance = GameObject.FindObjectOfType<T>();
                    instance = GameObject.FindObjectOfType<T>();
                return instance;
            }
        }

        // 显示
        // public static void Show()
        // {
        //     if (instance == null)
        //         instance = GameObject.FindObjectOfType<T>();
        //     // debug
        //     // Debug.Log(instance == null);
        //     instance.gameObject.SetActive(true);
        // }

        // // 隐藏
        // public static void Hide()
        // {
        //     Instance.gameObject.SetActive(false);
        // }

    }
}