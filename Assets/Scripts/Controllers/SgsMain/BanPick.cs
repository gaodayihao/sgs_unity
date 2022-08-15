using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class BanPick : MonoBehaviour
    {
        private View.BanPick view => View.BanPick.Instance;

        // Start is called before the first frame update
        void Start()
        {
            Model.BanPick.Instance.StartTimerView += view.ShowTimer;
            Model.BanPick.Instance.OnPickView += view.OnPick;
            Model.BanPick.Instance.StartBanView += view.StartBan;
            Model.BanPick.Instance.BanView += view.OnBan;
            Model.BanPick.Instance.SelfPickView += view.SelfPick;
            Model.SgsMain.Instance.GeneralView += view.Destroy;
        }

        void OnDestroy()
        {
            Model.BanPick.Instance.StartTimerView -= view.ShowTimer;
            Model.BanPick.Instance.OnPickView -= view.OnPick;
            Model.BanPick.Instance.StartBanView -= view.StartBan;
            Model.BanPick.Instance.BanView -= view.OnBan;
            Model.BanPick.Instance.SelfPickView -= view.SelfPick;
            Model.SgsMain.Instance.GeneralView -= view.Destroy;
        }
    }
}