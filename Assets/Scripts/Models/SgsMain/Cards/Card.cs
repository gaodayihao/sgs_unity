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
            string cardInfo = Convertion ? "" : "【" + Suit + Weight.ToString() + "】";
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
            if (!Convertion)
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
            string cardInfo = Convertion ? "" : "【" + Suit + Weight.ToString() + "】";
            Debug.Log((player.Position + 1).ToString() + "号位打出了" + Name + cardInfo);

            // 使用者失去此手牌
            if (!Convertion)
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
        public bool Convertion { get; private set; } = false;
        public List<Card> PrimiTives { get; private set; } = new List<Card>();

        public static T Convert<T>(List<Card> primitives) where T : Card, new()
        {
            var card = new T();
            card.Convertion = true;
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

        private static UnityAction<Card> useCardView;
        public static event UnityAction<Card> UseCardView
        {
            add => useCardView += value;
            remove => useCardView -= value;
        }
    }
}