using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class 咆哮 : Skill
    {
        public 咆哮(Player src) : base(src, "咆哮", true, int.MaxValue){ }

        public bool Effect(Card card)
        {
            return card is 杀;
        }

        public override void OnEnabled()
        {
            Src.unlimitedCard += Effect;
        }

        public override void OnDisabled()
        {
            Src.unlimitedCard -= Effect;
        }
    }
}