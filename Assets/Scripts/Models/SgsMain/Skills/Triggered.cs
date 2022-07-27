using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class Triggered : Skill
    {
        public Triggered(Player src, string name, bool passive, int timeLimit = int.MaxValue)
            : base(src, name, passive, timeLimit) { }
    }
}
