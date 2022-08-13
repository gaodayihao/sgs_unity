using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

namespace Model
{
    public class 好施 : Triggered
    {
        public 好施(Player src) : base(src, "好施", false) { }

        public override void OnEnabled()
        {
            Src.playerEvents.WhenGetCard.AddEvent(Src, Execute);
        }

        public override void OnDisabled()
        {
            Src.playerEvents.WhenGetCard.RemoveEvent(Src, Execute);
        }

        public async Task Execute(GetCardFromPile getCard)
        {
            if (!getCard.InGetCardPhase || !await base.ShowTimer()) return;

            Execute();
            (getCard as GetCardFromPile).Count += 2;
            Src.playerEvents.finishPhaseEvents[Phase.Get].AddEvent(Src, Give);
            TurnSystem.Instance.AfterTurn += Reset;
        }

        public async Task Give()
        {
            if (Src.HandCardCount <= 5) return;

            int count = Src.HandCardCount / 2;
            int min = SgsMain.Instance.MinHand(Src);

            TimerTask.Instance.Hint = "请选择" + count + "张手牌，交给一名手牌最少的角色";
            TimerTask.Instance.ValidDest = (dest, card, first) => dest.HandCardCount == min;
            TimerTask.Instance.ValidCard = card => Src.HandCards.Contains(card);
            TimerTask.Instance.Refusable = false;
            bool result = await TimerTask.Instance.Run(Src, count, 1);

            var cards = result ? TimerTask.Instance.Cards : Src.HandCards.Take(count).ToList();
            var dest = result ? TimerTask.Instance.Dests[0] :
                SgsMain.Instance.AlivePlayers.Find(x => x.HandCardCount == min);

            await new GetCardFromElse(dest, Src, cards).Execute();
        }

        protected override void Reset()
        {
            // base.Reset();
            Src.playerEvents.finishPhaseEvents[Phase.Get].RemoveEvent(Src, Give);
            TurnSystem.Instance.AfterTurn -= Reset;
        }
    }

    public class 缔盟 : Active
    {
        public 缔盟(Player src) : base(src, "缔盟", 1) { }

        public override int MaxDest(List<Card> cards) => 2;
        public override int MinDest(List<Card> cards) => 2;
        public override bool IsValidDest(Player dest, Player first)
        {
            if (Src == dest) return false;

            int count = Src.HandCardCount;
            foreach (var i in Src.Equipages.Values) if (i != null) count++;
            return first is null || Mathf.Abs(first.HandCardCount - dest.HandCardCount) <= count;
        }

        public override async Task Execute(List<Player> dests, List<Card> cards, string other)
        {
            // 弃牌
            int count = Mathf.Abs(dests[0].HandCardCount - dests[1].HandCardCount);
            if (count > 0)
            {
                TimerTask.Instance.Refusable = false;
                bool result = await TimerTask.Instance.Run(Src, count, 0);
                List<Card> discard = null;
                if (result) discard = TimerTask.Instance.Cards;
                else if (Src.HandCardCount >= count) discard = Src.HandCards.Take(count).ToList();
                else
                {
                    discard = new List<Card>(Src.HandCards);
                    foreach (var i in Src.Equipages.Values)
                    {
                        if (discard.Count == count) break;
                        if (i != null) discard.Add(i);
                    }
                }
                await new Discard(Src, discard).Execute();
            }

            TurnSystem.Instance.SortDest(dests);
            await base.Execute(dests, cards, other);

            List<Card> card0 = new List<Card>(dests[0].HandCards);
            List<Card> card1 = new List<Card>(dests[1].HandCards);
            await new LoseCard(dests[0], card0).Execute();
            await new LoseCard(dests[1], card1).Execute();
            await new GetCard(dests[0], card1).Execute();
            await new GetCard(dests[1], card0).Execute();
        }
    }
}