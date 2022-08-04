using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using NativeWebSocket;

namespace View
{
    public class SgsStart : SingletonMono<SgsStart>
    {
        public GameObject startPanel;
        public GameObject login;
        public GameObject register;
        public Button start;
        public Button logout;

        public Text playerName;

        // Start is called before the first frame update
        void Start()
        {
            startPanel.SetActive(false);
            login.SetActive(false);
            register.SetActive(false);

            start.onClick.AddListener(ClickStart);
            logout.onClick.AddListener(ClickLogout);

            Getinfo();
            Bgm.Instance.Load(Urls.AUDIO_URL + "bgm/outbgm_2.mp3");
        }

        private void ClickStart()
        {
            Model.Room.Instance.IsSingle = false;
            SceneManager.Instance.LoadSceneFromAB("Menu");
        }

        private void ClickLogout()
        {
            LogoutAsync();
        }

        private async void LogoutAsync()
        {
            // UnityWebRequest www = UnityWebRequest.Get(Urls.LOGIN_URL + "logout/");
            // www.SendWebRequest();
            // while (!www.isDone) await Task.Yield();

            // if (www.result != UnityWebRequest.Result.Success)
            // {
            //     Debug.Log(www.error);
            // }
            // else
            // {
            var json = await WebRequest.GetString(Urls.LOGIN_URL + "logout/");
            var getinfoResponse = JsonUtility.FromJson<ResultResponse>(json);
            if (getinfoResponse.result == "success")
            {
                // 显示登录面板
                startPanel.SetActive(false);
                login.SetActive(true);
            }
            // }
        }


        public async void Getinfo()
        {
            // Debug.Log("getinfo");
            // UnityWebRequest www = UnityWebRequest.Get(Urls.LOGIN_URL + "getinfo/");
            // www.SendWebRequest();
            // while (!www.isDone) await Task.Yield();

            // if (www.result != UnityWebRequest.Result.Success)
            // {
            //     Debug.Log(www.error);

            //     // 显示登录面板
            //     login.SetActive(true);
            // }
            // else
            // {
            string json = await WebRequest.GetString(Urls.LOGIN_URL + "getinfo/");
            var getinfoResponse = JsonUtility.FromJson<GetinfoResponse>(json);
            if (getinfoResponse.result == "success")
            {
                // 初始化用户信息到本地
                Model.User.Instance.Init(getinfoResponse);
                // 显示开始面板
                startPanel.SetActive(true);
                playerName.text = Model.User.Instance.Username;
            }
            else
            {
                // 显示登录面板
                login.SetActive(true);
            }
            // }
        }

    }
}