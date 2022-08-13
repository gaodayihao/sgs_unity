using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class 散谣 : Active
    {
        public 散谣(Player src) : base(src, "散谣", 1) { }

        public override int MaxCard
        {
            get
            {
                int maxHp = SgsMain.Instance.MaxHp(Src);
                int count = 0;
                foreach (var i in SgsMain.Instance.AlivePlayers)
                {
                    if (i.Hp == maxHp) count++;
                }
                return count;
            }
        }

        public override int MinCard => 1;

        public override int MaxDest(List<Card> cards) => cards.Count;

        public override int MinDest(List<Card> cards) => cards.Count;

        public override bool IsValidDest(Player dest, Player first)
        {
            return dest.Hp == SgsMain.Instance.MaxHp(Src) && dest != Src;
        }

        public override async Task Execute(List<Player> dests, List<Card> cards, string other)
        {
            TurnSystem.Instance.SortDest(dests);
            await base.Execute(dests, cards, other);

            await new Discard(Src, cards).Execute();
            foreach (var i in dests) await new Damaged(i, Src).Execute();
        }
    }

    public class 制蛮 : Triggered
    {
        public 制蛮(Player src) : base(src, "制蛮", false) { }

        public override void OnEnabled()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                if (i != Src) i.playerEvents.whenDamaged.AddEvent(Src, Execute);
            }
        }

        public override void OnDisabled()
        {
            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                if (i != Src) i.playerEvents.whenDamaged.RemoveEvent(Src, Execute);
            }
        }

        public async Task Execute(Damaged damaged)
        {
            if (damaged.Src is null || damaged.Src != Src) return;
            if (!await base.ShowTimer()) return;
            Execute();
            damaged.Value = 0;
            if (!damaged.player.RegionHaveCard()) return;
            var card = await CardPanel.Instance.SelectCard(Src, damaged.player, true);
            await new GetCardFromElse(Src, damaged.player, new List<Card> { card }).Execute();
        }
    }
}