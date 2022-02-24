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
            Debug.Log((Src.Position + 1).ToString() + "号位使用了" + Name + "【" + Suit + Weight.ToString() + "】");
            useCardView?.Invoke(this);

            // 使用者失去此手牌
            if (Src.HandCards.Contains(this))
            {
                if (!(this is Equipage)) CardPile.Instance.AddToDiscard(this);
                await new LoseCard(Src, new List<Card> { this }).Execute();
            }

            // 指定目标时
            await Src.playerEvents.whenUseCard.Execute(this);
            // 指定目标后
            await Src.playerEvents.afterUseCard.Execute(this);
        }

        /// <summary>
        /// 重置卡牌
        /// </summary>
        protected void ResetCard()
        {
            Src = null;
            Dests = null;
        }

        private static UnityAction<Card> useCardView;
        public static event UnityAction<Card> UseCardView
        {
            add => useCardView += value;
            remove => useCardView -= value;
        }
    }
}