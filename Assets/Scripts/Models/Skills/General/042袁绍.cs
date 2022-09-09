using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
    public class 乱击 : Converted
    {
        public 乱击(Player src) : base(src, "乱击", false, int.MaxValue, "万箭齐发") { }

        public override Card Execute(List<Card> cards) => Card.Convert<万箭齐发>(cards);

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
}
