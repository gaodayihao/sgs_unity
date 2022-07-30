using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        private Player self { get => SgsMain.Instance.self; }
        private EquipArea equipArea { get => EquipArea.Instance; }

        // 被选中卡牌
        public List<Card> SelectedCard { get; private set; } = new List<Card>();
        public List<Model.Card> model
        {
            get
            {
                var m = new List<Model.Card>();
                foreach (var i in SelectedCard) m.Add(i.model);
                foreach (var i in equipArea.SelectedCard) m.Add(i.model);
                return m;
            }
        }
        private int maxCount;
        private int minCount;
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
            // while (!ABManager.Instance.ABMap.ContainsKey("sgsasset")) await Task.Yield();
            var card = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>("Card");

            // 实例化新卡牌，添加到手牌区，并根据卡牌id初始化
            bool active = operation.player == self.model;
            foreach (var i in cards)
            {
                if (handcards.ContainsKey(i.Id)) continue;
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
            // UpdateSpacing();
        }

        /// <summary>
        /// 在手牌区中移除手牌
        /// </summary>
        public void RemoveHandCard(Model.LoseCard operation)
        {
            if (!operation.player.isSelf) return;

            foreach (var i in operation.Cards)
            {
                if (handcards.ContainsKey(i.Id))
                {
                    Destroy(handcards[i.Id].gameObject);
                    handcards.Remove(i.Id);
                }
            }
        }

        public void InitCardArea(TimerType timerType)
        {
            var timerTask = Model.TimerTask.Instance;

            // 可选卡牌数量
            if (timerType == timerTask.timerType)
            {
                maxCount = timerTask.maxCount;
                minCount = timerTask.minCount;
            }
            else
            {
                switch (timerType)
                {
                    case TimerType.丈八蛇矛:
                        maxCount = 3;
                        minCount = 3;
                        break;

                    case TimerType.CallSkill:
                        var skill = SkillArea.Instance.SelectedSkill.model;
                        maxCount = skill.MaxCard();
                        minCount = skill.MinCard();
                        break;

                    default:
                        maxCount = 0;
                        minCount = 0;
                        break;
                }
            }

            if (maxCount == 0)
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
            if (timerType == TimerType.无懈可击)
            {
                foreach (var i in handcards.Values) i.gameObject.SetActive(i.name == "无懈可击");
                UpdateSpacing();
            }

            // 判断每张卡牌是否可选

            if (Model.TimerTask.Instance.timerType == timerType && Model.TimerTask.Instance.GivenCard != null)
            {
                foreach (var i in handcards.Values)
                {
                    if (!i.gameObject.activeSelf) continue;
                    i.button.interactable = Model.TimerTask.Instance.GivenCard.Contains(i.name);
                }
            }

            else
            {
                switch (timerType)
                {
                    // 出牌阶段
                    case TimerType.PerformPhase:
                        foreach (var card in handcards.Values)
                        {
                            // 设置不能使用的手牌
                            card.button.interactable = Model.CardArea.PerformPhase(self.model, card.Id);
                        }
                        break;

                    case TimerType.CallSkill:
                        var skill = SkillArea.Instance.SelectedSkill.model;
                        foreach (var i in handcards.Values)
                        {
                            if (!i.gameObject.activeSelf) continue;
                            // 设置不能使用的手牌
                            i.button.interactable = skill.IsValidCard(i.model);
                        }
                        break;

                    // 弃牌
                    default:
                        foreach (var card in handcards.Values) card.button.interactable = true;
                        break;
                }
            }

            // 设置禁用卡牌
            if (timerType == TimerType.PerformPhase || timerType == TimerType.UseCard || timerType == TimerType.无懈可击)
            {
                foreach (var i in handcards.Values)
                {
                    if (self.model.DisabledCard(i.model)) i.button.interactable = false;
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
            // 重置手牌状态
            foreach (var card in handcards.Values) if (card.gameObject.activeSelf) card.ResetCard();
            if (OperationArea.Instance.timerType == TimerType.无懈可击)
            {
                foreach (var i in handcards.Values) i.gameObject.SetActive(self.model.HandCards.Contains(i.model));
                UpdateSpacing();
            }

            IsSettled = false;
        }

        public void ResetCardArea(Model.TimerTask timerTask)
        {
            if (timerTask.timerType != TimerType.无懈可击 && self.model != timerTask.player) return;

            ResetCardArea();
        }

        /// <summary>
        /// 更新手牌区
        /// </summary>
        public void UpdateCardArea()
        {
            int count = SelectedCard.Count + equipArea.SelectedCard.Count;
            foreach (var card in equipArea.SelectedCard)
            {
                if (card.name == Model.TimerTask.Instance.GivenSkill)
                {
                    count--;
                    break;
                }
            }

            // 若已选中手牌数量超出范围，取消第一个选中的手牌
            while (count > maxCount)
            {
                if (SelectedCard.Count > 0) SelectedCard[0].Unselect();
                else if (equipArea.SelectedCard[0].name != Model.TimerTask.Instance.GivenSkill)
                {
                    equipArea.SelectedCard[0].Unselect();
                }
                else equipArea.SelectedCard[1].Unselect();
                count--;
            }

            IsSettled = count >= minCount;
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
        /// <param name="count"></param>
        private void UpdateSpacing()
        {
            int count = 0;
            foreach (var i in handcards.Values) if (i.gameObject.activeSelf) count++;

            // 若手牌数小于7，则不用设置负间距，直接返回
            if (count < 8)
            {
                handCardArea.GetComponent<GridLayoutGroup>().spacing = new Vector2(0, 0);
                return;
            }

            // Card.width = 121.5, HandCardArea.width = 950
            float spacing = -(count * 121.5f - 950) / (float)(count - 1) - 0.001f;
            handCardArea.GetComponent<GridLayoutGroup>().spacing = new Vector2(spacing, 0);
        }
    }
}