using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class 刚烈 : Triggered
    {
        public 刚烈(Player src) : base(src, "刚烈", false) { }

        public override void OnEnabled()
        {
            Src.playerEvents.afterDamaged.AddEvent(Src, Execute);
        }

        public override void OnDisabled()
        {
            Src.playerEvents.afterDamaged.RemoveEvent(Src, Execute);
        }

        public async Task Execute(Damaged damaged)
        {
            if (damaged.Src is null || damaged.Src == Src) return;
            for (int i = 0; i < -damaged.Value; i++)
            {
                if (!await base.ShowTimer()) return;
                Execute();

                var card = await new Judge().Execute();
                if (card.Suit == "红桃" || card.Suit == "方片") await new Damaged(damaged.Src, Src).Execute();
                else
                {
                    CardPanel.Instance.Hint = "对" + (damaged.Src.Position + 1).ToString() + "号位发动刚烈，弃置其一张牌";
                    var c = await CardPanel.Instance.SelectCard(Src, damaged.Src);
                    await new Discard(damaged.Src, new List<Card> { c }).Execute();
                }
            }
        }
    }
    public class 清俭 : Triggered
    {
        public 清俭(Player src) : base(src, "清俭", false, 1) { }

        public override void OnEnabled()
        {
            Src.playerEvents.getCard.AddEvent(Src, Execute);
            TurnSystem.Instance.AfterTurn += Reset;
        }

        public override void OnDisabled()
        {
            Src.playerEvents.getCard.RemoveEvent(Src, Execute);
            TurnSystem.Instance.AfterTurn -= Reset;
        }

        public override int MaxCard()
        {
            return int.MaxValue;
        }

        public override int MinCard()
        {
            return 1;
        }

        public override int MaxDest(List<Card> cards)
        {
            return 1;
        }

        public override int MinDest(List<Card> cards)
        {
            return 1;
        }

        public override bool IsValidDest(Player dest, List<Card> cards, Player firstDest = null)
        {
            return dest != Src;
        }

        public async Task Execute(GetCard getCard)
        {
            if (!IsValid() || getCard is GetCardFromPile && (getCard as GetCardFromPile).InGetCardPhase) return;

            if (!await base.ShowTimer()) return;
            Execute();
            await new GetCardFromElse(TimerTask.Instance.Dests[0], Src, TimerTask.Instance.Cards).Execute();
        }
    }
}
