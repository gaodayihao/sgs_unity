using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class Player : MonoBehaviour
    {
        // 位置
        public bool IsSelf { get; private set; }
        // public int Position { get; private set; }
        public Image positionImage;

        // 武将
        public Image heroImage;
        public Material gray;

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

        // 回合内边框
        public Image turnBorder;

        // 判定区
        public Transform judgeArea;

        public Model.Player model { get; private set; }

        private void Start()
        {
            heroImage.material = null;
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
        }

        /// <summary>
        /// 更新体力上限
        /// </summary>
        public void UpdateHpLimit(Model.Player player)
        {
            if (model != player) return;

            // 若体力上限<=5，用阴阳鱼表示
            if (player.HpLimit <= 5)
            {
                imageGroup.SetActive(true);
                numberGroup.SetActive(false);

                for (int i = 0; i < 5; i++)
                {
                    if (player.HpLimit > i) yinYangYu[i].gameObject.SetActive(true);
                    else yinYangYu[i].gameObject.SetActive(false);
                }
            }
            // 若体力上限>5，用数字表示
            else
            {
                imageGroup.SetActive(false);
                numberGroup.SetActive(true);

                hpLimit.text = player.HpLimit.ToString();
            }
        }

        // public void UpdateHpLimit(Model.PlayerOperation operation)
        // {
        //     UpdateHpLimit(operation.player);
        // }

        /// <summary>
        /// 更新体力
        /// </summary>
        public void UpdateHp(Model.Player player)
        {
            if (model != player) return;

            // 阴阳鱼或数字颜色
            int colorIndex = GetColorIndex(player.Hp, player.HpLimit);

            if (player.HpLimit <= 5)
            {
                for (int i = 0; i < player.HpLimit; i++)
                {
                    if (player.Hp > i) yinYangYu[i].sprite = Sprites.Instance.yinYangYu[colorIndex];
                    // 以损失体力设为黑色
                    else yinYangYu[i].sprite = Sprites.Instance.yinYangYu[0];
                }
            }
            else
            {
                hp.text = Mathf.Max(player.Hp, 0).ToString();

                hp.color = hpColor[colorIndex];
                slash.color = hpColor[colorIndex];
                hpLimit.color = hpColor[colorIndex];
                yinYangYuSingle.sprite = Sprites.Instance.yinYangYu[colorIndex];
            }

            // 是否进入濒死状态
            nearDeath.gameObject.SetActive(player.Hp < 1);
        }

        public void UpdateHp(Model.UpdateHp operation)
        {
            UpdateHp(operation.player);
        }

        public void OnDead(Model.Die operation)
        {
            if (operation.player != model) return;
            nearDeath.gameObject.SetActive(false);
            heroImage.material = gray;
            death.gameObject.SetActive(true);
        }

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
            if (ratio > 0.66) return 3;
            // 黄
            return 2;
        }
    }
}