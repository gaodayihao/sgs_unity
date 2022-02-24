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
            Dests[0].JudgeArea.Remove(this);
            removeJudgeView?.Invoke(this);
        }

        public virtual async Task Judge()
        {
            RemoveToJudgeArea();
            CardPile.Instance.AddToDiscard(this);
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

    public class LeBuSiShu : DelayScheme
    {
        public override async Task Judge()
        {
            await base.Judge();

            if (judgeCard.Suit != "红桃") TurnSystem.Instance.SkipPhase[Owner][Phase.Perform] = true;
        }
    }
}
