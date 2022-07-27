using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class CardArea : MonoBehaviour
    {
        // 手牌区
        public GameObject handCardArea;
        // 手牌数
        public Text handCardText;
        // 手牌
        public Dictionary<int, Card> handcards = new Dictionary<int, Card>();

        private Self self { get => GetComponent<Self>(); }

        // 被选中卡牌
        public List<Card> SelectedCard { get; private set; } = new List<Card>();
        public List<Model.Card> model
        {
            get
            {
                var model = new List<Model.Card>();
                foreach (var i in SelectedCard) model.Add(i.model);
                return model;
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
            var player = operation.player;
            if (self.model != operation.player) return;

            // 所有新获得手牌的id
            var cards = operation.Cards;

            // 从assetbundle中加载卡牌预制件
            // while (!ABManager.Instance.ABMap.ContainsKey("sgsasset")) await Task.Yield();
            var card = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>("Card");

            // 实例化新卡牌，添加到手牌区，并根据卡牌id初始化
            foreach (var i in cards)
            {
                var instance = Instantiate(card);
                instance.transform.SetParent(handCardArea.transform, false);
                handcards.Add(i.Id, instance.GetComponent<Card>());
                handcards[i.Id].Init(i);
            }
        }

        /// <summary>
        /// 在手牌区中移除手牌
        /// </summary>
        public void RemoveHandCard(Model.LoseCard operation)
        {
            if (self.model != operation.player) return;
            if (operation.Cards is null) return;

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
                    case TimerType.ZBSM:
                        maxCount = 3;
                        minCount = 3;
                        break;

                    case TimerType.CallSkill:
                        var skill = GetComponent<SkillArea>().SelectedSkill.model;
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
                foreach (var card in handcards.Values)
                {
                    card.button.interactable = false;
                    card.AddShadow();
                }
                IsSettled = true;
                return;
            }

            // 判断每张卡牌是否可选

            if (Model.TimerTask.Instance.timerType == timerType && Model.TimerTask.Instance.GivenCard != null)
            {
                foreach (var card in handcards.Values)
                {
                    card.button.interactable = Model.TimerTask.Instance.GivenCard.Contains(card.name);
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
                        var skill = GetComponent<SkillArea>().SelectedSkill.model;
                        foreach (var card in handcards.Values)
                        {
                            // 设置不能使用的手牌
                            card.button.interactable = skill.IsValidCard(card.model);
                        }
                        break;

                    // 弃牌
                    default:
                        foreach (var card in handcards.Values) card.button.interactable = true;
                        break;
                }
            }

            // 设置禁用卡牌
            if (timerType == TimerType.PerformPhase || timerType == TimerType.UseCard || timerType == TimerType.UseWxkj)
            {
                foreach (var i in handcards.Values)
                {
                    if (self.model.DisabledCard(i.model)) i.button.interactable = false;
                }
            }

            // 对已禁用的手牌设置阴影
            foreach (var card in handcards.Values) card.AddShadow();
        }

        /// <summary>
        /// 重置手牌区（进度条结束时调用）
        /// </summary>
        public void ResetCardArea()
        {
            Debug.Log("重置手牌状态");
            // 重置手牌状态
            foreach (var card in handcards.Values) card.ResetCard();

            IsSettled = false;
        }

        public void ResetCardArea(Model.TimerTask timerTask)
        {
            if (timerTask.timerType != TimerType.UseWxkj && self.model != timerTask.player) return;

            ResetCardArea();
        }

        /// <summary>
        /// 更新手牌区
        /// </summary>
        public void UpdateCardArea()
        {
            var equipArea = GetComponent<EquipArea>();

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
        private void UpdateHandCardText(Model.Player player)
        {
            // 若目标玩家不是自己，直接返回
            if (self.model != player) return;

            handCardText.text = player.HandCardCount.ToString() + "/" + player.HandCardLimit.ToString();
            UpdateSpacing();
        }

        public void UpdateHandCardText(Model.GetCard operation)
        {
            UpdateHandCardText(operation.player);
        }

        public void UpdateHandCardText(Model.LoseCard operation)
        {
            UpdateHandCardText(operation.player);
        }

        public void UpdateHandCardText(Model.UpdateHp operation)
        {
            UpdateHandCardText(operation.player);
        }

        /// <summary>
        /// 更新卡牌间距
        /// </summary>
        /// <param name="count"></param>
        private void UpdateSpacing()
        {
            int count = handcards.Count;

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