using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class Equipage : MonoBehaviour
    {
        public int Id { get; private set; }
        public Image cardImage;
        public Image suit;
        public Image weight;

        public Button button;
        public bool IsSelected { get; private set; }

        public EquipArea equipArea { get => GetComponentInParent<EquipArea>(); }
        public OperationArea operationArea { get => GetComponentInParent<OperationArea>(); }
        public Model.Card model { get => Model.CardPile.Instance.cards[Id]; }

        void Start()
        {
            // button.interactable = false;
            button.onClick.AddListener(ClickCard);
        }

        public void Init(Model.Equipage card)
        {
            Id = card.Id;
            name = card.Name;

            var sprites = Sprites.Instance;
            cardImage.sprite = sprites.equipImage[name];
            suit.sprite = sprites.cardSuit[card.Suit];
            if (card.Suit == "黑桃" || card.Suit == "草花") weight.sprite = sprites.blackWeight[card.Weight];
            else weight.sprite = sprites.redWeight[card.Weight];
        }

        /// <summary>
        /// 点击卡牌
        /// </summary>
        private void ClickCard()
        {
            if (name == "丈八蛇矛")
            {
                if (operationArea.timerType == TimerType.PerformPhase ||
                    operationArea.timerType == TimerType.UseCard)
                {
                    operationArea.ChangeType(TimerType.丈八蛇矛);
                }
                else if (operationArea.timerType == TimerType.丈八蛇矛)
                {
                    operationArea.ChangeType(Model.TimerTask.Instance.timerType);
                    Select();
                }
            }
            
            // 选中卡牌
            if (!IsSelected) Select();
            else Unselect();
            GetComponentInParent<CardArea>().UpdateCardArea();
            GetComponentInParent<DestArea>().InitDestArea();
            GetComponentInParent<OperationArea>().UpdateButtonArea();

        }

        /// <summary>
        /// 选中卡牌
        /// </summary>
        public void Select()
        {
            if (IsSelected) return;

            IsSelected = true;
            GetComponent<RectTransform>().anchoredPosition += new Vector2(20, 0);
            equipArea.SelectedCard.Add(this);
            // Debug.Log(equipArea.SelectedCard.Count);
        }

        /// <summary>
        /// 取消选中
        /// </summary>
        public void Unselect()
        {
            if (!IsSelected) return;
            IsSelected = false;
            GetComponent<RectTransform>().anchoredPosition -= new Vector2(20, 0);
            equipArea.SelectedCard.Remove(this);
        }

        /// <summary>
        /// 重置卡牌
        /// </summary>
        public void ResetCard()
        {
            button.interactable = false;
            Unselect();
        }
    }
}
