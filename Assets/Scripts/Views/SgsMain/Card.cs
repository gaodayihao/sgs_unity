using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class Card : MonoBehaviour
    {
        // 卡牌图片
        public Image image;
        // 花色
        public Image suit;
        // 点数
        public Image weight;
        // 阴影
        public Image shadow;
        // 按键
        public Button button;

        // 编号
        public int Id { get; private set; }
        // 是否被选中
        public bool IsSelected { get; private set; }

        // 手牌区
        private CardArea cardArea { get => CardArea.Instance; }

        public Model.Card model { get => Model.CardPile.Instance.cards[Id]; }

        /// <summary>
        /// 初始化卡牌
        /// </summary>
        public async void Init(Model.Card model)
        {
            Id = model.Id;
            name = model.Name;

            var sprites = Sprites.Instance;
            while (sprites.cardImage is null) await Task.Yield();

            // 初始化sprite
            image.sprite = sprites.cardImage[name];
            suit.sprite = sprites.cardSuit[model.Suit];
            if (model.Suit == "黑桃" || model.Suit == "草花") weight.sprite = sprites.blackWeight[model.Weight];
            else weight.sprite = sprites.redWeight[model.Weight];

            button.onClick.AddListener(ClickCard);
        }

        /// <summary>
        /// 点击卡牌
        /// </summary>
        private void ClickCard()
        {
            // 选中卡牌
            if (!IsSelected) Select();
            else Unselect();

            cardArea.UpdateCardArea();
            DestArea.Instance.ResetDestArea();
            DestArea.Instance.InitDestArea();
            OperationArea.Instance.UpdateButtonArea();
        }

        /// <summary>
        /// 选中卡牌
        /// </summary>
        public void Select()
        {
            if (IsSelected) return;
            IsSelected = true;
            GetComponent<RectTransform>().anchoredPosition += new Vector2(0, 20);
            cardArea.SelectedCard.Add(this);
        }

        /// <summary>
        /// 取消选中
        /// </summary>
        public void Unselect()
        {
            if (!IsSelected) return;
            IsSelected = false;
            GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, 20);
            cardArea.SelectedCard.Remove(this);
        }

        /// <summary>
        /// 设置阴影
        /// </summary>
        public void AddShadow()
        {
            if (!button.interactable) shadow.color = new Color(0, 0, 0, 0.5f);
        }

        /// <summary>
        /// 重置卡牌
        /// </summary>
        public void ResetCard()
        {
            button.interactable = false;
            Unselect();
            shadow.color = new Color(0, 0, 0, 0);
        }

        public void InitInPanel(Model.Card model, bool known)
        {
            Id = model.Id;
            name = model.Name;

            var sprites = Sprites.Instance;

            // 初始化sprite
            if (known)
            {
                image.sprite = sprites.cardImage[name];
                suit.sprite = sprites.cardSuit[model.Suit];
                if (model.Suit == "黑桃" || model.Suit == "草花") weight.sprite = sprites.blackWeight[model.Weight];
                else weight.sprite = sprites.redWeight[model.Weight];
            }
            else
            {
                image.sprite = sprites.cardImage["未知牌"];
                suit.gameObject.SetActive(false);
                weight.gameObject.SetActive(false);
            }

            button.interactable = true;
        }

        public void InitInRegion(Model.Card model, bool known = true)
        {
            InitInPanel(model, known);
            button.onClick.AddListener(ClickInRegion);
        }

        public void InitInQlg(Model.Card model)
        {
            InitInPanel(model, true);
            button.onClick.AddListener(ClickInQlg);
        }

        /// <summary>
        /// 点击卡牌
        /// </summary>
        private void ClickInRegion()
        {
            // 选中卡牌
            // if (!IsSelected) Select();
            // else Unselect();
            var panel = GetComponentInParent<RegionPanel>();

            panel.selectCards.Add(this);
            panel.UpdatePanel();
        }

        private void ClickInQlg()
        {
            var panel = GetComponentInParent<QlgPanel>();

            panel.SelectCard = this;
            panel.UpdatePanel();
        }
    }
}
