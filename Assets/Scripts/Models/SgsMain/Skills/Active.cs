using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
    /// <summary>
    /// 主动技
    /// </summary>
    public class Active : Skill
    {
        public Active(Player src, string name, bool passive, int timeLimit)
            : base(src, name, passive, timeLimit) { }

        /// <summary>
        /// 发动技能
        /// </summary>
        /// <param name="dests">选中目标</param>
        /// <param name="cards">选中卡牌</param>
        /// <param name="additional">附加信息</param>
        public virtual async Task Execute(List<Player> dests, List<Card> cards, string additional)
        {
            Debug.Log((Src.Position + 1).ToString() + "号位使用了" + Name);
            Time++;
            await Task.Yield();
        }

        public override bool IsValid()
        {
            return TimerTask.Instance.timerType == TimerType.PerformPhase && base.IsValid();
        }
    }
}