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
        public async Task<bool> ShowTimer()
        {
            TimerTask.Instance.GivenSkill = Name;
            TimerTask.Instance.Hint = "是否发动" + Name + "？";
            // TimerTask.Instance.MaxDest = MaxDest;
            // TimerTask.Instance.MinDest = MinDest;
            // return await TimerTask.Instance.Run(Src, TimerType.CallSkill, MaxCard(), MinCard(), 0, 0);
            return await TimerTask.Instance.Run(Src, TimerType.CallSkill);
        }
    }
}
