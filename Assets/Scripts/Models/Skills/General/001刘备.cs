using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class 仁德 : Active
    {
        public 仁德(Player src) : base(src, "仁德", int.MaxValue) { }

        public override int MaxCard => int.MaxValue;

        public override int MinCard => 1;

        public override int MaxDest(List<Card> cards) => 1;

        public override int MinDest(List<Card> cards) => 1;

        public override bool IsValidCard(Card card) => Src.HandCards.Contains(card);

        public override bool IsValidDest(Player dest, Player first) => dest != Src && !invalidDest.Contains(dest);

        private List<Player> invalidDest = new List<Player>();
        private int count = 0;
        private bool done = false;

        public override async Task Execute(List<Player> dests, List<Card> cards, string additional)
        {
            await base.Execute(dests, cards, additional);

            count += cards.Count;
            invalidDest.Add(dests[0]);
            await new GetCardFromElse(dests[0], Src, cards).Execute();
            if (count < 2 || done) return;

            var list = new List<Card>
            {
                Card.Convert<杀>(), Card.Convert<火杀>(), Card.Convert<雷杀>(), Card.Convert<酒>(), Card.Convert<桃>()
            };
            foreach (var i in list) TimerTask.Instance.MultiConvert.Add(i);
            TimerTask.Instance.ValidCard = CardArea.ValidCard;
            TimerTask.Instance.MaxDest = DestArea.MaxDest;
            TimerTask.Instance.ValidDest = DestArea.ValidDest;
            if (!await TimerTask.Instance.Run(Src)) return;

            // Debug.Log(TimerTask.Instance.Other);
            var card = list.Find(x => x.Name == TimerTask.Instance.Other);
            await card.UseCard(Src, TimerTask.Instance.Dests);
        }

        protected override void Reset()
        {
            base.Reset();
            count = 0;
            done = false;
            invalidDest.Clear();
        }
    }
}