using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class CardArea
    {
        public static bool PerformPhase(Player player, int id)
        {
            Card card = CardPile.Instance.cards[id];

            switch (card.Name)
            {
                case "闪":
                case "无懈可击":
                    return false;

                case "桃":
                    return player.Hp < player.HpLimit;

                case "杀":
                    return UseSha(player, card);

                default:
                    return true;
            }
        }

        public static bool UseSha(Player player, Card card = null)
        {
            // if (player.Equipages["武器"] is ZhuGeLianNu) return true;
            // return player.ShaCount < 1;
            if (card is null) card = Card.Convert<杀>(new List<Card>());
            return player.ShaCount < 1 || player.UnlimitedCard(card);
        }
    }
}
