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

        public override bool IsValidCard(Card card)
        {
            return !Src.DisabledCard(card);
        }
    }
}
