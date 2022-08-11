using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Model
{
    public class DelayScheme : Card
    {
        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            await base.UseCard(src, dests);
            CardPile.Instance.RemoveToDiscard(this);
            AddToJudgeArea(dests[0]);
        }

        public Player Owner { get; private set; }

        public void AddToJudgeArea(Player owner)
        {
            Owner = owner;
            Owner.JudgeArea.Insert(0, this);
            addJudgeView?.Invoke(this);
        }

        public void RemoveToJudgeArea()
        {
            Owner.JudgeArea.Remove(this);
            removeJudgeView?.Invoke(this);
        }

        public virtual async Task Judge()
        {
            RemoveToJudgeArea();
            judgeCard = await new Judge().Execute();
        }

        protected Card judgeCard;

        private static UnityAction<DelayScheme> addJudgeView;
        private static UnityAction<DelayScheme> removeJudgeView;

        public static event UnityAction<DelayScheme> AddJudgeView
        {
            add => addJudgeView += value;
            remove => addJudgeView -= value;
        }
        public static event UnityAction<DelayScheme> RemoveJudgeView
        {
            add => removeJudgeView += value;
            remove => removeJudgeView -= value;
        }
    }

    public class 乐不思蜀 : DelayScheme
    {
        public 乐不思蜀()
        {
            Type = "延时锦囊";
            Name = "乐不思蜀";
        }

        public override async Task Judge()
        {
            CardPile.Instance.AddToDiscard(this);
            await base.Judge();

            if (judgeCard.Suit != "红桃") TurnSystem.Instance.SkipPhase[Owner][Phase.Perform] = true;
        }
    }

    public class 兵粮寸断 : DelayScheme
    {
        public 兵粮寸断()
        {
            Type = "延时锦囊";
            Name = "兵粮寸断";
        }

        public override async Task Judge()
        {
            CardPile.Instance.AddToDiscard(this);
            await base.Judge();

            if (judgeCard.Suit != "草花") TurnSystem.Instance.SkipPhase[Owner][Phase.Get] = true;
        }
    }

    public class 闪电 : DelayScheme
    {
        public 闪电()
        {
            Type = "延时锦囊";
            Name = "闪电";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            await base.UseCard(src, new List<Player> { src });
        }

        public override async Task Judge()
        {
            await base.Judge();

            if (judgeCard.Suit == "黑桃" && judgeCard.Weight >= 2 && judgeCard.Weight <= 9)
            {
                CardPile.Instance.AddToDiscard(this);
                await new Damaged(Owner, null, this, 3, Damage.Thunder).Execute();
            }
            else AddToJudgeArea(Owner.Next);
        }
    }
}