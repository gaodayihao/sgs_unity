using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class 麒麟弓 : CardPanel
    {
        // 装备区
        public GameObject equips;
        protected override void Start()
        {
            base.Start();

            var plus = model.dest.plusHorse;
            if (plus != null) InitCard(plus, equips.transform);

            var sub = model.dest.subHorse;
            if (sub != null) InitCard(sub, equips.transform);
        }
    }
}
