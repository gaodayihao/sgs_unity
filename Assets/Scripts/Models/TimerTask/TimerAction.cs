using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Model
{
    public static class TimerAction
    {
        /// <summary>
        /// 选择手牌
        /// </summary>
        public static async Task<List<Card>> SelectHandCard(Player player, int count)
        {
            TimerTask.Instance.ValidCard = (card) => player.HandCards.Contains(card);
            TimerTask.Instance.Refusable = false;
            bool result = await TimerTask.Instance.Run(player, count, 0);
            return result ? TimerTask.Instance.Cards : player.HandCards.Take(count).ToList();
        }


        /// <summary>
        /// 弃手牌
        /// </summary>
        public static async Task DiscardFromHand(Player player, int count)
        {
            TimerTask.Instance.Hint = "请弃置" + count.ToString() + "张手牌。";
            await new Discard(player, await SelectHandCard(player, count)).Execute();
        }

        /// <summary>
        /// 展示手牌
        /// </summary>
        public static async Task<List<Card>> ShowCardTimer(Player player, int count = 1)
        {
            var cards = await SelectHandCard(player, count);
            var showCard = new ShowCard(player, cards);
            await showCard.Execute();
            return showCard.Cards;
        }

        // public static async Task Compete(Player src,Player dest)
        // {
        //     // 
        // }
    }
}
