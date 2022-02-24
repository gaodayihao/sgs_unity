using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
    public class WuXieKeJi : Card
    {
        public static async Task<bool> Call(Card card, Player dest)
        {
            string hint = dest != null ? "对" + (dest.Position + 1).ToString() + "号位" : "";
            TimerTask.Instance.Hint = card.Name + "即将" + hint + "生效，是否使用无懈可击？";

            bool result = await TimerTask.Instance.RunWxkj();
            if (result)
            {
                Debug.Log(TimerTask.Instance.Cards[0].Name);
                var wxkj = (WuXieKeJi)TimerTask.Instance.Cards[0];
                await wxkj.UseCard(TimerTask.Instance.player);
                if (!wxkj.isCountered) return true;
            }

            return false;

        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            await base.UseCard(src, dests);

            isCountered = await Call(this, null);
        }

        private bool isCountered;
    }

    public class GuoHeChaiQiao : Card
    {
        public override async Task UseCard(Player src, List<Player> dests)
        {
            await base.UseCard(src, dests);
            foreach (var dest in Dests)
            {
                if (await WuXieKeJi.Call(this, dest)) break;

                CardPanel.Instance.Title = "过河拆桥";
                bool result = await CardPanel.Instance.Run(Src, dest, TimerType.GHCQ);

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

                if (card is DelayScheme && dest.JudgeArea.Contains((DelayScheme)card))
                {
                    ((DelayScheme)card).RemoveToJudgeArea();
                    CardPile.Instance.AddToDiscard(card);
                }
                else await new Discard(dest, new List<Card> { card }).Execute();
            }
        }
    }

    public class ShunShouQianYang : Card
    {
        public override async Task UseCard(Player src, List<Player> dests)
        {
            await base.UseCard(src, dests);
            foreach (var dest in Dests)
            {
                if (await WuXieKeJi.Call(this, dest)) break;

                CardPanel.Instance.Title = "顺手牵羊";
                bool result = await CardPanel.Instance.Run(Src, dest, TimerType.GHCQ);

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

                if (card is DelayScheme && dest.JudgeArea.Contains((DelayScheme)card))
                {
                    ((DelayScheme)card).RemoveToJudgeArea();
                    await new GetCard(src, new List<Card> { card }).Execute();
                }
                else await new GetCardFromElse(src, dest, new List<Card> { card }).Execute();
            }
        }
    }

    public class JueDou : Card
    {
        public override async Task UseCard(Player src, List<Player> dests)
        {
            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (await WuXieKeJi.Call(this, dest)) break;

                var player = dest;
                bool done = false;
                while (!done)
                {
                    done = !await Sha.Call(player);
                    if (done) await new Damaged(player, 1, player == dest ? src : dest, this).Execute();
                    else player = player == dest ? src : dest;
                }
            }
        }
    }

    public class NanManRuQin : Card
    {
        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            dests = new List<Player>();
            for (Player i = src.Next; i != src; i = i.Next) dests.Add(i);

            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (await WuXieKeJi.Call(this, dest)) break;

                if (!await Sha.Call(dest)) await new Damaged(dest, 1, src, this).Execute();
            }
        }
    }

    public class WanJianQiFa : Card
    {
        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            dests = new List<Player>();
            for (Player i = src.Next; i != src; i = i.Next) dests.Add(i);

            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (await WuXieKeJi.Call(this, dest)) break;

                if (!await Shan.Call(dest)) await new Damaged(dest, 1, src, this).Execute();
            }
        }
    }

    public class TaoYuanJieYi : Card
    {
        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            dests = new List<Player>();
            Player i = src;
            do
            {
                if (i.Hp < i.HpLimit) dests.Add(i);
                i = i.Next;
            } while (i != src);

            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (await WuXieKeJi.Call(this, dest)) break;

                await new Recover(dest).Execute();
            }
        }
    }

    public class WuZhongShengYou : Card
    {
        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            // 默认将目标设为使用者
            if (dests is null || dests.Count == 0) dests = new List<Player> { src };
            
            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (await WuXieKeJi.Call(this, dest)) break;

                await new GetCardFromPile(dest, 2).Execute();
            }
        }
    }
}
