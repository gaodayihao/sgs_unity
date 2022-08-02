using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Model
{
    public class Card
    {
        // 编号
        public int Id { get; set; } = 0;
        // 花色
        public string Suit { get; set; }
        // 点数(1~13)
        public int Weight { get; set; }
        // 类别
        public string Type { get; set; }
        // 卡牌名
        public string Name { get; set; }

        // 使用者
        public Player Src { get; private set; }
        // 目标
        public List<Player> Dests { get; private set; }

        /// <summary>
        /// 使用牌
        /// </summary>
        public virtual async Task UseCard(Player src, List<Player> dests = null)
        {
            Src = src;
            Dests = dests;
            string cardInfo = IsConvert ? "" : "【" + Suit + Weight.ToString() + "】";
            Debug.Log((Src.Position + 1).ToString() + "号位使用了" + Name + cardInfo);
            useCardView?.Invoke(this);

            // 目标角色排序
            if (Dests != null && Dests.Count > 1)
            {
                Dests.Sort((x, y) =>
                {
                    Player i = TurnSystem.Instance.CurrentPlayer;
                    while (true)
                    {
                        if (x == i) return -1;
                        else if (y == i) return 1;
                        i = i.Next;
                    }
                });
            }

            // 使用者失去此手牌
            if (!IsConvert)
            {
                if (!(this is Equipage)) CardPile.Instance.AddToDiscard(this);
                await new LoseCard(Src, new List<Card> { this }).Execute();
            }
            else if (PrimiTives.Count != 0)
            {
                CardPile.Instance.AddToDiscard(PrimiTives);
                await new LoseCard(Src, PrimiTives).Execute();
            }

            // 指定目标时
            await Src.playerEvents.whenUseCard.Execute(this);
            // 指定目标后
            await Src.playerEvents.afterUseCard.Execute(this);
        }

        /// <summary>
        /// 打出牌
        /// </summary>
        public async Task Put(Player player)
        {
            Src = player;
            string cardInfo = IsConvert ? "" : "【" + Suit + Weight.ToString() + "】";
            Debug.Log((player.Position + 1).ToString() + "号位打出了" + Name + cardInfo);
            useCardView?.Invoke(this);

            // 使用者失去此手牌
            if (!IsConvert)
            {
                if (!(this is Equipage)) CardPile.Instance.AddToDiscard(this);
                await new LoseCard(player, new List<Card> { this }).Execute();
            }
            else if (PrimiTives.Count != 0)
            {
                CardPile.Instance.AddToDiscard(PrimiTives);
                await new LoseCard(player, PrimiTives).Execute();
            }
        }

        #region 转化牌
        public bool IsConvert { get; private set; } = false;
        public List<Card> PrimiTives { get; private set; } = new List<Card>();

        /// <summary>
        /// 转化牌
        /// </summary>
        /// <param name="primitives">原卡牌</param>
        /// <typeparam name="T">类型</typeparam>
        public static T Convert<T>(List<Card> primitives = null) where T : Card, new()
        {
            if (primitives is null) primitives = new List<Card>();
            // 二次转化
            if (primitives.Count > 0 && primitives[0].IsConvert) return Convert<T>(primitives[0].PrimiTives);

            var card = new T();
            card.IsConvert = true;
            card.PrimiTives = primitives;

            if (primitives.Count == 0) return card;

            card.Suit = primitives[0].Suit;
            card.Weight = primitives[0].Weight;

            foreach (var i in primitives)
            {
                if (i.Suit == card.Suit) continue;
                if (i.Suit == "黑桃" || i.Suit == "草花")
                {
                    if (card.Suit == "黑桃" || card.Suit == "草花" || card.Suit == "黑色") card.Suit = "黑色";
                    else
                    {
                        card.Suit = "无花色";
                        break;
                    }
                }
                else
                {
                    if (card.Suit == "红桃" || card.Suit == "方片" || card.Suit == "红色") card.Suit = "红色";
                    else
                    {
                        card.Suit = "无花色";
                        break;
                    }
                }
            }

            foreach (var i in primitives)
            {
                if (i.Weight != card.Weight)
                {
                    card.Weight = 0;
                    break;
                }
            }

            return card;
        }
        #endregion

        /// <summary>
        /// 判断此牌是否在弃牌堆
        /// </summary>
        /// <returns></returns>
        public List<Card> InDiscardPile()
        {
            if (!IsConvert)
            {
                if (CardPile.Instance.discardPile.Contains(this)) return new List<Card> { this };
                else return null;
            }

            if (PrimiTives.Count == 0) return null;

            var list = new List<Card>();
            foreach (var i in PrimiTives)
            {
                if (CardPile.Instance.discardPile.Contains(i)) list.Add(i);
            }
            return list;
        }

        private static UnityAction<Card> useCardView;
        public static event UnityAction<Card> UseCardView
        {
            add => useCardView += value;
            remove => useCardView -= value;
        }
    }
}