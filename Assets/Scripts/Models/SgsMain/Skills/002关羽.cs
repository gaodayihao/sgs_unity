using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
    /// <summary>
    /// 义绝
    /// </summary>
    public class YiJue : Active
    {
        public YiJue(Player src) : base(src, "义绝", false, 1) { }

        public override async Task Execute(List<Player> dests, List<Card> cards, string additional)
        {
            await base.Execute(dests, cards, additional);
            await new Discard(Src, cards).Execute();
            var showCard = (await ShowCard.ShowCardTimer(dests[0]));
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
