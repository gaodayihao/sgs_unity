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

        private Player self { get => SgsMain.Instance.self; }
        private CardArea cardArea { get => CardArea.Instance; }
        private Model.TimerTask timerTask { get => Model.TimerTask.Instance; }

        public List<PlayerButton> SelectedPlayer { get; private set; } = new List<PlayerButton>();
        private int maxCount;
        private int minCount;
        // 是否已设置
        public bool IsSettled { get; private set; } = false;

        public void InitDestArea()
        {
            if (!cardArea.IsSettled) return;
            var skill = SkillArea.Instance.SelectedSkill;

            if (cardArea.Converted != null)
            {
                var cards = new List<Model.Card> { cardArea.Converted };
                maxCount = timerTask.MaxDest is null ? timerTask.maxDest : timerTask.MaxDest(cards);
                minCount = timerTask.MinDest is null ? timerTask.minDest : timerTask.MinDest(cards);

            }

            else if (skill != null)
            {
                maxCount = skill.MaxDest(cardArea.model);
                minCount = skill.MinDest(cardArea.model);

            }
            else
            {
                maxCount = timerTask.MaxDest is null ? timerTask.maxDest : timerTask.MaxDest(cardArea.model);
                minCount = timerTask.MinDest is null ? timerTask.minDest : timerTask.MinDest(cardArea.model);
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
            // 重置目标按键状态
            foreach (var player in Players) player.ResetButton();

            IsSettled = false;
        }

        public void ResetDestArea(Model.TimerTask timerTask)
        {
            if (!timerTask.isWxkj && self.model != timerTask.player) return;

            ResetDestArea();
        }

        public void UpdateDestArea()
        {
            // 若已选中角色的数量超出范围，取消第一个选中的角色
            while (SelectedPlayer.Count > maxCount) SelectedPlayer[0].Unselect();

            IsSettled = SelectedPlayer.Count >= minCount;
            if (maxCount == 0) return;

            var skill = SkillArea.Instance.SelectedSkill;
            var firstDest = SelectedPlayer.Count > 0 ? SelectedPlayer[0].model : null;

            // 判断每名角色能否成为目标

            if (cardArea.Converted != null)
            {
                foreach (var i in Players)
                {
                    i.button.interactable = Model.TimerTask.Instance.ValidDest(i.model, cardArea.Converted, firstDest);
                }
            }

            else if (skill != null)
            {
                foreach (var player in Players)
                {
                    // 设置不能指定的目标
                    player.button.interactable = skill.IsValidDest(player.model, cardArea.model, firstDest);
                }

            }
            else
            {
                var card = cardArea.SelectedCard.Count > 0 ? cardArea.SelectedCard[0].model : null;
                foreach (var i in Players)
                {
                    i.button.interactable = Model.TimerTask.Instance.ValidDest(i.model, card, firstDest);
                }
            }

            // 对不能选择的角色设置阴影
            foreach (var player in Players) player.AddShadow();
        }

    }
}
