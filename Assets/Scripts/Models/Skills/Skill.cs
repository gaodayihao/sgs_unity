using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

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

            SetActive(true);
        }

        /// <summary>
        /// 最大可选卡牌数
        /// </summary>
        public virtual int MaxCard => 0;

        /// <summary>
        /// 最小可选卡牌数
        /// </summary>
        public virtual int MinCard => 0;

        /// <summary>
        /// 判断卡牌是否可选
        /// </summary>
        public virtual bool IsValidCard(Card card) => true;

        /// <summary>
        /// 最大目标数
        /// </summary>
        public virtual int MaxDest => 0;

        /// <summary>
        /// 最小目标数
        /// </summary>
        public virtual int MinDest => 0;

        /// <summary>
        /// 判断目标是否可选
        /// </summary>
        /// <param name="dest">目标</param>
        public virtual bool IsValidDest(Player dest) => true;

        /// <summary>
        /// 是否有效
        /// </summary>
        public int Enabled { get; set; } = 0;

        public void SetActive(bool valid)
        {
            if (valid)
            {
                if (Enabled > 0) return;
                Enabled++;
                if (Enabled > 0) OnEnabled();
            }
            else
            {
                if (Enabled <= 0) return;
                Enabled--;
                if (Enabled <= 0) OnDisabled();
            }

        }

        public virtual void OnEnabled() { }

        public virtual void OnDisabled() { }

        /// <summary>
        /// 技能是否满足条件
        /// </summary>
        public virtual bool IsValid => Time < TimeLimit && Enabled > 0;

        public virtual void Execute()
        {
            Time++;
            useSkillView(this);
        }

        protected virtual void Reset()
        {
            Time = 0;
        }

        protected Player firstDest => Operation.Instance.Dests.Count == 0 ? null : Operation.Instance.Dests[0];

        private static UnityAction<Skill> useSkillView;
        public static event UnityAction<Skill> UseSkillView
        {
            add => useSkillView += value;
            remove => useSkillView -= value;
        }

        public static Dictionary<string, Type> SkillMap { get; set; } = new Dictionary<string, Type>
        {
            { "仁德", typeof(仁德) },
            { "武圣", typeof(武圣) },
            { "义绝", typeof(义绝) },
            { "咆哮", typeof(咆哮) },
            { "制衡", typeof(制衡) },
            { "苦肉", typeof(苦肉) },
            { "诈降", typeof(诈降) },
            { "奸雄", typeof(奸雄) },
            { "刚烈", typeof(刚烈) },
            { "清俭", typeof(清俭) },
            { "突袭", typeof(突袭) },
            { "无双", typeof(无双) },
            { "利驭", typeof(利驭) },
            { "离间", typeof(离间) },
            { "闭月", typeof(闭月) },
            { "驱虎", typeof(驱虎) },
            { "节命", typeof(节命) },
            { "好施", typeof(好施) },
            { "缔盟", typeof(缔盟) },
            { "恩怨", typeof(恩怨) },
            { "眩惑", typeof(眩惑) },
            { "散谣", typeof(散谣) },
            { "制蛮", typeof(制蛮) },
            { "明策", typeof(明策) },
            { "智迟", typeof(智迟) },
            { "烈弓", typeof(烈弓) },
            { "乱击", typeof(乱击) },
            { "父魂", typeof(父魂) },
        };
    }
}