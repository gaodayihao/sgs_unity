using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class Self : MonoBehaviour
    {
        private Player self { get => SgsMain.Instance.self; }

        // 阶段信息
        public Image currentPhase;
        // 每阶段对应sprite
        private Dictionary<Phase, Sprite> phaseSprite;

        public Button changeSkin;
        public Button teammate;
        public Button surrender;
        public GameObject teammatePanel;

        void Start()
        {
            phaseSprite = Sprites.Instance.self_phase;

            currentPhase.gameObject.SetActive(false);
            changeSkin.onClick.AddListener(ChangeSkin);
            teammate.onClick.AddListener(ClickTeammate);
            surrender.onClick.AddListener(ClickSurrender);
        }

        /// <summary>
        /// 显示并更新阶段信息
        /// </summary>
        /// <param name="phase">新阶段</param>
        public void ShowPhase(Model.TurnSystem turnSystem)
        {
            // while (player is null) await Task.Yield();
            if (turnSystem.CurrentPlayer != self.model) return;

            currentPhase.gameObject.SetActive(true);
            currentPhase.sprite = phaseSprite[turnSystem.CurrentPhase];
        }

        /// <summary>
        /// 隐藏阶段信息(回合外)
        /// </summary>
        public void HidePhase(Model.TurnSystem turnSystem)
        {
            if (turnSystem.CurrentPlayer != self.model) return;
            currentPhase.gameObject.SetActive(false);
        }

        private void ChangeSkin()
        {
            self.UpdateSkin();
        }

        private void ClickTeammate()
        {
            teammatePanel.SetActive(!teammatePanel.activeSelf);
        }

        private void ClickSurrender()
        {
            SgsMain.Instance.GameOver();
        }
    }
}
