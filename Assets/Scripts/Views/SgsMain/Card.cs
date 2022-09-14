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
            target.name = model.Name + "target";
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
            name = model.Name;
            target.name = model.Name + "target";

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

        // private Vector3 pos;

        public bool inPanel = false;

        void Start()
        {
            // target = Instantiate(ABManager.Instance.GetSgsAsset("CardTarget")).transform;
            // target.name = name;
            if (!inPanel) transform.SetParent(CardSystem.Instance.transform, false);
            // Debug.Log(name + " parent " + transform.parent.name);
        }

        public void SetParent(Transform parent)
        {
            // Debug.Log(name + " setparent " + parent.name);
            // pos = transform.position;
            target.SetParent(parent, false);
            // StartCoroutine(Move(second));
            // CardSystem.Instance.UpdateAll(second);
            // target.position = pos;
        }

        public void SetAsLastSibling()
        {
            target.SetAsLastSibling();
            // CardSystem.Instance.UpdateAll(0);
            // transform.position = target.position;
        }

        public bool isMoving { get; private set; } = false;

        public void Move(float second)
        {
            // await SgsMain.Instance.WaitFrame();
            // Debug.Log("move" + name);
            if (isMoving)
            {
                // Debug.Log(name + " ismoving");
                return;
            }

            if (second == 0 || !gameObject.activeSelf)
            {
                // if (!gameObject.activeSelf) Debug.Log(name + " !gameObject.activeSelf");
                transform.position = target.position;
                return;
            }

            // if (!gameObject.activeSelf) return;

            isMoving = true;
            StartCoroutine(MoveAsync(second));
        }

        private IEnumerator MoveAsync(float second)
        {
            yield return null;
            // Debug.Log("moveasync" + name);
            while (transform.position != target.position)
            {
                yield return null;
                var dx = (target.position - transform.position).magnitude / second * Time.deltaTime;
                second -= Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, target.position, dx);
                // Debug.Log(transform.position);
            }
            isMoving = false;
        }

        // public void SetActive(bool value)
        // {
        //     gameObject.SetActive(value);
        //     target.gameObject.SetActive(value);
        // }

        void OnEnable()
        {
            if (target == null)
            {
                target = Instantiate(ABManager.Instance.GetSgsAsset("CardTarget")).transform;
                // Debug.Log(target.gameObject is null);
                // target.name = name;
            }
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
            // Debug.Log(name + " ondestroy");
            if (target != null) Destroy(target.gameObject);
            // Debug.Log(DiscardArea.Instance.discards.Contains(this));
            if (DiscardArea.Instance.discards.Contains(this))
            {
                DiscardArea.Instance.discards.Remove(this);
                // Debug.Log("remove");
            }
        }
    }
}
