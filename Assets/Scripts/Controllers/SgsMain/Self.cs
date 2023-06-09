using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class Self : MonoBehaviour
    {
        private View.Self self => View.Self.Instance;
        private View.CardArea cardArea => View.CardArea.Instance;
        private View.OperationArea operationArea => View.OperationArea.Instance;
        private View.DestArea destArea => View.DestArea.Instance;
        private View.EquipArea equipArea => View.EquipArea.Instance;
        private View.SkillArea skillArea => View.SkillArea.Instance;
        private View.队友手牌 teammate;

        void Start()
        {
            teammate = GameObject.Find("Canvas").transform.Find("队友手牌Panel").GetComponent<View.队友手牌>();

            // 阶段信息
            Model.TurnSystem.Instance.StartPhaseView += self.ShowPhase;
            Model.TurnSystem.Instance.FinishPhaseView += self.HidePhase;

            // 进度条
            Model.TimerTask.Instance.StartTimerView += operationArea.ShowTimer;
            Model.TimerTask.Instance.StopTimerView += operationArea.HideTimer;

            // 手牌区
            Model.TimerTask.Instance.StopTimerView += cardArea.ResetCardArea;

            // 目标区
            Model.TimerTask.Instance.StopTimerView += destArea.ResetDestArea;

            // 获得牌
            Model.GetCard.ActionView += cardArea.AddHandCard;
            Model.GetCard.ActionView += cardArea.UpdateHandCardText;

            // 失去牌
            Model.LoseCard.ActionView += cardArea.RemoveHandCard;
            Model.LoseCard.ActionView += cardArea.UpdateHandCardText;

            // 改变体力
            Model.UpdateHp.ActionView += cardArea.UpdateHandCardText;

            // 装备区
            Model.Equipage.AddEquipView += equipArea.ShowEquipage;
            Model.Equipage.RemoveEquipView += equipArea.HideEquipage;
            Model.TimerTask.Instance.StopTimerView += equipArea.ResetEquipArea;

            // 技能区
            Model.SgsMain.Instance.GeneralView += skillArea.InitSkill;
            Model.UpdateSkill.ActionView += skillArea.InitSkill;
            Model.TimerTask.Instance.StopTimerView += skillArea.ResetSkillArea;

            // 移动座位
            Model.TimerTask.Instance.MoveSeat += cardArea.MoveSeat;
            Model.TimerTask.Instance.MoveSeat += cardArea.UpdateHandCardText;
            Model.TimerTask.Instance.MoveSeat += equipArea.MoveSeat;
            Model.TimerTask.Instance.MoveSeat += skillArea.MoveSeat;

            // 队友手牌
            Model.GetCard.ActionView += teammate.AddHandCard;
            Model.LoseCard.ActionView += teammate.RemoveHandCard;

            Model.SgsMain.Instance.GeneralView += self.ShowTeammateButton;
        }

        private void OnDestroy()
        {
            Model.TurnSystem.Instance.StartPhaseView -= self.ShowPhase;
            Model.TurnSystem.Instance.FinishPhaseView -= self.HidePhase;

            Model.TimerTask.Instance.StartTimerView -= operationArea.ShowTimer;
            Model.TimerTask.Instance.StopTimerView -= operationArea.HideTimer;

            Model.TimerTask.Instance.StopTimerView -= cardArea.ResetCardArea;

            Model.TimerTask.Instance.StopTimerView -= destArea.ResetDestArea;

            Model.GetCard.ActionView -= cardArea.AddHandCard;
            Model.GetCard.ActionView -= cardArea.UpdateHandCardText;

            Model.LoseCard.ActionView -= cardArea.RemoveHandCard;
            Model.LoseCard.ActionView -= cardArea.UpdateHandCardText;

            Model.UpdateHp.ActionView -= cardArea.UpdateHandCardText;

            Model.Equipage.AddEquipView -= equipArea.ShowEquipage;
            Model.Equipage.RemoveEquipView -= equipArea.HideEquipage;
            Model.TimerTask.Instance.StopTimerView -= equipArea.ResetEquipArea;

            Model.SgsMain.Instance.GeneralView -= skillArea.InitSkill;
            Model.UpdateSkill.ActionView -= skillArea.InitSkill;
            Model.TimerTask.Instance.StopTimerView -= skillArea.ResetSkillArea;

            Model.TimerTask.Instance.MoveSeat -= cardArea.MoveSeat;
            Model.TimerTask.Instance.MoveSeat -= cardArea.UpdateHandCardText;
            Model.TimerTask.Instance.MoveSeat -= equipArea.MoveSeat;
            Model.TimerTask.Instance.MoveSeat -= skillArea.MoveSeat;

            Model.GetCard.ActionView -= teammate.AddHandCard;
            Model.LoseCard.ActionView -= teammate.RemoveHandCard;

            Model.SgsMain.Instance.GeneralView -= self.ShowTeammateButton;
        }
    }
}
