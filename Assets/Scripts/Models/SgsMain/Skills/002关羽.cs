using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
    /// <summary>
    /// 武圣
    /// </summary>
    public class WuSheng : Converted
    {
        public WuSheng(Player src) : base(src, "武圣", false, int.MaxValue, "杀") { }

        public override Card Execute(List<Card> cards)
        {
            return Card.Convert<Sha>(cards);
        }

        public override bool IsValidCard(Card card)
        {
            return card.Suit == "红桃" || card.Suit == "方片";
        }

        public override int MaxCard()
        {
            return 1;
        }

        public override int MinCard()
        {
            return 1;
        }

        public override bool IsValid()
        {
            var timerType = TimerTask.Instance.timerType;

            // 使用或打出杀
            if (timerType == TimerType.UseCard)
            {
                return TimerTask.Instance.GivenCard.Contains("杀");
            }

            // 出牌阶段
            else if (timerType == TimerType.PerformPhase)
            {
                return CardArea.UseSha(Src);
            }

            return false;
        }
    }

    /// <summary>
    /// 义绝
    /// </summary>
    public class YiJue : Active
    {
        public YiJue(Player src) : base(src, "义绝", false, 1) { }

        public override async Task Execute(List<Player> dests, List<Card> cards, string additional)
        {
            await base.Execute(dests, cards, additional);

            // 弃一张手牌
            await new Discard(Src, cards).Execute();
            // 展示手牌
            var showCard = (await ShowCard.ShowCardTimer(dests[0]));
            // 红色
            if (showCard[0].Suit == "红桃" || showCard[0].Suit == "方片")
            {
                await new GetCardFromElse(Src, dests[0], showCard).Execute();
            }
        }

        public void Awake()
        {
            // 
        }

        public override int MaxCard()
        {
            return 1;
        }

        public override int MinCard()
        {
            return 1;
        }

        public override int MaxDest()
        {
            return 1;
        }

        public override int MinDest()
        {
            return 1;
        }

        public override bool IsValidDest(Player dest, List<Card> cards, Player firstDest = null)
        {
            return dest.HandCardCount > 0;
        }
    }
}
