using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class 队友手牌 : SingletonMono<队友手牌>
    {
        public GameObject handCardArea;
        public Dictionary<int, Card> handcards = new Dictionary<int, Card>();
        private Model.Player self { get => SgsMain.Instance.self.model; }

        void OnEnable()
        {
            transform.SetAsLastSibling();
            int count = 0;
            foreach (var i in handcards.Values)
            {
                if (self.Teammate.HandCards.Contains(i.model))
                {
                    i.gameObject.SetActive(true);
                    count++;
                }
                else i.gameObject.SetActive(false);
            }
            handCardArea.GetComponent<GridLayoutGroup>().spacing = UpdateSpacing(count);
        }

        public void AddHandCard(Model.GetCard operation)
        {
            if (!operation.player.isSelf) return;

            // 所有新获得手牌的id
            var cards = operation.Cards;

            // 从assetbundle中加载卡牌预制件
            var card = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>("Card");

            // 实例化新卡牌，添加到手牌区，并根据卡牌id初始化
            foreach (var i in cards)
            {
                if (handcards.ContainsKey(i.Id))
                {
                    handcards[i.Id].transform.SetAsLastSibling();
                    continue;
                }

                var instance = Instantiate(card);
                instance.SetActive(false);
                instance.transform.SetParent(handCardArea.transform, false);
                handcards.Add(i.Id, instance.GetComponent<Card>());
                handcards[i.Id].Init(i);
            }
        }

        public void RemoveHandCard(Model.LoseCard operation)
        {
            if (!operation.player.isSelf) return;

            foreach (var i in operation.Cards)
            {
                if (!handcards.ContainsKey(i.Id)) continue;
                if (!operation.player.Teammate.HandCards.Contains(i))
                {
                    Destroy(handcards[i.Id].gameObject);
                    handcards.Remove(i.Id);
                }
                else handcards[i.Id].gameObject.SetActive(self != operation.player);
            }
        }

        private Vector2 UpdateSpacing(int count)
        {

            // 若手牌数小于7，则不用设置负间距，直接返回
            if (count < 8) return new Vector2(0, 0);

            float spacing = -(count * 121.5f - 850) / (float)(count - 1) - 0.001f;
            return new Vector2(spacing, 0);
        }
    }
}
