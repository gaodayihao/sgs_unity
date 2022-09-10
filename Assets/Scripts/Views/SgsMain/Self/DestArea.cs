using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class DestArea : SingletonMono<DestArea>
    {
        private List<PlayerButton> players;

        private List<PlayerButton> Players
        {
            get
            {
                if (players is null)
                {
                    players = new List<PlayerButton>();
                    foreach (var i in SgsMain.Instance.players)
                    {
                        players.Add(i.GetComponent<PlayerButton>());
                    }
                }
                return players;
            }
        }

        private List<Model.Player> SelectedPlayer => Model.Operation.Instance.Dests;
        private Model.Skill skill => Model.Operation.Instance.skill;
        private Player self => SgsMain.Instance.self;
        private Model.TimerTask timerTask => Model.TimerTask.Instance;

        private int maxCount;
        private int minCount;
        // 是否已设置
        public bool IsSettled { get; private set; } = false;

        public void InitDestArea()
        {
            if (!CardArea.Instance.IsSettled || !CardArea.Instance.ConvertIsSettle) return;

            if (skill != null)
            {
                maxCount = skill.MaxDest;
                minCount = skill.MinDest;
            }
            else
            {
                maxCount = timerTask.MaxDest();
                minCount = timerTask.MinDest();
            }

            UpdateDestArea();

            int c = 0;
            foreach (var i in Players)
            {
                if (i.button.interactable) c++;
            }

            if (c == minCount)
            {
                foreach (var i in players) if (i.button.interactable) i.ClickPlayer();
            }
        }

        /// <summary>
        /// 重置目标区
        /// </summary>
        public void ResetDestArea()
        {
            if (!timerTask.isWxkj && self.model != timerTask.player) return;

            // 重置目标按键状态
            foreach (var player in Players) player.ResetButton();

            IsSettled = false;
        }

        public void UpdateDestArea()
        {
            // 若已选中角色的数量超出范围，取消第一个选中的角色
            while (SelectedPlayer.Count > maxCount) Players.Find(x => x.model == SelectedPlayer[0]).Unselect();

            IsSettled = SelectedPlayer.Count >= minCount;
            if (maxCount == 0) return;

            if (skill != null)
            {
                foreach (var player in Players)
                {
                    // 设置不能指定的目标
                    player.button.interactable = skill.IsValidDest(player.model);
                }
            }
            else
            {
                foreach (var i in Players)
                {
                    i.button.interactable = Model.TimerTask.Instance.IsValidDest(i.model);
                }
            }

            // 对不能选择的角色设置阴影
            foreach (var player in Players) player.AddShadow();
        }
    }
}
