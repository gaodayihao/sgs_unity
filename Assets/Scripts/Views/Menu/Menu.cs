using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class Menu : SceneBase<Menu>
    {
        public Button start_2v2;
        public Text text;
        // Start is called before the first frame update
        void Start()
        {
            // 加载背景图片
            // StartCoroutine(SetBackgroundImage(Urls.TEST_BACKGROUND_IMAGE));
            start_2v2.onClick.AddListener(Click_2v2);
            LoadBgm(Urls.AUDIO_URL + "bgm/outbgm_2.mp3");
        }

        private async void Click_2v2()
        {
            if (!Model.Room.Instance.IsSingle) await Model.Room.Instance.StartRank();

            SceneManager.Instance.LoadSceneFromAB("SgsMain");
        }

        // public void StartGame()
        // {
        //     StartCoroutine(SceneManager.Instance.LoadSceneFromAB("SgsMain"));
        // }

        public void UpdateProgress(float progress)
        {
            this.text.text = (progress * 100).ToString() + "%";
            // Debug.Log(this.progress.text);
        }
    }
}