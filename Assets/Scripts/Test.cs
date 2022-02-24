using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : View.SceneBase
{
    public Button chat;
    public Button tuoguan;
    // Start is called before the first frame update
    void Start()
    {
        chat.onClick.AddListener(hello);
        // tuoguan.onClick.AddListener(ClickTuoguan);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void ClickTuoguan()
    {
        // Model.SgsMain.Instance.StartGame();
    }

    void hello()
    {
        Debug.Log("聊天");
        StartCoroutine(SceneManager.Instance.LoadSceneFromAB("Menu"));
    }
}
