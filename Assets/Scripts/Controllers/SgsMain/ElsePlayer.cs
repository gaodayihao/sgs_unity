using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class ElsePlayer : MonoBehaviour
    {
        private View.ElsePlayer elsePlayer;
        void Start()
        {
            elsePlayer = GetComponent<View.ElsePlayer>();

            // 阶段信息
            Model.TurnSystem.Instance.StartPhaseView += elsePlayer.ShowPhase;
            Model.TurnSystem.Instance.FinishPhaseView += elsePlayer.HidePhase;

            // 进度条
            Model.TimerTask.Instance.StartTimerView += elsePlayer.ShowTimer;
            Model.TimerTask.Instance.StopTimerView += elsePlayer.HideTimer;

            Model.CardPanel.Instance.StartTimerView += elsePlayer.ShowTimer;
            Model.CardPanel.Instance.StopTimerView += elsePlayer.HideTimer;

            // 获得牌
            Model.GetCard.ActionView += elsePlayer.UpdateHandCardCount;

            // 失去牌
            Model.LoseCard.ActionView += elsePlayer.UpdateHandCardCount;

            // 装备区
            Model.Equipage.AddEquipView += elsePlayer.ShowEquip;
            Model.Equipage.RemoveEquipView += elsePlayer.HideEquip;
        }

        private void OnDestroy()
        {
            if (elsePlayer is null) return;

            Model.TurnSystem.Instance.StartPhaseView -= elsePlayer.ShowPhase;
            Model.TurnSystem.Instance.FinishPhaseView -= elsePlayer.HidePhase;

            Model.TimerTask.Instance.StartTimerView -= elsePlayer.ShowTimer;
            Model.TimerTask.Instance.StopTimerView -= elsePlayer.HideTimer;

            Model.CardPanel.Instance.StartTimerView -= elsePlayer.ShowTimer;
            Model.CardPanel.Instance.StopTimerView -= elsePlayer.HideTimer;

            Model.GetCard.ActionView -= elsePlayer.UpdateHandCardCount;

            Model.LoseCard.ActionView -= elsePlayer.UpdateHandCardCount;

            Model.Equipage.AddEquipView -= elsePlayer.ShowEquip;
            Model.Equipage.RemoveEquipView -= elsePlayer.HideEquip;
        }
    }
}
