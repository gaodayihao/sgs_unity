using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(load());

    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator load()
    {
        yield return ABManager.Instance.LoadAssetBundle("sgsmain");

        AssetBundle ab = ABManager.Instance.ABMap["sgsmain"];
        Canvas sgsMain = ab.LoadAsset<GameObject>("SgsMainCanvas").GetComponent<Canvas>();
        Instantiate(sgsMain);
    }
}
