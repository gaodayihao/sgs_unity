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
                    return player.Hp < player.HpLimit ? true : false;

                case "杀":
                    return UseSha(player);

                default:
                    return true;
            }
        }

        public static bool UseSha(Player player)
        {
            if (player.Equipages["武器"] is ZhuGeLianNu) return true;
            return player.ShaCount < 1;
        }
    }
}
