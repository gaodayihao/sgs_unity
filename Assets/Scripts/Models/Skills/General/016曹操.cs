using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class 奸雄 : Triggered
    {
        public 奸雄(Player src) : base(src, "奸雄", false) { }

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
            if (!await base.ShowTimer()) return;
            Execute();

            // var srcCard = damaged.SrcCard;
            List<Card> srcCard = null;
            if (damaged.SrcCard != null) srcCard = damaged.SrcCard.InDiscardPile();

            if (srcCard != null && srcCard.Count != 0) await new GetCard(Src, srcCard).Execute();
            await new GetCardFromPile(Src, 1).Execute();
        }
    }
}
