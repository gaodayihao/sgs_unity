using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
    /// <summary>
    /// 武圣
    /// </summary>
    public class 武圣 : Converted
    {
        public 武圣(Player src) : base(src, "武圣", false, int.MaxValue, "杀") { }

        public override Card Execute(List<Card> cards) => Card.Convert<杀>(cards);

        public override bool IsValidCard(Card card) => (card.Suit == "红桃" || card.Suit == "方片")
            && base.IsValidCard(card);

        public override void OnEnabled()
        {
            Src.unlimitedDst += IsUnlimited;
        }

        public override void OnDisabled()
        {
            Src.unlimitedDst -= IsUnlimited;
        }

        private bool IsUnlimited(Card card, Player dest) => card is 杀 && card.Suit == "方片";
    }

    /// <summary>
    /// 义绝
    /// </summary>
    public class 义绝 : Active
    {
        public 义绝(Player src) : base(src, "义绝", 1) { }

        public override int MaxCard => 1;

        public override int MinCard => 1;

        public override int MaxDest(List<Card> cards) => 1;

        public override int MinDest(List<Card> cards) => 1;

        public override bool IsValidDest(Player dest, Player first) => dest.HandCardCount > 0 && dest != Src;

        public override async Task Execute(List<Player> dests, List<Card> cards, string additional)
        {
            Dest = dests[0];
            await base.Execute(dests, cards, additional);

            // 弃一张手牌
            await new Discard(Src, cards).Execute();
            // 展示手牌
            TimerTask.Instance.Hint = Src.PosStr + "号位对你发动义绝，请展示一张手牌。";
            var showCard = await TimerAction.ShowCardTimer(Dest);

            // 红色
            if (showCard[0].Suit == "红桃" || showCard[0].Suit == "方片")
            {
                // 获得牌
                await new GetCardFromElse(Src, Dest, showCard).Execute();
                // 回复体力
                if (Dest.Hp < Dest.HpLimit)
                {
                    TimerTask.Instance.Hint = "是否让" + Dest.PosStr + "号位回复一点体力？";
                    bool result = await TimerTask.Instance.Run(Src);
                    if (result) await new Recover(Dest).Execute();
                }
            }
            // 黑色
            else
            {
                Dest.disabledCard += DisabledCard;
                foreach (var i in Dest.skills.Values) if (!i.Passive) i.SetActive(false);
                TurnSystem.Instance.AfterTurn += ResetEffect;
                Dest.playerEvents.whenDamaged.AddEvent(Src, WhenDamaged);
                isDone = false;
            }
        }

        private bool isDone;

        Player Dest;

        public bool DisabledCard(Card card) => true;

        public async Task WhenDamaged(Damaged damaged)
        {
            if (isDone) return;
            await Task.Yield();
            if (damaged.Src == Src && damaged.SrcCard is 杀 && damaged.SrcCard.Suit == "红桃")
            {
                damaged.Value--;
                isDone = true;
                // Dest.playerEvents.whenDamaged.RemoveEvent(Src, WhenDamaged);
            }
        }

        public void ResetEffect()
        {
            Dest.disabledCard -= DisabledCard;
            foreach (var i in Dest.skills.Values) if (!i.Passive && i.Enabled < 1) i.Enabled++;
            Dest.playerEvents.whenDamaged.RemoveEvent(Src, WhenDamaged);
            TurnSystem.Instance.AfterTurn -= ResetEffect;
        }
    }
}
