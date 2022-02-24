using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SgsMenu : MonoBehaviour
{
    private View.SgsMenu view;

    void Start()
    {
        view = GetComponent<View.SgsMenu>();

        Model.CardPile.Instance.PileCountView += view.UpdatePileCount;
    }

    private void OnDestroy()
    {
        Model.CardPile.Instance.PileCountView -= view.UpdatePileCount;
    }
}
