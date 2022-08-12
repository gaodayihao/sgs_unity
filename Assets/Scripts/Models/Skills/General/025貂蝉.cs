using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class 离间 : Active
    {
        public 离间(Player src) : base(src, "离间", 1) { }

        public override int MaxCard()
        {
            return 1;
        }

        public override int MinCard()
        {
            return 1;
        }

        public override int MaxDest(List<Card> cards)
        {
            return 2;
        }

        public override int MinDest(List<Card> cards)
        {
            return 2;
        }

        public override bool IsValidDest(Player dest, List<Card> cards, Player firstDest = null)
        {
            return dest != Src && dest.general.gender;
        }

        public override async Task Execute(List<Player> dests, List<Card> cards, string additional)
        {
            await base.Execute(dests, cards, additional);

            await new Discard(Src, cards).Execute();
            await Card.Convert<决斗>().UseCard(dests[1], new List<Player> { dests[0] });
        }
    }

    public class 闭月 : Triggered
    {
        public 闭月(Player src) : base(src, "闭月", false) { }

        public override void OnEnabled()
        {
            Src.playerEvents.startPhaseEvents[Phase.End].AddEvent(Src, Execute);
        }

        public override void OnDisabled()
        {
            Src.playerEvents.startPhaseEvents[Phase.End].RemoveEvent(Src, Execute);
        }

        public new async Task Execute()
        {
            if (!await base.ShowTimer()) return;
            base.Execute();
            await new GetCardFromPile(Src, Src.HandCardCount == 0 ? 2 : 1).Execute();
        }
    }
}