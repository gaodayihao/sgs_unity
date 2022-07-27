using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class 无双 : Triggered
    {
        public 无双(Player src) : base(src, "无双", true) { }

        public override void OnEnabled()
        {
            Src.playerEvents.afterUseCard.AddEvent(Src, Execute);
        }

        public override void OnDisabled()
        {
            Src.playerEvents.afterUseCard.RemoveEvent(Src, Execute);
        }

        public async Task<bool> Execute(Card card)
        {
            if (!(card is 杀)) return true;
            if ((card as 杀).ShanCount != 1) return true;

            await Task.Yield();
            (card as 杀).ShanCount = 2;
            return true;
        }
    }

    public class 利驭 : Triggered
    {
        public 利驭(Player src) : base(src, "利驭", false) { }

        public override void OnEnabled()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                if (i != Src) i.playerEvents.afterDamaged.AddEvent(Src, Execute);
            }
        }

        public override void OnDisabled()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                if (i != Src) i.playerEvents.afterDamaged.RemoveEvent(Src, Execute);
            }
        }

        public async Task<bool> Execute(Damaged damaged)
        {
            var dest = damaged.player;

            // 触发条件
            if (damaged.Src != Src || !(damaged.SrcCard is 杀)) return true;
            if (!dest.HaveCard()) return true;

            if (!await base.Execute()) return true;

            // 选择一张牌
            bool result = await CardPanel.Instance.Run(Src, damaged.player, TimerType.RegionPanel);
            Card card;
            if (!result)
            {
                if (dest.armor != null) card = dest.armor;
                else if (dest.plusHorse != null) card = dest.plusHorse;
                else if (dest.weapon != null) card = dest.weapon;
                else if (dest.subHorse != null) card = dest.subHorse;
                else if (dest.HandCardCount != 0) card = dest.HandCards[0];
                else card = dest.JudgeArea[0];
            }
            else card = CardPanel.Instance.Cards[0];

            // 获得牌
            await new GetCardFromElse(Src, dest, new List<Card> { card }).Execute();

            // 若为装备牌
            if (card is Equipage)
            {
                if (SgsMain.Instance.AlivePlayers.Count <= 2) return true;

                // 指定角色
                TimerTask.Instance.Extra = Src.Position.ToString();
                result = await TimerTask.Instance.Run(dest, TimerType.利驭, 0);

                Player dest1 = null;
                if (!result)
                {
                    foreach (var i in SgsMain.Instance.AlivePlayers)
                    {
                        if (i != Src && i != dest)
                        {
                            dest1 = i;
                            break;
                        }
                    }
                }
                else dest1 = TimerTask.Instance.Dests[0];

                // 使用决斗
                await Card.Convert<决斗>(new List<Card>()).UseCard(Src, new List<Player> { dest1 });
            }
            // 摸牌
            else await new GetCardFromPile(dest, 1).Execute();

            return true;
        }
    }
}
