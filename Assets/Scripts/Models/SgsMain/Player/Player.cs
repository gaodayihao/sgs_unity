using UnityEngine;
using System.Collections.Generic;
using System;

namespace Model
{
    public class Player
    {
        public Player()
        {
            playerEvents = new PlayerEvents(this);
            HpLimit = 5;
            Hp = HpLimit;
        }
        public string Username { get; set; }

        public PlayerEvents playerEvents;

        public bool isSelf { get; set; } = false;
        public bool isAI { get; set; }

        // 武将
        public General general { get; private set; }
        // 技能
        public Dictionary<string, Skill> skills { get; private set; } = new Dictionary<string, Skill>();
        // 是否存活
        public bool IsAlive { get; set; } = true;

        // 体力上限
        public int HpLimit { get; set; }
        // 体力
        public int Hp { get; set; }
        // 位置
        public int Position { get; set; }
        // 上家
        public Player Last { get; set; }
        // 下家
        public Player Next { get; set; }

        // 手牌
        public List<Card> HandCards { get; set; } = new List<Card>();
        // 手牌数
        public int HandCardCount { get => HandCards.Count; }
        // 手牌上限
        public int HandCardLimit { get => Hp; }

        // 装备区
        public Dictionary<string, Equipage> Equipages { get; set; } = new Dictionary<string, Equipage>
        {
            { "武器", null }, { "防具", null }, { "加一马", null }, { "减一马", null }
        };
        public Equipage weapon { get => Equipages["武器"]; }
        public Equipage armor { get => Equipages["防具"]; }
        public Equipage plusHorse { get => Equipages["加一马"]; }
        public Equipage subHorse { get => Equipages["减一马"]; }

        // 判定区
        public List<DelayScheme> JudgeArea { get; set; } = new List<DelayScheme>();

        // 其他角色计算与你的距离(+1)
        public int DstPlus { get; set; } = 0;
        // 你计算与其他角色的距离(-1)
        public int DstSub { get; set; } = 0;
        // 攻击范围
        public int AttackRange { get; set; } = 1;

        /// <summary>
        /// 计算距离
        /// </summary>
        /// <param name="dest">目标角色</param>
        public int GetDistance(Player dest)
        {
            if (!dest.IsAlive || dest == this) return 0;
            int distance = 1 + dest.DstPlus - DstSub;

            Player next = Next, last = Last;
            while (dest != next && dest != last)
            {
                next = next.Next;
                last = last.Last;
                distance++;
            }

            return Mathf.Max(distance, 1);
        }

        // 出杀次数
        public int ShaCount { get; set; }

        /// <summary>
        /// 按类型查找手牌(人机)
        /// </summary>
        public T FindCard<T>() where T : Card
        {
            foreach (var card in HandCards)
            {
                if (card is T && !DisabledCard(card)) return (T)card;
            }
            return null;
        }

        /// <summary>
        /// 初始化武将
        /// </summary>
        public void InitGeneral(General general)
        {
            this.general = general;
            HpLimit = general.hp_limit;
            Hp = HpLimit;
            InitSkill();
        }

        /// <summary>
        /// 初始化技能
        /// </summary>
        private void InitSkill()
        {
            foreach (var str in general.skill)
            {
                switch (str)
                {
                    case "武圣":
                        skills.Add(str, new WuSheng(this)); break;
                    case "义绝":
                        skills.Add(str, new YiJue(this)); break;
                }

                // player.skills.Add(skill);
            }
        }

        public Func<Card, bool> unlimitedCard = (card) => false;
        public bool UnlimitedCard(Card card)
        {
            foreach (Func<Card, bool> i in unlimitedCard.GetInvocationList())
            {
                if (i(card)) return true;
            }
            return false;
        }

        public Func<Card, bool> disabledCard = (card) => false;
        public bool DisabledCard(Card card)
        {
            foreach (Func<Card, bool> i in disabledCard.GetInvocationList())
            {
                if (i(card)) return true;
            }
            return false;
        }
    }
}
