using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class GameOver : MonoBehaviour
    {
        public GameObject win;
        public GameObject lose;

        // Start is called before the first frame update
        async void Start()
        {
            if (!Model.Room.Instance.IsSingle) await Wss.Instance.websocket.Close();
            if (Model.SgsMain.Instance.Loser == Model.User.Instance.team) lose.SetActive(true);
            else win.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                string scene = Model.Room.Instance.IsSingle ? "Login" : "Menu";
                SceneManager.Instance.LoadSceneFromAB(scene);
            }
        }
    }
}
