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
    }

    public class 青龙偃月刀 : Weapon
    {
        public 青龙偃月刀()
        {
            range = 3;
        }

        // public Sha sha { get; set; }

        public async Task Skill(Player dest)
        {
            TimerTask.Instance.SkillName = "青龙偃月刀";
            TimerTask.Instance.Hint = "是否发动青龙偃月刀？";
            TimerTask.Instance.GivenDest = dest;
            TimerTask.Instance.GivenCard = new List<string> { "杀", "雷杀", "火杀" };
            bool result = await TimerTask.Instance.Run(Owner, TimerType.CallEquipSkill);

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

        public async Task Skill(Player dest)
        {
            if (dest.plusHorse is null && dest.subHorse is null) return;

            TimerTask.Instance.SkillName = "麒麟弓";
            TimerTask.Instance.Hint = "是否发动麒麟弓？";
            TimerTask.Instance.GivenDest = dest;
            bool result = await TimerTask.Instance.Run(Owner, TimerType.CallEquipSkill, 0);

            if (!result) return;

            SkillView();
            CardPanel.Instance.Title = "麒麟弓";
            result = await CardPanel.Instance.Run(Owner, dest, TimerType.QlgPanel);

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
    }

    public class 青缸剑 : Weapon
    {
        public 青缸剑()
        {
            range = 2;
        }

        public bool Skill(杀 sha)
        {
            SkillView();
            bool result = false;
            foreach (var dest in sha.Dests)
            {
                if (dest.armor != null)
                {
                    ((Armor)dest.armor).enable = false;
                    result = true;
                }
            }
            return result;
        }

        public void ResetArmor(杀 sha)
        {
            foreach (var dest in sha.Dests) if (dest.armor != null) ((Armor)dest.armor).enable = false;
        }
    }

    public class 丈八蛇矛 : Weapon
    {
        public 丈八蛇矛()
        {
            range = 3;
        }

        public 杀 ConvertSkill(List<Card> primitives)
        {
            SkillView();
            return Card.Convert<杀>(primitives);
        }
    }

    public class 诸葛连弩 : Weapon
    {
        public 诸葛连弩()
        {
            range = 1;
        }
    }

    public class 贯石斧 : Weapon
    {
        public 贯石斧()
        {
            range = 3;
        }

        public async Task<bool> Skill()
        {
            TimerTask.Instance.SkillName = "贯石斧";
            TimerTask.Instance.Hint = "是否发动贯石斧？";
            bool result = await TimerTask.Instance.Run(Owner, TimerType.Discard, 2);
            if (result)
            {
                var list = TimerTask.Instance.Cards.Union(TimerTask.Instance.Equipages).ToList();
                list.Remove(this);
                await new Discard(Owner, list).Execute();
                return true;
            }
            else return false;
        }
    }

    public class 方天画戟 : Weapon
    {
        public 方天画戟()
        {
            range = 4;
        }
    }
}
