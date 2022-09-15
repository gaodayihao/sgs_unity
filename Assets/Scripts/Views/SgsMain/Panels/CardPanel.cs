using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.EventSystems;

namespace View
{
    public class CardPanel : SingletonMono<CardPanel>
    {

        public Card selectCard;

        // 进度条
        public Slider slider;
        // 标题
        // public Text title;

        // 手牌区
        public GameObject handCards;
        // 装备区
        public GameObject equips;
        // 标题
        public Text title;
        public Text hint;
        public Image image;

        private GameObject cardPrefab => ABManager.Instance.GetSgsAsset("Card");
        protected Model.CardPanel model => Model.CardPanel.Instance;

        async void Start()
        {
            hint.text = model.Hint;
            title.text = model.Title;
            // image.sprite=

            // 从assetbundle中加载卡牌预制件
            // cardPrefab = ABManager.Instance.GetSgsAsset("Card");

            StartTimer(model.second);

            switch (model.timerType)
            {
                case TimerType.区域内:
                    foreach (var i in model.dest.HandCards) InitCard(i, handCards, model.display);
                    handCards.GetComponent<GridLayoutGroup>().spacing = UpdateSpacing(handCards.transform.childCount);

                    foreach (var i in model.dest.Equipages.Values)
                    {
                        if (i != null) InitCard(i, equips);
                    }

                    if (Model.CardPanel.Instance.judgeArea)
                    {
                        foreach (var i in model.dest.JudgeArea) InitCard(i, equips);
                    }
                    break;

                case TimerType.手牌:
                    foreach (var i in model.dest.HandCards) InitCard(i, handCards, model.display);
                    handCards.GetComponent<GridLayoutGroup>().spacing = UpdateSpacing(handCards.transform.childCount);
                    break;

                case TimerType.麒麟弓:
                    var plus = model.dest.plusHorse;
                    var sub = model.dest.subHorse;
                    if (plus != null) InitCard(plus, equips);
                    if (sub != null) InitCard(sub, equips);
                    break;
            }

            int id = SgsMain.Instance.players[model.dest.Position].GetComponent<Player>().CurrentSkin.id;
            string url = Urls.GENERAL_IMAGE + "Window/" + id + ".png";
            var texture = await WebRequest.GetTexture(url);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        protected void InitCard(Model.Card card, GameObject parent, bool display = true)
        {
            if (!parent.activeSelf) parent.SetActive(true);
            var instance = Instantiate(cardPrefab).GetComponent<Card>();
            instance.inPanel = true;
            instance.transform.SetParent(parent.transform, false);
            instance.InitInPanel(card, display);
        }

        public void UpdatePanel()
        {
            if (selectCard != null)
            {
                StopAllCoroutines();
                Model.CardPanel.Instance.SendResult(new List<int> { selectCard.Id }, true);
            }
        }

        /// <summary>
        /// 开始倒计时
        /// </summary>
        private void StartTimer(int second)
        {
            slider.value = 1;
            StartCoroutine(UpdateTimer(second));
        }

        private IEnumerator UpdateTimer(int second)
        {
            while (slider.value > 0)
            {
                slider.value -= 0.1f / (second - 0.5f);
                yield return new WaitForSeconds(0.1f);
            }
            // StopAllCoroutines();
            Model.CardPanel.Instance.SendResult();
        }

        /// <summary>
        /// 更新卡牌间距
        /// </summary>
        protected Vector2 UpdateSpacing(int count)
        {

            // 若手牌数小于7，则不用设置负间距，直接返回
            if (count < 8) return new Vector2(0, 0);

            float spacing = -(count * 121.5f - 850) / (float)(count - 1) - 0.001f;
            return new Vector2(spacing, 0);
        }
    }
}