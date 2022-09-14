// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class Triggered : Skill
    {
        public Triggered(Player src, string name, bool passive, int timeLimit = int.MaxValue)
            : base(src, name, passive, timeLimit) { }

        /// <summary>
        /// 询问是否发动技能
        /// </summary>
        protected async Task<bool> ShowTimer()
        {
            TimerTask.Instance.GivenSkill = Name;
            TimerTask.Instance.Hint = "是否发动" + Name + "？";
            if (isAI)
            {
                TimerTask.Instance.maxCard = MaxCard;
                TimerTask.Instance.minCard = MinCard;
                TimerTask.Instance.MaxDest = () => MaxDest;
                TimerTask.Instance.MinDest = () => MinDest;
                TimerTask.Instance.IsValidCard = IsValidCard;
                TimerTask.Instance.IsValidDest = IsValidDest;
            }
            return await TimerTask.Instance.Run(Src) || isAI && AIResult();
        }

        protected virtual bool AIResult() => true;
    }
}
