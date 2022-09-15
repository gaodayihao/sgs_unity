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

        public Transform target;

        // 编号
        public int Id { get; private set; }
        // 是否被选中
        public bool IsSelected { get; private set; }

        // 手牌区
        private CardArea cardArea => CardArea.Instance;

        public Model.Card model { get; private set; }

        private Model.Card Converted
        {
            get => Model.Operation.Instance.Converted;
            set => Model.Operation.Instance.Converted = value;
        }

        public async void Init(Model.Card model, bool known)
        {
            this.model = model;
            name = model.Name;
            target.name = model.Name + "target";

            if (!known) return;

            var sprites = Sprites.Instance;
            while (sprites.cardImage is null) await Task.Yield();

            // 初始化sprite
            image.sprite = sprites.cardImage[name];
            if (!model.IsConvert)
            {

                suit.gameObject.SetActive(true);
                weight.gameObject.SetActive(true);
                Id = model.Id;
                suit.sprite = sprites.cardSuit[model.Suit];
                if (model.Suit == "黑桃" || model.Suit == "草花") weight.sprite = sprites.blackWeight[model.Weight];
                else weight.sprite = sprites.redWeight[model.Weight];
            }
        }

        /// <summary>
        /// 初始化卡牌
        /// </summary>
        public void InitInSelf(Model.Card model)
        {
            Init(model, true);
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

            if (!model.IsConvert) cardArea.UpdateCardArea();
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
            if (!model.IsConvert) Model.Operation.Instance.Cards.Add(model);
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
            if (!model.IsConvert) Model.Operation.Instance.Cards.Remove(model);
            else Converted = null;
        }

        /// <summary>
        /// 设置阴影
        /// </summary>
        public void SetShadow()
        {
            shadow.color = new Color(0, 0, 0, button.interactable ? 0 : 0.5f);
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
            Init(model, known);
            button.interactable = true;
            button.onClick.AddListener(ClickInPanel);
        }

        /// <summary>
        /// 点击卡牌
        /// </summary>
        private void ClickInPanel()
        {
            CardPanel.Instance.selectCard = this;
            CardPanel.Instance.UpdatePanel();
        }

        public bool inPanel = false;

        void Start()
        {
            if (!inPanel) transform.SetParent(CardSystem.Instance.transform, false);
        }

        public void SetParent(Transform parent)
        {
            target.SetParent(parent, false);
        }

        public bool isMoving { get; private set; } = false;

        public void Move(float second)
        {
            if (isMoving) return;

            if (second == 0 || !gameObject.activeSelf)
            {
                transform.position = target.position;
                return;
            }

            isMoving = true;
            StartCoroutine(MoveAsync(second));
        }

        private IEnumerator MoveAsync(float second)
        {
            yield return null;
            while (transform.position != target.position)
            {
                yield return null;
                var dx = (target.position - transform.position).magnitude / second * Time.deltaTime;
                second -= Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, target.position, dx);
            }
            isMoving = false;
        }

        void OnEnable()
        {
            if (target == null) target = Instantiate(ABManager.Instance.GetSgsAsset("CardTarget")).transform;
            target.gameObject.SetActive(true);
        }

        void OnDisable()
        {
            if (target != null) target.gameObject.SetActive(false);
            isMoving = false;
        }

        public bool WillDestroy = false;

        void OnDestroy()
        {
            if (target != null) Destroy(target.gameObject);
            if (DiscardArea.Instance.discards.Contains(this)) DiscardArea.Instance.discards.Remove(this);
        }
    }
}