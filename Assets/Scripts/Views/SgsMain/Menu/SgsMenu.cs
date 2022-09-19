using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class SgsMenu : MonoBehaviour
    {
        public Text pileCount;
        public Text frame;

        public void UpdatePileCount(Model.CardPile model)
        {
            pileCount.text = "牌堆数" + model.PileCount.ToString();
        }

        void Start()
        {
            StartCoroutine(UpdateFrame());
        }

        public IEnumerator UpdateFrame()
        {
            while (true)
            {
                frame.text = "FPS: " + 1f / Time.deltaTime;
                yield return new WaitForSeconds(1f);
                // Debug.Log(frame.text);
            }
        }
    }
}
