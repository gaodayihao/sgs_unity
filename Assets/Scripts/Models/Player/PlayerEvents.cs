using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class PlayerEvents
    {
        public PlayerEvents(Player player)
        {
            this.player = player;

            startPhaseEvents = new Dictionary<Phase, EventSet>();
            // phaseEvents = new Dictionary<Phase, PhaseEvents>();
            finishPhaseEvents = new Dictionary<Phase, EventSet>();

            foreach (Phase phase in System.Enum.GetValues(typeof(Phase)))
            {
                // SkipPhase.Add(phase, false);
                startPhaseEvents.Add(phase, new EventSet());
                // phaseEvents.Add(phase, new PhaseEvents(player));
                finishPhaseEvents.Add(phase, new EventSet());
            }

            acquireCard = new EventSet<GetCard>();
            loseCard = new EventSet<LoseCard>();

            recover = new EventSet<Recover>();
            whenDamaged = new EventSet<Damaged>();
            afterDamaged = new EventSet<Damaged>();

            whenUseCard = new EventSet<Card>();
            afterUseCard = new EventSet<Card>();
        }

        public Player player { get; private set; }

        // 阶段开始时事件
        public Dictionary<Phase, EventSet> startPhaseEvents;
        // 阶段事件
        // public Dictionary<Phase, PhaseEvents> phaseEvents;
        // 阶段结束时事件
        public Dictionary<Phase, EventSet> finishPhaseEvents;

        // 获得牌后事件
        public EventSet<GetCard> acquireCard;
        // 失去牌后事件
        public EventSet<LoseCard> loseCard;

        // 回复体力后事件
        public EventSet<Recover> recover;
        
        // 受到伤害时事件
        public EventSet<Damaged> whenDamaged;
        // 受到伤害后事件
        public EventSet<Damaged> afterDamaged;

        // 使用牌时事件
        public EventSet<Card> whenUseCard;
        // 使用牌后事件
        public EventSet<Card> afterUseCard;
    }
}
