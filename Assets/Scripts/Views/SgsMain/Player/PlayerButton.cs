using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace View
{
    public class PlayerButton : MonoBehaviour
    {
        // 按键
        public Button button;
        // 边框
        public Image border;
        // 武将图片(设置阴影)
        public Image heroImage;
        // 是否被选中
        public bool IsSelected { get; private set; }

        private DestArea destArea { get => FindObjectOfType<DestArea>(); }
        public Model.Player model { get => GetComponent<Player>().model; }
        
        void Start()
        {
            button.onClick.AddListener(ClickPlayer);
        }

        public void ClickPlayer()
        {
            // 选中目标角色
            if (!IsSelected) Select();
            else Unselect();

            destArea.UpdateDestArea();
            FindObjectOfType<OperationArea>().UpdateButtonArea();
        }

        /// <summary>
        /// 选中目标角色
        /// </summary>
        public void Select()
        {
            if (IsSelected) return;
            IsSelected = true;
            border.gameObject.SetActive(true);
            destArea.SelectedPlayer.Add(this);
        }

        /// <summary>
        /// 取消选中
        /// </summary>
        public void Unselect()
        {
            if (!IsSelected) return;
            IsSelected = false;
            border.gameObject.SetActive(false);
            destArea.SelectedPlayer.Remove(this);
        }

        /// <summary>
        /// 设置阴影
        /// </summary>
        public void AddShadow()
        {
            if (!button.interactable && !IsSelected && model.IsAlive) heroImage.color = new Color(0.5f, 0.5f, 0.5f);
            else heroImage.color = new Color(1, 1, 1);
        }

        /// <summary>
        /// 重置玩家按键
        /// </summary>
        public void ResetButton()
        {
            button.interactable = false;
            Unselect();
            heroImage.color = new Color(1, 1, 1);
        }

    }
}