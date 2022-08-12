using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class 驱虎 : Active
    {
        public 驱虎(Player src) : base(src, "驱虎", 1) { }

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
            return dest.Hp > Src.Hp && dest.HandCardCount > 0;
        }

        public override bool IsValid()
        {
            return base.IsValid() && Src.HandCardCount > 0;
        }

        public override async Task Execute(List<Player> dests, List<Card> cards, string additional)
        {
            await base.Execute(dests, cards, additional);

            var compete = new Compete(Src, dests[0]);
            await compete.Execute();
            if (compete.Result)
            {
                TimerTask.Instance.ValidDest = (player, card, fstPlayer) => DestArea.UseSha(dests[0], player);
                await TimerTask.Instance.Run(Src, 0, 1);
                await new Damaged(TimerTask.Instance.Dests[0], dests[0]).Execute();
            }
            else await new Damaged(Src, dests[0]).Execute();
        }
    }

    public class 节命 : Triggered
    {
        public 节命(Player src) : base(src, "节命", false) { }

        public override int MaxDest(List<Card> cards)
        {
            return 1;
        }

        public override int MinDest(List<Card> cards)
        {
            return 1;
        }

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
            for (int i = 0; i < -damaged.Value; i++)
            {
                if (!await base.ShowTimer()) return;
                Execute();
                var dest = TimerTask.Instance.Dests[0];
                int count = dest.HpLimit - dest.HandCardCount;
                if (count > 0) await new GetCardFromPile(dest, count).Execute();
            }
        }
    }
}