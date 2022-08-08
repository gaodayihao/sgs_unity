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
            cardPrefab = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>("Card");

            StartTimer(model.second);
        }

        protected void InitCard(Model.Card card, Transform parent, bool display = true)
        {
            var instance = Instantiate(cardPrefab);
            instance.transform.SetParent(parent, false);
            instance.GetComponent<Card>().InitInPanel(card, display);
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
    }
}