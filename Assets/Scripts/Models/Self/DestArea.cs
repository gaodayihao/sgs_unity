using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class DestArea
    {

        /// <summary>
        /// 根据玩家和卡牌id初始化目标数量
        /// </summary>
        /// <returns>目标数量最大值与最小值</returns>
        public static int MaxDest(List<Card> cards)
        {
            var player = TimerTask.Instance.player;
            var cardName = cards[0].Name;
            switch (cardName)
            {
                case "杀" + "":
                case "雷杀":
                case "火杀":
                    return ShaMaxDest(player);
                case "决斗":
                case "过河拆桥":
                case "顺手牵羊":
                case "乐不思蜀":
                case "兵粮寸断":
                case "火攻":
                    return 1;
                case "借刀杀人":
                    return 2;
                case "铁索连环":
                    return 2;
                default:
                    return 0;
            }
        }

        public static int MinDest(List<Card> cards)
        {
            switch (cards[0].Name)
            {
                case "杀" + "":
                case "雷杀":
                case "火杀":
                case "决斗":
                case "过河拆桥":
                case "顺手牵羊":
                case "乐不思蜀":
                case "兵粮寸断":
                case "火攻":
                    return 1;
                case "借刀杀人":
                    return 2;
                default:
                    return 0;
            }
        }

        public static int ShaMaxDest(Player player)
        {
            int maxCount = 1;
            if (player.HandCardCount == 1 && player.Equipages["武器"] is 方天画戟) maxCount += 2;
            return maxCount;
        }

        /// <summary>
        /// 判断dest是否能成为src的目标
        /// </summary>
        public static bool ValidDest(Player dest, Card card, Player firstdest)
        {
            var src = TurnSystem.Instance.CurrentPlayer;
            if (!dest.IsAlive) return false;
            if (src.UnlimitedDst(card, dest)) return true;

            switch (card.Name)
            {
                case "杀":
                case "火杀":
                case "雷杀":
                    return UseSha(src, dest);

                case "过河拆桥":
                    return src != dest && dest.HaveCard();

                case "顺手牵羊":
                    return src.GetDistance(dest) == 1 && dest.HaveCard();

                case "借刀杀人":
                    if (firstdest is null) return src != dest && dest.weapon != null;
                    else return UseSha(firstdest, dest);

                case "决斗":
                    return src != dest;

                case "火攻":
                    return dest.HandCardCount != 0;

                default:
                    return true;
            }
        }

        public static bool UseSha(Player src, Player dest)
        {
            return src != dest && src.AttackRange >= src.GetDistance(dest);
        }
    }
}
