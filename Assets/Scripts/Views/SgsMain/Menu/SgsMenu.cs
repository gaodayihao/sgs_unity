using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class SgsMenu : MonoBehaviour
    {
        public Text pileCount;

        public void UpdatePileCount(Model.CardPile model)
        {
            pileCount.text = "牌堆数" + model.PileCount.ToString();
        }
    }
}
