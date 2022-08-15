using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class SgsMain : SingletonCtrl<SgsMain>
    {
        private View.SgsMain view => View.SgsMain.Instance;

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

            // Model.SgsMain.Instance.StartGame();
        }

        private void OnDestroy()
        {
            Model.SgsMain.Instance.GameOverView -= view.GameOver;

            Model.SgsMain.Instance.PositionView -= view.InitPlayers;

            Model.CardPanel.Instance.StartTimerView -= view.ShowPanel;
            Model.CardPanel.Instance.StopTimerView -= view.HidePanel;

            Model.TimerTask.Instance.MoveSeat -= view.MoveSeat;
        }

    }
}