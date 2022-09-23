using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{

    public class 当先 : Triggered
    {
        public 当先(Player src) : base(src) { }
        public override bool Passive => true;

        public override void OnEnable()
        {
            Src.playerEvents.startPhaseEvents[Phase.Prepare].AddEvent(Src, Execute);
            Src.playerEvents.startPhaseEvents[Phase.Perform].AddEvent(Src, StartPerform);
        }

        public override void OnDisable()
        {
            Src.playerEvents.startPhaseEvents[Phase.Prepare].RemoveEvent(Src, Execute);
            Src.playerEvents.startPhaseEvents[Phase.Perform].RemoveEvent(Src, StartPerform);
        }

        public new async Task Execute()
        {
            base.Execute();
            TurnSystem.Instance.ExtraPhase.Add(Phase.Perform);
            inSkill = true;
            await Task.Yield();
        }

        private bool inSkill;
        private bool change;

        public async Task StartPerform()
        {
            if (!inSkill) return;
            inSkill = false;
            if (change)
            {
                TimerTask.Instance.Hint = "是否失去1点体力并从弃牌堆获得一张【杀】？";
                if (!await TimerTask.Instance.Run(Src)) return;
            }

            await new UpdateHp(Src, -1).Execute();
            var card = CardPile.Instance.discardPile.Find(x => x is 杀);
            if (card != null) await new GetDisCard(Src, new List<Card> { card }).Execute();
        }
    }

    // public class 伏枥 : Triggered
    // {
    //     public 伏枥(Player src) : base(src){}
    //     public override bool Ultimate => true;

    //     // 
    // }
}