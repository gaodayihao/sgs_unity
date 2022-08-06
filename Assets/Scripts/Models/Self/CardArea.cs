using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class CardArea
    {
        public static bool ValidCard(Card card)
        {
            var player = TurnSystem.Instance.CurrentPlayer;
            if (!player.HandCards.Contains(card) && !card.IsConvert) return false;
            if (player.DisabledCard(card)) return false;
            switch (card.Name)
            {
                case "闪":
                case "无懈可击":
                    return false;

                case "桃":
                    return player.Hp < player.HpLimit;

                case "杀":
                case "雷杀":
                case "火杀":
                    return UseSha(player, card);

                default:
                    return true;
            }
        }

        public static bool UseSha(Player player, Card card = null)
        {
            if (card is null) card = Card.Convert<杀>();
            return player.ShaCount < 1 || player.UnlimitedCard(card);
        }
    }
}
