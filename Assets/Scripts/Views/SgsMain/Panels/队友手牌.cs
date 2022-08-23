using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class 队友手牌 : SingletonMono<队友手牌>
    {
        public GameObject handCardArea;
        public Image image;
        public Dictionary<int, Card> handcards = new Dictionary<int, Card>();
        private Model.Player self => SgsMain.Instance.self.model;

        async void OnEnable()
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
            handCardArea.GetComponent<HorizontalLayoutGroup>().spacing = UpdateSpacing(count);

            int id = SgsMain.Instance.players[self.Teammate.Position].GetComponent<Player>().CurrentSkin.id;
            string url = Urls.GENERAL_IMAGE + "Window/" + id + ".png";
            var texture = await WebRequest.GetTexture(url);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0)) gameObject.SetActive(false);
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

        private float UpdateSpacing(int count)
        {

            // 若手牌数小于7，则不用设置负间距，直接返回
            if (count < 7) return 0;

            return -(count * 121.5f - 820) / (float)(count - 1) - 0.001f;
        }
    }
}
