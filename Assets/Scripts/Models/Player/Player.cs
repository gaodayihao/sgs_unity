using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Model
{
    public class Player
    {
        public Player(Team team)
        {
            this.team = team;
            playerEvents = new PlayerEvents(this);
        }

        public PlayerEvents playerEvents;

        public bool isSelf { get; set; } = false;
        public bool isAI { get; set; }
        public Player Teammate { get; set; }
        public Team team { get; private set; }

        // 武将
        public General general { get; private set; }
        // 皮肤
        public List<Skin> skins { get; private set; }
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
        public string PosStr => (Position + 1).ToString();
        // 上家
        public Player Last { get; set; }
        // 下家
        public Player Next { get; set; }

        // 手牌
        public List<Card> HandCards { get; set; } = new List<Card>();
        // 手牌数
        public int HandCardCount => HandCards.Count;
        // 手牌上限
        public int HandCardLimit => Hp + HandCardLimitOffset;
        // 手牌上线偏移
        public int HandCardLimitOffset { get; set; } = 0;

        // 铁锁
        public bool IsLocked { get; set; } = false;

        // 装备区
        public Dictionary<string, Equipage> Equipages { get; set; } = new Dictionary<string, Equipage>
        {
            { "武器", null }, { "防具", null }, { "加一马", null }, { "减一马", null }
        };
        public Weapon weapon { get => Equipages["武器"] as Weapon; }
        public Armor armor { get => Equipages["防具"] as Armor; }
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
        // 出杀次数
        public int ShaCount { get; set; }
        public bool Use酒 { get; set; } = false;
        public int 酒Count { get; set; }

        /// <summary>
        /// 计算距离
        /// </summary>
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

        /// <summary>
        /// 判断区域内是否有牌
        /// </summary>
        public bool RegionHaveCard => CardCount > 0 || JudgeArea.Count > 0;
        // return false;
        public int CardCount => HandCardCount + Equipages.Values.Where(x => x != null).Count();
        // {
        //     get
        //     {
        //         if (HandCardCount != 0) return true;
        //         foreach (var i in Equipages.Values) if (i != null) return true;
        //         return false;
        //     }
        // }

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
                    case "仁德": skills.Add(str, new 仁德(this)); break;
                    case "武圣": skills.Add(str, new 武圣(this)); break;
                    case "义绝": skills.Add(str, new 义绝(this)); break;
                    case "咆哮": skills.Add(str, new 咆哮(this)); break;
                    case "制衡": skills.Add(str, new 制衡(this)); break;
                    case "苦肉": skills.Add(str, new 苦肉(this)); break;
                    case "诈降": skills.Add(str, new 诈降(this)); break;
                    case "奸雄": skills.Add(str, new 奸雄(this)); break;
                    case "刚烈": skills.Add(str, new 刚烈(this)); break;
                    case "清俭": skills.Add(str, new 清俭(this)); break;
                    case "突袭": skills.Add(str, new 突袭(this)); break;
                    case "无双": skills.Add(str, new 无双(this)); break;
                    case "利驭": skills.Add(str, new 利驭(this)); break;
                    case "离间": skills.Add(str, new 离间(this)); break;
                    case "闭月": skills.Add(str, new 闭月(this)); break;
                    case "驱虎": skills.Add(str, new 驱虎(this)); break;
                    case "节命": skills.Add(str, new 节命(this)); break;
                    case "好施": skills.Add(str, new 好施(this)); break;
                    case "缔盟": skills.Add(str, new 缔盟(this)); break;
                    case "恩怨": skills.Add(str, new 恩怨(this)); break;
                    case "眩惑": skills.Add(str, new 眩惑(this)); break;
                    case "散谣": skills.Add(str, new 散谣(this)); break;
                    case "制蛮": skills.Add(str, new 制蛮(this)); break;
                    case "明策": skills.Add(str, new 明策(this)); break;
                    case "智迟": skills.Add(str, new 智迟(this)); break;
                    case "烈弓": skills.Add(str, new 烈弓(this)); break;
                }

                // player.skills.Add(skill);
            }
        }

        /// <summary>
        /// 无次数限制
        /// </summary>
        public Func<Card, bool> unlimitedCard = (card) => false;
        public bool UnlimitedCard(Card card)
        {
            foreach (Func<Card, bool> i in unlimitedCard.GetInvocationList())
            {
                if (i(card)) return true;
            }
            return false;
        }

        /// <summary>
        /// 禁用卡牌
        /// </summary>
        public Func<Card, bool> disabledCard = (card) => false;
        public bool DisabledCard(Card card)
        {
            foreach (Func<Card, bool> i in disabledCard.GetInvocationList())
            {
                if (i(card)) return true;
            }
            return false;
        }

        /// <summary>
        /// 卡牌对你无效
        /// </summary>
        public Func<Card, bool> disabledForMe = (card) => false;
        public bool DisabledForMe(Card card)
        {
            foreach (Func<Card, bool> i in disabledForMe.GetInvocationList())
            {
                if (i(card)) return true;
            }
            return false;
        }

        /// <summary>
        /// 无距离限制
        /// </summary>
        public Func<Card, Player, bool> unlimitedDst = (card, player) => false;
        public bool UnlimitedDst(Card card, Player dest)
        {
            foreach (Func<Card, Player, bool> i in unlimitedDst.GetInvocationList())
            {
                if (i(card, dest)) return true;
            }
            return false;
        }
    }
}
