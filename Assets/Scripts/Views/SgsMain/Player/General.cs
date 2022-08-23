using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace View
{
    public class General : MonoBehaviour
    {
        public int Id { get; private set; }

        // 武将
        public Image generalImage;
        public Text generalName;
        public Image nationBack;
        public Image nation;

        // 体力
        public GameObject imageGroup;
        public Image[] yinYangYu;
        public GameObject numberGroup;
        public Text hp;
        public Text slash;
        public Text hpLimit;
        public Color[] hpColor;
        public Image yinYangYuSingle;
        public Button button;

        public Image isPicked;
        private Transform parent;
        // public Text isPickedText;

        public Model.General model { get; private set; }

        private void Start()
        {
            button.onClick.AddListener(SetBpResult);
        }

        public void SetBpResult()
        {
            button.interactable = false;
            Model.BanPick.Instance.SendBpResult(Id);
        }

        public void SelfPick()
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(Select);
            button.interactable = true;
            parent = transform.parent;
            rectTransform = GetComponent<RectTransform>();
            originPos = rectTransform.anchoredPosition;
        }

        private RectTransform rectTransform;
        private Vector2 originPos;
        private bool isSelect;

        public void Select()
        {
            if (!isSelect)
            {
                if (BanPick.Instance.general0 is null)
                {
                    transform.SetParent(BanPick.Instance.seat0);
                    BanPick.Instance.general0 = this;
                }
                else if (BanPick.Instance.general1 is null)
                {
                    transform.SetParent(BanPick.Instance.seat1);
                    BanPick.Instance.general1 = this;
                }
                else return;
                rectTransform.anchoredPosition = new Vector2(0, 7);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                isSelect = true;
            }
            else
            {
                if (BanPick.Instance.general0 == this) BanPick.Instance.general0 = null;
                else BanPick.Instance.general1 = null;

                transform.SetParent(parent);

                rectTransform.anchoredPosition = originPos;
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.anchorMin = new Vector2(0, 1);
                isSelect = false;
            }
            BanPick.Instance.UpdateButton();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(Model.General model)
        {
            this.model = model;
            Id = model.id;
            generalName.text = model.name;
            nationBack.sprite = Sprites.Instance.nationBack[model.nation];
            nation.sprite = Sprites.Instance.nation[model.nation];

            UpdateHpLimit();
            InitSkin();
        }

        public async void InitSkin()
        {
            string url = Urls.GENERAL_IMAGE + "Seat/1" + Id.ToString().PadLeft(3, '0') + "01.png";
            var texture = await WebRequest.GetTexture(url);
            generalImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        /// <summary>
        /// 更新体力上限
        /// </summary>
        public void UpdateHpLimit()
        {
            // 若体力上限<=5，用阴阳鱼表示
            if (model.hp_limit <= 5)
            {
                imageGroup.SetActive(true);
                numberGroup.SetActive(false);

                for (int i = 0; i < 5; i++)
                {
                    if (model.hp_limit > i)
                    {
                        yinYangYu[i].gameObject.SetActive(true);
                        yinYangYu[i].sprite = Sprites.Instance.yinYangYu[3];
                    }
                    else yinYangYu[i].gameObject.SetActive(false);
                }
            }
            // 若体力上限>5，用数字表示
            else
            {
                imageGroup.SetActive(false);
                numberGroup.SetActive(true);

                hpLimit.text = model.hp_limit.ToString();

                hp.color = hpColor[3];
                slash.color = hpColor[3];
                hpLimit.color = hpColor[3];
                yinYangYuSingle.sprite = Sprites.Instance.yinYangYu[3];
            }
        }

        public void OnPick(Team team)
        {
            generalImage.color = new Color(0.4f, 0.4f, 0.4f);
            isPicked.gameObject.SetActive(true);
            isPicked.sprite = team == Team.Red ? Sprites.Instance.redSelect : Sprites.Instance.blueSelect;
        }
    }
}