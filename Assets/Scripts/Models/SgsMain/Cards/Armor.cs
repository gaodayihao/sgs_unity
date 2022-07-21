using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class Armor : Equipage
    {
        public bool enable { get; set; }

        public override async Task AddEquipage(Player owner)
        {
            enable = true;
            await base.AddEquipage(owner);
        }
    }

    public class BaGuaZhen : Armor
    {
        public async Task<bool> Skill()
        {
            if (!enable) return false;

            TimerTask.Instance.Hint = "是否发动八卦阵？";
            bool result = await TimerTask.Instance.Run(Owner, TimerType.CallEquipSkill, 0);
            if (!result && !Owner.isAI) return false;

            SkillView();
            var card = await new Judge().Execute();
            return card.Suit == "红桃" || card.Suit == "方片";
        }
    }
}
