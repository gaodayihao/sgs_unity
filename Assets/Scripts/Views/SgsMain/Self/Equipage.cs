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
        public bool UseSkill { get; private set; }

        public EquipArea equipArea { get => EquipArea.Instance; }
        public OperationArea operationArea { get => OperationArea.Instance; }
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
            // 可发动丈八蛇矛
            if (name == "丈八蛇矛" && Model.TimerTask.Instance.ValidCard(Model.Card.Convert<Model.杀>()))
            {
                var skill = SkillArea.Instance.SelectedSkill;
                if (skill == null)
                {
                    SkillArea.Instance.SelectedSkill = (model as Model.丈八蛇矛).skill;
                    operationArea.UseSkill();
                    Use();
                }
                else if (skill == (model as Model.丈八蛇矛).skill)
                {
                    SkillArea.Instance.SelectedSkill = null;
                    operationArea.UseSkill();
                    Cancel();
                }
            }
            else
            {
                // 选中卡牌
                if (!IsSelected) Select();
                else Unselect();
                CardArea.Instance.UpdateCardArea();
                DestArea.Instance.InitDestArea();
                OperationArea.Instance.UpdateButtonArea();
            }

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
            Cancel();
        }

        /// <summary>
        /// 发动技能
        /// </summary>
        public void Use()
        {
            if (UseSkill) return;

            UseSkill = true;
            GetComponent<RectTransform>().anchoredPosition += new Vector2(20, 0);
        }

        /// <summary>
        /// 取消
        /// </summary>
        public void Cancel()
        {
            if (!UseSkill) return;
            UseSkill = false;
            GetComponent<RectTransform>().anchoredPosition -= new Vector2(20, 0);
        }
    }
}
