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
}
