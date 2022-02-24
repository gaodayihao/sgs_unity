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
        private int maxCount;
        private int minCount;
        // 是否已设置
        public bool IsSettled { get; private set; } = false;


        /// <summary>
        /// 在手牌区中添加手牌
        /// </summary>
        public void AddHandCard(Model.AcquireCard operation)
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
            if (operation.HandCards is null) return;

            foreach (var i in operation.HandCards)
            {
                Destroy(handcards[i.Id].gameObject);
                handcards.Remove(i.Id);
            }
        }

        /// <summary>
        /// 初始化手牌区（显示进度条时调用）
        /// </summary>
        // public void InitCardArea(TimerType timerType, int maxCount, int minCount)
        // {
        //     this.maxCount = maxCount;
        //     this.minCount = minCount;

        //     if (maxCount == 0)
        //     {
        //         foreach (var card in handcards.Values)
        //         {
        //             card.button.interactable = false;
        //             card.AddShadow();
        //         }
        //         IsSettled = true;
        //         return;
        //     }

        //     switch (timerType)
        //     {
        //         // 出牌阶段
        //         case TimerType.PerformPhase:
        //             foreach (var card in handcards.Values)
        //             {
        //                 // 设置不能使用的手牌
        //                 card.button.interactable = Model.CardArea.PerformPhase(self.model, card.Id) ? true : false;
        //             }
        //             break;

        //         case TimerType.AskForShan:
        //             foreach (var card in handcards.Values)
        //             {
        //                 // 设置不能使用的手牌
        //                 card.button.interactable = card.name == "闪" ? true : false;
        //             }
        //             break;

        //         case TimerType.AskForTao:
        //             foreach (var card in handcards.Values)
        //             {
        //                 // 设置不能使用的手牌
        //                 card.button.interactable = card.name == "桃" ? true : false;
        //             }
        //             break;

        //         // case TimerType.CallSkill:
        //         //     break;

        //         // 弃牌
        //         default:
        //             foreach (var card in handcards.Values) card.button.interactable = true;
        //             break;
        //     }

        //     // 对已禁用的手牌设置阴影
        //     foreach (var card in handcards.Values) card.AddShadow();
        // }

        public void InitCardArea(Model.TimerTask timerTask)
        {
            if (timerTask.timerType != TimerType.UseWxkj && self.model != timerTask.player) return;

            this.maxCount = timerTask.maxCount;
            this.minCount = timerTask.minCount;

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

            if (timerTask.GivenCard != null)
            {
                foreach (var card in handcards.Values)
                {
                    card.button.interactable = timerTask.GivenCard.Contains(card.name);
                    card.AddShadow();
                }
                return;
            }

            switch (timerTask.timerType)
            {
                // 出牌阶段
                case TimerType.PerformPhase:
                    foreach (var card in handcards.Values)
                    {
                        // 设置不能使用的手牌
                        card.button.interactable = Model.CardArea.PerformPhase(self.model, card.Id) ? true : false;
                    }
                    break;

                // case TimerType.AskForShan:
                //     foreach (var card in handcards.Values)
                //     {
                //         // 设置不能使用的手牌
                //         card.button.interactable = card.name == "闪" ? true : false;
                //     }
                //     break;

                // case TimerType.AskForTao:
                //     foreach (var card in handcards.Values)
                //     {
                //         // 设置不能使用的手牌
                //         card.button.interactable = card.name == "桃" ? true : false;
                //     }
                //     break;

                // case TimerType.CallSkill:
                //     break;

                // 弃牌
                default:
                    foreach (var card in handcards.Values) card.button.interactable = true;
                    break;
            }

            // 对已禁用的手牌设置阴影
            foreach (var card in handcards.Values) card.AddShadow();
        }

        /// <summary>
        /// 重置手牌区（进度条结束时调用）
        /// </summary>
        public void ResetCardArea(Model.TimerTask timerTask)
        {
            if (timerTask.timerType != TimerType.UseWxkj && self.model != timerTask.player) return;

            // 重置手牌状态
            foreach (var card in handcards.Values) card.ResetCard();

            IsSettled = false;
        }

        /// <summary>
        /// 更新手牌区
        /// </summary>
        public void UpdateCardArea()
        {
            var equipArea = GetComponent<EquipArea>();
            // 若已选中手牌数量超出范围，取消第一个选中的手牌
            while (SelectedCard.Count + equipArea.SelectedCard.Count > maxCount)
            {
                if (SelectedCard.Count > 0) SelectedCard[0].Unselect();
                else equipArea.SelectedCard[0].Unselect();
            }

            IsSettled = SelectedCard.Count + equipArea.SelectedCard.Count >= minCount;
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

        public void UpdateHandCardText(Model.AcquireCard operation)
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