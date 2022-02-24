using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
    public class Sha : Card
    {
        // public Sha()
        // {
        //     Name = "杀";
        //     Type = "基本牌";
        // }

        // public override async Task UseCard()
        // {
        //     await base.UseCard();

        //     foreach (var dest in Dests)
        //     {
        //         if (!await TimerTask.Instance.Run(dest, TimerType.AskForShan))
        //         {
        //             await new Damaged(dest, Src, 1).Execute();
        //         }
        //         else
        //         {
        //             if (Src.Equipages["武器"] is QingLongYanYueDao) await ((QingLongYanYueDao)Src.Equipages["武器"]).Skill(dest);
        //         }
        //     }
        //     ResetCard();
        // }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            src.ShaCount++;

            if (src.ShaCount > 1 && src.weapon is ZhuGeLianNu) src.weapon.SkillView();
            else if (dests.Count > 1 && src.weapon is FangTianHuaJi) src.weapon.SkillView();

            await base.UseCard(src, dests);

            // 发动青釭剑
            bool qgj = false;
            if (src.weapon is QingGangJian) qgj = ((QingGangJian)src.weapon).Skill(this);

            foreach (var dest in Dests)
            {
                bool hit = false;
                if (ShanCount == 0) hit = true;
                else
                {
                    for (int i = 0; i < ShanCount; i++)
                    {
                        if (!await Shan.Call(dest))
                        {
                            hit = true;
                            break;
                        }
                    }
                }

                if (hit == false)
                {
                    // 询问青龙偃月刀
                    if (src.weapon is QingLongYanYueDao) await ((QingLongYanYueDao)src.weapon).Skill(dest);
                    // 询问贯石斧
                    else if (src.weapon is GuanShiFu && await ((GuanShiFu)src.weapon).Skill()) hit = true;
                }

                if (hit == true)
                {
                    if (src.weapon is QiLinGong) await ((QiLinGong)src.weapon).Skill(dest);
                    await new Damaged(dest, Src, 1).Execute();
                }
            }

            // 重新激活防具
            if (qgj) ((QingGangJian)src.weapon).ResetArmor(this);
        }

        public int ShanCount { get; set; } = 1;
    }

    public class Shan : Card
    {
        public static async Task<bool> Call(Player player)
        {
            bool result;
            if (player.Equipages["防具"] is BaGuaZhen)
            {
                result = await ((BaGuaZhen)player.Equipages["防具"]).Skill();
                if (result) return true;
            }

            TimerTask.Instance.Hint = "请使用一张闪。";
            TimerTask.Instance.GivenCard = new List<string> { "闪" };
            result = await TimerTask.Instance.Run(player, TimerType.UseCard);

            if (result)
            {
                await TimerTask.Instance.Cards[0].UseCard(player);
                return true;
            }
            else
            {
                if (player.isAI) foreach (var card in player.HandCards)
                        if (card is Shan)
                        {
                            await card.UseCard(player);
                            return true;
                        }

                return false;
            }

        }
    }

    public class Tao : Card
    {
        // public override async Task UseCard()
        // {
        //     // 默认将目标设为使用者
        //     if (Dests is null) Dests = new Player[1] { Src };

        //     await base.UseCard();

        //     // 回复体力
        //     foreach (var dest in Dests) await new Recover(dest).Execute();

        //     ResetCard();
        // }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            // 默认将目标设为使用者
            if (dests is null || dests.Count == 0) dests = new List<Player> { src };
            // Debug.Log(src.Position);

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
            if (result)
            {
                await TimerTask.Instance.Cards[0].UseCard(player, new List<Player> { dest });
                return true;
            }
            else
            {
                if (player.isAI && player == dest) foreach (var card in player.HandCards)
                        if (card is Tao)
                        {
                            await card.UseCard(player);
                            return true;
                        }

                return false;
            }
        }
    }
}
