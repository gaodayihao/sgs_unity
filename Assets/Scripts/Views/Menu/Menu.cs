using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace View
{
    public class Menu : SingletonMono<Menu>
    {
        public Button start_2v2;
        public Text text;
        public Text timer;
        private bool isRanking = false;
        // Start is called before the first frame update
        void Start()
        {
            // 加载背景图片
            // StartCoroutine(SetBackgroundImage(Urls.TEST_BACKGROUND_IMAGE));
            start_2v2.onClick.AddListener(ClickStartRank);
            Bgm.Instance.Load(Urls.AUDIO_URL + "bgm/outbgm_2.mp3");
        }

        private void ClickStartRank()
        {
            if (!isRanking)
            {
                isRanking = true;
                text.text = "匹配中";
                timer.gameObject.SetActive(true);
                timerActive = true;
                StartCoroutine(Timer());
                StartRank();
            }
            else
            {
                StopRank();
                isRanking = false;
                text.text = "开始匹配";
                timer.gameObject.SetActive(false);
                timerActive = false;
            }
        }

        public async void StartRank()
        {
            var json = new CreatePlayerJson();
            json.eventname = "create_player";
            json.username = Model.User.Instance.Username;

            while (Wss.Instance.websocket is null) await Task.Yield();
            Wss.Instance.Connect();
            while (Wss.Instance.websocket.State != NativeWebSocket.WebSocketState.Open)
            {
                await Task.Yield();
            }
            Wss.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));

        }

        public async void StopRank()
        {
            await Wss.Instance.websocket.Close();
        }

        public void StartGame(StartGameJson json)
        {
            StopCoroutine(Timer());
            Model.Room.Instance.InitPlayers(json);
            SceneManager.Instance.LoadSceneFromAB("SgsMain");
        }

        private bool timerActive;
        private IEnumerator Timer()
        {
            int second = 0;
            while (timerActive)
            {
                timer.text = second++.ToString();
                yield return new WaitForSeconds(1);
            }
        }

        public void UpdateProgress(float progress)
        {
            timer.text = (progress * 100).ToString() + "%";
            // Debug.Log(this.progress.text);
        }
    }
}