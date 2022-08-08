using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
namespace Model
{
    public class 突袭 : Triggered
    {
        public 突袭(Player src) : base(src, "突袭", false) { }

        public override void OnEnabled()
        {
            Src.playerEvents.getCard.AddEvent(Src, Execute);
        }

        public override void OnDisabled()
        {
            Src.playerEvents.getCard.RemoveEvent(Src, Execute);
        }

        public override int MaxDest(List<Card> cards)
        {
            return getCardFromPile.Count;
        }

        public override int MinDest(List<Card> cards)
        {
            return 1;
        }

        private GetCardFromPile getCardFromPile;

        public async Task Execute(GetCard getCard)
        {
            if (!(getCard is GetCardFromPile)) return;
            getCardFromPile = getCard as GetCardFromPile;
            if (!getCardFromPile.InGetCardPhase || !await base.ShowTimer()) return;
            Execute();

            getCardFromPile.Count -= TimerTask.Instance.Dests.Count;
            foreach (var i in TimerTask.Instance.Dests)
            {
                if (Src.Teammate == i) CardPanel.Instance.display = true;
                bool result = await CardPanel.Instance.Run(Src, i, TimerType.手牌);
                var card = result ? CardPanel.Instance.Cards : new List<Card> { i.HandCards[0] };
                await new GetCardFromElse(Src, i, card).Execute();
            }
        }
    }
}