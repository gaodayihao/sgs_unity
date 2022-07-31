using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class QlgPanel : Drag
    {
        // 装备区
        public GameObject equips;

        public Card SelectCard { get; set; }

        // 进度条
        public Slider slider;
        // 标题
        public Text title;
        public void Init(Model.CardPanel model)
        {
            // gameObject.SetActive(true);
            // title.text = model.Title;

            // 从assetbundle中加载卡牌预制件
            var card = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>("Card");

            var plus = model.dest.plusHorse;
            if (plus != null)
            {
                var instance = Instantiate(card);
                instance.transform.SetParent(equips.transform, false);
                instance.GetComponent<Card>().InitInQlg(plus);
            }

            var sub = model.dest.subHorse;
            if (sub != null)
            {
                var instance = Instantiate(card);
                instance.transform.SetParent(equips.transform, false);
                instance.GetComponent<Card>().InitInQlg(sub);
            }

            StartTimer(model.second);
        }

        public void UpdatePanel()
        {
            if (SelectCard != null)
            {
                StopAllCoroutines();
                Model.CardPanel.Instance.SetResult(new List<int> { SelectCard.Id });
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
