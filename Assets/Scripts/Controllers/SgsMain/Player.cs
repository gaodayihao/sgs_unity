using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class Player : MonoBehaviour
    {
        private View.Player player;

        private void Start()
        {
            player = GetComponent<View.Player>();
            
            // 初始化
            Model.SgsMain.Instance.HeroView += player.UpdateHpLimit;
            Model.SgsMain.Instance.HeroView += player.UpdateHp;

            // 回合
            Model.TurnSystem.Instance.StartTurnView += player.StartTurn;
            Model.TurnSystem.Instance.FinishTurnView += player.FinishTurn;

            // 改变体力
            Model.UpdateHp.ActionView += player.UpdateHp;

            // 阵亡
            Model.Die.ActionView += player.OnDead;

            // 判定区
            Model.DelayScheme.AddJudgeView += player.AddJudgeCard;
            Model.DelayScheme.RemoveJudgeView += player.RemoveJudgeCard;
        }

        private void OnDestroy()
        {
            Model.SgsMain.Instance.HeroView -= player.UpdateHpLimit;
            Model.SgsMain.Instance.HeroView -= player.UpdateHp;
            
            Model.TurnSystem.Instance.StartTurnView -= player.StartTurn;
            Model.TurnSystem.Instance.FinishTurnView -= player.FinishTurn;

            Model.UpdateHp.ActionView -= player.UpdateHp;
            
            Model.Die.ActionView -= player.OnDead;
            
            Model.DelayScheme.AddJudgeView -= player.AddJudgeCard;
            Model.DelayScheme.RemoveJudgeView -= player.RemoveJudgeCard;
        }
    }
}
