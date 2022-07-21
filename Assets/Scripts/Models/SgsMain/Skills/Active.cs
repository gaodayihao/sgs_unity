using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
    public class Active : Skill
    {
        public Active(Player src, string name, bool passive,
            int timeLimit)
            : base(src, name, passive)
        {
            TimeLimit = timeLimit;
            // MaxDest = maxDest;
            // MinDest = minDest;
            // MaxCard = maxCard;
            // MinCard = minCard;
        }

        // 已使用次数
        public int Time { get; set; }
        // 限定次数
        public int TimeLimit { get; private set; }

        // 最多目标
        // public int MaxDest { get; private set; }
        // // 最少目标
        // public int MinDest { get; private set; }
        // // 判断每名角色是否能成为目标
        // public virtual bool IsValidDest(Player dest)
        // {
        //     return true;
        // }

        // // 最多选中卡牌
        // public int MaxCard { get; private set; }
        // // 最少选中卡牌
        // public int MinCard { get; private set; }
        // // 判断每张卡牌能否选中
        // public virtual bool IsValidCard(Card card)
        // {
        //     return true;
        // }

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
    }
}