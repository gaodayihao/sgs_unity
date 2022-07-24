using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    /// <summary>
    /// 技能基类
    /// </summary>
    public class Skill
    {
        // 所属玩家
        public Player Src { get; private set; }
        // 技能名称
        public string Name { get; private set; }
        // 锁定技
        public bool Passive { get; private set; } = false;
        // 限定次数
        public int TimeLimit { get; private set; }
        // 已使用次数
        public int Time { get; set; }

        public Skill(Player src, string name, bool passive, int timeLimit)
        {
            Src = src;
            Name = name;
            Passive = passive;
            TimeLimit = timeLimit;
        }

        /// <summary>
        /// 最大可选卡牌数
        /// </summary>
        public virtual int MaxCard()
        {
            return 0;
        }

        /// <summary>
        /// 最小可选卡牌数
        /// </summary>
        public virtual int MinCard()
        {
            return 0;
        }

        /// <summary>
        /// 判断卡牌是否可选
        /// </summary>
        public virtual bool IsValidCard(Card card)
        {
            return true;
        }

        /// <summary>
        /// 最大目标数
        /// </summary>
        public virtual int MaxDest()
        {
            return 0;
        }

        /// <summary>
        /// 最小目标数
        /// </summary>
        public virtual int MinDest()
        {
            return 0;
        }

        /// <summary>
        /// 判断目标是否可选
        /// </summary>
        /// <param name="dest">目标</param>
        /// <param name="cards">已选卡牌</param>
        /// <param name="firstDest">第一个已选目标</param>
        public virtual bool IsValidDest(Player dest, List<Card> cards, Player firstDest = null)
        {
            return true;
        }

        /// <summary>
        /// 是否有效
        /// </summary>
        public int Enabled { get; set; } = 1;

        /// <summary>
        /// 技能是否可使用
        /// </summary>
        public virtual bool IsValid()
        {
            return Time < TimeLimit && Enabled > 0;
        }
    }
}