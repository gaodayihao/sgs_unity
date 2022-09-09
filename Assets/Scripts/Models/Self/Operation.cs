using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class Operation : SingletonMono<Operation>
    {
        public List<Card> Cards { get; private set; } = new List<Card>();
        public Card Converted { get; set; }
        public List<Player> Dests { get; private set; } = new List<Player>();
        public List<Card> Equips { get; private set; } = new List<Card>();
        public Skill skill { get; set; }
    }
}