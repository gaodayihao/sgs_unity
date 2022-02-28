using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class DestArea : MonoBehaviour
    {
        private List<PlayerButton> players;

        private List<PlayerButton> Players
        {
            get
            {
                if (players is null)
                {
                    players = new List<PlayerButton>();

                    players.Add(transform.Find("Player").GetComponent<PlayerButton>());
                    foreach (Transform player in GameObject.Find("ElsePlayers").transform)
                    {
                        players.Add(player.GetComponent<PlayerButton>());
                    }
                }
                return players;
            }
        }

        private Self self { get => GetComponent<Self>(); }
        private CardArea cardArea { get => GetComponent<CardArea>(); }
        private OperationArea operationArea { get => GetComponent<OperationArea>(); }

        public List<PlayerButton> SelectedPlayer { get; private set; } = new List<PlayerButton>();
        private int maxCount;
        private int minCount;
        // 是否已设置
        public bool IsSettled { get; private set; } = false;

        // 定时器类型
        // private Model.TimerTask timerTask;

        public void InitDestArea()
        {
            if (Model.TimerTask.Instance.GivenDest != null)
            {
                maxCount = 1;
                minCount = 1;
                foreach (var player in players)
                {
                    if (player.model == Model.TimerTask.Instance.GivenDest)
                    {
                        player.Select();
                        break;
                    }
                }
                // IsSettled = true;
                // // 对不能指定的角色设置阴影
                // foreach (var player in Players) player.AddShadow();
                // return;

                // if(usesha)
            }

            // 设置目标数量范围
            switch (operationArea.timerType)
            {
                // 出牌阶段
                case TimerType.PerformPhase:
                    if (!cardArea.IsSettled) return;
                    var destCount = Model.DestArea.InitDestCount(self.model, cardArea.SelectedCard[0].name);
                    maxCount = destCount[0];
                    minCount = destCount[1];
                    break;

                case TimerType.ZBSM:
                case TimerType.UseSha:
                    // 打出
                    if (Model.TimerTask.Instance.timerType == TimerType.UseCard)
                    {
                        maxCount = 0;
                        minCount = 0;
                    }
                    // 使用
                    else
                    {
                        var shaDestCount = Model.DestArea.ShaDestCount(self.model);
                        maxCount = shaDestCount[0];
                        minCount = shaDestCount[1];
                    }
                    break;

                default:
                    if (Model.TimerTask.Instance.GivenDest == null)
                    {
                        maxCount = 0;
                        minCount = 0;
                    }
                    break;
            }

            UpdateDestArea();
        }

        // public void InitDestArea(Model.TimerTask timerTask)
        // {
        //     if (timerTask.timerType != TimerType.UseWxkj && self.model != timerTask.player) return;

        //     this.timerTask = timerTask;
        //     InitDestArea();
        // }

        /// <summary>
        /// 重置目标区
        /// </summary>
        public void ResetDestArea()
        {
            // 重置目标按键状态
            foreach (var player in Players) player.ResetButton();

            IsSettled = false;
        }

        public void ResetDestArea(Model.TimerTask timerTask)
        {
            if (timerTask.timerType != TimerType.UseWxkj && self.model != timerTask.player) return;

            ResetDestArea();
        }

        public void UpdateDestArea()
        {
            // 若已选中角色的数量超出范围，取消第一个选中的角色
            while (SelectedPlayer.Count > maxCount) SelectedPlayer[0].Unselect();

            IsSettled = SelectedPlayer.Count >= minCount;
            if (maxCount == 0) return;

            // 判断每个角色能否成为目标
            switch (operationArea.timerType)
            {
                case TimerType.PerformPhase:
                    foreach (var player in Players)
                    {
                        int cardId = cardArea.SelectedCard[0].Id;
                        int destPos = SelectedPlayer.Count != 0 ? SelectedPlayer[0].model.Position : -1;
                        bool enable = Model.DestArea.PerformPhase(self.model, player.model, cardId, destPos);
                        player.button.interactable = enable;
                    }
                    break;
                case TimerType.ZBSM:
                    foreach (var player in players)
                    {
                        player.button.interactable = Model.DestArea.UseSha(self.model, player.model);
                    }
                    break;
            }


            foreach (var player in players)
            {
                // 目标已指定
                if (Model.TimerTask.Instance.GivenDest != null && maxCount == 1) player.button.interactable = false;
                // 可选择额外目标
                else if (player.model == Model.TimerTask.Instance.GivenDest) player.button.interactable = false;
                // 对不能选择的角色设置阴影
                player.AddShadow();
            }
        }

    }
}
