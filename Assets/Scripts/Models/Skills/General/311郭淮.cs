using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{

    public class 精策 : Triggered
    {
        public 精策(Player src) : base(src) { }

        public override void OnEnable()
        {
            Src.playerEvents.startPhaseEvents[Phase.End].AddEvent(Src, Execute);
            Src.playerEvents.whenUseCard.AddEvent(Src, WhenUseCard);
            TurnSystem.Instance.AfterTurn += Reset1;
        }

        public override void OnDisable()
        {
            Src.playerEvents.startPhaseEvents[Phase.End].RemoveEvent(Src, Execute);
            Src.playerEvents.whenUseCard.RemoveEvent(Src, WhenUseCard);
            TurnSystem.Instance.AfterTurn -= Reset1;
        }

        public new async Task Execute()
        {
            if (cards.Count < Src.Hp) return;
            if (!await base.ShowTimer()) return;
            if (cards.Find(x => x.Weight < Src.Hp) != null)
            {
                TimerTask.Instance.Hint = "点击确定执行一个额外的摸牌，点击取消执行出牌阶段";
                bool result = await TimerTask.Instance.Run(Src);
                TurnSystem.Instance.ExtraPhase.Add(result ? Phase.Get : Phase.Perform);
            }
            else
            {
                TurnSystem.Instance.ExtraPhase.Add(Phase.Get);
                TurnSystem.Instance.ExtraPhase.Add(Phase.Perform);
            }
            base.Execute();
        }

        public async Task WhenUseCard(Card card)
        {
            if (TurnSystem.Instance.CurrentPlayer != Src) return;
            if (!card.IsConvert) cards.Add(card);
            else cards.AddRange(card.PrimiTives);
            await Task.Yield();
        }

        private List<Card> cards = new List<Card>();

        public void Reset1()
        {
            if (TurnSystem.Instance.CurrentPlayer == Src) cards.Clear();
        }
    }
}