using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public abstract class Converted : Skill
    {
        public Converted(Player src, string name, bool passive, int timeLimit, string cardName)
           : base(src, name, passive, timeLimit)
        {
            CardName = cardName;
        }

        // 转化牌名称
        public string CardName { get; private set; }

        public abstract Card Execute(List<Card> cards);

        public override int MaxCard => 1;

        public override int MinCard => 1;

        public override bool IsValidCard(Card card) => !Src.DisabledCard(card);

        public override bool IsValid => base.IsValid
            && TimerTask.Instance.maxCard > 0
            && TimerTask.Instance.IsValidCard(Execute(null));

        // public override void Execute()
        // {
        //     // Dests = TimerTask.Instance.Dests;
        //     base.Execute();
        // }
    }
}
