using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class Login : MonoBehaviour
    {
        // 登录按键
        public Button login;
        // 单机按键
        public Button singleMode;
        // 注册按键
        public Button toRegister;

        // 用户名
        public InputField username;
        // 密码
        public InputField password;
        // 错误信息
        public Text errorMessage;

        void Start()
        {
            // 登录按键
            login.onClick.AddListener(ClickLogin);
            // 注册按键
            toRegister.onClick.AddListener(ClickToRegister);
            singleMode.onClick.AddListener(ClickSingleMode);
        }

        public void UpdateErrorMessage(string errorMessage)
        {
            this.errorMessage.text = errorMessage;
        }

        private void ClickLogin()
        {
            // 清空错误信息
            UpdateErrorMessage("");

            if (username.text == "" || username.text == null)
            {
                UpdateErrorMessage("用户名不能为空！");
                return;
            }
            if (password.text == "")
            {
                UpdateErrorMessage("密码不能为空！");
                return;
            }
            LoginAsync();
        }

        private async void LoginAsync()
        {
            string url = Urls.LOGIN_URL + "login?username=" + username.text + "&password=" + password.text;
            var loginResponse = JsonUtility.FromJson<ResultResponse>(await WebRequest.GetString(url));
            if (loginResponse.result == "success")
            {
                // SgsStart.Instance.startPanel.gameObject.SetActive(true);
                gameObject.SetActive(false);
                GetComponentInParent<SgsStart>().Getinfo();
                // Debug.Log("done");
                // Login.Hide();
            }
            else
            {
                UpdateErrorMessage(loginResponse.result);
            }
        }

        public void ClickToRegister()
        {
            // Login.Hide();
            // Register.Show();
            this.gameObject.SetActive(false);
            transform.parent.Find("注册面板").gameObject.SetActive(true);
            // GameObject.Find("注册面板").SetActive(true);
        }

        public void ClickSingleMode()
        {
            SceneManager.Instance.LoadSceneFromAB("SgsMain");
        }

    }
}
