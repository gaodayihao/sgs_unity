using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace Model
{
    public abstract class PlayerAction<T>
    {
        public Player player { get; private set; }
        public PlayerAction(Player player)
        {
            this.player = player;
        }

        /// <summary>
        /// 执行操作
        /// </summary>
        public abstract Task Execute();

        protected static UnityAction<T> actionView;
        public static event UnityAction<T> ActionView
        {
            add => actionView += value;
            remove => actionView -= value;
        }
    }

    public class AcquireCard : PlayerAction<AcquireCard>
    {
        /// <summary>
        /// 获得牌
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="cards">卡牌数组</param>
        public AcquireCard(Player player, List<Card> cards = null) : base(player)
        {
            Cards = cards;
        }
        public List<Card> Cards { get; protected set; }

        public override async Task Execute()
        {
            // 获得牌
            foreach (var card in Cards)
            {
                player.HandCards.Add(card);
            }
            actionView?.Invoke(this);

            // 执行弃牌后事件
            await player.playerEvents.acquireCard.Execute(this);
        }
    }

    public class GetCard : AcquireCard
    {
        /// <summary>
        /// 摸牌
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="count">摸牌数</param>
        public GetCard(Player player, int count) : base(player, new List<Card>())
        {
            this.count = count;
        }
        private int count;

        public override async Task Execute()
        {
            Debug.Log((player.Position + 1).ToString() + "号位摸了" + count.ToString() + "张牌");
            // 摸牌
            for (int i = 0; i < count; i++) Cards.Add(await CardPile.Instance.Pop());
            await base.Execute();
        }
    }

    public class LoseCard : PlayerAction<LoseCard>
    {
        /// <summary>
        /// 失去牌
        /// </summary>
        public LoseCard(Player player, List<Card> handCards, List<Equipage> equipages = null) : base(player)
        {
            HandCards = handCards;
            Equipages = equipages;
        }
        public List<Card> HandCards { get; private set; }
        public List<Equipage> Equipages { get; private set; }

        public override async Task Execute()
        {
            // 失去手牌
            if (HandCards != null) foreach (var handCard in HandCards) player.HandCards.Remove(handCard);
            // 失去装备牌
            if (Equipages != null) foreach (var equipage in Equipages) equipage.RemoveEquipage();

            actionView?.Invoke(this);

            // 执行失去牌后事件
            await player.playerEvents.loseCard.Execute(this);
        }
    }

    public class Discard : LoseCard
    {
        /// <summary>
        /// 弃牌
        /// </summary>
        public Discard(Player player, List<Card> handCards, List<Equipage> equipages = null) :
        base(player, handCards, equipages)
        { }

        public override async Task Execute()
        {
            string str = "";
            if (HandCards != null) foreach (var handCard in HandCards)
                    str += "【" + handCard.Name + handCard.Suit + handCard.Weight.ToString() + "】";
            if (Equipages != null) foreach (var equipage in Equipages)
                    str += "【" + equipage.Name + equipage.Suit + equipage.Weight.ToString() + "】";
            Debug.Log((player.Position + 1).ToString() + "号位弃置了" + str);

            // 失去手牌
            if (HandCards != null) CardPile.Instance.AddToDiscard(HandCards);
            // 失去装备牌
            if (Equipages != null) foreach (var equipage in Equipages) CardPile.Instance.AddToDiscard(equipage);

            // losecard
            await base.Execute();
        }

        public static async Task DiscardFromHand(Player player, int count)
        {
            TimerTask.Instance.Hint = "请弃置" + count.ToString() + "张手牌。";
            bool result = await TimerTask.Instance.Run(player, TimerType.DiscardFromHand, count);
            if (result) await new Discard(player, TimerTask.Instance.Cards).Execute();
            else
            {
                List<Card> cards = new List<Card>();
                for (int i = 0; i < count; i++) cards.Add(player.HandCards[i]);
                await new Discard(player, cards).Execute();
            }
        }
    }

    public class UpdateHp : PlayerAction<UpdateHp>
    {
        /// <summary>
        /// 改变体力
        /// </summary>
        public UpdateHp(Player player, int value) : base(player)
        {
            Value = value;
        }
        public int Value { get; protected set; }

        public override async Task Execute()
        {
            // 更新体力
            player.Hp += Value;
            actionView?.Invoke(this);

            // 执行事件(濒死)
            if (player.Hp < 1) await NearDeath();
        }

        public async Task NearDeath()
        {
            Player askedPlayer = TurnSystem.Instance.CurrentPlayer;
            do
            {
                while (await Tao.Call(askedPlayer, player))
                {
                    if (player.Hp >= 1) return;
                }

                askedPlayer = askedPlayer.Next;
            } while (askedPlayer != TurnSystem.Instance.CurrentPlayer);

            if (this is Damaged) await new Die(player, ((Damaged)this).DamageSrc).Execute();
            else await new Die(player, null).Execute();
        }
    }

    public class Die : PlayerAction<Die>
    {
        public Die(Player player, Player damageSrc) : base(player)
        {
            this.damageSrc = damageSrc;
        }

        private Player damageSrc;

        public override async Task Execute()
        {
            actionView?.Invoke(this);

            player.IsAlive = false;
            player.Next.Last = player.Last;
            player.Last.Next = player.Next;

            if (damageSrc != null) await new GetCard(damageSrc, 3).Execute();
        }
    }

    public class Recover : UpdateHp
    {
        /// <summary>
        /// 回复体力
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="value">回复量</param>
        public Recover(Player player, int value = 1) : base(player, value) { }

        public override async Task Execute()
        {
            // 判断体力是否超过上限
            int t = player.Hp + Value - player.HpLimit;
            if (t > 0)
            {
                Value -= t;
                if (Value == 0) return;
            }

            Debug.Log((player.Position + 1).ToString() + "回复了" + Value.ToString() + "点体力");

            // 回复体力
            await base.Execute();

            // 执行事件
            await player.playerEvents.recover.Execute(this);
        }
    }

    public class Damaged : UpdateHp
    {
        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="damageSrc">伤害来源</param>
        /// <param name="value">伤害量</param>
        public Damaged(Player player, Player damageSrc, int value = 1) : base(player, -value)
        {
            DamageSrc = damageSrc;
        }

        public Player DamageSrc { get; private set; }

        public override async Task Execute()
        {
            Debug.Log((player.Position + 1).ToString() + "受到了" + Value.ToString() + "点伤害");

            // 受到伤害时
            await player.playerEvents.whenDamaged.Execute(this);

            // 受到伤害
            await base.Execute();

            // 受到伤害后
            await player.playerEvents.afterDamaged.Execute(this);
        }
    }

    // public class AcquireFromPlayer:PlayerAction<AcquireFromPlayer>
    // {
    //     public override async Task Execute()
    //     {
    //         throw new System.NotImplementedException();
    //     }
    // }

    public class Judge
    {
        public async Task<Card> Execute()
        {
            JudgeCard = await CardPile.Instance.Pop();
            CardPile.Instance.AddToDiscard(JudgeCard);
            Debug.Log("判定结果为【" + JudgeCard.Name + JudgeCard.Suit + JudgeCard.Weight + "】");

            await modifyJudge.Execute(this);

            return JudgeCard;
        }

        public Card JudgeCard { get; set; }

        public EventSet<Judge> modifyJudge = new EventSet<Judge>();
    }

    // 翻面
}