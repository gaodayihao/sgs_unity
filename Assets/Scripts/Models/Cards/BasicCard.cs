using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
    public class 杀 : Card
    {
        /// <summary>
        /// 杀
        /// </summary>
        public 杀()
        {
            Type = "基本牌";
            Name = "杀";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            src.ShaCount++;

            if (src.ShaCount > 1 && src.weapon is 诸葛连弩) src.weapon.SkillView();
            else if (dests.Count > 1 && src.weapon is 方天画戟) src.weapon.SkillView();

            await base.UseCard(src, dests);

            // 发动青釭剑
            bool qgj = false;
            if (src.weapon is 青缸剑) qgj = ((青缸剑)src.weapon).Skill(this);

            foreach (var dest in Dests)
            {
                bool hit = false;
                if (ShanCount == 0) hit = true;
                else
                {
                    for (int i = 0; i < ShanCount; i++)
                    {
                        if (!await 闪.Call(dest))
                        {
                            hit = true;
                            break;
                        }
                    }
                }

                if (hit == false)
                {
                    // 询问青龙偃月刀
                    if (src.weapon is 青龙偃月刀) await ((青龙偃月刀)src.weapon).Skill(dest);
                    // 询问贯石斧
                    else if (src.weapon is 贯石斧 && await ((贯石斧)src.weapon).Skill()) hit = true;
                }

                if (hit == true)
                {
                    if (src.weapon is 麒麟弓) await ((麒麟弓)src.weapon).Skill(dest);
                    await new Damaged(dest, 1, Src, this).Execute();
                }
            }

            // 重新激活防具
            if (qgj) ((青缸剑)src.weapon).ResetArmor(this);
        }

        public int ShanCount { get; set; } = 1;

        public static async Task<bool> Call(Player player)
        {
            TimerTask.Instance.Hint = "请打出一张杀。";
            TimerTask.Instance.GivenCard = new List<string> { "杀", "雷杀", "火杀" };
            bool result = await TimerTask.Instance.Run(player, TimerType.UseCard);

            if (player.isAI)
            {
                var card = player.FindCard<杀>();
                if (card != null)
                {
                    TimerTask.Instance.Cards.Add(card);
                    result = true;
                }
            }

            if (!result) return false;

            await TimerTask.Instance.Cards[0].Put(player);
            return true;
        }
    }

    /// <summary>
    /// 闪
    /// </summary>
    public class 闪 : Card
    {
        public 闪()
        {
            Type = "基本牌";
            Name = "闪";
        }

        public static async Task<bool> Call(Player player)
        {
            bool result;
            if (player.Equipages["防具"] is 八卦阵)
            {
                result = await ((八卦阵)player.Equipages["防具"]).Skill();
                if (result) return true;
            }

            TimerTask.Instance.Hint = "请使用一张闪。";
            TimerTask.Instance.GivenCard = new List<string> { "闪" };
            result = await TimerTask.Instance.Run(player, TimerType.UseCard);

            if (player.isAI)
            {
                // foreach (var card in player.HandCards)
                // {
                //     if (card is Shan)
                //     {
                //         TimerTask.Instance.Cards = new List<Card> { card };
                //         result = true;
                //         break;
                //     }
                // }
                var card = player.FindCard<闪>();
                if (card != null)
                {
                    TimerTask.Instance.Cards.Add(card);
                    result = true;
                }
            }

            if (!result) return false;

            else
            {
                await TimerTask.Instance.Cards[0].UseCard(player);
                return true;
            }

        }
    }

    /// <summary>
    /// 桃
    /// </summary>
    public class 桃 : Card
    {
        public 桃()
        {
            Type = "基本牌";
            Name = "桃";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            // 默认将目标设为使用者
            if (dests is null || dests.Count == 0) dests = new List<Player> { src };

            await base.UseCard(src, dests);

            // 回复体力
            foreach (var dest in Dests) await new Recover(dest).Execute();
        }

        public static async Task<bool> Call(Player player, Player dest)
        {
            TimerTask.Instance.Hint = "请使用一张桃。";
            TimerTask.Instance.GivenDest = dest;
            TimerTask.Instance.GivenCard = new List<string> { "桃" };
            bool result = await TimerTask.Instance.Run(player, TimerType.UseCard);

            if (player.isAI && player == dest)
            {
                // foreach (var card in player.HandCards)
                // {
                //     if (card is Tao)
                //     {
                //         TimerTask.Instance.Cards = new List<Card> { card };
                //         result = true;
                //         break;
                //     }
                // }
                var card = player.FindCard<桃>();
                if (card != null)
                {
                    TimerTask.Instance.Cards.Add(card);
                    result = true;
                }
            }

            if (!result) return false;

            else
            {
                await TimerTask.Instance.Cards[0].UseCard(player);
                return true;
            }
        }
    }
}
