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

        public async Task<bool> Execute(Damaged damaged)
        {
            TimerTask.Instance.GivenSkill = Name;
            TimerTask.Instance.Hint = "是否发动奸雄？";
            var result = await TimerTask.Instance.Run(Src, TimerType.CallSkill, 0);

            var srcCard = damaged.SrcCard;
            List<Card> cards;
            if (srcCard is null) cards = null;
            else if (srcCard.Convertion) cards = srcCard.PrimiTives.Count > 0 ? srcCard.PrimiTives : null;
            else cards = new List<Card> { srcCard };

            if (srcCard != null) await new GetCard(Src, cards).Execute();
            await new GetCardFromPile(Src, 1).Execute();
            return true;
        }
    }
}
