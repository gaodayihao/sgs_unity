using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
namespace Model
{
    public class 恩怨 : Triggered
    {
        public 恩怨(Player src) : base(src, "恩怨", false) { }

        public override int MaxDest => 1;
        public override int MinDest => 1;
        public override bool IsValidDest(Player dest1) => dest1 == dest;

        public override void OnEnabled()
        {
            Src.playerEvents.AfterGetCard.AddEvent(Src, Execute);
            Src.playerEvents.afterDamaged.AddEvent(Src, Execute);
        }

        public override void OnDisabled()
        {
            Src.playerEvents.AfterGetCard.RemoveEvent(Src, Execute);
            Src.playerEvents.afterDamaged.RemoveEvent(Src, Execute);
        }

        public async Task Execute(GetCard getCard)
        {
            if (!(getCard is GetCardFromElse)) return;
            var getCardFromElse = getCard as GetCardFromElse;
            dest = getCardFromElse.Dest;
            if (getCardFromElse.Cards.Count < 2 || !await base.ShowTimer()) return;
            Execute();

            await new GetCardFromPile(getCardFromElse.Dest, 1).Execute();
        }

        public async Task Execute(Damaged damaged)
        {
            if (damaged.Src is null) return;
            dest = damaged.Src;
            for (int i = 0; i < -damaged.Value; i++)
            {
                if (!await base.ShowTimer()) return;
                Execute();
                TimerTask.Instance.Hint = "点确定交给法正一张手牌，点取消失去一点体力";
                TimerTask.Instance.IsValidCard = card => damaged.Src.HandCards.Contains(card);
                bool result = await TimerTask.Instance.Run(damaged.Src, 1, 0);

                if (Room.Instance.IsSingle && damaged.Src.isAI)
                {
                    if (damaged.Src.HandCardCount > 0)
                    {
                        Operation.Instance.Cards.Add(damaged.Src.HandCards[0]);
                        Operation.Instance.AICommit();
                        result = true;
                    }
                    else result = false;
                }

                if (result)
                {
                    var card = TimerTask.Instance.Cards[0];
                    await new GetCardFromElse(Src, damaged.Src, TimerTask.Instance.Cards).Execute();
                    if (card.Suit != "红桃") await new GetCardFromPile(Src, 1).Execute();
                }
                else await new UpdateHp(damaged.Src, -1).Execute();
            }
        }

        private Player dest;

        protected override bool AIResult()
        {
            AI.Instance.SelectDest();
            return true;
        }
    }

    public class 眩惑 : Triggered
    {
        public 眩惑(Player src) : base(src, "眩惑", false) { }

        public override void OnEnabled()
        {
            Src.playerEvents.finishPhaseEvents[Phase.Get].AddEvent(Src, Execute);
        }

        public override void OnDisabled()
        {
            Src.playerEvents.finishPhaseEvents[Phase.Get].RemoveEvent(Src, Execute);
        }

        public override int MaxCard => 2;
        public override int MinCard => 2;
        public override int MaxDest => 2;
        public override int MinDest => 2;

        public override bool IsValidCard(Card card) => Src.HandCards.Contains(card);

        public override bool IsValidDest(Player dest) => Operation.Instance.Dests.Count > 0 || dest != Src;

        public new async Task Execute()
        {
            if (!await base.ShowTimer()) return;
            base.Execute();
            var dest0 = TimerTask.Instance.Dests[0];
            var dest1 = TimerTask.Instance.Dests[1];
            await new GetCardFromElse(dest0, Src, TimerTask.Instance.Cards).Execute();
            var list = new List<Card>
            {
                Card.Convert<杀>(),
                Card.Convert<火杀>(),
                Card.Convert<雷杀>(),
                Card.Convert<决斗>(),
            };
            foreach (var i in list) TimerTask.Instance.MultiConvert.Add(i);
            TimerTask.Instance.IsValidCard = card => true;
            TimerTask.Instance.IsValidDest = player => player == dest1;
            bool result = await TimerTask.Instance.Run(dest0, 0, 1);
            if (result)
            {
                var card = list.Find(x => x.Name == TimerTask.Instance.Other);
                await card.UseCard(dest0, TimerTask.Instance.Dests);
            }
            else await new GetCardFromElse(Src, dest0, new List<Card>(dest0.HandCards)).Execute();
        }

        protected override bool AIResult() => false;
    }
}