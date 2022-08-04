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
        public override void RemoveEquipage()
        {
            Owner.AttackRange -= range - 1;
            base.RemoveEquipage();
        }
        protected int range;

        public virtual async Task UseShaSkill(杀 sha)
        {
            await Task.Yield();
        }

        public virtual async Task CounteredSkill(杀 sha, Player dest)
        {
            await Task.Yield();
        }

        public virtual async Task DamageSkill(杀 sha, Player dest)
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

        public override async Task CounteredSkill(杀 sha, Player dest)
        {
            TimerTask.Instance.GivenSkill = "青龙偃月刀";
            TimerTask.Instance.Hint = "是否发动青龙偃月刀？";
            TimerTask.Instance.ValidCard = (card) => card is 杀;
            TimerTask.Instance.ValidDest = (player, card, fstPlayer) => player == dest;
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

        public override async Task DamageSkill(杀 sha, Player dest)
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

        public override async Task UseShaSkill(杀 sha)
        {
            foreach (var i in sha.Dests)
            {
                if (i.general.gender == sha.Src.general.gender) continue;
                TimerTask.Instance.GivenSkill = "雌雄双股剑";
                TimerTask.Instance.Hint = "是否对" + (i.Position + 1).ToString() + "号位发动雌雄双股剑？";
                if (!await TimerTask.Instance.Run(Owner)) continue;

                SkillView();
                if (i.HandCardCount == 0)
                {
                    await new GetCardFromPile(sha.Src, 1).Execute();
                    continue;
                }

                TimerTask.Instance.Hint = (Src.Position + 1).ToString() + "号位对你发动雌雄双股剑，请弃一张手牌或令其摸一张牌";
                TimerTask.Instance.ValidCard = (card) => i.HandCards.Contains(card);
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

        public override async Task UseShaSkill(杀 sha)
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

        public override void RemoveEquipage()
        {
            skill = null;
            base.RemoveEquipage();
        }

        public ZBSMSkill skill { get; private set; }

        public class ZBSMSkill : Converted
        {
            public ZBSMSkill(Player src) : base(src, "丈八蛇矛", false, int.MaxValue, "杀") { }

            public override Card Execute(List<Card> cards)
            {
                return Card.Convert<杀>(cards);
            }

            public override bool IsValidCard(Card card)
            {
                return Src.HandCards.Contains(card) || card == Src.weapon;
            }

            public override int MaxCard()
            {
                return 2;
            }

            public override int MinCard()
            {
                return 2;
            }
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

        public override void RemoveEquipage()
        {
            Owner.unlimitedCard -= Effect;
            base.RemoveEquipage();
        }

        private bool Effect(Card card)
        {
            return card is 杀;
        }

        public override async Task UseShaSkill(杀 sha)
        {
            await Task.Yield();
            if (sha.Src.ShaCount > 1) SkillView();
        }
    }

    public class 贯石斧 : Weapon
    {
        public 贯石斧()
        {
            range = 3;
        }

        public override async Task CounteredSkill(杀 sha, Player dest)
        {
            TimerTask.Instance.GivenSkill = "贯石斧";
            TimerTask.Instance.Hint = "是否发动贯石斧？";
            TimerTask.Instance.ValidCard = (card) => card != Owner.weapon && !card.IsConvert;
            if (!await TimerTask.Instance.Run(Owner, 2, 0)) return;

            await new Discard(Owner, TimerTask.Instance.Cards).Execute();
            sha.isDamage = true;
        }
    }

    public class 方天画戟 : Weapon
    {
        public 方天画戟()
        {
            range = 4;
        }

        public override async Task UseShaSkill(杀 sha)
        {
            await Task.Yield();
            if (sha.Dests.Count > 1) SkillView();
        }
    }
}
