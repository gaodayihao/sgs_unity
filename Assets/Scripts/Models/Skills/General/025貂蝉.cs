using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class 离间 : Active
    {
        public 离间(Player src) : base(src, "离间", false, 1) { }

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

            await Card.Convert<决斗>().UseCard(dests[1], new List<Player> { dests[0] });
        }
    }
}