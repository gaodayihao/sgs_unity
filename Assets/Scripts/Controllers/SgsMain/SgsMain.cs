using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class SgsMain : SingletonCtrl<SgsMain>
    {
        private View.SgsMain view => View.SgsMain.Instance;
        private View.CardSystem cardAnime => View.CardSystem.Instance;

        // Start is called before the first frame update
        void Start()
        {

            // view = GetComponent<View.SgsMain>();

            Model.SgsMain.Instance.GameOverView += view.GameOver;

            Model.SgsMain.Instance.PositionView += view.InitPlayers;

            Model.CardPanel.Instance.StartTimerView += view.ShowPanel;
            Model.CardPanel.Instance.StopTimerView += view.HidePanel;

            Model.TimerTask.Instance.MoveSeat += view.MoveSeat;

            Model.BanPick.Instance.ShowPanelView += view.ShowBP;

            Model.GetCard.ActionView += cardAnime.GetCardFromPile;
            Model.GetCard.ActionView += cardAnime.GetCardFromElse;
            Model.GetCard.ActionView += cardAnime.GetDiscard;
            Model.GetCard.ActionView += cardAnime.GetJudgeCard;
            Model.ExChange.ActionView += cardAnime.Exchange;
            Model.Card.UseCardView += cardAnime.UseCard;
            Model.Skill.UseSkillView += cardAnime.UseSkill;

            // Model.SgsMain.Instance.StartGame();
        }

        private void OnDestroy()
        {
            Model.SgsMain.Instance.GameOverView -= view.GameOver;

            Model.SgsMain.Instance.PositionView -= view.InitPlayers;

            Model.CardPanel.Instance.StartTimerView -= view.ShowPanel;
            Model.CardPanel.Instance.StopTimerView -= view.HidePanel;

            Model.TimerTask.Instance.MoveSeat -= view.MoveSeat;

            Model.GetCard.ActionView -= cardAnime.GetCardFromPile;
            Model.GetCard.ActionView -= cardAnime.GetCardFromElse;
            Model.GetCard.ActionView -= cardAnime.GetDiscard;
            Model.GetCard.ActionView -= cardAnime.GetJudgeCard;
            Model.ExChange.ActionView -= cardAnime.Exchange;
            Model.Card.UseCardView -= cardAnime.UseCard;
            Model.Skill.UseSkillView -= cardAnime.UseSkill;
        }

    }
}