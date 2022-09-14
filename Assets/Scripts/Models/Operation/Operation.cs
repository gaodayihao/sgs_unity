using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Model
{
    public class Operation : SingletonMono<Operation>
    {
        public List<Card> Cards { get; private set; } = new List<Card>();
        public Card Converted { get; set; }
        public List<Player> Dests { get; private set; } = new List<Player>();
        public List<Card> Equips { get; private set; } = new List<Card>();
        public Skill skill { get; set; }

        public void Clear()
        {
            Cards.Clear();
            Converted = null;
            Dests.Clear();
            Equips.Clear();
            skill = null;
        }

        public bool AICommit()
        {
            if (Cards.Count > maxCard || Cards.Count < minCard || Dests.Count > MaxDest() || Dests.Count < MinDest())
            {
                // Debug.Log("dest.count=" + Dests.Count);
                foreach (var i in Dests) Debug.Log("dest:" + i.PosStr);
                Clear();
                return false;
            }
            // Debug.Log("aicommit");

            TimerTask.Instance.Cards.AddRange(Cards);
            TimerTask.Instance.Dests.AddRange(Dests);
            TimerTask.Instance.Cards.AddRange(Equips);
            TimerTask.Instance.Skill = skill != null ? skill.Name : "";

            Clear();
            return true;
        }

        // public void AutoCommit()
        // {
        //     Dests.AddRange(SgsMain.Instance.AlivePlayers.Where(IsValidDest));
        //     AICommit();
        // }

        public int maxCard { get; set; } = 0;
        public int minCard { get; set; } = 0;
        public Func<int> MaxDest { get; set; }
        public Func<int> MinDest { get; set; }
        public Func<Card, bool> IsValidCard { get; set; }
        public Func<Player, bool> IsValidDest { get; set; }

        public void CopyTimer()
        {
            maxCard = TimerTask.Instance.maxCard;
            minCard = TimerTask.Instance.minCard;
            MaxDest = TimerTask.Instance.MaxDest;
            MinDest = TimerTask.Instance.MinDest;
            IsValidCard = TimerTask.Instance.IsValidCard;
            IsValidDest = TimerTask.Instance.IsValidDest;
        }
    }
}