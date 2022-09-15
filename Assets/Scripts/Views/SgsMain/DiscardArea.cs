using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class DiscardArea : SingletonMono<DiscardArea>
    {
        public List<Card> discards = new List<Card>();
        private Dictionary<int, Card> handCards => CardArea.Instance.handcards;

        public async void AddDiscard(List<Model.Card> cards)
        {
            var prefab = ABManager.Instance.GetSgsAsset("Card");
            var player = cards[0].Src;

            if (player != null && !CardSystem.Instance.IsViewSelf(player))
            {
                var cardAnime = Instantiate(ABManager.Instance.GetSgsAsset("CardAnime"), transform.parent).transform;
                cardAnime.position = CardSystem.Instance.PlayerPos(player);

                foreach (var i in cards)
                {
                    var instance = Instantiate(prefab).GetComponent<Card>();

                    instance.InitInSelf(i);
                    instance.SetParent(cardAnime);
                    discards.Add(instance);
                }
                int f = Time.frameCount;
                while (f == Time.frameCount) await Task.Yield();
                CardSystem.Instance.UpdateAll(0);

                foreach (var i in cards) discards.Find(x => x.Id == i.Id).SetParent(transform);

                Destroy(cardAnime.gameObject, 0.5f);
            }
            else
            {
                foreach (var i in cards)
                {
                    var instance = Instantiate(prefab).GetComponent<Card>();
                    instance.Init(i, true);
                    discards.Add(instance);
                    instance.SetParent(transform);

                    if (player is null) instance.transform.position = transform.position;
                    else if (!player.HandCards.Contains(i)) instance.transform.position = Self.Instance.transform.position;
                    else instance.transform.position = handCards[i.Id].transform.position;
                    instance.transform.position = transform.InverseTransformPoint(instance.transform.position);
                }
            }

            UpdateSpacing();
            CardSystem.Instance.UpdateAll(0.5f);
        }

        public void AddDiscard(Model.ShowCard model)
        {
            AddDiscard(model.Cards);
        }

        public async void Clear(Model.TurnSystem turnSystem)
        {
            foreach (var i in discards) Destroy(i.gameObject, 2);

            await Model.SgsMain.Instance.Delay(2.1f);
            UpdateSpacing();
            foreach (var i in discards) i.Move(0.2f);
        }

        /// <summary>
        /// 弃牌数量达到8时，更新间距
        /// </summary>
        private void UpdateSpacing()
        {
            if (transform.childCount >= 7)
            {
                var spacing = (810 - 121.5f * transform.childCount) / (float)(transform.childCount - 1);
                GetComponent<HorizontalLayoutGroup>().spacing = spacing;
            }
            GetComponent<HorizontalLayoutGroup>().spacing = 0;
        }
    }
}
