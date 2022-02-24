using UnityEngine;
using System.Collections.Generic;

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
        // 技能
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
            if (!dest.IsAlive) return 0;
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

        public int ShaCount { get; set; }
    }
}
