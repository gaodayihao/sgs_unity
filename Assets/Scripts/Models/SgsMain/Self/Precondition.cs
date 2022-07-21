// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

namespace Model
{
    public interface Precondition
    {
        public int MaxCard();
        public int MinCard();
        public bool IsValidCard(Card card);

        public int MaxDest();
        public int MinDest();
        public bool IsValidDest(Player player);

        public bool IsValidSkill(Skill skill);
    }
}
