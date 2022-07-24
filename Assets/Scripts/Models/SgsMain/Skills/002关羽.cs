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
            return (card.Suit == "红桃" || card.Suit == "方片") && base.IsValidCard(card);
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

        public override async Task Execute(List<Player> dests, List<Card> cards, string additional)
        {
            Dest = dests[0];
            await base.Execute(dests, cards, additional);

            // 弃一张手牌
            await new Discard(Src, cards).Execute();
            // 展示手牌
            var showCard = (await ShowCard.ShowCardTimer(Dest));
            // 红色
            if (showCard[0].Suit == "红桃" || showCard[0].Suit == "方片")
            {
                await new GetCardFromElse(Src, Dest, showCard).Execute();
            }
            else
            {
                Dest.disabledCard += DisabledCard;
                foreach (var i in Dest.skills.Values) if (!i.Passive) i.Enabled--;
                TurnSystem.Instance.AfterTurn += Reset;
                Dest.playerEvents.whenDamaged.AddEvent(Src, WhenDamaged);
            }
        }

        Player Dest;

        public bool DisabledCard(Card card)
        {
            return true;
        }

        public async Task<bool> WhenDamaged(Damaged damaged)
        {
            await Task.Yield();
            if (damaged.Src == Src && damaged.SrcCard.Suit == "红桃")
            {
                damaged.Value--;
                Dest.playerEvents.whenDamaged.RemoveEvent(Src, WhenDamaged);
            }
            return true;
        }

        public void Reset()
        {
            Dest.disabledCard -= DisabledCard;
            foreach (var i in Dest.skills.Values) if (!i.Passive && i.Enabled < 1) i.Enabled++;
            Dest.playerEvents.whenDamaged.RemoveEvent(Src, WhenDamaged);
            TurnSystem.Instance.AfterTurn -= Reset;
        }
    }
}
