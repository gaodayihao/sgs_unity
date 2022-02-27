using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace View
{
    public class SgsMain : SceneBase
    {
        private GameObject self;
        private GameObject elsePlayers;

        private void Awake()
        {
#if UNITY_EDITOR
            ABManager.Instance.LoadSgsMain();
            // #else
            //             StartCoroutine(ABManager.Instance.LoadAssetBundle("sgsasset"));
#endif
        }

        void Start()
        {
            // gameObject.AddComponent<Model.Connection>();
            // 加载背景图片
            // StartCoroutine(SetBackgroundImage(Urls.TEST_BACKGROUND_IMAGE));
            SetBackgroundImage(Urls.TEST_BACKGROUND_IMAGE);

            self = transform.Find("Self").gameObject;
            elsePlayers = transform.Find("ElsePlayers").gameObject;
        }

        /// <summary>
        /// 初始化每个View.Player
        /// </summary>
        public void InitPlayers(Model.Player[] players)
        {
            int i;
            for (i = 0; i < 4; i++)
            {
                if (players[i].isSelf)
                {
                    self.GetComponentInChildren<Player>().Init(players[i]);
                    // players[i].IsSelf = true;
                    break;
                }
            }
            int j = 2, k = i;
            for (i = (i + 1) % 4; i != k; i = (i + 1) % 4)
            {
                elsePlayers.transform.Find("Player" + j.ToString()).GetComponent<Player>().Init(players[i]);
                j++;
            }
        }

        private GameObject panel;

        public void ShowPanel(Model.CardPanel model)
        {
            if (self.GetComponent<Self>().model != model.player) return;

            // if (model.Title == "过河拆桥" || model.Title == "顺手牵羊") ShowRegion(model);
            // else if (model.Title == "麒麟弓") ShowQlg(model);

            switch (model.timerType)
            {
                case TimerType.RegionPanel:
                case TimerType.SSQY:
                    ShowRegion(model);
                    break;
                case TimerType.QlgPanel:
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
            if (self.GetComponent<Self>().model != model.player) return;
            Destroy(panel);
        }
    }
}