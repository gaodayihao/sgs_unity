using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace View
{
    public class Register : MonoBehaviour
    {
        // 注册按键
        public Button register;
        // 返回登录
        public Button toLogin;

        // 用户名
        public InputField username;
        // 密码
        public InputField password;
        // 确认密码
        public InputField confirmPassword;
        // 错误信息
        public Text errorMessage;

        void Start()
        {
            register.onClick.AddListener(ClickRegister);
            toLogin.onClick.AddListener(ClickToLogin);
        }

        private void ClickRegister()
        {
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
            if (password.text != confirmPassword.text)
            {
                UpdateErrorMessage("两次密码不一致！");
                return;
            }

            RegisterAsync();
        }

        private async void RegisterAsync()
        {
            string url = Urls.LOGIN_URL + "register?username=" + username.text + "&password=" +
                password.text + "&confirmPassword=" + confirmPassword.text;

            var loginResponse = JsonUtility.FromJson<ResultResponse>(await WebRequest.GetString(url));
            if (loginResponse.result == "success")
            {
                // SgsStart.Instance.startPanel.gameObject.SetActive(true);
                this.gameObject.SetActive(false);
                GetComponentInParent<SgsStart>().Getinfo();
                // Register.Hide();
            }
            else
            {
                UpdateErrorMessage(loginResponse.result);
            }
        }

        private void ClickToLogin()
        {
            // Register.Hide();
            // Login.Show();
            this.gameObject.SetActive(false);
            transform.parent.Find("登录面板").gameObject.SetActive(true);
        }

        public void UpdateErrorMessage(string errorMessage)
        {
            this.errorMessage.text = errorMessage;
        }
    }
}
