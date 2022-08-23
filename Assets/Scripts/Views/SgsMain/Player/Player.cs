using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace View
{
    public class Player : MonoBehaviour
    {
        #region 属性
        // 位置
        public bool IsSelf { get; private set; }
        public Image positionImage;

        // 武将
        public Image generalImage;
        public Text generalName;
        public Image nationBack;
        public Image nation;
        public Material gray;
        public Image team;

        // 体力
        public GameObject imageGroup;
        public Image[] yinYangYu;
        public GameObject numberGroup;
        public Text hp;
        public Text slash;
        public Text hpLimit;
        public Color[] hpColor;
        public Image yinYangYuSingle;

        // 濒死状态
        public Image nearDeath;
        // 阵亡
        public Image death;
        public Image deadText;

        //横置
        public GameObject Lock;

        // 回合内边框
        public Image turnBorder;

        // 判定区
        public Transform judgeArea;

        #endregion

        public Model.Player model { get; private set; }
        private List<Model.Skin> skins;
        public Model.Skin CurrentSkin { get; private set; }
        public Dictionary<int, Dictionary<string, List<string>>> voices;

        private void Start()
        {
            generalImage.material = null;
            death.gameObject.SetActive(false);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(Model.Player player)
        {
            model = player;
            IsSelf = model.isSelf;
            positionImage.sprite = Sprites.Instance.position[model.Position];
            team.sprite = Sprites.Instance.camp[IsSelf ? 0 : 1];
        }

        public async void InitGeneral()
        {
            // if (this.model != model) return;
            nation.transform.parent.gameObject.SetActive(true);
            imageGroup.transform.parent.gameObject.SetActive(true);

            generalName.text = model.general.name;
            nationBack.sprite = Sprites.Instance.nationBack[model.general.nation];
            nation.sprite = Sprites.Instance.nation[model.general.nation];

            gameObject.AddComponent<GeneralInfoButton>();

            // if (IsSelf) SkillArea.Instance.InitSkill(model);

            await InitSkin();
            index = Random.Range(0, skins.Count);
            UpdateSkin();
            // UpdateSkin(model, skins[Random.Range(0, skins.Count)]);
            if (!IsSelf) StartCoroutine(RandomSkin(model));
        }

        public async Task InitSkin()
        {
            string url = Urls.JSON_URL + "skin/" + model.general.id.ToString().PadLeft(3, '0') + ".json";
            skins = JsonList<Model.Skin>.FromJson(await WebRequest.GetString(url));

            voices = new Dictionary<int, Dictionary<string, List<string>>>();
            foreach (var i in skins)
            {
                voices.Add(i.id, new Dictionary<string, List<string>>());
                foreach (var j in i.voice)
                {
                    voices[i.id].Add(j.name, new List<string>());
                    foreach (var k in j.url) voices[i.id][j.name].Add(k);
                }
            }
        }

        public async void UpdateSkin(Model.Player player, Model.Skin skin)
        {
            if (model != player) return;
            CurrentSkin = skin;

            // string url = Urls.GENERAL_IMAGE + player.general.id.ToString().PadLeft(3, '0') + "/" + skin.id + ".png";
            string url = Urls.GENERAL_IMAGE + "Seat/" + skin.id + ".png";
            var texture = await WebRequest.GetTexture(url);
            generalImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        private int index = 0;
        public void UpdateSkin()
        {
            if (skins is null) return;
            UpdateSkin(model, skins[index++ % skins.Count]);
        }

        public IEnumerator RandomSkin(Model.Player player)
        {
            while (true)
            {
                yield return new WaitForSeconds(10);
                float r = Random.Range(0, 1.0f);
                if (r > 0.8f)
                {
                    UpdateSkin(player, skins[Random.Range(0, skins.Count)]);
                }
            }
        }

        #region 体力值
        /// <summary>
        /// 更新体力上限
        /// </summary>
        public void UpdateHpLimit()
        {
            // if (this.model != model) return;

            // 若体力上限<=5，用阴阳鱼表示
            if (model.HpLimit <= 5)
            {
                imageGroup.SetActive(true);
                numberGroup.SetActive(false);

                for (int i = 0; i < 5; i++)
                {
                    if (model.HpLimit > i) yinYangYu[i].gameObject.SetActive(true);
                    else yinYangYu[i].gameObject.SetActive(false);
                }
            }
            // 若体力上限>5，用数字表示
            else
            {
                imageGroup.SetActive(false);
                numberGroup.SetActive(true);

                hpLimit.text = model.HpLimit.ToString();
            }
        }

        // public void UpdateHpLimit(Model.PlayerOperation operation)
        // {
        //     UpdateHpLimit(operation.player);
        // }

        /// <summary>
        /// 更新体力
        /// </summary>
        public void UpdateHp()
        {
            // 阴阳鱼或数字颜色
            int colorIndex = GetColorIndex(model.Hp, model.HpLimit);

            if (model.HpLimit <= 5)
            {
                for (int i = 0; i < model.HpLimit; i++)
                {
                    if (model.Hp > i) yinYangYu[i].sprite = Sprites.Instance.yinYangYu[colorIndex];
                    // 以损失体力设为黑色
                    else yinYangYu[i].sprite = Sprites.Instance.yinYangYu[0];
                }
            }
            else
            {
                hp.text = Mathf.Max(model.Hp, 0).ToString();

                hp.color = hpColor[colorIndex];
                slash.color = hpColor[colorIndex];
                hpLimit.color = hpColor[colorIndex];
                yinYangYuSingle.sprite = Sprites.Instance.yinYangYu[colorIndex];
            }

            // 是否进入濒死状态
            nearDeath.gameObject.SetActive(model.Hp < 1);
        }

        public void UpdateHp(Model.UpdateHp operation)
        {
            if (operation.player != model) return;
            UpdateHp();
        }

        /// <summary>
        /// 根据体力与上限比值返回颜色(红，黄，绿)
        /// </summary>
        /// <returns>颜色对应索引</returns>
        public int GetColorIndex(int hp, int hpLimit)
        {
            var ratio = hp / (float)hpLimit;
            // 红
            if (ratio < 0.34) return 1;
            // 绿
            if (ratio > 0.67) return 3;
            // 黄
            return 2;
        }
        #endregion

        public void OnDead(Model.Die operation)
        {
            if (operation.player != model) return;
            nearDeath.gameObject.SetActive(false);
            generalImage.material = gray;
            death.gameObject.SetActive(true);
            deadText.sprite = Sprites.Instance.deadText[IsSelf ? 0 : 1];
        }

        #region 回合内
        public void StartTurn(Model.TurnSystem turnSystem)
        {
            if (turnSystem.CurrentPlayer != model) return;

            turnBorder.gameObject.SetActive(true);
            if (!IsSelf) positionImage.gameObject.SetActive(false);
        }

        public void FinishTurn(Model.TurnSystem turnSystem)
        {
            if (turnSystem.CurrentPlayer != model) return;

            turnBorder.gameObject.SetActive(false);
            positionImage.gameObject.SetActive(true);
        }
        #endregion

        #region 判定区
        public void AddJudgeCard(Model.DelayScheme card)
        {
            if (card.Owner != model) return;

            var instance = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>("判定牌");
            instance = Instantiate(instance);
            instance.transform.SetParent(judgeArea, false);
            instance.name = card.Name;
            instance.GetComponent<Image>().sprite = Sprites.Instance.judgeCard[card.Name];
        }

        public void RemoveJudgeCard(Model.DelayScheme card)
        {
            if (card.Owner != model) return;

            foreach (Transform i in judgeArea)
            {
                if (i.name == card.Name)
                {
                    Destroy(i.gameObject);
                    break;
                }
            }
        }
        #endregion

        public void OnLock(Model.SetLock setLock)
        {
            if (setLock.player != model) return;
            Lock.SetActive(setLock.player.IsLocked);
        }
    }
}