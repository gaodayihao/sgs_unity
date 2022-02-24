using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class Self : MonoBehaviour
    {
        public Model.Player model { get => GetComponentInChildren<Player>().model; }

        // 阶段信息
        public Image currentPhase;
        // 每阶段对应sprite
        private Dictionary<Phase, Sprite> phaseSprite;

        void Start()
        {
            phaseSprite = Sprites.Instance.self_phase;

            currentPhase.gameObject.SetActive(false);
        }

        /// <summary>
        /// 显示并更新阶段信息
        /// </summary>
        /// <param name="phase">新阶段</param>
        public void ShowPhase(Model.TurnSystem turnSystem)
        {
            // while (player is null) await Task.Yield();
            if (turnSystem.CurrentPlayer != model) return;

            currentPhase.gameObject.SetActive(true);
            currentPhase.sprite = phaseSprite[turnSystem.CurrentPhase];
        }

        /// <summary>
        /// 隐藏阶段信息(回合外)
        /// </summary>
        public void HidePhase(Model.TurnSystem turnSystem)
        {
            if (turnSystem.CurrentPlayer != model) return;
            currentPhase.gameObject.SetActive(false);
        }

    }
}
