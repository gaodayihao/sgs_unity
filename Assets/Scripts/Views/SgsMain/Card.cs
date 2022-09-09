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
        private bool isConvert;

        // 手牌区
        private CardArea cardArea => CardArea.Instance;

        public Model.Card model { get; private set; }

        private Model.Card Converted
        {
            get => Model.Operation.Instance.Converted;
            set => Model.Operation.Instance.Converted = value;
        }

        /// <summary>
        /// 初始化卡牌
        /// </summary>
        public async void Init(Model.Card model, bool isConvert = false)
        {
            this.model = model;
            name = model.Name;
            this.isConvert = isConvert;

            var sprites = Sprites.Instance;
            while (sprites.cardImage is null) await Task.Yield();

            // 初始化sprite
            image.sprite = sprites.cardImage[name];
            if (!isConvert)
            {
                Id = model.Id;
                suit.sprite = sprites.cardSuit[model.Suit];
                if (model.Suit == "黑桃" || model.Suit == "草花") weight.sprite = sprites.blackWeight[model.Weight];
                else weight.sprite = sprites.redWeight[model.Weight];
            }
            else
            {
                suit.gameObject.SetActive(false);
                weight.gameObject.SetActive(false);
            }

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

            if (!isConvert) cardArea.UpdateCardArea();
            else cardArea.UpdateConvertCard();

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
            if (!isConvert) Model.Operation.Instance.Cards.Add(model);
            else
            {
                if (Converted != null) cardArea.ConvertedCards[Converted.Name].Unselect();
                Converted = model;
            }
        }

        /// <summary>
        /// 取消选中
        /// </summary>
        public void Unselect()
        {
            if (!IsSelected) return;
            IsSelected = false;
            GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, 20);
            if (!isConvert) Model.Operation.Instance.Cards.Remove(model);
            else Converted = null;
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

        public void InitInPanel(Model.Card model, bool known = true)
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
            button.onClick.AddListener(ClickInPanel);
        }

        /// <summary>
        /// 点击卡牌
        /// </summary>
        private void ClickInPanel()
        {
            CardPanel.Instance.selectCards.Add(this);
            CardPanel.Instance.UpdatePanel();
        }
    }
}
