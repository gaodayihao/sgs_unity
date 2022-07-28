using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class ElsePlayer : MonoBehaviour
    {
        public Image currentPhase;
        public Slider timer;
        public Text handCardCount;

        public PlayerEquip[] equipArray;
        public Dictionary<string, PlayerEquip> equipages;

        private Sprites sprites { get => Sprites.Instance; }

        public Model.Player model { get => GetComponentInParent<Player>().model; }

        void Start()
        {
            currentPhase.gameObject.SetActive(false);
            timer.gameObject.SetActive(false);

            equipages = new Dictionary<string, PlayerEquip>
            {
                {"武器", equipArray[0]},
                {"防具", equipArray[1]},
                {"加一马", equipArray[2]},
                {"减一马", equipArray[3]}
            };

            // phaseSprite = Sprites.Instance.phase;
        }

        /// <summary>
        /// 显示倒计时进度条
        /// </summary>
        public void ShowTimer(TimerType timerType, int second)
        {
            timer.gameObject.SetActive(true);
            timer.value = 1;
            StartCoroutine(UpdateTimer(timerType, second));
        }

        public void ShowTimer(Model.TimerTask timerTask)
        {
            if (!gameObject.activeSelf) return;
            if (timerTask.timerType != TimerType.UseWxkj && timerTask.player != model) return;
            ShowTimer(timerTask.timerType, timerTask.second);
        }

        public void ShowTimer(Model.CardPanel cardPanel)
        {
            if (!gameObject.activeSelf) return;
            if (cardPanel.player != model) return;
            ShowTimer(cardPanel.timerType, cardPanel.second);
        }

        /// <summary>
        /// 隐藏倒计时进度条
        /// </summary>
        public void HideTimer()
        {
            StopAllCoroutines();
            timer.gameObject.SetActive(false);
        }

        public void HideTimer(Model.TimerTask timerTask)
        {
            if (timerTask.timerType != TimerType.UseWxkj && timerTask.player != model) return;
            HideTimer();
        }

        public void HideTimer(Model.CardPanel cardPanel)
        {
            if (cardPanel.player != model) return;
            HideTimer();
        }

        /// <summary>
        /// 显示并更新阶段信息
        /// </summary>
        /// <param name="phase">新阶段</param>
        public async void ShowPhase(Model.TurnSystem turnSystem)
        {
            if (turnSystem.CurrentPlayer != model) return;

            currentPhase.gameObject.SetActive(true);

            while (sprites.phase is null) await Task.Yield();
            currentPhase.sprite = sprites.phase[turnSystem.CurrentPhase];
        }

        /// <summary>
        /// 隐藏阶段信息(回合外)
        /// </summary>
        public void HidePhase(Model.TurnSystem turnSystem)
        {
            if (turnSystem.CurrentPlayer != model) return;

            currentPhase.gameObject.SetActive(false);
        }

        /// <summary>
        /// 更新手牌数
        /// </summary>
        private void UpdateHandCardCount(Model.Player player)
        {
            if (player != model) return;

            handCardCount.text = player.HandCardCount.ToString();
        }

        public void UpdateHandCardCount(Model.GetCard operation)
        {
            UpdateHandCardCount(operation.player);
        }

        public void UpdateHandCardCount(Model.LoseCard operation)
        {
            UpdateHandCardCount(operation.player);
        }

        public void ShowEquip(Model.Equipage card)
        {
            if (card.Src != model) return;

            equipages[card.Type].gameObject.SetActive(true);
            equipages[card.Type].Init(card);
        }

        public void HideEquip(Model.Equipage card)
        {
            if (card.Src != model) return;
            if (card.Id != equipages[card.Type].Id) return;

            equipages[card.Type].gameObject.SetActive(false);
        }

        /// <summary>
        /// 每帧更新进度条
        /// </summary>
        private IEnumerator UpdateTimer(TimerType timerType, int second)
        {
            if (timerType == TimerType.UseWxkj)
            {
                bool done = false;
                while (timer.value > 0)
                {
                    timer.value -= 0.1f / second;
                    if (!done && model.isAI && timer.value <= 1 - 1.0f / second)
                    {
                        done = true;
                        Model.TimerTask.Instance.SetWxkjResult(model.Position, false, null, "");
                    }
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else
            {
                while (timer.value > 0)
                {
                    timer.value -= 0.1f / second;
                    if (model.isAI && timer.value <= 1 - 1.0f / second)
                    {
                        switch (timerType)
                        {
                            case TimerType.RegionPanel:
                            case TimerType.顺手牵羊:
                            case TimerType.麒麟弓:
                                Model.CardPanel.Instance.SendResult();
                                break;
                            default:
                                Model.TimerTask.Instance.SendResult();
                                break;
                        }
                        break;
                    }
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }
}