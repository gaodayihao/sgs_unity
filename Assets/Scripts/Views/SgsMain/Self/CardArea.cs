using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace View
{
    public class CardArea : SingletonMono<CardArea>
    {
        // 手牌区
        public GameObject handCardArea;
        // 手牌数
        public Text handCardText;
        // 手牌
        public Dictionary<int, Card> handcards = new Dictionary<int, Card>();

        private Player self => SgsMain.Instance.self;
        private Model.TimerTask timerTask => Model.TimerTask.Instance;

        // 已选卡牌
        private List<Model.Card> SelectedCard => Model.Operation.Instance.Cards;
        // 已选装备
        private List<Model.Card> SelectedEquip => Model.Operation.Instance.Equips;
        // 已选技能
        private Model.Skill skill => Model.Operation.Instance.skill;
        // 转化牌
        private Model.Card Converted
        {
            get => Model.Operation.Instance.Converted;
            set => Model.Operation.Instance.Converted = value;
        }

        public int MaxCount { get; private set; }
        public int MinCount { get; private set; }
        // 是否已设置
        public bool IsSettled { get; private set; } = false;


        /// <summary>
        /// 在手牌区中添加手牌
        /// </summary>
        public void AddHandCard(Model.GetCard operation)
        {
            if (!operation.player.isSelf) return;

            // 所有新获得手牌的id
            var cards = operation.Cards;

            // 从assetbundle中加载卡牌预制件
            var card = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>("Card");

            // 实例化新卡牌，添加到手牌区，并根据卡牌id初始化
            bool active = operation.player == self.model;
            foreach (var i in cards)
            {
                if (handcards.ContainsKey(i.Id))
                {
                    handcards[i.Id].transform.SetAsLastSibling();
                    continue;
                }

                var instance = Instantiate(card);
                instance.SetActive(active);
                instance.transform.SetParent(handCardArea.transform, false);
                handcards.Add(i.Id, instance.GetComponent<Card>());
                handcards[i.Id].Init(i);
            }
        }

        public void MoveSeat(Model.Player model)
        {
            foreach (var i in handcards.Values) i.gameObject.SetActive(model.HandCards.Contains(i.model));
        }

        /// <summary>
        /// 在手牌区中移除手牌
        /// </summary>
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
                else handcards[i.Id].gameObject.SetActive(self.model != operation.player);
            }
        }

        public void InitCardArea()
        {
            // 可选卡牌数量

            if (skill != null)
            {
                MaxCount = skill.MaxCard;
                MinCount = skill.MinCard;
            }
            else
            {
                MaxCount = timerTask.maxCard;
                MinCount = timerTask.minCard;
            }

            if (MaxCount == 0)
            {
                foreach (var i in handcards.Values)
                {
                    if (!i.gameObject.activeSelf) continue;
                    i.button.interactable = false;
                    i.AddShadow();
                }
                IsSettled = true;
                return;
            }

            // 无懈可击
            if (timerTask.isWxkj)
            {
                foreach (var i in handcards.Values) i.gameObject.SetActive(i.name == "无懈可击");
                UpdateSpacing();
            }

            // 判断每张卡牌是否可选

            if (skill != null)
            {
                foreach (var i in handcards.Values)
                {
                    if (!i.gameObject.activeSelf) continue;
                    i.button.interactable = skill.IsValidCard(i.model);
                }
            }
            else
            {
                foreach (var i in handcards.Values)
                {
                    if (!i.gameObject.activeSelf) continue;
                    i.button.interactable = timerTask.IsValidCard(i.model);
                }
            }

            // 对已禁用的手牌设置阴影
            foreach (var card in handcards.Values) if (card.gameObject.activeSelf) card.AddShadow();
        }

        /// <summary>
        /// 重置手牌区（进度条结束时调用）
        /// </summary>
        public void ResetCardArea()
        {
            if (!timerTask.isWxkj && self.model != timerTask.player) return;

            // 重置手牌状态
            foreach (var card in handcards.Values) card.ResetCard();
            if (timerTask.isWxkj)
            {
                foreach (var i in handcards.Values) i.gameObject.SetActive(self.model.HandCards.Contains(i.model));
                UpdateSpacing();
            }
            if (ConvertedCards.Count > 0)
            {
                foreach (var i in ConvertedCards.Values) Destroy(i.gameObject);
                ConvertedCards.Clear();
                UpdateSpacing();
            }

            IsSettled = false;
            Converted = null;
        }

        /// <summary>
        /// 更新手牌区
        /// </summary>
        public void UpdateCardArea()
        {
            int count = SelectedCard.Count + SelectedEquip.Count;

            // 若已选中手牌数量超出范围，取消第一个选中的手牌
            while (count > MaxCount)
            {
                if (SelectedCard.Count > 0) handcards[SelectedCard[0].Id].Unselect();
                else EquipArea.Instance.Equips.Values.ToList().Find(x => x.model == SelectedEquip[0]).Unselect();

                count--;
            }

            IsSettled = count >= MinCount;

            if (IsSettled && skill != null && skill is Model.Converted)
            {
                Converted = (skill as Model.Converted).Execute(SelectedCard);
            }
            else Converted = null;
        }

        public Dictionary<string, Card> ConvertedCards { get; set; } = new Dictionary<string, Card>();
        public bool ConvertIsSettle { get; private set; }

        public void InitConvertCard()
        {
            ConvertIsSettle = timerTask.MultiConvert.Count == 0;
            if (!IsSettled || ConvertIsSettle) return;

            foreach (var i in handcards.Values) i.button.interactable = false;
            var card = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>("Card");
            foreach (var i in timerTask.MultiConvert)
            {
                var instance = Instantiate(card);
                instance.transform.SetParent(handCardArea.transform, false);
                ConvertedCards.Add(i.Name, instance.GetComponent<Card>());
                ConvertedCards[i.Name].Init(i, true);
            }
            UpdateSpacing();

            foreach (var i in ConvertedCards.Values)
            {
                i.button.interactable = timerTask.IsValidCard(i.model);
            }

            foreach (var i in ConvertedCards.Values) if (i.gameObject.activeSelf) i.AddShadow();
        }

        public void UpdateConvertCard()
        {
            ConvertIsSettle = Converted != null;
        }

        /// <summary>
        /// 更新手牌数与手牌上限信息
        /// </summary>
        public void UpdateHandCardText(Model.Player player)
        {
            handCardText.text = player.HandCardCount.ToString() + "/" + player.HandCardLimit.ToString();
            UpdateSpacing();
        }

        public void UpdateHandCardText(Model.GetCard operation)
        {
            if (self.model != operation.player) return;
            UpdateHandCardText(operation.player);
        }

        public void UpdateHandCardText(Model.LoseCard operation)
        {
            if (self.model != operation.player) return;
            UpdateHandCardText(operation.player);
        }

        public void UpdateHandCardText(Model.UpdateHp operation)
        {
            if (self.model != operation.player) return;
            UpdateHandCardText(operation.player);
        }

        /// <summary>
        /// 更新卡牌间距
        /// </summary>
        private void UpdateSpacing()
        {
            int count = 0;
            foreach (var i in handcards.Values) if (i.gameObject.activeSelf) count++;
            foreach (var i in ConvertedCards) count++;

            // 若手牌数小于7，则不用设置负间距，直接返回
            if (count < 8)
            {
                handCardArea.GetComponent<GridLayoutGroup>().spacing = new Vector2(0, 0);
                return;
            }

            float spacing = -(count * 121.5f - 950) / (float)(count - 1) - 0.001f;
            handCardArea.GetComponent<GridLayoutGroup>().spacing = new Vector2(spacing, 0);
        }
    }
}