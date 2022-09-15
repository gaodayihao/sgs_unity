// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// // using UnityEngine.EventSystems;

// namespace View
// {
//     public class 区域内 : CardPanel
//     {
//         // 手牌区
//         public GameObject handCards;
//         // 装备区
//         public GameObject equips;
//         // 标题
//         public Text title;
//         // public Text hint;

//         protected override void Start()
//         {
//             base.Start();
//             title.text = model.Title;

//             foreach (var i in model.dest.HandCards) InitCard(i, handCards.transform, model.display);
//             handCards.GetComponent<GridLayoutGroup>().spacing = UpdateSpacing(handCards.transform.childCount);

//             foreach (var i in model.dest.Equipages.Values)
//             {
//                 if (i != null) InitCard(i, equips.transform);
//             }

//             if (Model.CardPanel.Instance.judgeArea)
//             {
//                 foreach (var i in model.dest.JudgeArea) InitCard(i, equips.transform);
//             }
//         }
//     }
// }