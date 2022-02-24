using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class Menu : SingletonCtrl<Menu>
    {
        private View.Menu view;
        // Start is called before the first frame update
        void Start()
        {
            view = GetComponent<View.Menu>();

            ABManager.Instance.ProgressEvent += view.UpdateProgress;
        }

        private void OnDestroy()
        {
            ABManager.Instance.ProgressEvent -= view.UpdateProgress;
        }
    }
}
