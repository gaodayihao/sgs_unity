using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class DiscardArea : MonoBehaviour
    {
        public void AddDiscard(List<Model.Card> cards)
        {
            // 从assetbundle中加载卡牌预制件
            // while (!ABManager.Instance.ABMap.ContainsKey("sgsasset")) await Task.Yield();
            var card = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>("Card");

            // 实例化新卡牌，根据卡牌id初始化
            foreach (var discard in cards)
            {
                var instance = Instantiate(card);
                instance.transform.SetParent(transform, false);
                instance.GetComponent<Card>().Init(discard);
            }

            UpdateSpacing();
        }

        // 使用牌时调用
        // public void AddDiscard(Model.Card card)
        // {
        //     // if (card.Id == 0) return;

        //     AddDiscard(new Model.Card[1] { card });
        // }

        // 替换装备时调用
        // public async void AddDiscard(Model.Equipage newEquip)
        // {
        //     if (newEquip.Src.Equipages[newEquip.Type] != null)
        //     {
        //         await AddDiscard(new Model.Card[1] { newEquip.Src.Equipages[newEquip.Type] });
        //     }
        // }

        // // 弃牌时调用
        // public async void AddDiscard(Model.LoseCard operation)
        // {
        //     if (!(operation is Model.Discard)) return;

        //     if (operation.HandCards != null) await AddDiscard(operation.HandCards);
        //     if (operation.Equipages != null) await AddDiscard(operation.Equipages);
        // }

        private List<GameObject> discards;

        /// <summary>
        /// 清空弃牌区
        /// </summary>
        public IEnumerator Clear()
        {
            yield return new WaitForSeconds(2);
            // foreach (Transform card in transform) Destroy(card.gameObject);
            foreach (var card in discards) Destroy(card);
        }

        public void Clear(Model.TurnSystem turnSystem)
        {
            if (transform.childCount == 0) return;
            StopAllCoroutines();

            discards = new List<GameObject>();
            foreach (Transform card in transform) discards.Add(card.gameObject);
            StartCoroutine(Clear());
        }

        /// <summary>
        /// 弃牌数量达到8时，更新间距
        /// </summary>
        private void UpdateSpacing()
        {
            if (transform.childCount < 7)
            {
                GetComponent<RectTransform>().sizeDelta = new Vector2(121.5f * transform.childCount, 190);
                GetComponent<HorizontalLayoutGroup>().spacing = 0;
            }
            else
            {
                GetComponent<RectTransform>().sizeDelta = new Vector2(810, 171);
                var spacing = (810 - 121.5f * transform.childCount) / (float)(transform.childCount - 1) + 0.001f;
                GetComponent<HorizontalLayoutGroup>().spacing = spacing;
            }
        }
    }
}
