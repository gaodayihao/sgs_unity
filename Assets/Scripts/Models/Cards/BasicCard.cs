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
            if (src.weapon != null)
            {
                Src = src;
                Dests = dests;
                if (src.weapon is 朱雀羽扇 && await (src.weapon as 朱雀羽扇).Skill(this)) return;
                src.weapon.WhenUseSha(this);
            }

            ShanCount = 1;
            DamageValue = 1;
            IgnoreArmor = false;

            if (src.Use酒)
            {
                src.Use酒 = false;
                DamageValue++;
            }

            src.ShaCount++;
            await base.UseCard(src, dests);

            // 青釭剑 雌雄双股剑
            if (src.weapon != null) await src.weapon.AfterUseSha(this);

            foreach (var dest in Dests)
            {
                // 仁王盾 藤甲
                // if (!IgnoreArmor && dest.armor != null && dest.armor.Disable(this)) continue;
                if (Disabled(dest)) continue;

                IsDamage = false;
                if (ShanCount == 0) IsDamage = true;
                else
                {
                    for (int i = 0; i < ShanCount; i++)
                    {
                        if (!await 闪.Call(dest, this))
                        {
                            IsDamage = true;
                            break;
                        }
                    }
                }

                if (!IsDamage && src.weapon != null) await src.weapon.ShaMiss(this, dest);

                if (IsDamage)
                {
                    if (src.weapon != null) await src.weapon.WhenDamage(this, dest);
                    if (!IsDamage) continue;
                    Damage type = this is 火杀 ? Damage.Fire : this is 雷杀 ? Damage.Thunder : Damage.Normal;
                    await new Damaged(dest, Src, this, DamageValue, type).Execute();
                }
            }
        }

        public int ShanCount { get; set; }
        public int DamageValue { get; set; }
        public bool IgnoreArmor { get; set; }
        public bool IsDamage { get; set; }

        public static async Task<bool> Call(Player player)
        {
            TimerTask.Instance.Hint = "请打出一张杀。";
            TimerTask.Instance.ValidCard = card => card is 杀 && !player.DisabledCard(card);
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
            TimerTask.Instance.ValidCard = card => card is 闪 && !player.DisabledCard(card);
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
            TimerTask.Instance.ValidCard = card => (card is 桃 || card is 酒 && dest == player)
                && !player.DisabledCard(card);
            TimerTask.Instance.ValidDest = (player, card, first) => player == dest;
            bool result = await TimerTask.Instance.Run(player, 1, 1);

            if (player.isAI && (player == dest || player.team == dest.team))
            {
                var card = player.FindCard<酒>() as Card;
                if (card is null) card = player.FindCard<桃>();
                if (card != null)
                {
                    TimerTask.Instance.Cards.Add(card);
                    result = true;
                }
            }

            if (!result) return false;

            else
            {
                await TimerTask.Instance.Cards[0].UseCard(player, new List<Player> { dest });
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

    public class 酒 : Card
    {
        public 酒()
        {
            Type = "基本牌";
            Name = "酒";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            // 默认将目标设为使用者
            if (dests is null || dests.Count == 0) dests = new List<Player> { src };

            await base.UseCard(src, dests);

            if (Dests[0].Hp < 1) await new Recover(Dests[0]).Execute();
            else
            {
                Dests[0].Use酒 = true;
                Dests[0].酒Count++;
            }
        }
    }
}
