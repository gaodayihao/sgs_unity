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
            Src.playerEvents.afterUseCard.AddEvent(Src, Execute杀);
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                i.playerEvents.afterUseCard.AddEvent(Src, Execute决斗);
            }
        }

        public override void OnDisabled()
        {
            Src.playerEvents.afterUseCard.RemoveEvent(Src, Execute杀);
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                i.playerEvents.afterUseCard.RemoveEvent(Src, Execute决斗);
            }
        }

        public async Task Execute杀(Card card)
        {
            if (card is not 杀) return;
            // if ((card as 杀).ShanCount != 1) return;

            await Task.Yield();
            Execute();
            foreach (var i in card.Dests)
            {
                if ((card as 杀).ShanCount[i.Position] == 1) (card as 杀).ShanCount[i.Position] = 2;
            }
            // (card as 杀).ShanCount = 2;
        }

        public async Task Execute决斗(Card card)
        {
            if (card is not 决斗) return;
            if (card.Src != Src && !card.Dests.Contains(Src)) return;

            await Task.Yield();
            Execute();
            if (card.Src == Src) (card as 决斗).DestShaCount = 2;
            else (card as 决斗).SrcShaCount = 2;
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

        public async Task Execute(Damaged damaged)
        {
            var dest = damaged.player;

            // 触发条件
            if (damaged.Src != Src || !(damaged.SrcCard is 杀)) return;
            if (dest.CardCount == 0) return;

            if (!await base.ShowTimer()) return;
            Execute();

            CardPanel.Instance.Hint = "对" + dest.PosStr + "号位发动利驭，获得其一张牌";
            var card = await CardPanel.Instance.SelectCard(Src, damaged.player);

            // 获得牌
            await new GetCardFromElse(Src, dest, new List<Card> { card }).Execute();

            // 若为装备牌
            if (card is Equipage)
            {
                if (SgsMain.Instance.AlivePlayers.Count <= 2) return;

                // 指定角色
                TimerTask.Instance.Hint = Src.PosStr + "号位对你发动利驭，选择一名角色";
                TimerTask.Instance.IsValidDest = player => player != Src && player != dest;
                bool result = await TimerTask.Instance.Run(dest, 0, 1);

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
        }
    }
}
