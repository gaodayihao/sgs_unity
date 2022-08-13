using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

namespace Model
{
    public class Weapon : Equipage
    {
        public override async Task AddEquipage(Player owner)
        {
            owner.AttackRange += range - 1;
            await base.AddEquipage(owner);
        }
        public override async Task RemoveEquipage()
        {
            Owner.AttackRange -= range - 1;
            await base.RemoveEquipage();
        }
        protected int range;

        public virtual void WhenUseSha(杀 sha) { }

        public virtual async Task AfterUseSha(杀 sha)
        {
            await Task.Yield();
        }

        public virtual async Task ShaMiss(杀 sha, Player dest)
        {
            await Task.Yield();
        }

        public virtual async Task WhenDamage(杀 sha, Player dest)
        {
            await Task.Yield();
        }
    }

    public class 青龙偃月刀 : Weapon
    {
        public 青龙偃月刀()
        {
            range = 3;
        }

        public override async Task ShaMiss(杀 sha, Player dest)
        {
            TimerTask.Instance.GivenSkill = "青龙偃月刀";
            TimerTask.Instance.Hint = "是否发动青龙偃月刀？";
            TimerTask.Instance.ValidCard = card => card is 杀;
            TimerTask.Instance.ValidDest = (player, card, first) => player == dest;
            bool result = await TimerTask.Instance.Run(Owner, 1, 1);

            if (!result) return;

            SkillView();
            await TimerTask.Instance.Cards[0].UseCard(Owner, new List<Player> { dest });
        }
    }

    public class 麒麟弓 : Weapon
    {
        public 麒麟弓()
        {
            range = 5;
        }

        public override async Task WhenDamage(杀 sha, Player dest)
        {
            if (dest.plusHorse is null && dest.subHorse is null) return;

            TimerTask.Instance.GivenSkill = "麒麟弓";
            TimerTask.Instance.Hint = "是否发动麒麟弓？";
            bool result = await TimerTask.Instance.Run(Owner);

            if (!result) return;

            SkillView();
            CardPanel.Instance.Title = "麒麟弓";
            result = await CardPanel.Instance.Run(Owner, dest, TimerType.麒麟弓);

            Card horse;
            if (result) horse = CardPanel.Instance.Cards[0];
            else horse = dest.plusHorse is null ? dest.subHorse : dest.plusHorse;

            await new Discard(dest, new List<Card> { horse }).Execute();
        }
    }

    public class 雌雄双股剑 : Weapon
    {
        public 雌雄双股剑()
        {
            range = 2;
        }

        public override async Task AfterUseSha(杀 sha)
        {
            foreach (var i in sha.Dests)
            {
                if (i.general.gender == sha.Src.general.gender) continue;
                TimerTask.Instance.GivenSkill = "雌雄双股剑";
                TimerTask.Instance.Hint = "是否对" + i.PosStr + "号位发动雌雄双股剑？";
                if (!await TimerTask.Instance.Run(Owner)) continue;

                SkillView();
                if (i.HandCardCount == 0)
                {
                    await new GetCardFromPile(sha.Src, 1).Execute();
                    continue;
                }

                TimerTask.Instance.Hint = Src.PosStr + "号位对你发动雌雄双股剑，请弃一张手牌或令其摸一张牌";
                TimerTask.Instance.ValidCard = card => i.HandCards.Contains(card);
                bool result = await TimerTask.Instance.Run(i, 1, 0);
                if (result) await new Discard(i, TimerTask.Instance.Cards).Execute();
                else await new GetCardFromPile(sha.Src, 1).Execute();
            }
        }
    }

    public class 青釭剑 : Weapon
    {
        public 青釭剑()
        {
            range = 2;
        }

        public override async Task AfterUseSha(杀 sha)
        {
            await Task.Yield();
            SkillView();
            sha.IgnoreArmor = true;
        }
    }

    public class 丈八蛇矛 : Weapon
    {
        public 丈八蛇矛()
        {
            range = 3;
        }

        public override async Task AddEquipage(Player owner)
        {
            skill = new ZBSMSkill(owner);
            await base.AddEquipage(owner);
        }

        public override async Task RemoveEquipage()
        {
            skill = null;
            await base.RemoveEquipage();
        }

        public ZBSMSkill skill { get; private set; }

        public class ZBSMSkill : Converted
        {
            public ZBSMSkill(Player src) : base(src, "丈八蛇矛", false, int.MaxValue, "杀") { }

            public override Card Execute(List<Card> cards)
            {
                return Card.Convert<杀>(cards);
            }

            public override bool IsValidCard(Card card) => Src.HandCards.Contains(card) || card == Src.weapon;

            public override int MaxCard => 2;

            public override int MinCard => 2;
        }
    }

    public class 诸葛连弩 : Weapon
    {
        public 诸葛连弩()
        {
            range = 1;
        }
        public override async Task AddEquipage(Player owner)
        {
            await base.AddEquipage(owner);
            owner.unlimitedCard += Effect;
        }

        public override async Task RemoveEquipage()
        {
            Owner.unlimitedCard -= Effect;
            await base.RemoveEquipage();
        }

        private bool Effect(Card card)
        {
            return card is 杀;
        }

        public override void WhenUseSha(杀 sha)
        {
            if (sha.Src.ShaCount > 1) SkillView();
        }
    }

    public class 贯石斧 : Weapon
    {
        public 贯石斧()
        {
            range = 3;
        }

        public override async Task ShaMiss(杀 sha, Player dest)
        {
            TimerTask.Instance.GivenSkill = "贯石斧";
            TimerTask.Instance.Hint = "是否发动贯石斧？";
            TimerTask.Instance.ValidCard = card => card != Owner.weapon && !card.IsConvert;
            if (!await TimerTask.Instance.Run(Owner, 2, 0)) return;

            await new Discard(Owner, TimerTask.Instance.Cards).Execute();
            sha.IsDamage = true;
        }
    }

    public class 方天画戟 : Weapon
    {
        public 方天画戟()
        {
            range = 4;
        }

        public override void WhenUseSha(杀 sha)
        {
            if (sha.Dests.Count > 1) SkillView();
        }
    }

    public class 朱雀羽扇 : Weapon
    {
        public 朱雀羽扇()
        {
            range = 4;
        }

        public async Task<bool> Skill(杀 sha)
        {
            if (sha is 火杀 || sha is 雷杀) return false;
            TimerTask.Instance.GivenSkill = "朱雀羽扇";
            TimerTask.Instance.Hint = "是否发动朱雀羽扇？";
            if (!await TimerTask.Instance.Run(Owner)) return false;

            SkillView();
            await Card.Convert<火杀>(new List<Card> { sha }).UseCard(sha.Src, sha.Dests);
            return true;
        }
    }

    public class 古锭刀 : Weapon
    {
        public 古锭刀()
        {
            range = 2;
        }

        public override async Task WhenDamage(杀 sha, Player dest)
        {
            if (dest.HandCardCount != 0) return;
            await Task.Yield();
            SkillView();
            sha.DamageValue++;
        }
    }

    public class 寒冰剑 : Weapon
    {
        public 寒冰剑()
        {
            range = 2;
        }

        public override async Task WhenDamage(杀 sha, Player dest)
        {
            if (!dest.HaveCard()) return;
            TimerTask.Instance.GivenSkill = "寒冰剑";
            TimerTask.Instance.Hint = "是否发动寒冰剑？";
            if (!await TimerTask.Instance.Run(Owner)) return;

            SkillView();
            sha.DamageValue = 0;
            var card = await CardPanel.Instance.SelectCard(Owner, dest);
            await new Discard(dest, new List<Card> { card }).Execute();
            card = await CardPanel.Instance.SelectCard(Owner, dest);
            await new Discard(dest, new List<Card> { card }).Execute();
        }
    }
}
