using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace View
{
    public class SgsMain : SceneBase<SgsMain>
    {
        // public Self self;
        // private GameObject elsePlayers;
        public RawImage backgroundImage;

        public GameObject[] players;
        public Player self { get; private set; }

        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            ABManager.Instance.LoadSgsMain();
#endif
        }

        void Start()
        {
            SetBackgroundImage(Urls.TEST_BACKGROUND_IMAGE);
            LoadBgm(Urls.AUDIO_URL + "bgm/bgm_1.mp3");
        }

        public async void SetBackgroundImage(string url)
        {
            backgroundImage.texture = await WebRequest.GetTexture(url);
            // 调整原始图像大小，以使其像素精准。
            backgroundImage.SetNativeSize();

            // 适应屏幕
            Texture texture = backgroundImage.texture;
            Vector2 canvasSize = GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>().sizeDelta;
            float radio = Mathf.Max(canvasSize.x / texture.width, canvasSize.y / texture.height);
            backgroundImage.rectTransform.sizeDelta *= radio;
        }
        // 适配屏幕
        // AdaptScreen();

        /// <summary>
        /// 初始化每个View.Player
        /// </summary>
        public void InitPlayers(Model.Player[] model)
        {
            // int i;
            for (int i = 0; i < 4; i++)
            {
                players[i].GetComponent<Player>().Init(model[i]);
            }
            // self = players[0].GetComponent<Player>();

            foreach (var i in players)
            {
                if (i.GetComponent<Player>().model.isSelf)
                {
                    MoveSeat(i.GetComponent<Player>().model);
                    break;
                }
            }
        }

        public void GameOver()
        {
            Debug.Log("gameover");
            string scene = Model.Room.Instance.IsSingle ? "Login" : "Menu";
            SceneManager.Instance.LoadSceneFromAB(scene);
        }

        /// <summary>
        /// 更新座位
        /// </summary>
        /// <param name="model">self</param>
        public void MoveSeat(Model.Player model)
        {
            if (self != null)
            {
                if (self.model == model) return;
                self.transform.Find("其他角色").gameObject.SetActive(true);
            }

            self = players[model.Position].GetComponent<Player>();
            self.transform.Find("其他角色").gameObject.SetActive(false);

            int i = model.Position;
            SelfPos(players[i++]);
            RightPos(players[i++ % 4]);
            TopPos(players[i++ % 4]);
            LeftPos(players[i % 4]);
        }

        public void SelfPos(GameObject player)
        {
            RectTransform rectTransform = player.GetComponent<RectTransform>();
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(1, 0);
            rectTransform.anchoredPosition = new Vector2(-10, 24);
        }
        public void RightPos(GameObject player)
        {
            RectTransform rectTransform = player.GetComponent<RectTransform>();
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.anchorMin = new Vector2(1, 0.5f);
            rectTransform.pivot = new Vector2(1, 0.5f);
            rectTransform.anchoredPosition = new Vector2(-10, 150);
        }
        public void TopPos(GameObject player)
        {
            RectTransform rectTransform = player.GetComponent<RectTransform>();
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = new Vector2(0, -15);
        }
        public void LeftPos(GameObject player)
        {
            RectTransform rectTransform = player.GetComponent<RectTransform>();
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.anchoredPosition = new Vector2(10, 150);
        }

        private GameObject panel;

        public void ShowPanel(Model.CardPanel model)
        {
            if (self.model != model.player) return;

            // if (model.Title == "过河拆桥" || model.Title == "顺手牵羊") ShowRegion(model);
            // else if (model.Title == "麒麟弓") ShowQlg(model);

            switch (model.timerType)
            {
                case TimerType.RegionPanel:
                case TimerType.顺手牵羊:
                    ShowRegion(model);
                    break;
                case TimerType.麒麟弓:
                    ShowQlg(model);
                    break;
            }
        }

        private void ShowRegion(Model.CardPanel model)
        {
            panel = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>("RegionPanel");
            panel = Instantiate(this.panel);
            panel.transform.SetParent(transform, false);
            panel.GetComponent<RegionPanel>().Init(model);
        }

        private void ShowQlg(Model.CardPanel model)
        {
            panel = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>("QlgPanel");
            panel = Instantiate(this.panel);
            panel.transform.SetParent(transform, false);
            panel.GetComponent<QlgPanel>().Init(model);
        }

        public void HidePanel(Model.CardPanel model)
        {
            if (self.model != model.player) return;
            Destroy(panel);
        }
    }
}