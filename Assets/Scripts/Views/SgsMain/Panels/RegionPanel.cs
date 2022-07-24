using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.EventSystems;

namespace View
{
    public class RegionPanel : Drag
    {
        // 手牌区
        public GameObject handCards;
        // 装备区
        public GameObject equips;
        // 判定区
        public GameObject judges;

        public List<Card> selectCards = new List<Card>();

        // 进度条
        public Slider slider;
        // 标题
        public Text title;

        public void Init(Model.CardPanel model)
        {
            // gameObject.SetActive(true);
            title.text = model.Title;

            // 从assetbundle中加载卡牌预制件
            var card = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>("Card");

            // 实例化新卡牌，添加到手牌区，并根据卡牌id初始化
            // if (model.dest.HandCardCount != 0)
            // {
            foreach (var i in model.dest.HandCards)
            {
                var instance = Instantiate(card);
                instance.transform.SetParent(handCards.transform, false);
                instance.GetComponent<Card>().InitInRegion(i, false);
            }
            // }

            foreach (var i in model.dest.Equipages.Values)
            {
                if (i != null)
                {
                    var instance = Instantiate(card);
                    instance.transform.SetParent(equips.transform, false);
                    instance.GetComponent<Card>().InitInRegion(i);
                }
            }

            // if (model.dest.JudgeArea.Count != 0)
            // {
            foreach (var i in model.dest.JudgeArea)
            {
                var instance = Instantiate(card);
                instance.transform.SetParent(judges.transform, false);
                instance.GetComponent<Card>().InitInRegion(i);
            }
            // }

            StartTimer(model.second);
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
            StopAllCoroutines();
            Model.CardPanel.Instance.SendResult();
        }
    }
}