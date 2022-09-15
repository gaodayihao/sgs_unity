// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// namespace View
// {
//     public class 手牌 : CardPanel
//     {
//         public GameObject handCards;

//         protected override void Start()
//         {
//             base.Start();

//             foreach (var i in model.dest.HandCards) InitCard(i, handCards.transform, model.display);
//             handCards.GetComponent<GridLayoutGroup>().spacing = UpdateSpacing(handCards.transform.childCount);
//         }
//     }
// }
