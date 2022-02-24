using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class SgsMain : SingletonCtrl<SgsMain>
    {
        private Model.SgsMain model;
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

            Model.SgsMain.Instance.PositionView += view.InitPlayers;

            Model.CardPanel.Instance.StartTimerView += view.ShowPanel;
            Model.CardPanel.Instance.StopTimerView += view.HidePanel;

            model = Model.SgsMain.Instance;
            Model.Mode mode = new Model.Mode();
            mode.playerCount = 4;
            mode.performTimer = 10;
            model.StartGame(mode);
        }

        private void OnDestroy()
        {
            Model.SgsMain.Instance.PositionView -= view.InitPlayers;
            
            Model.CardPanel.Instance.StartTimerView -= view.ShowPanel;
            Model.CardPanel.Instance.StopTimerView -= view.HidePanel;
        }

    }
}