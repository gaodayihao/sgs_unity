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

        public Skill(Player src, string name, bool passive)
        {
            Src = src;
            Name = name;
            Passive = passive;
        }

        public virtual int MaxCard()
        {
            return 0;
        }

        public virtual int MinCard()
        {
            return 0;
        }

        public virtual bool IsValidCard(Card card)
        {
            return true;
        }

        public virtual int MaxDest()
        {
            return 0;
        }

        public virtual int MinDest()
        {
            return 0;
        }

        public virtual bool IsValidDest(Player dest, List<Card> cards, Player firstDest = null)
        {
            return true;
        }

        // public virtual bool IsValidSkill(Skill skill)
        // {
        //     return false;
        // }
    }
}