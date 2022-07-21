using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class DiscardArea : MonoBehaviour
    {
        private View.DiscardArea view;

        void Start()
        {
            view = GetComponent<View.DiscardArea>();

            // 添加卡牌
            Model.CardPile.Instance.DiscardView += view.AddDiscard;
            Model.ShowCard.ActionView += view.AddDiscard;
            // Model.Discard.ActionView += view.AddDiscard;
            // Model.Card.UseCardView += view.AddDiscard;
            // Model.Equipage.AddEquipView += view.AddDiscard;

            // 清空卡牌
            Model.TurnSystem.Instance.FinishPerformView += view.Clear;
            Model.TurnSystem.Instance.FinishPhaseView += view.Clear;
        }

        private void OnDestroy()
        {
            Model.CardPile.Instance.DiscardView -= view.AddDiscard;
            Model.ShowCard.ActionView -= view.AddDiscard;
            // Model.LoseCard.ActionView -= view.AddDiscard;
            // Model.Card.UseCardView -= view.AddDiscard;

            Model.TurnSystem.Instance.FinishPerformView -= view.Clear;
            Model.TurnSystem.Instance.FinishPhaseView -= view.Clear;
        }
    }
}
