using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.EventSystems;

namespace View
{
    public class CardPanel : Drag<CardPanel>
    {

        public List<Card> selectCards = new List<Card>();

        // 进度条
        public Slider slider;
        // 标题
        // public Text title;
        public Text hint;
        private GameObject cardPrefab;
        protected Model.CardPanel model { get => Model.CardPanel.Instance; }

        protected virtual void Start()
        {
            hint.text = model.Hint;

            // 从assetbundle中加载卡牌预制件
            cardPrefab = ABManager.Instance.GetSgsAsset("Card");

            StartTimer(model.second);
        }

        protected void InitCard(Model.Card card, Transform parent, bool display = true)
        {
            var instance = Instantiate(cardPrefab).GetComponent<Card>();
            instance.inPanel = true;
            instance.transform.SetParent(parent, false);
            instance.InitInPanel(card, display);
        }

        public void UpdatePanel()
        {
            if (selectCards.Count > 0)
            {
                StopAllCoroutines();
                Model.CardPanel.Instance.SendResult(new List<int> { selectCards[0].Id }, true);
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