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
            await base.UseCard(src, dests);

            // 询问青釭剑 雌雄双股剑 诸葛连弩 方天画戟
            if (src.weapon != null) await src.weapon.UseShaSkill(this);

            foreach (var dest in Dests)
            {
                if (ShanCount == 0) isDamage = true;
                else
                {
                    for (int i = 0; i < ShanCount; i++)
                    {
                        if (!await 闪.Call(dest, this))
                        {
                            isDamage = true;
                            break;
                        }
                    }
                }

                if (!isDamage && src.weapon != null) await src.weapon.CounteredSkill(this, dest);

                if (isDamage)
                {
                    if (src.weapon != null) await src.weapon.DamageSkill(this, dest);
                    Damage type = this is 火杀 ? Damage.Fire : this is 雷杀 ? Damage.Thunder : Damage.Normal;
                    await new Damaged(dest, Src, this, 1, type).Execute();
                }
            }

            ShanCount = 1;
            IgnoreArmor = false;
        }

        public int ShanCount { get; set; } = 1;
        public bool IgnoreArmor { get; set; } = false;
        public bool isDamage { get; set; }

        public static async Task<bool> Call(Player player)
        {
            TimerTask.Instance.Hint = "请打出一张杀。";
            TimerTask.Instance.ValidCard = (card) => card is 杀 && !player.DisabledCard(card);
            bool result = await TimerTask.Instance.Run(player, 1, 0);

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

        public static async Task<bool> Call(Player player, 杀 sha = null)
        {
            bool result;
            if (player.Equipages["防具"] is 八卦阵 && (sha is null || !sha.IgnoreArmor))
            {
                result = await ((八卦阵)player.Equipages["防具"]).Skill();
                if (result) return true;
            }

            TimerTask.Instance.Hint = "请使用一张闪。";
            TimerTask.Instance.ValidCard = (card) => card is 闪 && !player.DisabledCard(card);
            result = await TimerTask.Instance.Run(player, 1, 0);

            if (player.isAI)
            {
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
            TimerTask.Instance.ValidCard = (card) => card is 桃 && !player.DisabledCard(card);
            TimerTask.Instance.ValidDest = (player, card, fstPlayer) => player == dest;
            bool result = await TimerTask.Instance.Run(player, 1, 1);

            if (player.isAI && player == dest)
            {
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

    public class 火杀 : 杀
    {
        public 火杀()
        {
            Type = "基本牌";
            Name = "火杀";
        }
    }

    public class 雷杀 : 杀
    {
        public 雷杀()
        {
            Type = "基本牌";
            Name = "雷杀";
        }
    }
}
