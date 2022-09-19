using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
namespace Model
{
    public class 突袭 : Triggered
    {
        public 突袭(Player src) : base(src) { }

        public override void OnEnable()
        {
            Src.playerEvents.WhenGetCard.AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.playerEvents.WhenGetCard.RemoveEvent(Src, Execute);
        }

        public override int MaxDest => getCardFromPile.Count;
        public override int MinDest => 1;

        public override bool IsValidDest(Player dest) => dest.HandCardCount > 0;

        private GetCardFromPile getCardFromPile;

        public async Task Execute(GetCardFromPile getCard)
        {
            getCardFromPile = getCard;
            if (!getCard.InGetCardPhase || !await base.ShowTimer()) return;
            TurnSystem.Instance.SortDest(TimerTask.Instance.Dests);
            Execute();

            getCard.Count -= TimerTask.Instance.Dests.Count;
            foreach (var i in TimerTask.Instance.Dests)
            {
                CardPanel.Instance.Title = "突袭";
                CardPanel.Instance.Hint = "对" + i.PosStr + "号位发动突袭，获得其一张牌";
                if (Src.team == i.team) CardPanel.Instance.display = true;
                bool result = await CardPanel.Instance.Run(Src, i, TimerType.手牌);
                var card = result ? CardPanel.Instance.Cards : new List<Card> { i.HandCards[0] };
                await new GetCardFromElse(Src, i, card).Execute();
            }
        }

        protected override bool AIResult() => AI.Instance.SelectDest();
        // {
        //     foreach (var i in AI.Instance.DestList)
        //     {
        //         if (i.HandCardCount > 0)
        //         {
        //             Operation.Instance.Dests.Add(i);
        //             if (Operation.Instance.Dests.Count == MaxDest) break;
        //         }
        //     }
        //     return Operation.Instance.AICommit();
        // }
    }
}