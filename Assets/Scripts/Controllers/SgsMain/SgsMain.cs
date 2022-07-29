using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class SgsMain : SingletonCtrl<SgsMain>
    {
        private View.SgsMain view;

        //         private void Awake()
        //         {
        // #if UNITY_EDITOR
        //             ABManager.Instance.LoadSgsMain();
        // #else
        //             ABManager.Instance.LoadAssetBundle("sgsasset");
        // #endif

        //         }

        // Start is called before the first frame update
        void Start()
        {

            view = GetComponent<View.SgsMain>();

            Model.SgsMain.Instance.GameOverView += view.GameOver;

            Model.SgsMain.Instance.PositionView += view.InitPlayers;

            Model.CardPanel.Instance.StartTimerView += view.ShowPanel;
            Model.CardPanel.Instance.StopTimerView += view.HidePanel;

            Model.TimerTask.Instance.MoveSeat += view.MoveSeat;

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