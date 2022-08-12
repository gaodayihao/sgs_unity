using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using System.Linq;

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

    /// <summary>
    /// 获得牌
    /// </summary>
    public class GetCard : PlayerAction<GetCard>
    {
        /// <summary>
        /// 获得牌
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="cards">卡牌数组</param>
        public GetCard(Player player, List<Card> cards) : base(player)
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

            // 执行获得牌后事件
            await player.playerEvents.AfterGetCard.Execute(this);
        }

        // public async Task FromElse(Player dest)
        // {
        //     await new LoseCard(dest, Cards).Execute();
        //     await Execute();
        // }
    }

    /// <summary>
    /// 摸牌
    /// </summary>
    public class GetCardFromPile : GetCard
    {
        /// <summary>
        /// 摸牌
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="count">摸牌数</param>
        public GetCardFromPile(Player player, int count) : base(player, new List<Card>())
        {
            Count = count;
        }
        public int Count { get; set; }

        public override async Task Execute()
        {
            await player.playerEvents.WhenGetCard.Execute(this);
            if (Count == 0) return;
            Debug.Log(player.PosStr + "号位摸了" + Count.ToString() + "张牌");
            // 摸牌
            for (int i = 0; i < Count; i++) Cards.Add(await CardPile.Instance.Pop());
            await base.Execute();
        }

        public bool InGetCardPhase { get; set; } = false;
    }

    /// <summary>
    /// 失去牌
    /// </summary>
    public class LoseCard : PlayerAction<LoseCard>
    {
        /// <summary>
        /// 失去牌
        /// </summary>
        public LoseCard(Player player, List<Card> cards) : base(player)
        {
            Cards = cards;
        }
        public List<Card> Cards { get; private set; }

        public override async Task Execute()
        {
            foreach (var card in Cards)
            {
                if (player.HandCards.Contains(card)) player.HandCards.Remove(card);
                else if (card is Equipage) await (card as Equipage).RemoveEquipage();
            }

            actionView?.Invoke(this);

            // 执行失去牌后事件
            await player.playerEvents.loseCard.Execute(this);
        }
    }

    /// <summary>
    /// 弃牌
    /// </summary>
    public class Discard : LoseCard
    {
        /// <summary>
        /// 弃牌
        /// </summary>
        public Discard(Player player, List<Card> cards) : base(player, cards) { }

        public override async Task Execute()
        {
            string str = "";
            foreach (var card in Cards) str += "【" + card.Name + card.Suit + card.Weight.ToString() + "】";
            Debug.Log(player.PosStr + "号位弃置了" + str);

            CardPile.Instance.AddToDiscard(Cards);

            // losecard
            await base.Execute();
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
        public int Value { get; set; }

        public override async Task Execute()
        {
            // 更新体力
            player.Hp += Value;
            actionView?.Invoke(this);

            // 执行事件(濒死)
            if (player.Hp < 1) await NearDeath();
            // 失去体力
            if (Value < 0 && !(this is Damaged)) await player.playerEvents.afterLoseHp.Execute(this);
        }

        public async Task NearDeath()
        {
            Player i = TurnSystem.Instance.CurrentPlayer;
            while (true)
            {
                while (await 桃.Call(i, player))
                {
                    if (player.Hp >= 1) return;
                }
                i = i.Next;
                if (i == TurnSystem.Instance.CurrentPlayer) break;
            }

            if (this is Damaged) await new Die(player, ((Damaged)this).Src).Execute();
            else await new Die(player, null).Execute();
        }
    }

    /// <summary>
    /// 阵亡
    /// </summary>
    public class Die : PlayerAction<Die>
    {
        public Die(Player player, Player damageSrc) : base(player)
        {
            DamageSrc = damageSrc;
        }

        public Player DamageSrc { get; private set; }

        public override async Task Execute()
        {
            actionView?.Invoke(this);

            player.skills.Clear();
            player.IsAlive = false;
            player.Next.Last = player.Last;
            player.Last.Next = player.Next;
            SgsMain.Instance.AlivePlayers.Remove(player);

            // 弃置所有牌
            List<Card> cards = new List<Card>();
            foreach (var i in player.HandCards) cards.Add(i);
            foreach (var i in player.Equipages.Values) if (i != null) cards.Add(i);
            foreach (var i in player.JudgeArea) cards.Add(i);
            // CardPile.Instance.AddToDiscard(cards);
            foreach (var i in cards) Debug.Log(i.Id);
            await new Discard(player, cards).Execute();

            if (!player.Teammate.IsAlive) await SgsMain.Instance.GameOver();
            else await new GetCardFromPile(player.Teammate, 1).Execute();
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

            Debug.Log(player.PosStr + "回复了" + Value.ToString() + "点体力");

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
        /// <param name="src">伤害来源</param>
        /// <param name="value">伤害量</param>
        public Damaged(Player player, Player src, Card srcCard = null, int value = 1, Damage type = Damage.Normal)
            : base(player, -value)
        {
            Src = src;
            SrcCard = srcCard;
            damageType = type;
        }

        public Player Src { get; private set; }
        public Card SrcCard { get; private set; }
        public Damage damageType { get; private set; }
        public bool IsConDucted { get; set; } = false;
        private bool conduct = false;

        public override async Task Execute()
        {
            // 受到伤害时
            await player.playerEvents.whenDamaged.Execute(this);

            if (Value == 0) return;
            if (player.armor != null && !(SrcCard is 杀 && (SrcCard as 杀).IgnoreArmor)) player.armor.WhenDamaged(this);

            Debug.Log(player.PosStr + "受到了" + (-Value).ToString() + "点伤害");

            // 受到伤害
            if (player.IsLocked)
            {
                await new SetLock(player, true).Execute();
                if (!IsConDucted) conduct = true;
            }
            await base.Execute();

            // 受到伤害后
            await player.playerEvents.afterDamaged.Execute(this);

            if (conduct) await Conduct();
        }

        /// <summary>
        /// 铁索连环传导
        /// </summary>
        private async Task Conduct()
        {
            Player i = TurnSystem.Instance.CurrentPlayer;
            while (true)
            {
                if (i.IsLocked)
                {
                    var damaged = new Damaged(i, Src, SrcCard, -Value, damageType);
                    damaged.IsConDucted = true;
                    await damaged.Execute();
                }
                i = i.Next;
                if (i == TurnSystem.Instance.CurrentPlayer) break;
            }
        }
    }

    /// <summary>
    /// 获得其他角色的牌
    /// </summary>
    public class GetCardFromElse : GetCard
    {
        public GetCardFromElse(Player player, Player dest, List<Card> cards) : base(player, cards)
        {
            Dest = dest;
            Cards = cards;
        }
        public Player Dest { get; private set; }

        public override async Task Execute()
        {
            // 获得牌
            foreach (var card in Cards)
            {
                player.HandCards.Add(card);
            }
            actionView?.Invoke(this);

            // 目标失去牌
            await new LoseCard(Dest, Cards).Execute();

            // 执行获得牌后事件
            await player.playerEvents.AfterGetCard.Execute(this);
        }
    }

    /// <summary>
    /// 判定
    /// </summary>
    public class Judge
    {
        public async Task<Card> Execute()
        {
            var JudgeCard = await CardPile.Instance.Pop();
            CardPile.Instance.AddToDiscard(JudgeCard);
            Debug.Log("判定结果为【" + JudgeCard.Name + JudgeCard.Suit + JudgeCard.Weight + "】");

            await modifyJudge.Execute(this);

            return JudgeCard;
        }

        // public Card JudgeCard { get; set; }

        public EventSet<Judge> modifyJudge = new EventSet<Judge>();
    }

    /// <summary>
    /// 展示手牌
    /// </summary>
    public class ShowCard : PlayerAction<ShowCard>
    {
        public ShowCard(Player player, List<Card> cards) : base(player)
        {
            Cards = cards;
        }
        public List<Card> Cards { get; protected set; }

        public override async Task Execute()
        {
            await Task.Yield();
            actionView?.Invoke(this);
        }
    }

    /// <summary>
    /// 横置 (重置)
    /// </summary>
    public class SetLock : PlayerAction<SetLock>
    {
        public SetLock(Player player, bool byDamage = false) : base(player)
        {
            ByDamage = byDamage;
        }

        public bool ByDamage { get; private set; }

        public override async Task Execute()
        {
            player.IsLocked = !player.IsLocked;
            actionView?.Invoke(this);
            await Task.Yield();
        }
    }

    public class Compete : PlayerAction<Compete>
    {
        public Compete(Player player, Player dest) : base(player)
        {
            Dest = dest;
        }

        public Player Dest { get; private set; }
        public Card Card0 { get; private set; }
        public Card Card1 { get; private set; }
        public bool Result { get; private set; }

        public override async Task Execute()
        {
            TimerTask.Instance.Hint = "请选择一张手牌拼点";
            if (player.Teammate == Dest)
            {
                Card0 = (await TimerAction.SelectHandCard(player, 1))[0];
                Card1 = (await TimerAction.SelectHandCard(Dest, 1))[0];
            }
            else
            {
                await TimerTask.Instance.Compete(player, Dest);
                Card0 = CardPile.Instance.cards[TimerTask.Instance.card0];
                Card1 = CardPile.Instance.cards[TimerTask.Instance.card1];
            }

            CardPile.Instance.AddToDiscard(Card0);
            CardPile.Instance.AddToDiscard(Card1);
            await new LoseCard(player, new List<Card> { Card0 }).Execute();
            await new LoseCard(Dest, new List<Card> { Card1 }).Execute();

            Result = Card0.Weight > Card1.Weight;
        }
    }

    // 翻面
}