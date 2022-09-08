using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class 烈弓 : Triggered
    {
        public 烈弓(Player src) : base(src, "烈弓", false) { }

        public override void OnEnabled()
        {
            Src.playerEvents.afterUseCard.AddEvent(Src, Execute);
            Src.unlimitedDst += IsUnlimited;
        }

        public override void OnDisabled()
        {
            Src.playerEvents.afterUseCard.RemoveEvent(Src, Execute);
            Src.unlimitedDst -= IsUnlimited;
        }

        public async Task Execute(Card card)
        {
            if (card is not 杀) return;
            foreach (var i in card.Dests)
            {
                if (i.HandCardCount > Src.HandCardCount && i.Hp < Src.Hp) continue;
                if (!await base.ShowTimer()) continue;
                Execute();
                if (i.HandCardCount <= Src.HandCardCount) (card as 杀).ShanCount[i.Position] = 0;
                if (i.Hp >= Src.Hp) (card as 杀).DamageValue[i.Position]++;
            }
        }

        private bool IsUnlimited(Card card, Player dest) => card is 杀 && card.Weight >= Src.GetDistance(dest);
    }
}