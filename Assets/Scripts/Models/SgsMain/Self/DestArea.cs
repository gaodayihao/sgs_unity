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
        public static int[] InitDestCount(Player player, int id)
        {
            int maxCount = 0;
            int minCount = 0;
            Card card = CardPile.Instance.cards[id];
            switch (card.Name)
            {
                case "杀":
                case "雷杀":
                case "火杀":
                    maxCount = 1;
                    minCount = 1;
                    if (player.HandCardCount == 1 && player.Equipages["武器"] is FangTianHuaJi) maxCount += 2;
                    break;
                case "决斗":
                case "过河拆桥":
                case "顺手牵羊":
                case "乐不思蜀":
                case "兵粮寸断":
                case "火攻":
                    maxCount = 1;
                    minCount = 1;
                    break;
                case "借刀杀人":
                    maxCount = 2;
                    minCount = 2;
                    break;
                case "铁索连环":
                    maxCount = 2;
                    break;
                default:
                    break;
            }
            return new int[2] { maxCount, minCount };
        }

        /// <summary>
        /// 判断dest是否能成为src的目标
        /// </summary>
        public static bool PerformPhase(Player src, Player dest, int id, int firstdest = -1)
        {
            if (!dest.IsAlive) return false;
            Card card = CardPile.Instance.cards[id];

            switch (card.Name)
            {
                case "杀":
                case "火杀":
                case "雷杀":
                    return UseSha(src, dest);

                case "过河拆桥":
                    return src != dest && HaveCard(dest);

                case "顺手牵羊":
                    return src.GetDistance(dest) == 1 && HaveCard(dest);

                case "借刀杀人":
                    if (firstdest == -1) return src != dest && src.weapon != null;
                    else return UseSha(SgsMain.Instance.players[firstdest], dest);

                case "决斗":
                    return src != dest;

                default:
                    return true;
            }
        }

        private static bool HaveCard(Player player)
        {
            if (player.HandCardCount != 0) return true;
            foreach (var equip in player.Equipages.Values) if (equip != null) return true;
            if (player.JudgeArea.Count != 0) return true;
            return false;
        }

        public static bool UseSha(Player src, Player dest)
        {
            return src != dest && src.AttackRange >= src.GetDistance(dest);
        }
    }
}
